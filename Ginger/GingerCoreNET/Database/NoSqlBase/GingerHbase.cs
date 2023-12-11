using Amdocs.Ginger.Common;
using Applitools.Utils;
using GingerCore.Actions;
using Microsoft.HBase.Client;
using MongoDB.Driver;
using OctaneStdSDK.Entities.Base;
using org.apache.hadoop.hbase.rest.protobuf.generated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using static GingerCore.Actions.ActDBValidation;
using BinaryComparator = Microsoft.HBase.Client.Filters.BinaryComparator;
using Cell = org.apache.hadoop.hbase.rest.protobuf.generated.Cell;
using CompareFilter = Microsoft.HBase.Client.Filters.CompareFilter;
using FamilyFilter = Microsoft.HBase.Client.Filters.FamilyFilter;
using Filter = Microsoft.HBase.Client.Filters.Filter;
using FilterList = Microsoft.HBase.Client.Filters.FilterList;
using FirstKeyOnlyFilter = Microsoft.HBase.Client.Filters.FirstKeyOnlyFilter;
using RegexStringComparator = Microsoft.HBase.Client.Filters.RegexStringComparator;
using RequestOptions = Microsoft.HBase.Client.RequestOptions;
using Scanner = org.apache.hadoop.hbase.rest.protobuf.generated.Scanner;
using Microsoft.HBase.Client.Filters;
using SingleColumnValueFilter = Microsoft.HBase.Client.Filters.SingleColumnValueFilter;
namespace GingerCore.NoSqlBase
{

    public class GingerHbase : NoSqlBase
    {       
        ActDBValidation Act = null;
        string connectionUrl;
        string userName;
        string password;
        private SecureString _clusterPassword;
        
        private static ClusterCredentials ClCredential;
        private static readonly Encoding _encoding = Encoding.UTF8;
        private static string familyName;
       
        public GingerHbase(string url,string Uname,string passwd)
        {
            this.connectionUrl = url;
            this.userName = Uname;
            this.password = passwd;
        }
        public GingerHbase(eDBValidationType DBValidationtype, Environments.Database mDB, ActDBValidation mact)
        {
            Action = DBValidationtype;
            this.Db = mDB;
            Act = mact;            
        }

        public override bool MakeSureConnectionIsOpen()
        {
            return Connect();
        }
        HBaseClient client;
        RequestOptions requestOption;
        public override  bool Connect()
        {

            this.connectionUrl = Db.DatabaseOperations.TNSCalculated;
            this.userName = Db.DatabaseOperations.UserCalculated;
            this.password = Db.DatabaseOperations.PassCalculated;
            var ConnectionUri = new Uri(this.connectionUrl);           
            
            ClCredential = new(ConnectionUri, this.userName,this.password);
           

            requestOption =  new RequestOptions
            {
                RetryPolicy = Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.RetryPolicy.NoRetry,
                KeepAlive = true,
                TimeoutMillis = 30000,
                ReceiveBufferSize = 1048576,
                SerializationBufferSize = 1048576,
                UseNagle = false,
                AlternativeEndpoint = "",
                Port = ConnectionUri.Port,
                AlternativeHost = null
            };

            client = new HBaseClient(ClCredential, requestOption);
                
            try
            {
                              
                TableList tables = new TableList();
                Task getTablesTask = Task.Run(() =>
                {
                    try
                    {
                        tables = client.ListTablesAsync().Result;
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.WARN, "Unable to connect to Hbase and get table list ", ex);                        
                    }                                 
                    
                });

                getTablesTask.Wait();

                if (tables.name.Count() == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.WARN, "Unable to connect to Hbase", ex);
                return false;
            }         
        }       

