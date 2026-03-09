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

#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GingerAutoPilot.APIModelLib.Postman;
public class PostmanCollection
{
    [JsonPropertyName("info")]
    public PostmanCollectionInfo? Info { get; set; }

    [JsonPropertyName("item")]
    public List<Item>? Item { get; set; }
    [JsonPropertyName("variable")]
    public List<Variable>? Variable { get; set; }
}

public partial class PostmanCollectionInfo
{
    [JsonPropertyName("_postman_id")]
    public string? PostmanId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("schema")]
    public string? Schema { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("description")]
    [JsonConverter(typeof(DescriptionConverter))]
    public Description? Description { get; set; }
}

public class Item
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    [JsonConverter(typeof(DescriptionConverter))]
    public Description? Description { get; set; }

    [JsonPropertyName("request")]
    public Request? Request { get; set; }

    [JsonPropertyName("item")]
    public List<Item>? ItemList { get; set; }
    public string FolderPath { get; set; } = "";

}

public class Request
{
    [JsonPropertyName("method")]
    public string? Method { get; set; }

    [JsonPropertyName("header")]
    public List<Header>? Header { get; set; }

    [JsonPropertyName("body")]
    public Body? Body { get; set; }

    [JsonPropertyName("url")]
    public Url? Url { get; set; }

    [JsonPropertyName("description")]
    [JsonConverter(typeof(DescriptionConverter))]
    public Description? Description { get; set; }
}

public class Header
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("description")]
    [JsonConverter(typeof(DescriptionConverter))]
    public Description? Description { get; set; }
    [JsonPropertyName("disabled")]
    public bool? Disabled { get; set; } = false;
}

public class Body
{
    [JsonPropertyName("mode")]
    public string? Mode { get; set; }

    [JsonPropertyName("raw")]
    public string? Raw { get; set; }

    [JsonPropertyName("formdata")]
    public List<Formdata>? Formdata { get; set; }

    [JsonPropertyName("urlencoded")]
    public List<Urlencoded>? Urlencoded { get; set; }

    [JsonPropertyName("graphql")]
    public Graphql? GraphQL { get; set; }
}

public class Formdata
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("description")]
    [JsonConverter(typeof(DescriptionConverter))]
    public Description? Description { get; set; }
}

public class Urlencoded
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("description")]
    [JsonConverter(typeof(DescriptionConverter))]
    public Description? Description { get; set; }
}

public class Graphql
{
    [JsonPropertyName("query")]
    public string? Query { get; set; }

    [JsonPropertyName("variables")]
    public string? Variables { get; set; }

    [JsonPropertyName("operationName")]
    public string? OperationName { get; set; }
}

public class Url
{
    [JsonPropertyName("raw")]
    public string? Raw { get; set; }

    [JsonPropertyName("protocol")]
    public string? Protocol { get; set; }

    [JsonPropertyName("host")]
    public List<string>? Host { get; set; }

    [JsonPropertyName("port")]
    public string? Port { get; set; }

    [JsonPropertyName("path")]
    public List<string>? Path { get; set; }

    [JsonPropertyName("query")]
    public List<Query>? Query { get; set; }
}

public class Query
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
    [JsonPropertyName("disabled")]
    public bool? Disabled { get; set; } = false;
}
public class Description
{
    public string? Content { get; set; }
    public string? Type { get; set; }
    public string? Version { get; set; }
}

public class Variable
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("description")]
    [JsonConverter(typeof(DescriptionConverter))]
    public Description? Description { get; set; }

    [JsonPropertyName("disabled")]
    public bool? Disabled { get; set; } = false;

}

public class DescriptionConverter : JsonConverter<Description>
{
    public override Description? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return new Description { Content = reader.GetString() };
        }
        else if (reader.TokenType == JsonTokenType.StartObject)
        {
            var description = JsonSerializer.Deserialize<Description>(ref reader, options);
            return description;
        }
        else if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }
        throw new JsonException("Invalid JSON for Description");
    }

    public override void Write(Utf8JsonWriter writer, Description value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else if (value.Content == null)
        {
            writer.WriteStringValue(value.Content);
        }
        else
        {
            writer.WriteStartObject();
            writer.WriteString("content", value.Content);
            writer.WriteString("type", value.Type);
            writer.WriteString("version", value.Version);
            writer.WriteEndObject();
        }
    }
}