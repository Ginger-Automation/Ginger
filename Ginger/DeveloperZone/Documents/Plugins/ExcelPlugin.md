# Ginger OpenXML Plugin

Using DocumentFormat.OpenXml from Microsoft

The Open XML SDK provides tools for working with Office Word, Excel, and PowerPoint documents. It supports scenarios such as:

- High-performance generation of word-processing documents, spreadsheets, and presentations.
- Populating content in Word files from an XML data source.
- Splitting up (shredding) a Word or PowerPoint file into multiple files, and combining multiple Word/PowerPoint files into a single file.
- Extraction of data from Excel documents.
- Searching and replacing content in Word/PowerPoint using regular expressions.
- Updating cached data and embedded spreadsheets for charts in Word/PowerPoint.
- Document modification, such as removing tracked revisions or removing unacceptable content from documents.

# Excel operations
## Enable to Read/Write from/to excel files
.NET Standards 2.0

Sample table

------------------
|    | A | B    | C      | D     |  E   |
| -- | --| --   | --     | --    | --   |
| 1  |ID |First	|Last    | Phone |	Used|
| 2  |12 |David	|Cohen   | 923646|	Yes |
| 3  |24 |Moshe	|Smith   | 073769|	No  |
| 4  |32 |Dana	|Roberts | 878375|	No  |


### Parameters
- Row - Can be "#3" (row 3) or "User='No'" (criteria)  - represent one row only
- Column - can be "#C" (column c), "#3" (column 3), "Last" (Column name from row 1) - all refer to the same column
- Columns - columns seperated with ',' - "#B, #C"   or "First,Last" if empty means all columns.
- Values - set of column,value - "Used='Yes'" or "ID='12', First='Dave'"  or just values "'55', 'John', 'Smith'"

#### Read Data from Excel

- ReadExcellCell(row, col)
- ReadExcellRow(row, columns) 
- ReadExcellAndUpdate(row, columns, Values) 
- Append(values)
- WriteCell(row, col, value)

### Examples:

- ReadExcellCell("#3", "#B", ) - will return: 'Moshe' in output values
- ReadExcellCell("First=Moshe", "ID") - will return: 'Moshe' in output values

- ReadExcellRow("#2","") - will return: '12', 'David', 'Cohen', '923646', 'Yes' in output values
- ReadExcellRow("Used=No", "") - will return the first row data withc match the criteria, in this case data of row 3
- ReadExcellRow("ID>30' and 'Used'='No'", "") - will return the first row data which match the criteria, in this case data of row 4

- ReadExcellAndUpdate("#2", "'Used'='No")


TODO: Excel with Key value(s)


