using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Azure.Identity;
using GingerCore.DataSource;
using GingerCore.Environments;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GingerCore.Actions.Communication.ActeMail;

namespace GingerCore.GeneralLib
{
    public sealed class EmailReadOperations : IEmailReadOperations
    {
        private static readonly IEnumerable<string> Scopes = new[]
        {
            "email",
            "offline_access",
            "https://graph.microsoft.com/Mail.Read"
        };
        private static readonly IEnumerable<string> SelectedMessageFields = new[]
        {
            "from",
            "body",
            "toRecipients",
            "subject",
            "receivedDateTime",
            "attachments",
            "hasAttachments"
        };
        private const int MessageRequestPageSize = 10;

        public async Task ReadEmails(EmailReadFilters filters, MSGraphConfig config, Action<ReadEmail> emailProcessor)
        {
            GraphServiceClient graphServiceClient = CreateGraphServiceClient(config);
            IEnumerable<ICollectionPage<Message>> messageCollections;
            if (filters.Folder == EmailReadFilters.eFolderFilter.All)
            {
                messageCollections = new List<ICollectionPage<Message>>() { await GetUserMessages(graphServiceClient, filters) };
            }
            else
            {
                messageCollections = await GetFoldersMessages(graphServiceClient, filters);
            }

            await IterateMessages(messageCollections, filters, graphServiceClient, emailProcessor);
        }

        private async Task IterateMessages(IEnumerable<ICollectionPage<Message>> messageCollections, EmailReadFilters filters, 
            GraphServiceClient graphServiceClient, Action<ReadEmail> emailProcessor)
        {
            IEnumerable<string> expectedRecipients = null;
            if (!string.IsNullOrEmpty(filters.To))
            {
                expectedRecipients = filters.To.Split(";", StringSplitOptions.RemoveEmptyEntries);
            }

            foreach (ICollectionPage<Message> messageCollection in messageCollections)
            {
                PageIterator<Message> messageIterator = PageIterator<Message>.CreatePageIterator(
                    graphServiceClient,
                    messageCollection,
                    message =>
                    {
                        if (!DoesSatisfyToFilter(message, expectedRecipients))
                        {
                            return true;
                        }
                        if (!DoesSatisfyAttachmentFilter(graphServiceClient, message, filters).Result)
                        {
                            return true;
                        }
                        if (!DoesSatisfyBodyFilter(message, filters.Body))
                        {
                            return true;
                        }

                        emailProcessor(ConvertMessageToReadEmail(message));
                        return true;
                    });
                await messageIterator.IterateAsync();
            }
        }

        private bool DoesSatisfyToFilter(Message message, IEnumerable<string> expectedRecipients)
        {
            if (expectedRecipients == null)
            {
                return true;
            }

            IEnumerable<string> actualRecipients = message.ToRecipients.Select(recipient => recipient.EmailAddress.Address);
            return HasAllExpectedRecipient(expectedRecipients, actualRecipients);
        }

        private async Task<bool> DoesSatisfyAttachmentFilter(GraphServiceClient graphServiceClient, Message message, EmailReadFilters filters)
        {
            if (filters.HasAttachments == EmailReadFilters.eHasAttachmentsFilter.Either ||
                filters.HasAttachments == EmailReadFilters.eHasAttachmentsFilter.No)
            {
                return true;
            }
            else if (!(await HasAnyNonInlineAttachment(graphServiceClient, message)))
            {
                return false;
            }

            if (string.IsNullOrEmpty(filters.AttachmentContentType))
            {
                return true;
            }
            IEnumerable<string> expectedContentTypes = null;
            if (!string.IsNullOrEmpty(filters.AttachmentContentType))
            {
                expectedContentTypes = filters.AttachmentContentType.Split(";", StringSplitOptions.RemoveEmptyEntries);
            }
            if (expectedContentTypes != null && expectedContentTypes.Count() == 0)
            {
                return true;
            }

            return await HasAnyAttachmentWithExpectedContentType(graphServiceClient, message, expectedContentTypes);
        }

