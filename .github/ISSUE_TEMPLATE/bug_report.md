---
Bug : 
Password encryption works fine during execution, but after generating the report it gets displayed without encryption.

To Reproduce Steps to reproduce the behavior:

1. Go to add parameters and select Parameter Password String

2. Assign the value to parameter and select generate and check if it is encrypted

3. Now run the business flow

4. After completion of the execution , click on generate reports and go to the destination where results are stored.

5. In execution details check for the details

Expected behavior : Password in the report should be displayed in encrypted form

Actual : Password is not encrypted.
