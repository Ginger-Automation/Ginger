### What are Page Objects Models used for?
POM from Screen-shot allows you to create Page Object Models by uploading a screen-shot of a web page. The feature uses Azure OpenAI to analyze the image, automatically generating HTML structure and element locator's. This is useful for preparing test automation before application deployment or when working with design mock-ups.

### How to create a Page Objects Model?
To create a POM from a screen-shot, simply upload your web page image to the wizard. The Azure OpenAI model will analyze it and generate both HTML preview and code automatically. Next, launch the agent which opens the URL on a browser to learn the elements in the usual way. After reviewing and making any necessary adjustments, save your new POM for immediate use in test automation.

### How to use Page Objects Model in Execution?
Import the POM into your test script and create an instance of the page object. Use the page object's methods to interact with web elements and build your test cases. Run your tests with the integrated POM, which contains all necessary locator's and methods for page interactions.