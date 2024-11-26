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
using Applitools.Utils;
using GingerCore.Actions;
using Microsoft.HBase.Client;
using MongoDB.Driver;
using OctaneStdSDK.Entities.Base;
using org.apache.hadoop.hbase.rest.protobuf.generated;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using SingleColumnValueFilter = Microsoft.HBase.Client.Filters.SingleColumnValueFilter;

namespace GingerCore.NoSqlBase
{

    public class GingerHbase : NoSqlBase
    {
        ActDBValidation Act = null;
        string connectionUrl;
        string userName;
        string password;

        private static readonly Encoding _encoding = Encoding.UTF8;


        public GingerHbase(string url, string Uname, string passwd)
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
        /// <summary>
        /// Establishes a connection to the HBase database using the provided credentials and connection URL.
        /// </summary>
        /// <returns>True if the connection is successful and tables are retrieved; otherwise, false.</returns>
        public override bool Connect()
        {
            this.connectionUrl = Db.DatabaseOperations.TNSCalculated;
            this.userName = Db.DatabaseOperations.UserCalculated;
            this.password = Db.DatabaseOperations.PassCalculated;
            var ConnectionUri = new Uri(this.connectionUrl);

            ClusterCredentials ClCredential = new(ConnectionUri, this.userName, this.password);

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

                if (tables.name.Count == 0)
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
                Reporter.ToLog(eLogLevel.ERROR, "Unable to connect to Hbase", ex);
                return false;
            }
        }

