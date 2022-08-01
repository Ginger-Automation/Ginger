### What are API Models used for?
API Models are used to store all API Request/Respond configurations in a template format which allows to parameterize all text based configurations. 
By using API Models you will be able to maintain relevant API configurations with much less effort because all API executions (Actions) will be mapped to the same configurations source/model. In addition, no technical knowledge is needed in execution design phase as users which will use those API Models will be asked to provide only the API input parameters values and will not need to deal with the API’s technical configurations which will be set and stored only once on the model level.

### How to create an API Model?
You can create an API Model by simply following the API Model Import wizard, which will automatically learn the API configurations from different sources like WSDL/Swagger/Sample XML or JSON Requests files. Once learned, you will also be able to automatically lean its possible parameters’ input values from previous API request files and also from Excel/Database.

### How to use API Model in Execution?
You can add the “Web API Model” Action in which you will be able to map the wanted API Model to execute and set its input parameters data. 