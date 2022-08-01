### What is Run set Auto Run?            
Run set Auto Run is a set of configurations which allows to trigger Ginger execution automatically via command line or API.
           
### Why to use Run set Auto Run?            
You should use Run set Auto Run in case you already created a Run set or have ready Business Flows to be executed and you want to execute them automatically on a scheduled time or for CI/CD purposes via different tools like Windows Task Scheduler, Jenkins, etc.
 
### Which Run set Auto Run Options are Supported?                       
You can configure to run existing Run Set or virtual Run Set (not existing Run set which will be created dynamically for the execution). Auto run options supporting executing existing Run set using few simple command arguments, customized Run set or Virtual Run set using JSON configuration file.
 
### Where I Can Run the Run set Auto Run?
You can execute the Run set Auto Run on any Operation System (Windows/*Linux).
There are dedicated Executers to be used, for Windows use "Ginger.exe" executer and for Linux use "GingerRuntime.dll".
The execution can be done locally via command line or remotely using an API call to the Ginger Execution Handler service. 
*For Linux - the Solution content (Actions) should be compatible for successful execution.
                       