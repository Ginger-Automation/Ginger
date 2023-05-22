using Amdocs.Ginger.Common.InterfacesLib;
using GingerCore.GeneralLib;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit;
using MimeKit;
//using Microsoft.Graph;
using GingerCore;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using Amdocs.Ginger.CoreNET.GeneralLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Diagnostics;
using VisualRegressionTracker;
using Applitools.Utils;
using NPOI.SS.Formula.Functions;
using NUglify.Helpers;
using System.Threading;

namespace Amdocs.Ginger.CoreNET.GeneralLib
{
    public sealed class EmailReadGmailOperations : IEmailReadOperations
    {
        MemoryStream ms = new MemoryStream();
        public Task ReadEmails(EmailReadFilters filters, EmailReadConfig config, Action<ReadEmail> emailProcessor)
        {
            ValidateIMapConfig(config);
            ImapClient client = new ImapClient();            
            client.Connect("imap.gmail.com", 993, true);
            String UserName = config.UserEmail;
            String UserPassword = config.UserPassword;            
            client.Authenticate(UserName, UserPassword);
            var inbox = client.Inbox;
            inbox.Open(FolderAccess.ReadOnly);
            CancellationToken cancellationToken = default(CancellationToken);
            

            SearchQuery query = new SearchQuery();
            SearchQuery subquery = new SearchQuery();
            BinarySearchQuery combinedquery = new BinarySearchQuery(SearchTerm.All,query,subquery);

            if (!string.IsNullOrEmpty(filters.From))
            {
                subquery = SearchQuery.FromContains(filters.From);      
                combinedquery = SearchQuery.And(combinedquery, subquery);
            }
            if(!string.IsNullOrEmpty(filters.To))
            {
                if (filters.To.Contains(","))
                {
                    IEnumerable<string> actualRecipients = filters.To.Split(',',StringSplitOptions.TrimEntries);
                    foreach (string expectedRecipient in actualRecipients)
                    {
                        subquery = SearchQuery.ToContains(expectedRecipient);
                        combinedquery = SearchQuery.And(combinedquery, subquery);
                    }
                }
                else
                {
                    subquery = SearchQuery.ToContains(filters.To);
                    combinedquery = SearchQuery.And(combinedquery, subquery);
                }
                
            }
            if (!string.IsNullOrEmpty(filters.Subject))
            {
                combinedquery = SearchQuery.And(combinedquery, subquery);
            }
            if (!string.IsNullOrEmpty(filters.Body))
            {
                subquery = SearchQuery.BodyContains(filters.Body);
                combinedquery = SearchQuery.And(combinedquery, subquery);
            }
            if(!((filters.ReceivedStartDate.Equals(DateTime.MinValue)) && (filters.ReceivedEndDate.Equals(DateTime.Today))))
            {
                subquery = SearchQuery.DeliveredAfter(filters.ReceivedStartDate);
                combinedquery = SearchQuery.And(combinedquery, subquery);
                subquery = SearchQuery.DeliveredBefore(filters.ReceivedEndDate);
                combinedquery = SearchQuery.And(combinedquery, subquery);
            }
            var matched = new UniqueIdSet();

            IList<UniqueId> list = inbox.Search(combinedquery, cancellationToken);
            foreach (var item in list)
            {
                MimeMessage message = inbox.GetMessage(item);               
                emailProcessor(ConvertMessageToReadEmail(message, filters));
                
            }
  
            return Task.CompletedTask;
        }


        /// /////////////////////////////////// 
        


        private async void DownloadAttachment(IEnumerable<MimeEntity> atchment, String id)
        {
            foreach (var attachment in atchment)
            {

                String downloadpath = "C:\\NP\\";
               
                HeaderList hlist = ((MimePart)attachment).Headers;             

                if (attachment is MimePart)
                {
                    
                    var file = File.Create(downloadpath + ((MimePart)attachment).FileName);

                    await ((MimePart)attachment).Content.DecodeToAsync(ms);
                }

            }
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

        private ReadEmail ConvertMessageToReadEmail(MimeMessage message, EmailReadFilters filters)
        {
                IEnumerable<ReadEmail.Attachment> attachments = new List<ReadEmail.Attachment>();      
           
                if (message.Attachments != null && message.Attachments.Count() > 0)
                {
                    attachments = message.Attachments
                        .Where(attachment => attachment.ContentType.Equals(filters.AttachmentContentType))
                        .Select(attachment => new ReadEmail.Attachment()
                        {
                            Name = ((MimePart)attachment).ContentType.ToString().Split(";", StringSplitOptions.RemoveEmptyEntries)[1],
                            ContentType = ((MimePart)attachment).ContentType.ToString().Split(";", StringSplitOptions.RemoveEmptyEntries)[0].Split(":", StringSplitOptions.RemoveEmptyEntries)[1].Trim(),
                            ContentBytes = ms.ToArray()
                        }).ToList();
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
    }
}
