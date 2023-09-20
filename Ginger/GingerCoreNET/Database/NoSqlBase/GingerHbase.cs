using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using static GingerCore.Actions.ActDBValidation;
using HBaseNet.Utility;
//using Pb;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Net;
using GingerCore.Drivers.Selenium.SeleniumBMP;
using Pb;

namespace GingerCore.NoSqlBase
{

    public class GingerHbase
    {
        public string Base64Encode(string text)
        {
            var textBytes = System.Text.Encoding.UTF8.GetBytes(text);
            return System.Convert.ToBase64String(textBytes);
        }
        public string Base64Decode(string base64)
        {
            var base64Bytes = System.Convert.FromBase64String(base64);
            return System.Text.Encoding.UTF8.GetString(base64Bytes);
        }
        public List<string> GetKeyspaceList()
        {
            throw new NotImplementedException();
        }
        //public List<string> GetColumnList(string table){

        //}


        public GingerHbase()
        {
            
        }
        public List<string> TableList ;
        public List<string> ColumnList ;

        public async Task GetTableList()
        {
            //List<string> dbList = new List<string>();                       
            Connect_HB("GetTableList","").Wait();
            //return TableList;
        }

        public async Task GetColumnList(string Tablename)
        {
            Connect_HB("GetColumnList",Tablename).Wait();            
            //return ColumnList;
        }



        public async Task Connect_HB(string Operation,string tableName)
        {
            //string hbaseBaseUrl = "http://localhost:8080";
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Accept", "text/xml");
               // string tableName = "Employee";
                string rowKey = "2";
                string apiUrl = "";

                switch (Operation)
                {
                    case "GetTableList":
                        apiUrl = "http://localhost:8080/";
                        break;
                    case "DBOperation":
                        apiUrl = "http://localhost:8080/" + tableName + "/scanner/";
                        string Filter_FieldName = Base64Encode("FirstName");
                        string Filter_FieldValue = Base64Encode("Reeta");
                        string FltOperand = "EQUAL";
                        //string pathName = "C://AQE//myFile.txt";
                        string FilterQuery = "<Scanner batch=\"10\">\r\n    <filter>\r\n        {\r\n            \"type\": \"SingleColumnValueFilter\",\r\n            \"op\": \"" + FltOperand + "\",\r\n            \"family\": \"UGVyc29uYWxEYXRh\",\r\n            \"qualifier\": \"" + Filter_FieldName + "\",\r\n            \"latestVersion\": true,\r\n            \"comparator\": {\r\n                \"type\": \"BinaryComparator\",\r\n                \"value\": \"" + Filter_FieldValue + "\"\r\n            }\r\n        }\r\n    </filter>\r\n</Scanner>";
                        var requestMessage = new HttpRequestMessage(HttpMethod.Put, apiUrl);
                        requestMessage.Headers.Add("Accept", "text/xml");
                        //requestMessage.Headers.Add("Content-Type", "text/xml");
                        requestMessage.Content = new StringContent(FilterQuery, Encoding.UTF8, "text/xml");
                        var result = await httpClient.SendAsync(requestMessage);
                        string absolutePath = result.Headers.Location.AbsolutePath;
                        int j = absolutePath.Length;
                        string fileid = "";
                        int i = absolutePath.LastIndexOf("/");
                        if (absolutePath.LastIndexOf("/") >= 0)
                            fileid = absolutePath.Substring(i + 1);
                        apiUrl = apiUrl + fileid;
                        break;
                    case "GetColumnList":
                        apiUrl = "http://localhost:8080/Employee/1";
                        break;
                    case "RowCount":
                        apiUrl = "http://localhost:8080/Employee/*";
                        break;

                }
                var FinalResponse = await httpClient.GetStringAsync(apiUrl);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(FinalResponse);
                string xpath = "";
                XmlNodeList nodes;
                switch (Operation)
                {
                    case "DBOperation":
                        xpath = "CellSet/Row/Cell";
                        nodes = xmlDoc.SelectNodes(xpath);
                        foreach (XmlNode childrenNode in nodes)
                        {
                            string fieldname = Base64Decode(childrenNode.Attributes[0].Value);
                            string fieldvalue = Base64Decode(childrenNode.InnerText);
                        }
                        break;                    
                    case "GetTableList":
                        xpath = "TableList/table";
                        nodes = xmlDoc.SelectNodes(xpath);
                        TableList = new List<string>();
                        foreach (XmlNode childrenNode in nodes)
                        {
                            string table_name = childrenNode.Attributes[0].Value;
                            TableList.Add(table_name);
                        }
                        break;
                    case "GetColumnList":
                        xpath = "CellSet/Row/Cell";
                        nodes = xmlDoc.SelectNodes(xpath);
                        ColumnList = new List<string>();
                        foreach (XmlNode childrenNode in nodes)
                        {
                            string columnname = Base64Decode(childrenNode.Attributes[0].Value);
                            ColumnList.Add(columnname);
                        }
                        break;
                    case "RowCount":
                        xpath = "CellSet/Row";
                        nodes = xmlDoc.SelectNodes(xpath);
                        int count = nodes.Count;
                        break;
                }

            }
            var list = this.ColumnList;

        }

       
    }


}
