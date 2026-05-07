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
    [JsonSerializable(typeof(List<SuperCandlesOrderStats5mDTO>))]
    [JsonSerializable(typeof(List<SuperCandlesFuturesTradeStats5mDTO>))]
    [JsonSerializable(typeof(List<SuperCandlesFuturesOrderBookStats5mDTO>))]
    [JsonSerializable(typeof(List<FutoiDTO>))]
    [JsonSerializable(typeof(List<Hi2AssetDTO>))]
    [JsonSerializable(typeof(List<Hi2FuturesDTO>))]
    [JsonSerializable(typeof(List<MegaAlertsDTO>))]
    public partial class AppJsonContext: JsonSerializerContext
    {
        

    }
}
