#region License
/*
Copyright © 2014-2022 European Support Limited

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

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicExecutionLib
{
    public class MailReport : AddRunsetOperation
    {
        public string MailFrom { get; set; }
        public string MailTo { get; set; }
        public string MailCC { get; set; }

        public string Subject { get; set; }
        public string Comments { get; set; }

        public bool SendViaOutlook { get; set; }

        public SmtpDetails SmtpDetails { get; set; }

        public bool IncludeAttachmentReport { get; set; } = true;
    }
}
