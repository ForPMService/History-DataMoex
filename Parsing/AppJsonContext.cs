using History_DataMoex.DataTransfers;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace History_DataMoex.Parsing
{
    [JsonSerializable(typeof(List<FuturesSecurityDTO>))]
    [JsonSerializable(typeof(List<StockSecurityDTO>))]
    [JsonSerializable(typeof(List<CandlesDTO>))]
    [JsonSerializable(typeof(List<SuperCandlesTradeStats5mDTO>))]
    [JsonSerializable(typeof(List<SuperCandlesOrderBookStats5mDTO>))]
    public partial class AppJsonContext: JsonSerializerContext
    {
        

    }
}
