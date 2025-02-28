// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault
// ReSharper disable SwitchStatementMissingSomeEnumCasesNoDefault

namespace Uci.Net;

/// <summary>
/// Extension methods for converting between UCI configuration objects and JSON representations.
/// </summary>
public static class UciConfigExtensions
{
    public const string SectionTypeKey = "__uci_section_type";

    internal static InvalidCastException CreateCastException<T>(JsonNode? node) where T : JsonNode
    {
        return new InvalidCastException($"Can not cast {node?.GetType()} to {typeof(T).Name}.");
    }

    internal static T AsNode<T>(this JsonNode? node) where T : JsonNode
    {
        return node as T ?? throw CreateCastException<T>(node);
    }

    /// <summary>
    /// Converts a UCI configuration to a serializable JSON object.
    /// The resulting JSON structure organizes sections by their types and names,
    /// allowing for easier access to configuration data in a hierarchical format.
    /// </summary>
    /// <param name="config">The UCI configuration to convert.</param>
    /// <returns>
    /// A JsonObject where:
    /// - Named sections are organized as {sectionType: {sectionName: {options}}}
    /// - Unnamed sections are organized as {sectionType: [{options}, {options}, ...]}
    /// </returns>
    public static JsonObject ToSerializableJsonObject(this UciConfig config)
    {
        var jsonObject = new JsonObject();
        foreach (var (type, name, options) in config.Sections)
        {
            var optionsNode = options.ToStructuredJsonObject();

            if (string.IsNullOrEmpty(name))
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
            else
            {
                // named section
                optionsNode[SectionTypeKey] = type;

                if (jsonObject.TryGetPropertyValue(name, out var node) && node is not null)
                {
                    foreach (var (key, value) in optionsNode)
                    {
                        node[key] = value;
                    }
                }
                else
                {
                    jsonObject[name] = optionsNode;
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

    private static UciOption[] ToUciOptions(this JsonObject jsonObject)
    {
        var options = new List<UciOption>();

        foreach (var (key, node) in jsonObject)
        {
            if (key == SectionTypeKey)
                continue;

            switch (node)
            {
                case JsonObject:
                {
                    break;
                }
                case JsonArray array:
                {
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

        return options.ToArray();
    }

    /// <summary>
    /// Converts a JSON object (serialized from a custom config type) to a UCI configuration.
    /// This method processes the hierarchical JSON structure and creates
    /// corresponding UCI sections and options.
    /// </summary>
    /// <param name="jsonObject">The structured JSON object to convert.</param>
    /// <returns>
    /// A UciConfig object containing sections and options derived from the JSON structure.
    /// The method handles both named sections (represented as nested objects)
    /// and unnamed sections (represented as arrays).
    /// </returns>
    public static UciConfig ToUciConfig(this JsonObject jsonObject)
    {
        var config = new UciConfig();

        foreach (var (key, node) in jsonObject)
        {
            switch (node)
            {
                case JsonObject obj:
                {
                    var type = obj[SectionTypeKey]?.ToString();
                    if (string.IsNullOrEmpty(type))
                        throw new InvalidOperationException($"Missing uci section type for section '{key}'.");

                    var section = CreateSection(type, key, obj);
                    config.Sections.Add(section);
                    break;
                }
                case JsonArray array:
                {
                    // unnamed section
                    foreach (var optionsNode in array)
                    {
                        var section = CreateSection(key, "", optionsNode);
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
        return token switch
        {
            null => "",
            JsonValue value when value.TryGetValue<string>(out var str) => str,
            _ => token.ToJsonString()
        };
    }
}