        public string[] getWhereParts(string refstring,string familyname) //This returns a list where the first element signifies the operator,second the family name ,third the fieldname  and fourth the fieldvalue
        {
            string[] resarray = new string[4];
            string[] words1 = new string[2];

            if (refstring.Contains(" Like "))
            {
                words1 = refstring.Split(" Like ");
                resarray[0] = "RegexComp";
            }
          
            if (refstring.Contains("!=") || refstring.Contains("<>"))
            {
                words1 = refstring.Split("!=");
                resarray[0] = "NotEqual";
            }
            else if (refstring.Contains("<="))
            {
                words1 = refstring.Split("<=");
                resarray[0] = "LessThanOrEqualTo";
            }
            else if (refstring.Contains(">="))
            {
                words1 = refstring.Split(">=");
                resarray[0] = "GreaterThanOrEqualTo";
            }

            else if (refstring.Contains('='))
            {
                words1 = refstring.Split('=');
                resarray[0] = "Equal";
            }
            else if (refstring.Contains('<'))
            {
                words1 = refstring.Split('<');
                resarray[0] = "LessThan";
            } 

            else if (refstring.Contains('>'))
            {
                words1 = refstring.Split('>');
                resarray[0] = "GreaterThan";

            }           

            if (words1[0].Contains(':'))
            {
                string[] temp = words1[0].Split(':');
                resarray[1] = temp[0].Trim(); // Family name
                resarray[2] = temp[1].Trim(); // Field name
                resarray[3] = words1[1].Trim(); // Field value
            }
            else
            {
                resarray[1] = familyname;    //Family Name
                resarray[2] = words1[0].Trim();  // Field Name
                resarray[3] = words1[1].Trim();  // Field Value

            }
            return resarray;

        }
        public Filter getFilter(string op, string family, string fieldName, string fieldValue)
        {
            CompareFilter.CompareOp compareOp = Enum.GetValues<CompareFilter.CompareOp>().FirstOrDefault(e => string.Equals(e.ToString(), op));
            if (string.Equals(compareOp.ToString(), op))
            {
                return new SingleColumnValueFilter(Encoding.UTF8.GetBytes(family), Encoding.UTF8.GetBytes(fieldName), compareOp, Encoding.UTF8.GetBytes(fieldValue));
            }
            else if (string.Equals("RegexComp", op))
            {
                RegexStringComparator comp = new RegexStringComparator(fieldValue);   // any value that starts with 'my'
                return new SingleColumnValueFilter(
                  Encoding.UTF8.GetBytes(family),
                  Encoding.UTF8.GetBytes(fieldName),
                  CompareFilter.CompareOp.Equal,
                  comp);
            }          
            else
            {
                //no CompareOp enum value found matching the given op
                return null;
            }
            
        }
        public Scanner getScanner(string wherepart, string familyname)
        {
            //This code has been written with the assumption that the user is using either the AND filter or the OR filter or the IN filter at a time. Not using them simultaneously.
            Scanner scanner = new Scanner();                      
            
            string[] Querydata = new string[4];
            Filter filter = null;

            if (!(wherepart.Contains(" AND ") || wherepart.Contains(" OR ") || wherepart.Contains(" IN ")))  // Adding the functionality of IN operator, need to check the functionality where the field name contains IN 
            {
                Querydata = getWhereParts(wherepart, familyname);// operator, familyname,fieldname,fieldvalue

                filter = getFilter(Querydata[0], Querydata[1], Querydata[2], Querydata[3]);               
                scanner.filter = filter.ToEncodedString();

            }
            else if (wherepart.Contains(" AND "))
            {
                string[] whereSubParts = wherepart.Split(" AND ", StringSplitOptions.None);
                Querydata = getWhereParts(whereSubParts[0], familyname);
                Filter firstfilter = getFilter(Querydata[0], Querydata[1], Querydata[2], Querydata[3]);
                //Filter firstfilter = new SkipFilter(new ValueFilter(CompareFilter.CompareOp.Equal, new BinaryComparator(new byte[] { 0 })));

                for (int i = 0; i < whereSubParts.Length; i++)
                {

                    Querydata = getWhereParts(whereSubParts[i], familyname);// operator, familyname,fieldname,fieldvalue
                    Filter nextfilter = getFilter(Querydata[0], Querydata[1], Querydata[2], Querydata[3]);
                    filter = new FilterList(FilterList.Operator.MustPassAll, firstfilter, nextfilter);
                    firstfilter = nextfilter;
                }
                scanner.filter = filter.ToEncodedString();
            }
            else if (wherepart.Contains(" IN "))
            {
                string[] whereSubParts = wherepart.Split(" IN ", StringSplitOptions.None); //whereSubParts[0] contains the fieldname to compare.Ex:  where id IN(111,222,333);
                string fieldName;

                if (whereSubParts[0].Contains(':'))
                {
                    familyname = whereSubParts[0].Split(':')[0].Trim();
                    fieldName = whereSubParts[0].Split(':')[1].Trim();
                }
                else
                {
                    fieldName = whereSubParts[0].Trim();
                }
                Filter firstfilter;
                if (whereSubParts[1].Contains(','))
                {
                    var values = whereSubParts[1].Substring(1, whereSubParts[1].Length - 2).Split(',');
                    firstfilter = getFilter("Equal", familyname, fieldName, values[0]);                    
                    for (int i = 1; i < values.Length; i++)
                    {                       
                        var nextfilter = getFilter("Equal", familyname, fieldName, values[i]);
                        filter = new FilterList(FilterList.Operator.MustPassOne, firstfilter, nextfilter);
                        firstfilter = nextfilter;
                    }
                    scanner.filter = filter.ToEncodedString();
                }
                else
                {
                    var value = whereSubParts[1].Substring(1, whereSubParts[1].Length - 2);
                    Filter testfilter = getFilter("Equal", familyname, fieldName, value);
                    scanner.filter = testfilter.ToEncodedString();
                }

            }
            else
            {
                string[] whereSubParts = wherepart.Split(" OR ", StringSplitOptions.None);
                string fieldName;
                Filter firstfilter;
                Querydata = getWhereParts(whereSubParts[0], familyname);

                firstfilter = getFilter(Querydata[0], Querydata[1], Querydata[2], Querydata[3]);
                for (int i = 1; i < whereSubParts.Length; i++)
                {

                    Querydata = getWhereParts(whereSubParts[i], familyname);// operator, familyname,fieldname,fieldvalue

                    Filter nextfilter = getFilter(Querydata[0], Querydata[1], Querydata[2], Querydata[3]);
                    filter = new FilterList(FilterList.Operator.MustPassOne, firstfilter, nextfilter);
                    firstfilter = nextfilter;
                }
                scanner.filter = filter.ToEncodedString();
            }
            return scanner;
        }
        public override async void PerformDBAction()
        {
                                          
                ValueExpression VE = new ValueExpression(Db.ProjEnvironment, Db.BusinessFlow, Db.DSList);
                VE.Value = Act.SQL;
                string SQLCalculated = VE.ValueCalculated;
                var ConnectionUri = new Uri(Db.DatabaseOperations.TNSCalculated);
                ClusterCredentials ClCredential = new(ConnectionUri, Db.DatabaseOperations.UserCalculated, Db.DatabaseOperations.PassCalculated);                

                requestOption = new RequestOptions
                {
                    RetryPolicy = Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.RetryPolicy.NoRetry,
                    KeepAlive = true,
                    TimeoutMillis = 30000,
                    ReceiveBufferSize = 1048576,
                    SerializationBufferSize = 1048576,
                    UseNagle = false,
                    AlternativeEndpoint = "",
                    Port = ConnectionUri.Port,
                    AlternativeHost = null
                };
                try
                {
                    HBaseClient client = new HBaseClient(ClCredential, requestOption);
                    Scanner scanner;                              
                    ScannerInformation scanInfo = null;
                    string familyName;
                    string table;
                    string wherepart;
                    string[] Querydata = new string[4];
                    Filter filter = null ;
                    switch (Action)
                    {
                        case eDBValidationType.RecordCount:

                            int nuRows = 0;
                            scanner = new Scanner();
                            FirstKeyOnlyFilter keyOnlyFilter = new FirstKeyOnlyFilter();
                            scanner.filter = keyOnlyFilter.ToEncodedString();                                    
                            scanInfo = client.CreateScannerAsync(Act.Details.Info, scanner, requestOption).Result;
                            nuRows = client.ScannerGetNextAsync(scanInfo, requestOption).Result.rows.Count;                      
                            Act.AddOrUpdateReturnParamActual("Record Count", nuRows.ToString());
                            break;

                        case eDBValidationType.SimpleSQLOneValue:

                            table = Act.Table;
                            if (string.IsNullOrEmpty(table))
                            {
                                throw new Exception("The table field can not be left empty : Please provide a value");
                            }
                            string colpart = Act.Column;
                            if (string.IsNullOrEmpty(colpart))
                            {
                                throw new Exception("The column field can not be left empty: Please provide a value");

                            }
                            wherepart = Act.Where;
                            if (string.IsNullOrEmpty(wherepart))
                            {
                                throw new Exception("The Where field can not be left empty: Please provide a valid condition in this field");

                            }
                            
                            familyName = client.GetTableSchemaAsync(table, null).Result.columns.ToList()[0].name;
                            
                            scanner = getScanner(wherepart, familyName);                           
                            scanInfo = client.CreateScannerAsync(table, scanner, requestOption).Result;
                            int path = 1;

                            CellSet currentColumnSet;
                            bool columnFound = false;
                            while ((currentColumnSet = client.ScannerGetNextAsync(scanInfo, requestOption).Result) != null)
                            {
                                foreach (CellSet.Row row in currentColumnSet.rows)
                                {
                                    string rowKey = _encoding.GetString(row.key);

                                    List<Cell> cells = row.values;

                                    foreach (Cell cell in cells)
                                    {
                                        string columnName = ExtractColumnName(cell.column);
                                        if (!string.IsNullOrEmpty(Querydata[1]))
                                        {
                                            columnName = Querydata[1] + ":" + columnName;
                                        }
                                        string columnValue = Encoding.ASCII.GetString(cell.data);

                                        if (string.Equals(columnName, Act.Column))
                                        {
                                            Act.AddOrUpdateReturnParamActualWithPath(columnName, Encoding.ASCII.GetString(cell.data), path.ToString());
                                            columnFound = true;
                                            break;
                                        }
                                    }
                                    if (columnFound == true)
                                    {
                                        break;
                                    }
                                }
                                if (columnFound == true)
                                {
                                    break;
                                }
                            }
                        
                            break;

                        case eDBValidationType.FreeSQL:

                            if (string.IsNullOrEmpty(SQLCalculated))
                            {
                                throw new Exception("Provide a valid query. It can not be left blank");
                            }
                           
                            if (SQLCalculated.Contains("where"))
                            {
                                table = SQLCalculated.Substring(SQLCalculated.IndexOf("from") + 4, (SQLCalculated.IndexOf("where") - SQLCalculated.IndexOf("from") - 4)).Trim();
                                wherepart = SQLCalculated.Substring(SQLCalculated.IndexOf("where") + 5);
                                familyName = client.GetTableSchemaAsync(table, null).Result.columns.ToList()[0].name;
                                scanner = getScanner(wherepart, familyName);
           
                            }
                            else
                            {
                               
                                scanner = new Scanner();
                                table = SQLCalculated.Substring(SQLCalculated.IndexOf("from") + 4).Trim();
                            }
                       
                            scanInfo = client.CreateScannerAsync(table, scanner, requestOption).Result;
                            int path1 = 1;

                            if (SQLCalculated.Contains('*'))
                            {

                                CellSet next;

                                while ((next = client.ScannerGetNextAsync(scanInfo, requestOption).Result) != null)
                                {
                                    foreach (CellSet.Row row in next.rows)
                                    {
                                        string rowKey = _encoding.GetString(row.key);

                                        List<Cell> cells = row.values;

                                        foreach (Cell c in cells)
                                        {

                                            Act.AddOrUpdateReturnParamActualWithPath(ExtractColumnName(c.column), Encoding.ASCII.GetString(c.data), path1.ToString());

                                        }
                                        path1++;
                                    }

                                }

                            }
                            else
                            {

                                int i = SQLCalculated.IndexOf("select");
                                int j = SQLCalculated.IndexOf("from");

                                string[] selectedcols = SQLCalculated.Trim().Substring((SQLCalculated.IndexOf("select") + 6), SQLCalculated.IndexOf("from") - SQLCalculated.IndexOf("select") - 6).Split(",");
                                CellSet next;
                                List<string> list = new();
                                foreach (string col in selectedcols)
                                    list.Add(col.Trim());

                                while ((next = client.ScannerGetNextAsync(scanInfo, requestOption).Result) != null)
                                {
                                    foreach (CellSet.Row row in next.rows)
                                    {
                                        string rowKey = _encoding.GetString(row.key);

                                        List<Cell> cells = row.values;
                                        //if cells contain all of the fileds in the where clause, then only the below codes will be executed or else it will be skipped
                                        foreach (Cell c in cells)
                                        {
                                            string colname = ExtractColumnName(c.column);
                                            if (list.Contains(colname))
                                            {
                                                Act.AddOrUpdateReturnParamActualWithPath(colname, Encoding.ASCII.GetString(c.data), path1.ToString());

                                            }

                                        }
                                        path1++;
                                    }
                                }

                            }                      

                            break;
                    }     
                }  
                catch (Exception ex)
                {
                    Act.Error = ex.Message;
                    Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
                }
                finally
                {
                    client.DisposeIfNotNull();
                }                                       

        }


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
        public override List<string> GetKeyspaceList()
        {
            throw new NotImplementedException();
        }
        public List<string> HBTableList;

