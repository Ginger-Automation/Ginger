# Negative Test with Exception

### Usage for methods which might return exception based on condition of input params (non GingerAction method)
```cs
 [TestMethod]
 [ExpectedException(typeof(System.DivideByZeroException))]
 public void DivideMethodTest()
 {
     DivideClass.DivideMethod(0);
 }
```

### Ginger Action - Use GA.AddError and verify no errors using Assert

```cs

 [GingerAction("ReadExcelCell", "Read From Excel")]
 public void ReadExcelCell(ref GingerAction GA, string FileName, string sheetName, string row, string column)
 {            
     if (!FileExist(filePath))
     {
         GA.AddError("File not found at: " + filePath);
     }
     // ...
  }


```