        /// <summary>
        /// Parses a reference string to extract the comparison operation, family name, field name, and field value.
        /// </summary>
        /// <param name="refstring">The reference string containing the comparison operation and field details.</param>
        /// <param name="familyname">The default family name to use if not specified in the reference string.</param>
        /// <returns>An array of strings containing the comparison operation, family name, field name, and field value.</returns>
        public string[] getWhereParts(string refstring, string familyname)
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
                resarray[1] = temp[0].Trim();
                resarray[2] = temp[1].Trim();
                resarray[3] = words1[1].Trim();
            }
            else
            {
                resarray[1] = familyname;
                resarray[2] = words1[0].Trim();
                resarray[3] = words1[1].Trim();

            }
            return resarray;

        }
        /// <summary>
        /// Creates a filter for HBase queries based on the provided operation, family, field name, and field value.
        /// Supports comparison operations and regex comparisons.
        /// </summary>
        /// <param name="op">The comparison operation or "RegexComp" for regex comparison.</param>
        /// <param name="family">The column family.</param>
        /// <param name="fieldName">The field name within the column family.</param>
        /// <param name="fieldValue">The value to compare against.</param>
        /// <returns>A Filter object for the specified criteria, or null if the operation is not supported.</returns>
        public Filter getFilter(string op, string family, string fieldName, string fieldValue)
        {
            CompareFilter.CompareOp compareOp = Enum.GetValues<CompareFilter.CompareOp>().FirstOrDefault(e => string.Equals(e.ToString(), op));
            if (string.Equals(compareOp.ToString(), op))
            {
                if (fieldValue.StartsWith('\'') && fieldValue.EndsWith('\''))
                {
                    fieldValue = fieldValue.Trim('\'');
                    return new SingleColumnValueFilter(Encoding.UTF8.GetBytes(family), Encoding.UTF8.GetBytes(fieldName), compareOp, Encoding.UTF8.GetBytes(fieldValue), filterIfMissing: true);
                }
                else if (fieldValue.Contains('.'))
                {
                    return new SingleColumnValueFilter(Encoding.UTF8.GetBytes(family), Encoding.UTF8.GetBytes(fieldName), compareOp, System.BitConverter.GetBytes(double.Parse(fieldValue)), filterIfMissing: true);
                }
                else
                {
                    var longBytes = System.BitConverter.GetBytes(long.Parse(fieldValue));
                    Array.Reverse(longBytes);
                    return new SingleColumnValueFilter(Encoding.UTF8.GetBytes(family), Encoding.UTF8.GetBytes(fieldName), compareOp, longBytes, filterIfMissing: true);
                }
            }
            else if (string.Equals("RegexComp", op))
            {
                RegexStringComparator comp = new RegexStringComparator(fieldValue);
                return new SingleColumnValueFilter(
                  Encoding.UTF8.GetBytes(family),
                  Encoding.UTF8.GetBytes(fieldName),
                  CompareFilter.CompareOp.Equal,
                  comp);
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Creates a Scanner object based on the provided where clause and family name.
        /// The method handles different logical operators (AND, OR, IN) to construct the appropriate filter.
        /// </summary>
        public Scanner getScanner(string wherepart, string familyname)
        {
            Scanner scanner = new Scanner();

            string[] Querydata;
            string[] whereSubParts;
            Filter filter = null;

            if (!(wherepart.Contains(" AND ") || wherepart.Contains(" OR ") || wherepart.Contains(" IN ")))
            {
                Querydata = getWhereParts(wherepart, familyname);

                filter = getFilter(Querydata[0], Querydata[1], Querydata[2], Querydata[3]);
                scanner.filter = filter?.ToEncodedString();
            }
            else if (wherepart.Contains(" AND "))
            {
                whereSubParts = wherepart.Split(" AND ");
                Querydata = getWhereParts(whereSubParts[0], familyname);
                Filter firstfilter = getFilter(Querydata[0], Querydata[1], Querydata[2], Querydata[3]);
                for (int i = 0; i < whereSubParts.Length; i++)
                {
                    Querydata = getWhereParts(whereSubParts[i], familyname);
                    Filter nextfilter = getFilter(Querydata[0], Querydata[1], Querydata[2], Querydata[3]);
                    filter = new FilterList(FilterList.Operator.MustPassAll, firstfilter, nextfilter);
                    firstfilter = nextfilter;
                }
                scanner.filter = filter?.ToEncodedString();
            }
            else if (wherepart.Contains(" IN "))
            {
                whereSubParts = wherepart.Split(" IN ");
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
                    var values = whereSubParts[1][1..^1].Split(',');
                    firstfilter = getFilter("Equal", familyname, fieldName, values[0]);
                    for (int i = 1; i < values.Length; i++)
                    {
                        var nextfilter = getFilter("Equal", familyname, fieldName, values[i]);
                        filter = new FilterList(FilterList.Operator.MustPassOne, firstfilter, nextfilter);
                        firstfilter = nextfilter;
                    }
                    scanner.filter = filter?.ToEncodedString();
                }
                else
                {
                    var value = whereSubParts[1][1..^1];
                    Filter testfilter = getFilter("Equal", familyname, fieldName, value);
                    scanner.filter = testfilter.ToEncodedString();
                }
            }
            else
            {
                whereSubParts = wherepart.Split(" OR ");
                string fieldName;
                Filter firstfilter;
                Querydata = getWhereParts(whereSubParts[0], familyname);

                firstfilter = getFilter(Querydata[0], Querydata[1], Querydata[2], Querydata[3]);
                for (int i = 1; i < whereSubParts.Length; i++)
                {
                    Querydata = getWhereParts(whereSubParts[i], familyname);

                    Filter nextfilter = getFilter(Querydata[0], Querydata[1], Querydata[2], Querydata[3]);
                    filter = new FilterList(FilterList.Operator.MustPassOne, firstfilter, nextfilter);
                    firstfilter = nextfilter;
                }
                scanner.filter = filter?.ToEncodedString();
            }
            return scanner;
        }



        /// <summary>
        /// Performs a database action asynchronously.
        /// </summary>
        public override async void PerformDBAction()
        {

            ValueExpression VE = new ValueExpression(Db.ProjEnvironment, Db.BusinessFlow, Db.DSList)
            {
                Value = Act.QueryValue
            };
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
                AlternativeHost = null,
            };
            try
            {
                HBaseClient actionClient = new HBaseClient(ClCredential, requestOption);
                Scanner scanner;
                ScannerInformation scanInfo = null;
                string familyName;
                string table;
                string wherepart;
                string[] Querydata = new string[4];
                Filter filter = null;
                switch (Action)
                {
                    case eDBValidationType.RecordCount:

                        int nuRows = 0;
                        scanner = new Scanner();
                        FirstKeyOnlyFilter keyOnlyFilter = new FirstKeyOnlyFilter();
                        scanner.filter = keyOnlyFilter.ToEncodedString();
                        scanInfo = actionClient.CreateScannerAsync(Act.Details.Info, scanner, requestOption).Result;
                        nuRows = actionClient.ScannerGetNextAsync(scanInfo, requestOption).Result.rows.Count;
                        Act.AddOrUpdateReturnParamActual("Record Count", nuRows.ToString());
                        break;

                    case eDBValidationType.SimpleSQLOneValue:

                        table = Act.Table;
                        if (string.IsNullOrEmpty(table))
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "The Table value can not be empty");
                            break;
                        }
                        string colpart = Act.Column;
                        if (string.IsNullOrEmpty(colpart))
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "The ColumnPart can not be empty");
                            break;

                        }
                        wherepart = Act.Where;
                        if (string.IsNullOrEmpty(wherepart))
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "The WherePart can not be empty");
                            break;
                        }

                        familyName = actionClient.GetTableSchemaAsync(table, null).Result.columns.ToList()[0].name;

                        scanner = getScanner(wherepart, familyName);
                        scanInfo = actionClient.CreateScannerAsync(table, scanner, requestOption).Result;
                        int path = 1;

                        CellSet currentColumnSet;
                        bool columnFound = false;
                        while ((currentColumnSet = actionClient.ScannerGetNextAsync(scanInfo, requestOption).Result) != null)
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
                                    string columnValue = ExtractColumnValue(cell.data);

                                    if (string.Equals(columnName, Act.Column))
                                    {
                                        Act.AddOrUpdateReturnParamActualWithPath(columnName, ExtractColumnValue(cell.data), path.ToString());
                                        columnFound = true;
                                        break;
                                    }
                                }
                                if (columnFound)
                                {
                                    break;
                                }
                            }
                            if (columnFound)
                            {
                                break;
                            }
                        }

                        break;

                    case eDBValidationType.FreeSQL:

                        if (string.IsNullOrEmpty(SQLCalculated))
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "The Query value can not be empty");
                            break;
                        }
                        CellSet next;
                        table = ExtractTableName(SQLCalculated);
                        if (SQLCalculated.IndexOf("where", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            wherepart = ExtractWherePart(SQLCalculated);
                            var familyNameList = actionClient.GetTableSchemaAsync(table, null).Result.columns.ToList();
                            scanner = null;
                            //choose family name where column exsist
                            if (!familyNameList.Any())
                            {
                                throw new InvalidOperationException($"No column families found for table {table}");
                            }
                            foreach (var i in familyNameList)
                            {
                                familyName = i.name;
                                scanner = getScanner(wherepart, familyName);
                                scanInfo = actionClient.CreateScannerAsync(table, scanner, requestOption).Result;

                                if ((actionClient.ScannerGetNextAsync(scanInfo, requestOption).Result) != null)
                                {
                                    break;
                                }

                            }

                        }
                        else
                        {
                            scanner = new Scanner();
                        }
                        string orderByColumnName = ExtractOrderByColumnName(SQLCalculated);

                        int path1 = 1;
                        bool isDataFound = false;
                        List<RowData> rowDataList = [];
                        scanInfo = actionClient.CreateScannerAsync(table, scanner, requestOption).Result;
                        //According to scanner info read data form hbase
                        while ((next = actionClient.ScannerGetNextAsync(scanInfo, requestOption).Result) != null)
                        {
                            isDataFound = true;
                            foreach (CellSet.Row row in next.rows)
                            {
                                string rowKey = _encoding.GetString(row.key);
                                List<Cell> cells = row.values;

                                RowData rowData = new RowData
                                {
                                    RowKey = rowKey,
                                    Columns = []
                                };

                                foreach (Cell c in cells)
                                {
                                    //Add data in list

                                    string columnName = ExtractColumnName(c.column);
                                    string columnValue = ExtractColumnValue(c.data);
                                    rowData.Columns[columnName] = columnValue;

                                }


                                rowDataList.Add(rowData);
                            }
                        }
                        //if want to sort data by particular column
                        if (!string.IsNullOrEmpty(orderByColumnName))
                        {
                            bool desc = string.Equals(ExtractOrderByDirection(SQLCalculated), "Desc", StringComparison.OrdinalIgnoreCase);
                            rowDataList.Sort(new RowDataComparer(orderByColumnName, desc));
                        }
                        //To show the all column value
                        if (SQLCalculated.Contains('*'))
                        {
                            foreach (var rowData in rowDataList)
                            {
                                foreach (var column in rowData.Columns)
                                {
                                    Act.AddOrUpdateReturnParamActualWithPath(column.Key, column.Value, path1.ToString());
                                }
                                path1++;
                            }
                        }
                        //To show the selected column value
                        else
                        {
                            int selectIndex = SQLCalculated.IndexOf("select", StringComparison.OrdinalIgnoreCase);
                            int fromIndex = SQLCalculated.IndexOf("from", StringComparison.OrdinalIgnoreCase);
                            string[] selectedcols = SQLCalculated.Trim().Substring(selectIndex + 6, fromIndex - selectIndex - 6).Split(",");
                            List<string> columnNameList = [];
                            foreach (string col in selectedcols)
                            {
                                columnNameList.Add(col.Trim());
                            }
                            foreach (var rowData in rowDataList)
                            {
                                foreach (var column in rowData.Columns)
                                {
                                    if (columnNameList.Contains(column.Key))
                                    {
                                        Act.AddOrUpdateReturnParamActualWithPath(column.Key, column.Value, path1.ToString());
                                    }
                                }
                                path1++;
                            }
                        }
                        //show error if data not found
                        if (!isDataFound)
                        {
                            throw new InvalidDataException("Data not found");
                        }
                        break;
                    default:
                        {
                            break;
                        }
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

        /// <summary>
        /// Extracts the where part from the specified SQL query.
        /// </summary>
        /// <param name="SQLCalculated">The SQL query.</param>
        /// <returns>The where part as a string.</returns>
        string ExtractWherePart(string SQLCalculated)
        {
            int whereIndex = SQLCalculated.IndexOf("where", StringComparison.OrdinalIgnoreCase);
            var wherepart = SQLCalculated[(whereIndex + 5)..];
            string searchString = "order by";
            int orderByIndex = wherepart.IndexOf(searchString, StringComparison.OrdinalIgnoreCase);
            if (orderByIndex >= 0)
            {
                // Remove the 'ORDER BY' clause and everything after it
                wherepart = wherepart[..orderByIndex].Trim();
            }
            return wherepart;
        }


        /// <summary>
        /// Extracts the column name used in the ORDER BY clause of the query.
        /// </summary>
        /// <param name="query">The SQL query.</param>
        /// <returns>The column name as a string.</returns>
        public string ExtractOrderByColumnName(string query)
        {
            string orderByClause = "order by";
            string orderByColumnName = "";

            int orderByIndex = query.IndexOf(orderByClause, StringComparison.OrdinalIgnoreCase);
            if (orderByIndex >= 0)
            {
                int startIndex = orderByIndex + orderByClause.Length + 1;
                int endIndex = query.IndexOf(" ", startIndex);
                if (endIndex == -1)
                {
                    endIndex = query.Length;
                }
                orderByColumnName = query[startIndex..endIndex].Trim();

                // Remove any trailing "desc" or "asc" if present
                orderByColumnName = orderByColumnName.Split(' ')[0];
            }

            return orderByColumnName;
        }


        /// <summary>
        /// Extracts the direction (ASC/DESC) used in the ORDER BY clause of the query.
        /// </summary>
        /// <param name="query">The SQL query.</param>
        /// <returns>The direction as a string.</returns>
        public string ExtractOrderByDirection(string query)
        {
            string orderByClause = "order by";
            string orderByDirection = "";

            int orderByIndex = query.IndexOf(orderByClause, StringComparison.OrdinalIgnoreCase);
            if (orderByIndex >= 0)
            {
                int startIndex = orderByIndex + orderByClause.Length;
                string orderByPart = query[startIndex..].Trim();

                if (orderByPart.EndsWith("desc", StringComparison.OrdinalIgnoreCase))
                {
                    orderByDirection = "DESC";
                }
                else if (orderByPart.EndsWith("asc", StringComparison.OrdinalIgnoreCase))
                {
                    orderByDirection = "ASC";
                }
                else
                {
                    // Default to ASC if no direction is specified
                    orderByDirection = "ASC";
                }
            }

            return orderByDirection;
        }


        /// <summary>
        /// Extracts the table name from the specified SQL query.
        /// </summary>
        /// <param name="query">The SQL query.</param>
        /// <returns>The table name as a string.</returns>
        static string ExtractTableName(string query)
        {
            string pattern = @"FROM\s+([a-zA-Z0-9_]+)";
            Match match = Regex.Match(query, pattern, RegexOptions.IgnoreCase);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return string.Empty;
            }
        }

        public enum DataType
        {
            Int,
            Long,
            Float,
            Double,
            String,
            Unknown
        }

        /// <summary>
        /// Extracts the column value from a byte array by inferring its data type and converting it accordingly.
        /// </summary>
        /// <param name="byteArray">The byte array containing the column value.</param>
        /// <returns>The extracted column value as a string.</returns>
        public static string ExtractColumnValue(byte[] byteArray)
        {
            if (byteArray == null || byteArray.Length == 0)
            {
                return "";
            }
            if (byteArray.Length <= 8 && byteArray.All(y => y == 0))
            {
                return "0";
            }
            DataType inferredType = DataType.String;
            if (byteArray != null && byteArray.Length > 0 && (byteArray[0] == 0))
            {
                inferredType = InferDataType(byteArray);
            }
            if (inferredType != DataType.String)
            {
                Array.Reverse(byteArray);
            }
            object value = ConvertByteArrayToType(byteArray, inferredType);

            if (inferredType == DataType.Double && !(Double.IsNaN(double.Parse(value.ToString())) || Double.IsInfinity(double.Parse(value.ToString()))))
            {
                //Not a double
                inferredType = DataType.Long;
                value = ConvertByteArrayToType(byteArray, inferredType);
            }

            if (inferredType == DataType.Long && (Double.IsNaN(double.Parse(value.ToString())) || Double.IsInfinity(double.Parse(value.ToString()))))
            {
                //Not a Long
                inferredType = DataType.Double;
                value = ConvertByteArrayToType(byteArray, inferredType);
            }

            if (inferredType == DataType.Long && value != null && value.ToString()[0] == '-' && value.ToString().Length >= 20)
            {
                inferredType = DataType.Double;
                value = ConvertByteArrayToType(byteArray, inferredType);
            }

            if (value != null && value.ToString().Contains('�'))
            {
                inferredType = InferDataType(byteArray);
                if (inferredType != DataType.String)
                {
                    Array.Reverse(byteArray);
                }
                value = ConvertByteArrayToType(byteArray, inferredType);
            }
            //Console.WriteLine($"Inferred Type: {inferredType}, Value: {value}");
            return value?.ToString();
        }

        /// <summary>
        /// Infers the data type from the byte array.
        /// </summary>
        /// <param name="byteArray">The byte array.</param>
        /// <returns>The inferred data type.</returns>
        public static DataType InferDataType(byte[] byteArray)
        {
            switch (byteArray.Length)
            {
                case 4:
                    // Potentially int or float  
                    if (System.BitConverter.ToInt32(byteArray, 0) != 0) // Example heuristic, adjust as necessary  
                        return DataType.Int; // could be an int  
                    if (System.BitConverter.ToSingle(byteArray, 0) != 0)
                        return DataType.Float; // could be a float  

                    return DataType.Unknown;

                case 8:
                    if (System.BitConverter.ToDouble(byteArray, 0) != 0)
                        return DataType.Double;
                    if (System.BitConverter.ToInt64(byteArray, 0) != 0) // Example heuristic, adjust as necessary  
                        return DataType.Long; // could be a long  
                                              // could be a double  

                    return DataType.Unknown;

                default:
                    // Consider any string values or unknowns  
                    return DataType.String; // treating as string for variable lengths  
            }
        }


        /// <summary>
        /// Converts the byte array to the specified data type.
        /// </summary>
        /// <param name="byteArray">The byte array.</param>
        /// <param name="dataType">The data type.</param>
        /// <returns>The converted object.</returns>
        public static object ConvertByteArrayToType(byte[] byteArray, DataType dataType)
        {
            return dataType switch
            {
                DataType.Int => System.BitConverter.ToInt32(byteArray, 0),
                DataType.Long => System.BitConverter.ToInt64(byteArray, 0),
                DataType.Float => System.BitConverter.ToSingle(byteArray, 0),
                DataType.Double => System.BitConverter.ToDouble(byteArray, 0),
                DataType.String => System.Text.Encoding.UTF8.GetString(byteArray),
                _ => null,// or throw an exception based on your needs  
            };
        }

        /// <summary>
        /// Encodes a given text string to its Base64 representation.
        /// </summary>
        public string Base64Encode(string text)
        {
            var textBytes = System.Text.Encoding.UTF8.GetBytes(text);
            return System.Convert.ToBase64String(textBytes);
        }
        /// <summary>
        /// Decodes a given Base64 string to its original text representation.
        /// </summary>
        public string Base64Decode(string base64)
        {
            var base64Bytes = System.Convert.FromBase64String(base64);
            return System.Text.Encoding.UTF8.GetString(base64Bytes);
        }
        /// <summary>
        /// Throws a NotImplementedException indicating that the method is not yet implemented.
        /// </summary>
        public override List<string> GetKeyspaceList()
        {
            throw new NotImplementedException();
        }
        public List<string> HBTableList;

        /// <summary>
        /// Retrieves the list of tables from the specified keyspace in HBase.
        /// </summary>
        /// <param name="Keyspace">The keyspace from which to retrieve the table list.</param>
        /// <returns>A list of table names.</returns>
        public override List<string> GetTableList(string Keyspace)
        {
            ClusterCredentials ClCredential = new(new System.Uri(this.connectionUrl), this.userName, this.password);
            HBaseClient client1 = new HBaseClient(ClCredential);
            HBTableList = [];
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
            int i = tables.name.Count;
            for (int j = 0; j < i; j++)
            {
                HBTableList.Add(tables.name[j]);
            }
            return HBTableList;
        }

        public List<string> ColumnList;

        public override async Task<List<string>> GetColumnList(string Tablename)
        {
            ColumnList = [];
            ClusterCredentials ClCredential = new(new System.Uri(Db.DatabaseOperations.TNSCalculated), Db.DatabaseOperations.UserCalculated, Db.DatabaseOperations.PassCalculated);
            HBaseClient client1 = new HBaseClient(ClCredential);
            var result11 = client1.GetTableSchemaAsync(Tablename, null).Result.columns.ToList();

            if (result11.Count > 1)
            {
                for (int i = 0; i < result11.Count; i++)
                {
                    var tmp = result11[i].name;
                    Scanner scanner = new Scanner();
                    var filter = new FamilyFilter(CompareFilter.CompareOp.Equal, new BinaryComparator(Encoding.UTF8.GetBytes(tmp)));
                    scanner.filter = filter?.ToEncodedString();
                    RequestOptions scanOptions = RequestOptions.GetDefaultOptions();
                    scanOptions.AlternativeEndpoint = "";
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
                                        ColumnList.Add(tmp + ":" + columnName);
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
                scanOptions.AlternativeEndpoint = "";
                ScannerInformation scanInfo;
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
    public class RowData
    {
        public string RowKey { get; set; }
        public Dictionary<string, string> Columns { get; set; }
    }
    public class RowDataComparer : IComparer<RowData>
    {
        private readonly string _colName;
        private readonly bool _desc;

        public RowDataComparer(string colName, bool desc)
        {
            _colName = colName;
            _desc = desc;
        }

        /// <summary>
        /// Compares two RowData objects based on a specified column name and sort direction.
        /// </summary>
        /// <param name="x">The first RowData object to compare.</param>
        /// <param name="y">The second RowData object to compare.</param>
        /// <returns>An integer that indicates the relative order of the objects being compared.</returns>
        public int Compare(RowData x, RowData y)
        {
            if (!x.Columns.TryGetValue(_colName, out string xVal))
            {
                xVal = string.Empty;
            }
            if (!y.Columns.TryGetValue(_colName, out string yVal))
            {
                yVal = string.Empty;
            }

            int direction = _desc ? -1 : 1;
            if (TryCompareAsLong(xVal, yVal, out int comparison))
            {
                return direction * comparison;
            }
            else if (TryCompareAsDouble(xVal, yVal, out comparison))
            {
                return direction * comparison;
            }
            else
            {
                return direction * xVal.CompareTo(yVal);
            }
        }

        /// <summary>
        /// Tries to compare two string values as long integers.
        /// </summary>
        /// <param name="x">The first string value to compare.</param>
        /// <param name="y">The second string value to compare.</param>
        /// <param name="comparison">The result of the comparison if successful.</param>
        /// <returns>True if both strings can be parsed as long integers and compared; otherwise, false.</returns>
        private bool TryCompareAsLong(string x, string y, out int comparison)
        {
            if (!long.TryParse(x, out long xLong))
            {
                comparison = 0;
                return false;
            }
            if (!long.TryParse(y, out long yLong))
            {
                comparison = 0;
                return false;
            }

            comparison = xLong.CompareTo(yLong);
            return true;
        }

        /// <summary>
        /// Tries to compare two string values as double-precision floating-point numbers.
        /// </summary>
        /// <param name="x">The first string value to compare.</param>
        /// <param name="y">The second string value to compare.</param>
        /// <param name="comparison">The result of the comparison if successful.</param>
        /// <returns>True if both strings can be parsed as double-precision floating-point numbers and compared; otherwise, false.</returns>
        private bool TryCompareAsDouble(string x, string y, out int comparison)
        {
            if (!double.TryParse(x, out double xDouble))
            {
                comparison = 0;
                return false;
            }
            if (!double.TryParse(y, out double yDouble))
            {
                comparison = 0;
                return false;
            }

            comparison = xDouble.CompareTo(yDouble);
            return true;
        }
    }
}
