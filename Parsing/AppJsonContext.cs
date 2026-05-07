using History_DataMoex.DataTransfers;
using History_DataMoex.DataTransfers.Calendar;
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
    [JsonSerializable(typeof(List<MegaAlertsAssetsDTO>))]
    [JsonSerializable(typeof(List<MegaAlertsFuturesDTO>))]

    // Списки DTO календаря
    [JsonSerializable(typeof(List<CalendarOffDaysAllDTO>))]
    [JsonSerializable(typeof(List<CalendarOffDaysMarketDTO>))]
    [JsonSerializable(typeof(List<CalendarStockSessionDTO>))]
    [JsonSerializable(typeof(List<CalendarFuturesSessionDTO>))]
    [JsonSerializable(typeof(List<CalendarSessionTypeDTO>))]
    [JsonSerializable(typeof(List<CalendarFortsContractDTO>))]
    [JsonSerializable(typeof(List<CalendarOptionsSeriesDTO>))]
    [JsonSerializable(typeof(List<CalendarSuspendedDTO>))]
    [JsonSerializable(typeof(List<CalendarSuspendedReasonDTO>))]
    [JsonSerializable(typeof(List<CalendarSecurityChangeDTO>))]
    [JsonSerializable(typeof(List<CalendarSecurityAttributeDTO>))]

    public partial class AppJsonContext: JsonSerializerContext
    {
        

    }
}
