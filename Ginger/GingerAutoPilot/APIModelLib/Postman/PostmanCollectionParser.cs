#region License
/*
Copyright © 2014-2026 European Support Limited
 
Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
 
http://www.apache.org/licenses/LICENSE-2.0
 
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Xml;

namespace GingerAutoPilot.APIModelLib.Postman;
public class PostmanCollectionParser : APIConfigurationsDocumentParserBase
{
    PostmanCollection PostmanCollection { get; set; }
    public static string GetServerUrlFromItemRequest(Request? request)
    {
        if (request == null || request.Url == null || string.IsNullOrEmpty(request.Url.Raw))
        {
            return string.Empty;
        }

        var host = string.Join(".", request.Url?.Host ?? Enumerable.Empty<string>());
        var serverUrl = $"{request.Url?.Protocol}://{host}";

        if (!string.IsNullOrEmpty(request.Url?.Port))
        {
            serverUrl += $":{request.Url.Port}";
        }

        return serverUrl;
    }

    public static bool IsItemRequestModeSupported(Request request)
    {
        if (request.Body != null && request.Body.Mode != null && request.Url != null && request.Url.Protocol != null)
        {
            // if the request body mode is not raw or urlencoded and the protocol is not http or https, return false
            if (!(request.Body.Mode.Equals("raw", StringComparison.OrdinalIgnoreCase) || request.Body.Mode.Equals("urlencoded", StringComparison.OrdinalIgnoreCase) || request.Body.Mode.Equals("formdata", StringComparison.OrdinalIgnoreCase)) && request.Url.Protocol.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        return true;
    }

    public static bool IsCollectionVersion2_1(string FileName)
    {
        string fileContent = Amdocs.Ginger.Common.GeneralLib.General.FileContentProvider(FileName);
        var jsonObject = JsonSerializer.Deserialize<Dictionary<string, object>>(fileContent);

        if (jsonObject != null && jsonObject.ContainsKey("info"))
        {

            var info = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonObject["info"].ToString() ?? string.Empty);


            if (info != null && info.ContainsKey("schema"))
            {
                if (info["schema"] != null && info["schema"].ToString() != null)
                {
                    var schema = info["schema"].ToString();
                    if (string.Equals(schema, "https://schema.getpostman.com/json/collection/v2.1.0/collection.json", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

            }
        }

        return false;
    }

    public override ObservableList<ApplicationAPIModel> ParseDocument(string FileName, ObservableList<ApplicationAPIModel> AAMSList, bool avoidDuplicatesNodes = false)
    {
        try
        {
            if (!IsCollectionVersion2_1(FileName))
            {
                Reporter.ToLog(eLogLevel.ERROR, $"The provided Postman collection JSON is not valid schema. Note: Only Postman Collection v2.1 are supported at this time.");
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, $"The provided Postman collection JSON is not valid schema. Note: Only Postman Collection v2.1 are supported at this time.");
                return [];
            }
            string fileContent = Amdocs.Ginger.Common.GeneralLib.General.FileContentProvider(FileName);
            PostmanCollection = JsonSerializer.Deserialize<PostmanCollection>(fileContent);
            AAMSList = ConvertToAPIModels(PostmanCollection, AAMSList);
            return AAMSList;
        }
        catch (Exception ex)
        {
            Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to read provided yaml document ", ex);
            Reporter.ToUser(eUserMsgKey.InvalidYAML);
            return [];
        }
    }

    public string GetInfoTitle()
    {
        return PostmanCollection?.Info?.Name;
    }

    public ObservableList<ApplicationAPIModel> ConvertToAPIModels(PostmanCollection postmanCollection, ObservableList<ApplicationAPIModel> applicationAPIModels)
    {
        var postmanRequests = FlattenItems(postmanCollection.Item, postmanCollection.Info.Name);
        foreach (var postmanRequest in postmanRequests)
        {
            if (postmanRequest.Request != null)
            {
                var applicationModel = new ApplicationAPIModel
                {
                    Name = postmanRequest.Name,
                    Description = $"{postmanRequest.Description?.Content}{postmanRequest.Request.Description?.Content}",
                    RelativeFilePath = postmanRequest.FolderPath
                };

                AddPostmanVariablesToAPIModelGlobalParameter(postmanCollection, applicationModel);

                ProcessRequest(postmanRequest.Request, applicationModel);

                applicationAPIModels.Add(applicationModel);
            }
        }
        return applicationAPIModels;
    }

    private static void AddPostmanVariablesToAPIModelGlobalParameter(PostmanCollection postmanCollection, ApplicationAPIModel applicationModel)
    {
        if (postmanCollection.Variable?.Count > 0)
        {
            foreach (var item in postmanCollection.Variable)
            {
                item.Key = item.Key.StartsWith("{{") && item.Key.EndsWith("}}") ? item.Key.Replace("{{", "").Replace("}}", "") : item.Key;
                //Handle global parameters(e.g., domain)
                string value = item.Value;
                if (string.IsNullOrEmpty(value))
                {
                    value = "{Current Value}";
                }
                var domainParam = new GlobalAppModelParameter()
                {
                    PlaceHolder = "{{" + item.Key + "}}",
                    OptionalValuesList = [new OptionalValue { Value = value, IsDefault = true }]

                };

                applicationModel.GlobalAppModelParameters.Add(domainParam);
            }
        }
    }

    public static List<Item> FlattenItems(List<Item> items, string currentPath = "")
    {
        var result = new List<Item>();

        foreach (var item in items)
        {
            item.FolderPath = string.IsNullOrEmpty(currentPath)
                ? item.Name ?? string.Empty
                : $"{currentPath}/{item.Name ?? string.Empty}";

            // Add the current item to the list if it has request data
            if (item.Request != null)
            {
                result.Add(item);
            }

            // If the item has nested items, flatten each one and add it to the list
            if (item.ItemList != null)
            {
                result.AddRange(FlattenItems(item.ItemList, item.FolderPath));
            }
        }

        return result;
    }

    public void ProcessRequest(Request postmanRequest, ApplicationAPIModel applicationAPIModel)
    {
        if (postmanRequest.Method == null)
        {
            applicationAPIModel.RequestType = ApplicationAPIUtils.eRequestType.GET;
        }
        else
        {
            applicationAPIModel.RequestType = postmanRequest.Method.ToUpper() switch
            {
                "GET" => ApplicationAPIUtils.eRequestType.GET,
                "DELETE" => ApplicationAPIUtils.eRequestType.DELETE,
                "HEAD" => ApplicationAPIUtils.eRequestType.Head,
                "OPTIONS" => ApplicationAPIUtils.eRequestType.Options,
                "PATCH" => ApplicationAPIUtils.eRequestType.PATCH,
                "POST" => ApplicationAPIUtils.eRequestType.POST,
                "PUT" => ApplicationAPIUtils.eRequestType.PUT,
                "TRACE" => ApplicationAPIUtils.eRequestType.POST,
                "UNDEFINED" => ApplicationAPIUtils.eRequestType.POST,
                _ => ApplicationAPIUtils.eRequestType.GET,
            };
        }

        ProcessUrl(postmanRequest.Url, applicationAPIModel);
        ProcessHeaders(postmanRequest.Header, applicationAPIModel);
        ProcessBody(postmanRequest.Body, applicationAPIModel);

        AddMissingModelParameterFromBody(applicationAPIModel);

    }

    private static void AddMissingModelParameterFromBody(ApplicationAPIModel applicationAPIModel)
    {
        if (string.IsNullOrEmpty(applicationAPIModel.RequestBody))
        {
            return;
        }

        try
        {
            // Check if the request body is JSON
            if (IsValidJson(applicationAPIModel.RequestBody))
            {
                using var doc = JsonDocument.Parse(applicationAPIModel.RequestBody);
                var root = doc.RootElement;
                ExtractAndAddParametersFromJson(root, applicationAPIModel);
            }
            // Check if the request body is XML
            else if (IsValidXml(applicationAPIModel.RequestBody))
            {
                var doc = new XmlDocument();
                doc.LoadXml(applicationAPIModel.RequestBody);
                ExtractAndAddParametersFromXml(doc.DocumentElement, applicationAPIModel);
            }

        }
        catch (Exception ex)
        {
            Reporter.ToLog(eLogLevel.ERROR, "Error occurred while parsing the request body", ex);
        }
    }

    private static void ExtractAndAddParametersFromJson(JsonElement element, ApplicationAPIModel applicationAPIModel)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.String && property.Value.GetString().StartsWith("{{") && property.Value.GetString().EndsWith("}}"))
                {
                    string placeholder = property.Value.GetString();
                    AddModelParameter(applicationAPIModel, property.Name, placeholder);
                }
                else
                {
                    ExtractAndAddParametersFromJson(property.Value, applicationAPIModel);
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                ExtractAndAddParametersFromJson(item, applicationAPIModel);
            }
        }
    }

    private static void ExtractAndAddParametersFromXml(XmlNode node, ApplicationAPIModel applicationAPIModel)
    {
        if (node == null)
        {
            return;
        }

        foreach (XmlNode childNode in node.ChildNodes)
        {
            if (childNode.NodeType == XmlNodeType.Element && childNode.InnerText.StartsWith("{{") && childNode.InnerText.EndsWith("}}"))
            {
                string placeholder = childNode.InnerText;
                AddModelParameter(applicationAPIModel, childNode.Name, placeholder);
            }
            else
            {
                ExtractAndAddParametersFromXml(childNode, applicationAPIModel);
            }
        }
    }

    private static bool IsValidJson(string strInput)
    {
        strInput = strInput.Trim();
        if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
            (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
        {
            try
            {
                var obj = JsonDocument.Parse(strInput);
                return true;
            }
            catch (JsonException) //Not valid JSON
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    private static bool IsValidXml(string strInput)
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(strInput);
            return true;
        }
        catch (XmlException) //Not valid XML
        {
            return false;
        }
    }

    private static void AddModelParameter(ApplicationAPIModel applicationAPIModel, string key, string value)
    {
        string placeholder = value;
        if (!applicationAPIModel.AppModelParameters.Any(p => p.PlaceHolder.Equals(placeholder, StringComparison.CurrentCultureIgnoreCase)) &&
            !applicationAPIModel.GlobalAppModelParameters.Any(p => p.PlaceHolder.Equals(placeholder, StringComparison.CurrentCultureIgnoreCase)))
        {
            var newParam = new GlobalAppModelParameter
            {
                PlaceHolder = placeholder,
                Description = key,
                OptionalValuesList = [new OptionalValue { Value = "{Current Value}", IsDefault = true }]
            };

            applicationAPIModel.GlobalAppModelParameters.Add(newParam);
        }
    }



    private static void ProcessUrl(Url url, ApplicationAPIModel applicationAPIModel)
    {
        if (url == null)
        {
            return;
        }

        if (url.Query != null && url.Query.Count != 0)
        {
            applicationAPIModel.EndpointURL = $"{url.Host.FirstOrDefault()}/{string.Join("/", url.Path)}";

            // Process path parameters that are formatted as placeholders
            if (url.Path?.Count > 0)
            {
                foreach (var item in url.Path)
                {
                    if (item.StartsWith("{{") && item.EndsWith("}}"))
                    {
                        AddModelParameter(applicationAPIModel, string.Empty, item);
                        continue;
                    }
                }
            }

            foreach (Query param in url.Query)
            {
                if (!param.Disabled.Value)
                {
                    applicationAPIModel.EndpointURL = !applicationAPIModel.EndpointURL.Contains('?') ?
                        applicationAPIModel.EndpointURL + "?" + param.Key + "=" + "{{" + param.Key + "}}" :
                        applicationAPIModel.EndpointURL + "&" + param.Key + "=" + "{{" + param.Key + "}}";

                    AddModelParameter(applicationAPIModel, param.Key, param.Value, $"{param.Key} in query");
                }

            }
        }
        // In few cases, the url has not any query present therefore endpoint urls coming double as copy copy, need to discuss that in these kinds of cases what could be done
        if (string.IsNullOrEmpty(applicationAPIModel.EndpointURL))
        {
            applicationAPIModel.EndpointURL = url.Raw;
        }
    }


    private static string AddModelParameter(ApplicationAPIModel applicationAPIModel, string Key, string Value, string Description = null)
    {
        string placeholder = "{{" + Key + "}}";
        if (!string.IsNullOrEmpty(Value) && Value.StartsWith("{{") && Value.EndsWith("}}"))
        {
            //Check if param already exists in global parameter, Add if not present
            if (!applicationAPIModel.GlobalAppModelParameters.Any(f => f.PlaceHolder.Equals(Value, StringComparison.CurrentCultureIgnoreCase)))
            {

                //Handle global parameters(e.g., domain)
                var domainParam = new GlobalAppModelParameter()
                {
                    PlaceHolder = Value,
                    Description = Description,
                    OptionalValuesList = [new OptionalValue { Value = "{Current Value}", IsDefault = true }]
                };

                applicationAPIModel.GlobalAppModelParameters.Add(domainParam);
            }
            return Value;
        }
        else
        {
            if (!applicationAPIModel.AppModelParameters.Any(f => f.PlaceHolder.Equals(placeholder, StringComparison.CurrentCultureIgnoreCase)))
            {

                ObservableList<OptionalValue> listOptions = [new OptionalValue() { Value = Value, ItemName = Value, IsDefault = true }];
                applicationAPIModel.AppModelParameters.Add(new AppModelParameter(placeholder, Description, "", "", listOptions));
            }
        }

        return placeholder;
    }

    private static void ProcessBody(Body body, ApplicationAPIModel applicationAPIModel)
    {
        if (body == null)
        {
            return;
        }

        switch (body.Mode.ToLower())
        {
            case "raw":
                ProcessRawBody(body, applicationAPIModel);
                break;
            case "urlencoded":
                ProcessUrlEncodedBody(body, applicationAPIModel);
                break;
            case "formdata":
                ProcessFormDataBody(body, applicationAPIModel);
                break;
            case "graphql":
                ProcessGraphQLBody(body, applicationAPIModel);
                break;
            default:
                applicationAPIModel.RequestBody = body.Raw;
                break;
        }

        applicationAPIModel.RequestBodyType = ApplicationAPIUtils.eRequestBodyType.FreeText;
    }

    private static void ProcessGraphQLBody(Body body, ApplicationAPIModel applicationAPIModel)
    {
        if (body?.GraphQL == null)
        {
            return;
        }

        var graphQLBody = body.GraphQL;

        // Create the object to serialize  
        var graphQLRequestBody = new
        {
            query = graphQLBody.Query,
            variables = graphQLBody.Variables,
            operationName = graphQLBody.OperationName // Use lowercase for property names  
        };

        // Serialize the object to JSON format using System.Text.Json  
        applicationAPIModel.RequestBody = JsonSerializer.Serialize(graphQLRequestBody);

        applicationAPIModel.AppModelParameters.Add(new AppModelParameter
        {
            PlaceHolder = "<GRAPHQL_QUERY>",
            Description = "GraphQL query",
            OptionalValuesList = { new OptionalValue { Value = graphQLBody.Query, IsDefault = true } }
        });

        if (!string.IsNullOrEmpty(graphQLBody.Variables))
        {
            applicationAPIModel.AppModelParameters.Add(new AppModelParameter
            {
                PlaceHolder = "<GRAPHQL_VARIABLES>",
                Description = "GraphQL variables",
                OptionalValuesList = { new OptionalValue { Value = JsonSerializer.Serialize(graphQLBody.Variables), IsDefault = true } }
            });
        }
        if (!string.IsNullOrEmpty(graphQLBody.OperationName))
        {
            applicationAPIModel.AppModelParameters.Add(new AppModelParameter
            {
                PlaceHolder = "<GRAPHQL_OPERATION_NAME>",
                Description = "GraphQL operation name",
                OptionalValuesList = { new OptionalValue { Value = graphQLBody.OperationName, IsDefault = true } }
            });
        }
        applicationAPIModel.RequestBodyType = ApplicationAPIUtils.eRequestBodyType.FreeText;
    }

    private void ProcessHeaders(List<Header> headers, ApplicationAPIModel applicationAPIModel)
    {
        // If there are no headers, return immediately
        if (headers?.Count == 0)
        {
            return;
        }

        // Iterate through each header
        foreach (Header header in headers)
        {
            // Skip disabled headers
            if (header.Disabled.Value)
            {
                continue;
            }

            // Process the "Accept" header to determine the response content type
            if (header.Key == "Accept")
            {
                applicationAPIModel.ResponseContentType = header.Value switch
                {
                    "application/x-www-form-urlencoded" => ApplicationAPIUtils.eResponseContentType.XwwwFormUrlEncoded,
                    "application/json" => ApplicationAPIUtils.eResponseContentType.JSon,
                    "application/xml" => ApplicationAPIUtils.eResponseContentType.XML,
                    "multipart/form-data" => ApplicationAPIUtils.eResponseContentType.FormData,
                    _ => ApplicationAPIUtils.eResponseContentType.Any
                };

                // If a specific response content type is set, skip further processing for this header
                if (applicationAPIModel.ResponseContentType != ApplicationAPIUtils.eResponseContentType.Any)
                {
                    continue;
                }
            }

            // Process the "Content-Type" header to determine the request content type
            if (header.Key == "Content-Type")
            {
                applicationAPIModel.RequestContentType = header.Value switch
                {
                    "application/x-www-form-urlencoded" => ApplicationAPIUtils.eRequestContentType.XwwwFormUrlEncoded,
                    "application/json" => ApplicationAPIUtils.eRequestContentType.JSon,
                    "application/xml" => ApplicationAPIUtils.eRequestContentType.XML,
                    "multipart/form-data" => ApplicationAPIUtils.eRequestContentType.FormData,
                    _ => ApplicationAPIUtils.eRequestContentType.TextPlain
                };

                // If a specific request content type is set, skip further processing for this header
                if (applicationAPIModel.RequestContentType != ApplicationAPIUtils.eRequestContentType.TextPlain)
                {
                    continue;
                }
            }

            // Create a model parameter for the header
            string modelParamName = AddModelParameter(applicationAPIModel, header.Key, header.Value, $"{header.Key} in headers. {header.Description}");

            // Create a new APIModelKeyValue instance for the header
            APIModelKeyValue modelKeyValue = new APIModelKeyValue
            {
                ItemName = header.Key,
                Param = header.Key,
                Value = modelParamName,
            };

            // Add the header to the application's HTTP headers
            applicationAPIModel.HttpHeaders.Add(modelKeyValue);
        }
    }

    private static void ProcessRawBody(Body body, ApplicationAPIModel applicationAPIModel)
    {
        if (body == null || string.IsNullOrEmpty(body.Raw))
        {
            return;
        }

        var rawBody = body.Raw;
        foreach (var param in body.Formdata ?? Enumerable.Empty<Formdata>())
        {
            string placeholder = AddModelParameter(applicationAPIModel, param.Key, param.Value, Description: param.Description.Content);

            if (!param.Value.StartsWith("{{") && !param.Value.EndsWith("}}"))
            {
                rawBody = rawBody.Replace($"\"{param.Value}\"", placeholder);
            }
        }

        applicationAPIModel.RequestBody = rawBody;
    }
    private static void ProcessUrlEncodedBody(Body body, ApplicationAPIModel applicationAPIModel)
    {
        if (body == null || body.Urlencoded == null)
        {
            return;
        }

        foreach (var param in body.Urlencoded)
        {
            string ModelParameterName = AddModelParameter(applicationAPIModel, param.Key, param.Value, param.Description?.Content);

            applicationAPIModel.APIModelBodyKeyValueHeaders.Add(new APIModelBodyKeyValue()
            {
                Param = param.Key,
                Value = ModelParameterName,
            });
        }
    }
    private static void ProcessFormDataBody(Body body, ApplicationAPIModel applicationAPIModel)
    {
        if (body == null || body.Formdata == null)
        {
            return;
        }

        foreach (var param in body.Formdata)
        {
            string ModelParameterName = AddModelParameter(applicationAPIModel, param.Key, param.Value, param.Description?.Content);

            applicationAPIModel.APIModelBodyKeyValueHeaders.Add(new APIModelBodyKeyValue()
            {
                Param = param.Key,
                Value = ModelParameterName,
            });
        }
    }
}
