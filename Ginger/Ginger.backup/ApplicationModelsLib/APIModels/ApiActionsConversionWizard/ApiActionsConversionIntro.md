### 1- Why to convert web services actions to API Models?
Web services Actions (from type WebAPI RESR/SOAP) should be converted to API Models in order to enjoy the benefits of using API Models like improve Solution maintenance by placing the API configurations on repository level and separate it from the execution flow on which only the data is been set.

### 2- What will happen during the conversion process?
During the conversion process, New API Model will be created for each unique web services Action in selected Business Flows and Action which execute the added API Model will be added after the converted Action (which will become disabled)