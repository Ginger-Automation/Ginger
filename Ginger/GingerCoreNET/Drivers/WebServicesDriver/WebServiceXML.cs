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

using System.Text;
using System.Net;    
using System.IO;
using System.Net.Security;  

public class WebServiceXML {

    public int ServiceConnectionTimeOut { get; set; }

    public string SendXMLRequest(string URL, string SOAPAction, string xmlRequest, ref string Status, ref bool FailFlag, NetworkCredential networkCreds = null)
    {
        // For SSL we need to say we trust the cert.
        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(
                delegate
                {
                    return true;
                }
                );
        
        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(URL);
        req.Method = "POST";
        req.ContinueTimeout = ServiceConnectionTimeOut <= 0 ? 350 : ServiceConnectionTimeOut;
        req.ContentType = "text/xml;charset=UTF-8";
        req.Headers.Add("SOAPAction", SOAPAction);
        req.Proxy = WebRequest.GetSystemWebProxy();

        if (networkCreds == null)
        {
            req.UseDefaultCredentials = true;
            req.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
        }
        else
        {
            //customized for ATT D2 Architecture to Pass Network Credentials
            req.UseDefaultCredentials = false;
            req.Credentials = networkCreds;
            //customized for ATT D2 Architecture
        }

        byte[] reqBytes = new UTF8Encoding().GetBytes(xmlRequest);
        req.ContentLength = reqBytes.Length;
        
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(reqBytes, 0, reqBytes.Length);
            }

        HttpWebResponse resp;
        string xmlResponse = null;
        try
        {

            resp = (HttpWebResponse)req.GetResponse();

            using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
            {
                xmlResponse = sr.ReadToEnd();
            }
            Status = resp.StatusDescription;
            return xmlResponse;
        }
        catch (WebException ex)
        {
            Status = ex.Message;
            FailFlag = true;

            using (var stream = ex.Response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                xmlResponse = reader.ReadToEnd();
                return xmlResponse;
            }
        }
    }    
}