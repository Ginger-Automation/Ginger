# Creating Ginger Wizard

### Wizard are used to help user naviage during a process while guiding step by step

### Each Wizard consist of 3 items
- Intro page
- Data collection page(s)
- Summary page

- Create each wizard in folder for example wizard for Addnew agent will be in AddAgentWizardLib
each wizard page is seperate Page
start with intro page

Sample of 3 screens intro and summary


# Miminal code create work class - Wizard pages only for data collection

### Create Wizard clas 

# Fields Validation

- xDriverTypeComboBox.AddValidationRule(new EmptyValidationRule());

- xAgentNameTextBox.AddValidationRule(new AgentNameValidationRule());

- xDriverTypeComboBox.AddValidationRule(eValidationRule.CannotBeEmpty);


How to check if items is unique - for example not to have duplicate agent name
Check File exist

Can add more thean one validation

# Page code
Should be minimal only for data collection, 

## Buttons

When are they enabled/disabled
- *Previous* if the current page is not the first page
- *Next* Enable when first time showing the page
         Disabled if user click next and validation(s) fails or this is the last page
- *Finish* Disabled when wizard just start and this is the first page
              Enabled Wizard return true for CanFinish validation

Enable disable buttons in code:
Next/Prev and Finish - dont change - automatically done based on validation


## Automated Testing

if there are 5 pages Finish button should be enabled at start, then validation, if fails disable, user doesn't have to go over all screen

You can use same valdiaiton for edit pages - for example Agent Edit Page - not allowed empty name


## finish code
in the Wizard

## Wizard events use Switch case


```cs
       switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (AddAgentWizard)WizardEventArgs.Wizard;                    
                    break;
            }
```

## Pages - start with Intro and fisnih with Summery
Use WizardIntroPage
