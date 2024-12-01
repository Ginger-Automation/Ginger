#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Applitools.Utils;
using GingerCore.GeneralLib;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Amdocs.Ginger.CoreNET.GeneralLib
{
    public sealed class EmailReadGmailOperations : IEmailReadOperations, IDisposable
    {
        public Task ReadEmails(EmailReadFilters filters, EmailReadConfig config, Action<ReadEmail> emailProcessor)
        {
            ImapClient client = new ImapClient();
            try
            {
                client.Connect(config.IMapHost, Int32.Parse(config.IMapPort), true);
                client.Authenticate(config.UserEmail, config.UserPassword);

                CancellationToken cancellationToken = default(CancellationToken);

                var queryToImap = new SearchQuery();
                DateTimeOffset receivedStartDateTimeToOffset = new DateTimeOffset();
                DateTimeOffset receivedEndDateTimeToOffset = new DateTimeOffset();

                if (!string.IsNullOrEmpty(filters.From))
                {
                    queryToImap = queryToImap.And(SearchQuery.FromContains(filters.From));
                }
                if (!string.IsNullOrEmpty(filters.To))
                {
                    if (filters.To.Contains(';'))
                    {
                        foreach (string expectedRecipient in filters.To.Split(';', StringSplitOptions.TrimEntries))
                        {
                            queryToImap = queryToImap.And(SearchQuery.ToContains(expectedRecipient));
                        }
                    }
                    else
                    {
                        queryToImap = queryToImap.And(SearchQuery.ToContains(filters.To));
                    }
                }
                if (!string.IsNullOrEmpty(filters.Subject))
                {
                    queryToImap = queryToImap.And(SearchQuery.SubjectContains(filters.Subject));
                }

                if (!filters.ReceivedStartDate.Date.Equals(DateTime.MinValue.Date))
                {
                    receivedStartDateTimeToOffset = new DateTimeOffset(filters.ReceivedStartDate);
                    queryToImap = queryToImap.And(SearchQuery.DeliveredAfter(filters.ReceivedStartDate));
                }
                if (!(filters.ReceivedEndDate.Equals(DateTime.Today)))
                {
                    receivedEndDateTimeToOffset = new DateTimeOffset(filters.ReceivedEndDate);
                    queryToImap = queryToImap.And(SearchQuery.DeliveredBefore(filters.ReceivedEndDate.AddDays(1)));
                }
                if (filters.ReadUnread)
                {
                    queryToImap = queryToImap.And(SearchQuery.NotSeen);
                }

                IEnumerable<string> expectedContentTypes = null;
                if (filters.HasAttachments == EmailReadFilters.eHasAttachmentsFilter.Yes && (!string.IsNullOrEmpty(filters.AttachmentContentType)))
                {
                    expectedContentTypes = filters.AttachmentContentType.Split(";", StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
                }


                var inbox = client.Inbox;
                inbox.Open(FolderAccess.ReadWrite);   //.ReadOnly            

                var list = new UniqueIdSet(MailKit.Search.SortOrder.Descending);
                foreach (var uid in inbox.Search(queryToImap, cancellationToken))
                {
                    list.Add(uid);
                }
                // Add a condition to the search query.                        
                MimeMessage message = null;

                int readcount = filters.ReadCount, count = 0;

                foreach (var item in list)
                {
                    if (count >= readcount)
                    {
                        break;
                    }
                    message = inbox.GetMessage(item);

                    if (filters.HasAttachments == EmailReadFilters.eHasAttachmentsFilter.Either ||
                        ((filters.HasAttachments == EmailReadFilters.eHasAttachmentsFilter.No) && !message.Attachments.Any()) ||
                        ((filters.HasAttachments == EmailReadFilters.eHasAttachmentsFilter.Yes) && DoesSatisfyAttachmentFilter(message, expectedContentTypes)))
                    {
                        if ((string.IsNullOrEmpty(filters.Body)) || (message.TextBody.Contains(filters.Body, StringComparison.OrdinalIgnoreCase)))
                        {
                            if ((!filters.ReceivedStartDate.Date.Equals(DateTime.MinValue.Date)) || (!(filters.ReceivedEndDate.Equals(DateTime.Today))) && ((message.Date >= receivedStartDateTimeToOffset && message.Date <= receivedEndDateTimeToOffset) || (receivedStartDateTimeToOffset == receivedEndDateTimeToOffset)))
                            {
                                emailProcessor(ConvertMessageToReadEmail(message, expectedContentTypes));
                                if (filters.MarkRead.Equals(true))
                                {
                                    inbox.AddFlags(item, MessageFlags.Seen, true);
                                }
                                count++;
                            }
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error while reading emails using IMAP client.", ex);
                throw ex;
            }
            finally
            {
                if (client.IsConnected)
                {
                    client.Disconnect(true);
                }
                client.DisposeIfNotNull();
            }
            return Task.CompletedTask;
        }

        private bool DoesSatisfyAttachmentFilter(MimeMessage message, IEnumerable<string> expectedContentType)
        {
            if ((expectedContentType == null || !expectedContentType.Any()) && message.Attachments.Any())
            {
                return true;
            }
            else
            {
                return HasAnyAttachmentWithExpectedContentType(message, expectedContentType);
            }
        }
        private bool HasAnyAttachmentWithExpectedContentType(MimeMessage message,
            IEnumerable<string> expectedContentTypes)
        {
            bool hasAnyAttachmentWithExpectedContentType = false;

            if (message.Attachments.Any())
            {
                foreach (var attachment in message.Attachments)
                {
                    if (!string.IsNullOrEmpty(attachment.ContentType.MimeType) && expectedContentTypes.Any(expectedContentType => expectedContentType.Equals(attachment.ContentType.MimeType)))
                    {
                        hasAnyAttachmentWithExpectedContentType = true;
                    }
                }
            }
            return hasAnyAttachmentWithExpectedContentType;

        }

        public static byte[] ReadFully(IMimeContent input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.DecodeTo(ms);
                return ms.ToArray();
            }
        }

        private ReadEmail ConvertMessageToReadEmail(MimeMessage message,
            IEnumerable<string> expectedContentTypes)
        {
            IEnumerable<ReadEmail.Attachment> attachments = null;
            try
            {

                if (message.Attachments != null && message.Attachments.Any())
                {
                    if (expectedContentTypes != null)
                    {

                        attachments = message.Attachments
                           .Where(attachment => expectedContentTypes.Contains(attachment.ContentType.MimeType))
                           .Select(attachment => new ReadEmail.Attachment()
                           {
                               Name = ((MimePart)attachment).ContentType.Name,
                               ContentType = ((MimePart)attachment).ContentType.MimeType,
                               ContentBytes = ReadFully(((MimePart)attachment).Content)
                           });
                    }
                    else
                    {
                        attachments = message.Attachments
                            .Select(attachment => new ReadEmail.Attachment()
                            {
                                Name = ((MimePart)attachment).ContentType.Name,
                                ContentType = ((MimePart)attachment).ContentType.MimeType,
                                ContentBytes = ReadFully(((MimePart)attachment).Content)
                            });
                    }
                }

                return new ReadEmail()
                {
                    From = message.From?.ToString(),
                    Subject = message.Subject?.ToString(),
                    Body = message.Body?.ToString(),
                    ReceivedDateTime = message.Date == null ? DateTime.MinValue : message.Date.LocalDateTime,
                    HasAttachments = message.Attachments != null && message.Attachments.Any(),
                    Attachments = attachments
                };

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in ConvertMessageToReadEmail, fail to convert the message.", ex);
                return null;
            }
        }
        public void Dispose()
        {
        }
    }
}