        public override List<string> GetTableList(string Keyspace)
        {
            //throw new NotImplementedException();

            ClusterCredentials ClCredential = new(new System.Uri(this.connectionUrl), this.userName, this.password);
            HBaseClient client1 = new HBaseClient(ClCredential);
            HBTableList = new List<string>() ;
            TableList tables = null!;
            Task getTablesTask = Task.Run(() =>
            {
                try
                {
                    tables = client1.ListTablesAsync().Result;
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.WARN, "Unable to connect to Hbase and get table list ", ex);
                }

            });

            getTablesTask.Wait();            
            int i = tables.name.Count();
            for (int j = 0; j < i; j++)
            {
                HBTableList.Add(tables.name[j]);
            }
            return HBTableList;

        }
        
        public List<string> ColumnList;       

        public override async Task<List<string>> GetColumnList(string Tablename)
        {
            ColumnList = new List<string>();
            ClusterCredentials ClCredential = new(new System.Uri(Db.DatabaseOperations.TNSCalculated), Db.DatabaseOperations.UserCalculated, Db.DatabaseOperations.PassCalculated);
            HBaseClient client1 = new HBaseClient(ClCredential);
            var result11 = client1.GetTableSchemaAsync(Tablename, null).Result.columns.ToList();
            // if more than one family name present then the if block will be executed 
            if (result11.Count > 1)
            {
                for (int i = 0; i < result11.Count; i++)
                {
                    var tmp = result11[i].name;//This returns the family names present in the table
                    Scanner scanner = new Scanner();
                    var filter = new FamilyFilter(CompareFilter.CompareOp.Equal, new BinaryComparator(Encoding.UTF8.GetBytes(tmp)));
                    scanner.filter = filter.ToEncodedString();
                    RequestOptions scanOptions = RequestOptions.GetDefaultOptions();
                    scanOptions.AlternativeEndpoint = "";//Constants.RestEndpointBaseZero;
                    ScannerInformation scanInfo = null;
                    try
                    {
                        scanInfo = client1.CreateScannerAsync(Tablename, scanner, scanOptions).Result;                       
                        CellSet next;
                        while ((next = client1.ScannerGetNextAsync(scanInfo, scanOptions).Result) != null)
                        {
                            foreach (CellSet.Row row in next.rows)
                            {
                                List<Cell> cells = row.values;
                                foreach (Cell c in cells)
                                {
                                    string columnName = ExtractColumnName(c.column);
                                    if (!ColumnList.Contains(columnName))
                                    {
                                        ColumnList.Add(tmp+":"+columnName);
                                    }
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.WARN, "Unable to connect to Hbase", ex);
                        return null;
                    }
                }
            }
            else
            {
                Scanner scanner = new Scanner();
                var filter = new FamilyFilter(CompareFilter.CompareOp.Equal, new BinaryComparator(Encoding.UTF8.GetBytes(result11[0].name)));
                scanner.filter = filter.ToEncodedString();
                RequestOptions scanOptions = RequestOptions.GetDefaultOptions();
                scanOptions.AlternativeEndpoint = "";//Constants.RestEndpointBaseZero;
                ScannerInformation scanInfo ;
                try
                {
                    scanInfo = client1.CreateScannerAsync(Tablename, scanner, scanOptions).Result;                    
                    CellSet next;
                    while ((next = client1.ScannerGetNextAsync(scanInfo, scanOptions).Result) != null)
                    {
                        foreach (CellSet.Row row in next.rows)
                        {
                            List<Cell> cells = row.values;
                            foreach (Cell c in cells)
                            {
                                string columnName = ExtractColumnName(c.column);
                                if (!ColumnList.Contains(columnName))
                                {
                                    ColumnList.Add(columnName);
                                }

                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.WARN, "Unable to connect to Hbase", ex);
                    return null;
                }
            }
            return ColumnList;
        }                    

        private string ExtractColumnName(Byte[] cellColumn)
        {
            string qualifiedColumnName = _encoding.GetString(cellColumn);
            string[] parts = qualifiedColumnName.Split(new[] { ':' }, 2);
            return parts[1];
        }   
        
       
    }
}
