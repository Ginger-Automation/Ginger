#region License
/*
Copyright © 2014-2025 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Azure.Identity;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerCore.GeneralLib
{
    public sealed class EmailReadMSGraphOperations : IEmailReadOperations
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
        private int MessageRequestPageSize = 10;

        public async Task ReadEmails(EmailReadFilters filters, EmailReadConfig config, Action<ReadEmail> emailProcessor)
        {
            GraphServiceClient graphServiceClient = CreateGraphServiceClient(config);
            IEnumerable<ICollectionPage<Message>> messageCollections;
            if (filters.Folder == EmailReadFilters.eFolderFilter.All)
            {
                messageCollections = [await GetUserMessages(graphServiceClient, filters)];
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
            int count = 1;
            if (!string.IsNullOrEmpty(filters.To))
            {
                expectedRecipients = filters.To.Split(";", StringSplitOptions.RemoveEmptyEntries);
            }
            try
            {

                foreach (ICollectionPage<Message> messageCollection in messageCollections)
                {

                    PageIterator<Message> messageIterator = PageIterator<Message>.CreatePageIterator(
                        graphServiceClient,
                        messageCollection,
                        message =>
                        {
                            if (count > filters.ReadCount)
                            {
                                return false;
                            }
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
                            if (filters.MarkRead)
                            {
                                MarkEmailAsRead(graphServiceClient, message);
                            }
                            emailProcessor(ConvertMessageToReadEmail(message));
                            count++;
                            return true;
                        });

                    await messageIterator.IterateAsync();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error Occured while Making connection.Please check Configuration Details", ex);
                if (ex.InnerException != null)
                {
                    throw ex.InnerException;
                }
                throw;
            }

        }

        private bool MarkEmailAsRead(GraphServiceClient graphServiceClient, Message message)
        {
            try
            {
                var task = Task.Run(() =>
                {
                    try
                    {
                        graphServiceClient.Me.Messages[message.Id].
                    Request().UpdateAsync(new Microsoft.Graph.Message() { IsRead = true });
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to mark mail as Read", ex);
                    }
                });
                task.Wait();

                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to mark mail as Read", ex);
            }
            return false;
        }

        private bool DoesSatisfyToFilter(Message message, IEnumerable<string> expectedRecipients)
        {
            if (expectedRecipients == null)
            {
                return true;
            }
            IEnumerable<string> actualRecipients = message.ToRecipients.Where(r => r.EmailAddress != null && !string.IsNullOrEmpty(r.EmailAddress.Address)).Select(recipient => recipient.EmailAddress.Address);
            if ((actualRecipients == null) || (actualRecipients.Count() == 0))
            {
                return false;
            }
            else
            {
                return HasAllExpectedRecipient(expectedRecipients, actualRecipients);
            }
        }

        private async Task<bool> DoesSatisfyAttachmentFilter(GraphServiceClient graphServiceClient, Message message, EmailReadFilters filters)
        {
            if (filters.HasAttachments is EmailReadFilters.eHasAttachmentsFilter.Either or
                EmailReadFilters.eHasAttachmentsFilter.No)
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
            if (expectedContentTypes != null && !expectedContentTypes.Any())
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

            try
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
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in checking recipient criteria.", ex);
                return false;
            }
            return true;
        }

        private bool DoesSatisfyBodyFilter(Message message, string expectedBody)
        {
            return message.Body.Content.Contains(expectedBody, StringComparison.OrdinalIgnoreCase);
        }

        private GraphServiceClient CreateGraphServiceClient(EmailReadConfig config)
        {
            try
            {
                ValidateMSGraphConfig(config);
                TokenCredentialOptions options = new()
                {
                    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
                };
                UsernamePasswordCredential userNamePasswordCredential = new(config.UserEmail, config.UserPassword, config.TenantId, config.ClientId, options);

                return new GraphServiceClient(userNamePasswordCredential, Scopes);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error Occured while Making connection.Please check Configuration Details", ex);
                if (ex.InnerException != null)
                {
                    throw ex.InnerException;
                }
                throw;
            }

        }

        private void ValidateMSGraphConfig(EmailReadConfig config)
        {
            if (string.IsNullOrEmpty(config.UserEmail))
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
            if (filters.ReadCount < MessageRequestPageSize)
            {
                MessageRequestPageSize = filters.ReadCount;
            }
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
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error Occured while reading emails using Graph API", e);
                if (e.InnerException != null)
                {
                    throw e.InnerException;
                }
                throw;
            }
        }

        private async Task<IEnumerable<IMailFolderMessagesCollectionPage>> GetFoldersMessages(GraphServiceClient graphServiceClient, EmailReadFilters filters)
        {
            IEnumerable<string> folderNames = filters.FolderNames.Split(";", StringSplitOptions.RemoveEmptyEntries);
            List<IMailFolderMessagesCollectionPage> foldersMessages = [];
            foreach (string folderName in folderNames)
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
            AppendReadUnreadFilter(filterParameter, filters);
            return (filterParameter.ToString(), orderBy);
        }

        private async Task<string> GetFolderId(GraphServiceClient graphServiceClient, string aggregatedFolderName)
        {
            IEnumerable<string> folderNames = SplitFolderNames(aggregatedFolderName);

            MailFolder currentFolder = null;
            for (int index = 0; index < folderNames.Count(); index++)
            {
                currentFolder = await GetFolderByName(graphServiceClient, folderNames.ElementAt(index), currentFolder);
            }

            if (currentFolder == null)
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

            if (mailFolders.Count == 0)
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

            if (childFolders.Count == 0)
            {
                throw new InvalidOperationException($"No child folder found by name {folderName}");
            }

            return childFolders[0];
        }

        private IEnumerable<string> SplitFolderNames(string aggregatedfolderName)
        {
            List<string> folderNames = [];
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
        private void AppendReadUnreadFilter(StringBuilder filterParameter, EmailReadFilters filters)
        {

            if (filters.ReadUnread)
            {
                if (filterParameter.Length > 0)
                {
                    filterParameter.Append(" and ");
                }
                filterParameter.Append("isRead ne true");

            }
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
            try
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
                    From = message.From?.EmailAddress?.Address,
                    Subject = message.Subject,
                    Body = message.Body?.Content,
                    ReceivedDateTime = message.ReceivedDateTime?.DateTime.ToLocalTime() ?? DateTime.MinValue,
                    HasAttachments = (message.HasAttachments ?? false) && attachments != null && attachments.Any(),
                    Attachments = attachments
                };
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in ConvertMessageToReadEmail, fail to convert the message.", ex);
                return null;
            }

        }
    }
}
