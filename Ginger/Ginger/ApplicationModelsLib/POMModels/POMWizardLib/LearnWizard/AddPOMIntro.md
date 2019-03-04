### What are Page Objects Models used for?
Page Objects Models are used to store a given GUI page’s objects such as buttons, text boxes, drop-down lists, etc.
By using Page Objects Models you will be able to maintain relevant page objects better and keep track of the objects’ availability within the pages, any time there are changes made to them. This will enable you to minimize potential failures encountered during execution and avoid duplications whenever using the same controls in different actions on the same page.

### How to create a Page Objects Model?
You can create a POM by simply following the POM wizard, which will automatically learn a page’s relevant objects and properties. Once learned, you will be able to filter the objects and select only those needed to create the automation required.

### How to use Page Objects Model in Execution?
You can map the UI Element Action to a Page Objects Model element by setting  ‘Locate By’ to ‘Page Objects Model Element’ and select the desired object to execute the operation on.