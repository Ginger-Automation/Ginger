### What is Agent?
            
Agents are serving as proxy with the application being automated or application under test."
                       
           
### How Agents Been Defined?
            
Agents are separated by their Platform (like: Web, Mobile, Java, etc.) and each Agent is supposed to be used for different type of tested application under the same Platform type.
            
Like: for Mobile testing you will have Agent type which fits for android testing and one for iOS, or for Web you will have Agent type matching to each Browser Type (IE, Chrome, Firefox, etc.).
                        
            
### Can I have Multiple Agents from Same Type?
            
Yes, you can define as many Agents as you need, each Agent store configurations list, so you can have 2 Agents from the same Type but each will have different configurations values."
            
Like: 2 Chrome Agents but one configured to use Proxy and the other not, or 2 Android Agents but each will point to different Android App in the device."            
            
            
### Where I Use Agents in my Automation Flow?
            
You will need to match between the ```$GingerCore.eTermResKey.BusinessFlow$``` Target Application and the Agent you want it to use (both must be from the same Platform type).
            
Like: your Target Application for testing is Facebook and the Agent you want to use is Chrome Agent because you want to test Facebook behaviour on Chrome browser."
                       