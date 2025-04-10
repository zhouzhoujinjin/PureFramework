using CyberStone.Core.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CyberStone.Core.Converters
{
  public class JsonValueSpaceConverter : JsonConverter<ValueSpace>
  {
    public override bool CanConvert(Type typeToConvert)
    {
      var result = typeof(ValueSpace) == typeToConvert;
      return result;
    }

    public override ValueSpace Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      using var jsonDocument = JsonDocument.ParseValue(ref reader);
      var jsonObject = jsonDocument.RootElement;

      var _ = jsonObject.TryGetProperty("valueSpaceType", out var valueSpaceType);

      var name = jsonObject.GetProperty("name").GetString()!;
      var title = jsonObject.GetProperty("title").GetString()!;
      var configureLevel = jsonObject.GetProperty("configureLevel").GetString() switch
      {
        "System" => ConfigureLevel.System,
        "system" => ConfigureLevel.System,
        "Configurable" => ConfigureLevel.Configurable,
        "configurable" => ConfigureLevel.Configurable,
        _ => ConfigureLevel.System
      };
      switch (valueSpaceType.GetString())
      {
        case "Code":
        case "code":
          var items = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonObject.GetProperty("conditions").GetRawText()) ?? new Dictionary<string, string>();
          var codeValueSpace = new CodeValueSpace(name, title, configureLevel, items);
          return codeValueSpace;

        case "Regex":
        case "regex":
          var regexes = JsonSerializer.Deserialize<List<string>>(jsonObject.GetProperty("conditions").GetRawText()) ?? new List<string>();
          var regexValueSpace = new RegexValueSpace(name, title, configureLevel, regexes);
          return regexValueSpace;

        case "Range":
        case "range":
          var ranges = JsonSerializer.Deserialize<Dictionary<string, float>>(jsonObject.GetProperty("conditions").GetRawText()) ?? new Dictionary<string, float>();
          var rangeValueSpace = new RangeValueSpace(name, title, configureLevel, ranges);
          return rangeValueSpace;

        default:
          throw new JsonException("ValueSpace 解析失败");
      }
    }

    public override void Write(Utf8JsonWriter writer, ValueSpace value, JsonSerializerOptions options)
    {
      switch (value.ValueSpaceType)
      {
        case ValueSpaceType.Code:
          JsonSerializer.Serialize(writer, value as CodeValueSpace, typeof(CodeValueSpace), options);
          break;

        case ValueSpaceType.Regex:
          JsonSerializer.Serialize(writer, value as RegexValueSpace, typeof(RegexValueSpace), options);
          break;

        case ValueSpaceType.Range:
          JsonSerializer.Serialize(writer, value as RangeValueSpace, typeof(RangeValueSpace), options);
          break;
      }
    }
  }
}