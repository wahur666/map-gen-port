using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MapGen;

public class BT_MAP_GEN_InfoConverter: JsonConverter {
	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
		throw new NotImplementedException();
	}

	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) {
		Info result = new Info();
        
		if (reader.TokenType == JsonToken.StartObject)
		{
			// It's a complex object - likely terrainInfo
			JObject jObject = JObject.Load(reader);
            
			// If it has terrainArchType, it's probably a terrainInfo object
			if (jObject["terrainArchType"] != null)
			{
				result.terrainInfo = jObject.ToObject<TerrainInfo>(serializer);
			}
		}
		else if (reader.TokenType == JsonToken.Integer || reader.TokenType == JsonToken.Float)
		{
			// It's a number - treat as overlap
			result.overlap = (DMapGen.OVERLAP)Convert.ToInt32(reader.Value);
		}
		else
		{
			throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing _info");
		}
        
		return result;
	}

	public override bool CanConvert(Type objectType) {
		return objectType == typeof(Info);
	}
}