        private async Task<bool> HasAnyNonInlineAttachment(GraphServiceClient graphServiceClient, Message message)
        {
            bool hasAnyNonInlineAttachments = false;
            PageIterator<Attachment> attachmentIterator = PageIterator<Attachment>.CreatePageIterator(
                graphServiceClient,
                message.Attachments,
                attachment =>
                {
                    if (!(attachment.IsInline ?? false))
                    {
                        hasAnyNonInlineAttachments = true;
                    }

                    bool continueIterating = !hasAnyNonInlineAttachments;
                    return continueIterating;
                });
            await attachmentIterator.IterateAsync();
            return hasAnyNonInlineAttachments;
        }

        private async Task<bool> HasAnyAttachmentWithExpectedContentType(GraphServiceClient graphServiceClient, Message message,
            IEnumerable<string> expectedContentTypes)
        {
            bool hasAnyAttachmentWithExpectedContentType = false;
            PageIterator<Attachment> attachmentIterator = PageIterator<Attachment>.CreatePageIterator(
                graphServiceClient,
                message.Attachments,
                attachment =>
                {
                    bool continueIterating = true;
                    if (attachment.IsInline ?? false)
                    {
                        return continueIterating;
                    }

                    if (expectedContentTypes.Any(expectedContentType => expectedContentType.Equals(attachment.ContentType)))
                    {
                        hasAnyAttachmentWithExpectedContentType = true;
                        continueIterating = false;
                    }

                    return continueIterating;
                });
            await attachmentIterator.IterateAsync();

            return hasAnyAttachmentWithExpectedContentType;
        }

        private Task<IMessageAttachmentsCollectionPage> GetMessageAttachments(GraphServiceClient graphServiceClient, string messageId,
            IEnumerable<string> expectedContentTypes)
        {
            StringBuilder filterParameter = new();
            filterParameter.Append("isInline eq false");
            filterParameter.Append(" and ");

            filterParameter.Append("contentType in (");
            foreach (string expectedContentType in expectedContentTypes)
            {
                filterParameter.Append($"'{expectedContentType}',");
            }

            if (filterParameter.Length > 0 && filterParameter[^1] == ',')
            {
                filterParameter.Remove(filterParameter.Length - 1, 1);
            }
            filterParameter.Append(')');

            return graphServiceClient
                .Me
                .Messages[messageId]
                .Attachments
                .Request()
                .Filter(filterParameter.ToString())
                .Top(10)
                .GetAsync();
        }

        private bool HasAllExpectedRecipient(IEnumerable<string> expectedRecipients, IEnumerable<string> actualRecipients)
        {
            foreach (string expectedRecipient in expectedRecipients)
            {
                bool hasExpectedRecipient = actualRecipients.Any(actualRecipient => 
                    actualRecipient.Equals(expectedRecipient, StringComparison.OrdinalIgnoreCase));
                if (!hasExpectedRecipient)
                {
                    return false;
                }
            }
            return true;
        }

        private bool DoesSatisfyBodyFilter(Message message, string expectedBody)
        {
            return message.Body.Content.Contains(expectedBody, StringComparison.OrdinalIgnoreCase);
        }

        private GraphServiceClient CreateGraphServiceClient(MSGraphConfig config)
        {
            ValidateMSGraphConfig(config);
            TokenCredentialOptions options = new()
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            string userPassword = config.UserPassword;
            UsernamePasswordCredential userNamePasswordCredential = new(config.UserEmail, userPassword, config.TenantId, config.ClientId, options);

            return new GraphServiceClient(userNamePasswordCredential, Scopes);
        }

        private void ValidateMSGraphConfig(MSGraphConfig config)
        {
            if(string.IsNullOrEmpty(config.UserEmail))
            {
                throw new ArgumentException("user email is required");
            }
            if (string.IsNullOrEmpty(config.UserPassword))
            {
                throw new ArgumentException("user password is required");
            }
            if (string.IsNullOrEmpty(config.TenantId))
            {
                throw new ArgumentException("tenant id is required");
            }
            if (string.IsNullOrEmpty(config.ClientId))
            {
                throw new ArgumentException("client id is required");
            }
        }

