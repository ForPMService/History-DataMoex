using System.Text.Json;
using System.Text.Json.Serialization;
namespace History_DataMoex.Parsing
{
    [JsonSerializable(typeof(List<FuturesSecurityDTO>))]
    [JsonSerializable(typeof(List<StockSecurityDTO>))]
    public partial class AppJsonContext: JsonSerializerContext
    {
        

    }
}
