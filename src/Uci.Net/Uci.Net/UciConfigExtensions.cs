// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault
// ReSharper disable SwitchStatementMissingSomeEnumCasesNoDefault

namespace Uci.Net;

public static class UciConfigExtensions
{
    internal static InvalidCastException CreateCastException<T>(JsonNode? node) where T : JsonNode
    {
        var actualKind = node?.GetValueKind() ?? JsonValueKind.Null;
        return new InvalidCastException($"Can not cast {nameof(JsonNode)} of type {actualKind} to {typeof(T).ShortName()}.");
    }

    internal static T AsNode<T>(this JsonNode? node) where T : JsonNode
    {
        return node as T ?? throw CreateCastException<T>(node);
    }

    public static JsonObject ToStructuredJsonObject(this UciConfig config)
    {
        var jsonObject = new JsonObject();
        foreach (var (type, name, options) in config.Sections)
        {
            var optionsNode = options.ToStructuredJsonObject();

            if (name.IsNotEmpty())
            {
                // named section
                if (jsonObject.TryGetPropertyValue(type, out var node))
                {
                    var obj = node.AsNode<JsonObject>();
                    obj[name] = optionsNode;
                }
                else
                {
                    jsonObject[type] = new JsonObject { { name, optionsNode } };
                }
            }
            else
            {
                // unnamed section
                if (jsonObject.TryGetPropertyValue(type, out var node))
                {
                    var array = node.AsNode<JsonArray>();
                    array.Add(optionsNode);
                }
                else
                {
                    jsonObject[type] = new JsonArray(optionsNode);
                }
            }
        }
        return jsonObject;
    }

    public static JsonObject ToStructuredJsonObject(this IEnumerable<UciOption> options)
    {
        var jsonObject = new JsonObject();
        foreach (var (key, value, isList) in options)
        {
            if (isList)
            {
                if (jsonObject.TryGetPropertyValue(key, out var node))
                {
                    var array = node.AsNode<JsonArray>();
                    array.Add(value);
                }
                else
                {
                    jsonObject[key] = new JsonArray(value);
                }
            }
            else
            {
                jsonObject[key] = value;
            }
        }
        return jsonObject;
    }

    public static UciOption[] ToUciOptions(this JsonObject jsonObject)
    {
        var options = new List<UciOption>();

        foreach (var (key, node) in jsonObject)
        {
            switch (node?.GetValueKind())
            {
                case JsonValueKind.Object:
                {
                    break;
                }
                case JsonValueKind.Array:
                {
                    var array = node.AsNode<JsonArray>();
                    foreach (var valueNode in array)
                    {
                        var option = new UciOption(key, GetValue(valueNode), true);
                        options.Add(option);
                    }
                    break;
                }
                default:
                {
                    var option = new UciOption(key, GetValue(node), false);
                    options.Add(option);
                    break;
                }
            }
        }

        return options.AsSpan().ToArray();
    }

    public static UciConfig ToUciConfig(this JsonObject jsonObject)
    {
        var config = new UciConfig();

        foreach (var (type, node) in jsonObject)
        {
            switch (node?.GetValueKind())
            {
                case JsonValueKind.Object:
                {
                    // named sections
                    foreach (var (name, optionsNode) in node.AsNode<JsonObject>())
                    {
                        var section = CreateSection(type, name, optionsNode);
                        config.Sections.Add(section);
                    }
                    break;
                }
                case JsonValueKind.Array:
                {
                    // unnamed section
                    foreach (var optionsNode in node.AsNode<JsonArray>())
                    {
                        var section = CreateSection(type, "", optionsNode);
                        config.Sections.Add(section);
                    }
                    break;
                }
            }
        }

        return config;

        static UciSection CreateSection(string type, string name, JsonNode? optionsNode)
        {
            var options = optionsNode?.AsNode<JsonObject>().ToUciOptions() ?? [];
            return new UciSection(type, name, options);
        }
    }

    private static string GetValue(JsonNode? token)
    {
        if (token is null)
            return "";

        var kind = token.GetValueKind();
        return kind switch
        {
            JsonValueKind.Null => "",
            JsonValueKind.String => token.GetValue<string>() ?? "",
            _ => token.ToJsonString(),
        };
    }
}