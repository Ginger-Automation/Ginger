### What are Page Objects Models used for?
The Update Multiple POM Wizard is a tool that lets you update several Page Object Models (POMs) at the same time. It helps you keep your object references consistent across all your test run sets and saves you time by updating everything together.

### How to create a Page Objects Model?
First, select your target application in the wizard. Next, choose the POMs you want to update from the filtered list. The wizard will then find all the run sets that use these POMs and update them automatically. After the update, you’ll see the status of each runset as the system verifies the changes in the background.

### How to use Page Objects Model in Execution?
When you create or edit a test action, set 'Locate By' to 'Page Objects Model Element'. Then, select the object you want to use. This method helps you avoid duplicates and reduces the chance of errors when using the same controls in different actions.