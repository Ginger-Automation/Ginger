# Ginger Wizard

- Wizard help the user naviage and fill smaller chuncks of data during a process while guiding step by step to accomplish a goal
- Wizard is helping the user to focus on one step at a time


### Each Wizard consist of 3 items
- Intro page - Generla infomation about what the wizard will do
- Data collection page(s) - forms with data the user needs to fill
- Summary page - Shows all users selections so he can verify before finish

### Guidelines
- Keep the number of steps between 3-5 pages
- Each step need to be clear, add label with explanation if needed
- Request only the minimal information needed, so user stay focused and can complete quickly
- Use good defaults if possible

### Coding
- Create each wizard in folder for example wizard for Addnew agent will be in AddAgentWizardLib
- Each wizard page is seperate Page
- Use the generic Intro/finish pages

Sample of 3 screens intro and summary


### Wizard Reuse - Windows/Linux/OSx
- Pgaes should have only daat colelction
code which does work need to be in .NET Standards class in it's DLL
- Windows - WPF

### Miminal code create work class - Wizard pages only for data collection
WPF for UI
.NET Standards - code to create/check etc. - so we can reuse for Web

### Create Wizard class

- in DLL not WPF project
- Pages - create in WPF project

### Fields Validation

Validation are done using rules and displayed to the user in red below the control

![alt text](../images/AddAgentWizradAgnetDetails.png?raw=true)

- Use *Field*.AddValidationRule()

- xDriverTypeComboBox.AddValidationRule(new EmptyValidationRule());

- xAgentNameTextBox.AddValidationRule(new AgentNameValidationRule());

- xDriverTypeComboBox.AddValidationRule(eValidationRule.CannotBeEmpty);


How to check if items is unique - for example not to have duplicate agent name
Check File exist

Can add more thean one validation

### Page code
Should be minimal only for data collection, 

### Buttons

When are they enabled/disabled
- *Previous* if the current page is not the first page
- *Next* Enable when first time showing the page
         Disabled if user click next and validation(s) fails or this is the last page
- *Finish* Disabled when wizard just start and this is the first page
              Enabled Wizard return true for CanFinish validation approved by all pages

Enable disable buttons in code:
Next/Prev and Finish - dont change - automatically done based on validation

- Pages List - user can jump to page, backword any, forward if validation pass

### Navigation list
- Show all pages including intro and summary
- User can switch page by click on page which he visited earlier (like 'back' to the page)
- Pages which haven't been visited yet, are greyed - disabled

### Automated Testing

if there are 5 pages Finish button should be enabled at start, then validation, if fails disable, user doesn't have to go over all screen

You can use same valdiaiton for edit pages - for example Agent Edit Page - not allowed empty name


### finish code
in the Wizard

### Wizard events use Switch case


```cs
       switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (AddAgentWizard)WizardEventArgs.Wizard;                    
                    break;
            }
```

### Pages - start with Intro and fisnih with Summery
Use WizardIntroPage



### Long task Show Processing

- Processing image if unknow length
- Progress bar if number of steps known


### Opening the wizard from code

code sample

use width if the size is bigger than 800