        private async Task<IUserMessagesCollectionPage> GetUserMessages(GraphServiceClient graphServiceClient, EmailReadFilters filters)
        {
            (string filterParameter, string orderByParameter) = BuildReadRequestFilterAndOrderParameters(filters);
            string selectParameter = SelectedMessageFields.Aggregate((aggr, value) => $"{aggr},{value}");
            try
            {
                return await graphServiceClient
                    .Me
                    .Messages
                    .Request()
                    .Header("Prefer", "outlook.body-content-type='text'")
                    .Select(selectParameter)
                    .Filter(filterParameter)
                    .OrderBy(orderByParameter)
                    .Expand("attachments")
                    .Top(MessageRequestPageSize)
                    .GetAsync();
            }
            catch(Exception e)
            {
                if(e.InnerException != null)
                {
                    throw e.InnerException;
                }    
                throw;
            }
        }

        private async Task<IEnumerable<IMailFolderMessagesCollectionPage>> GetFoldersMessages(GraphServiceClient graphServiceClient, EmailReadFilters filters)
        {
            IEnumerable<string> folderNames = filters.FolderNames.Split(";", StringSplitOptions.RemoveEmptyEntries);
            List<IMailFolderMessagesCollectionPage> foldersMessages = new();
            foreach(string folderName in folderNames)
            {
                foldersMessages.Add(await GetFolderMessages(folderName, graphServiceClient, filters));
            }

            return foldersMessages;
        }

        private async Task<IMailFolderMessagesCollectionPage> GetFolderMessages(string folderName, GraphServiceClient graphServiceClient, 
            EmailReadFilters filters)
        {
            (string filterParameter, string orderByParameter) = BuildReadRequestFilterAndOrderParameters(filters);
            string selectParameter = SelectedMessageFields.Aggregate((aggr, value) => $"{aggr},{value}");
            string folderId = await GetFolderId(graphServiceClient, folderName);
            try
            {
                return await graphServiceClient
                    .Me
                    .MailFolders[folderId]
                    .Messages
                    .Request()
                    .Header("Prefer", "outlook.body-content-type='text'")
                    .Select(selectParameter)
                    .Filter(filterParameter)
                    .OrderBy(orderByParameter)
                    .Expand("attachments")
                    .Top(MessageRequestPageSize)
                    .GetAsync();
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    throw e.InnerException;
                }
                throw;
            }
        }

        private (string filter, string orderBy) BuildReadRequestFilterAndOrderParameters(EmailReadFilters filters)
        {
            string orderBy = "receivedDateTime desc";

            StringBuilder filterParameter = new();

            AppendReceivedDateTimeFilter(filterParameter, filters);
            AppendFromFilter(filterParameter, filters);
            AppendSubjectFilter(filterParameter, filters);
            AppendHasAttachmentsFilter(filterParameter, filters);

            return (filterParameter.ToString(), orderBy);
        }

        private async Task<string> GetFolderId(GraphServiceClient graphServiceClient, string aggregatedFolderName)
        {
            IEnumerable<string> folderNames = SplitFolderNames(aggregatedFolderName);

            MailFolder currentFolder = null;
            for(int index = 0; index < folderNames.Count(); index++)
            {
                currentFolder = await GetFolderByName(graphServiceClient, folderNames.ElementAt(index), currentFolder);
            }

            if(currentFolder == null)
            {
                throw new InvalidOperationException($"No user folder found by name {aggregatedFolderName}");
            }

            return currentFolder.Id;
        }

        private Task<MailFolder> GetFolderByName(GraphServiceClient graphServiceClient, string folderName, MailFolder parentFolder = null)
        {
            if (parentFolder == null)
            {
                return GetUserMailFolderByName(graphServiceClient, folderName);
            }
            else
            {
                return GetChildMailFolderByName(graphServiceClient, folderName, parentFolder);
            }
        }

        private async Task<MailFolder> GetUserMailFolderByName(GraphServiceClient graphServiceClient, string folderName)
        {
            IUserMailFoldersCollectionPage mailFolders = await graphServiceClient
                .Me
                .MailFolders
                .Request()
                .Select("id, displayName")
                .Filter($"displayName eq '{folderName}'")
                .Top(1)
                .GetAsync();

            if(mailFolders.Count == 0)
            {
                throw new InvalidOperationException($"No user folder found by name {folderName}");
            }

            return mailFolders[0];
        }

