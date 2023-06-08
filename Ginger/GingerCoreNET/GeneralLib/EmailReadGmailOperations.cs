#region License
/*
Copyright © 2014-2023 European Support Limited

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

using Amdocs.Ginger.Common.InterfacesLib;
using GingerCore.GeneralLib;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit;
using MimeKit;
using Applitools.Utils;
using System.Threading;
using Amdocs.Ginger.Common;


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
                var inbox = client.Inbox;
                inbox.Open(FolderAccess.ReadOnly);
                CancellationToken cancellationToken = default(CancellationToken);

                var queryToImap = new SearchQuery();

                if (!string.IsNullOrEmpty(filters.From))
                {
                    queryToImap = queryToImap.And(SearchQuery.FromContains(filters.From));
                }
                if (!string.IsNullOrEmpty(filters.To))
                {
                    if (filters.To.Contains(","))
                    {
                        IEnumerable<string> actualRecipients = filters.To.Split(',', StringSplitOptions.TrimEntries);
                        foreach (string expectedRecipient in actualRecipients)
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
                if (!string.IsNullOrEmpty(filters.Body))
                {
                    queryToImap = queryToImap.And(SearchQuery.SubjectContains(filters.Body));
                }
                if (!(filters.ReceivedStartDate.Equals(DateTime.MinValue)) || (filters.ReceivedEndDate.Equals(DateTime.Today)))
                {
                    queryToImap = queryToImap.And(SearchQuery.DeliveredAfter(filters.ReceivedStartDate)).And(SearchQuery.DeliveredBefore(filters.ReceivedEndDate));
                }
                
                // Add a condition to the search query.
                IList<UniqueId> list = inbox.Search(queryToImap, cancellationToken);
                IEnumerable<string> expectedContentTypes = null;
                if (filters.HasAttachments == EmailReadFilters.eHasAttachmentsFilter.Yes && (!string.IsNullOrEmpty(filters.AttachmentContentType)))
                {                    
                    expectedContentTypes = filters.AttachmentContentType.Split(";", StringSplitOptions.RemoveEmptyEntries);
                }

                foreach (var item in list)
                {
                    MimeMessage message = inbox.GetMessage(item);
                   
                    if (filters.HasAttachments == EmailReadFilters.eHasAttachmentsFilter.Either || 
                        ((filters.HasAttachments == EmailReadFilters.eHasAttachmentsFilter.No) && message.Attachments.Count() == 0) ||
                        DoesSatisfyAttachmentFilter(message, expectedContentTypes))
                    {
                        emailProcessor(ConvertMessageToReadEmail(message, filters));
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
           
            if (expectedContentType == null || expectedContentType.Count() == 0)
            {
                return true;
            }
            else            
            {                
                return HasAnyAttachmentWithExpectedContentType(message, expectedContentType);
            }                     
        }


        private bool HasAnyAttachmentWithExpectedContentType( MimeMessage message,
            IEnumerable<string> expectedContentTypes)
        {
            bool hasAnyAttachmentWithExpectedContentType = false;
            IEnumerable<MimeEntity> attachments = message.Attachments;
            
            if (attachments.Count() > 0)
            {
                foreach (var attachment in attachments)
                {                                      
                    if (!string.IsNullOrEmpty(attachment.ContentType.MimeType) && expectedContentTypes.Any(expectedContentType => expectedContentType.Equals(attachment.ContentType.MimeType)))
                    {
                        hasAnyAttachmentWithExpectedContentType = true;
                    }
                }
            }
            return hasAnyAttachmentWithExpectedContentType;
                
        }


        private void ValidateIMapConfig(EmailReadConfig config)
        {
            if (string.IsNullOrEmpty(config.UserEmail))
            {
                throw new ArgumentException("user email is required");
            }
            if (string.IsNullOrEmpty(config.UserPassword))
            {
                throw new ArgumentException("user password is required");
            }
        }

        public static byte[] ReadFully(IMimeContent input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.DecodeTo(ms);
                return ms.ToArray();
            }
        }

        private ReadEmail ConvertMessageToReadEmail(MimeMessage message, EmailReadFilters filters)
        {
            IEnumerable<ReadEmail.Attachment> attachments = null ;

            if (message.Attachments != null && message.Attachments.Count() > 0)
            {
                attachments = message.Attachments
                    .Where(attachment => attachment.ContentType.MimeType.Equals(filters.AttachmentContentType))
                    .Select(attachment => new ReadEmail.Attachment()
                    {
                        Name = ((MimePart)attachment).ContentType.Name,
                        ContentType = ((MimePart)attachment).ContentType.MimeType,
                        ContentBytes = ReadFully(((MimePart)attachment).Content)
                    });
            }

            return new ReadEmail()
            {
                From = message.From.ToString(),
                Subject = message.Subject.ToString(),
                Body = message.Body.ToString(),
                ReceivedDateTime = message.Date.LocalDateTime,
                HasAttachments = message.Attachments.Count() > 0,
                Attachments = attachments
            };

        }
        public void Dispose()
        {
        }
    }
}
