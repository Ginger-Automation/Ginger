Wizard


Create each wizard in folder for example wizard for Addnew agent will be in AddAgentWizardLib
each wizard page is seperate Page
start with intro page

Sample of 3 screens intro and summary

Validation

xDriverTypeComboBox.AddValidationRule(new EmptyValidationRule());

xAgentNameTextBox.AddValidationRule(new AgentNameValidationRule());

xDriverTypeComboBox.AddValidationRule(eValidationRule.CannotBeEmpty);

How to check if items is unique - for example not to have duplicate agent name
Check File exist

Can add more thean one validation

Buttons

When are they enabled/disabled


Enable disable buttons in code:
Next/Prev and Finish - dont change - automatically done based on validation


Automated Testing

if there are 5 pages Finish button should be enabled at start, then validation, if fails disable, user doesn't have to go over all screen

You can use same valdiaiton for edit pages - for example Agent Edit Page - not allowed empty name


for Wizard event use Switch case