        private async Task<MailFolder> GetChildMailFolderByName(GraphServiceClient graphServiceClient, string folderName, MailFolder parentFolder)
        {
            IMailFolderChildFoldersCollectionPage childFolders = await graphServiceClient
                .Me
                .MailFolders[parentFolder.Id]
                .ChildFolders
                .Request()
                .Select("id, displayName")
                .Filter($"displayName eq '{folderName}'")
                .Top(1)
                .GetAsync();

            if(childFolders.Count == 0)
            {
                throw new InvalidOperationException($"No child folder found by name {folderName}");
            }

            return childFolders[0];
        }

        private IEnumerable<string> SplitFolderNames(string aggregatedfolderName)
        {
            List<string> folderNames = new();
            StringBuilder folderNameBuilder = new();
            char prevChar = '\0';
            foreach (char currentChar in aggregatedfolderName)
            {
                if (currentChar == '/')
                {
                    if (prevChar == '\\')
                    {
                        folderNameBuilder.Append(currentChar);
                    }
                    else if (folderNameBuilder.Length > 0)
                    {
                        folderNames.Add(folderNameBuilder.ToString());
                        folderNameBuilder.Clear();
                    }
                }
                else if (currentChar != '\\')
                {
                    folderNameBuilder.Append(currentChar);
                }

                prevChar = currentChar;
            }

            if (folderNameBuilder.Length > 0)
            {
                folderNames.Add(folderNameBuilder.ToString());
            }

            return folderNames;
        }

        private void AppendReceivedDateTimeFilter(StringBuilder filterParameter, EmailReadFilters filters)
        {
            if (filterParameter.Length > 0)
            {
                filterParameter.Append(" and ");
            }

            const string dateTimeFormat = "yyyy-MM-ddTHH:mm:ssK";

            string receivedStartDateTimeAsString = filters.ReceivedStartDate.ToUniversalTime().ToString(dateTimeFormat);
            string receivedEndDateTimeAsString = filters.ReceivedEndDate.ToUniversalTime().ToString(dateTimeFormat);
            filterParameter.Append($"receivedDateTime ge {receivedStartDateTimeAsString} and receivedDateTime le {receivedEndDateTimeAsString}");
        }

        private void AppendFromFilter(StringBuilder filterParameter, EmailReadFilters filters)
        {
            if (string.IsNullOrEmpty(filters.From))
            {
                return;
            }

            if (filterParameter.Length > 0)
            {
                filterParameter.Append(" and ");
            }

            filterParameter.Append($"from/emailAddress/address eq '{filters.From}'");
        }

        private void AppendSubjectFilter(StringBuilder filterParameter, EmailReadFilters filters)
        {
            if (string.IsNullOrEmpty(filters.Subject))
            {
                return;
            }

            if (filterParameter.Length > 0)
            {
                filterParameter.Append(" and ");
            }

            filterParameter.Append($"contains(subject,'{filters.Subject}')");
        }

        private void AppendHasAttachmentsFilter(StringBuilder filterParameter, EmailReadFilters filters)
        {
            if (filters.HasAttachments == EmailReadFilters.eHasAttachmentsFilter.Either)
            {
                return;
            }

            if (filterParameter.Length > 0)
            {
                filterParameter.Append(" and ");
            }

            filterParameter.Append($"hasAttachments eq {(filters.HasAttachments == EmailReadFilters.eHasAttachmentsFilter.Yes).ToString().ToLower()}");
        }

        private ReadEmail ConvertMessageToReadEmail(Message message)
        {
            IEnumerable<ReadEmail.Attachment> attachments = null;
            if (message.HasAttachments ?? false)
            {
                attachments = message.Attachments
                    .Where(attachment => attachment.GetType().Equals(typeof(FileAttachment)))
                    .Select(attachment => new ReadEmail.Attachment()
                    {
                        Name = attachment.Name,
                        ContentType = attachment.ContentType,
                        ContentBytes = ((FileAttachment)attachment).ContentBytes
                    });
            }
            return new ReadEmail()
            {
                From = message.From.EmailAddress.Address,
                Subject = message.Subject,
                Body = message.Body.Content,
                ReceivedDateTime = message.ReceivedDateTime?.DateTime.ToLocalTime() ?? DateTime.MinValue,
                HasAttachments = (message.HasAttachments ?? false) && attachments != null && attachments.Count() > 0,
                Attachments = attachments
            };
        }
    }
}
