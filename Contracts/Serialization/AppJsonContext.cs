using History_DataMoex.Contracts.Dto.Calendar;
using History_DataMoex.Contracts.Dto.Algopack;
using History_DataMoex.Contracts.Dto.Iss;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace History_DataMoex.Contracts.Serialization
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

    [JsonSerializable(typeof(FuturesSecurityDTO))]
    [JsonSerializable(typeof(StockSecurityDTO))]
    [JsonSerializable(typeof(CandlesDTO))]
    [JsonSerializable(typeof(SuperCandlesTradeStats5mDTO))]
    [JsonSerializable(typeof(SuperCandlesOrderBookStats5mDTO))]
    [JsonSerializable(typeof(SuperCandlesOrderStats5mDTO))]
    [JsonSerializable(typeof(SuperCandlesFuturesTradeStats5mDTO))]
    [JsonSerializable(typeof(SuperCandlesFuturesOrderBookStats5mDTO))]
    [JsonSerializable(typeof(FutoiDTO))]
    [JsonSerializable(typeof(Hi2AssetDTO))]
    [JsonSerializable(typeof(Hi2FuturesDTO))]
    [JsonSerializable(typeof(MegaAlertsAssetsDTO))]
    [JsonSerializable(typeof(MegaAlertsFuturesDTO))]

    // DTO календаря
    [JsonSerializable(typeof(CalendarOffDaysAllDTO))]
    [JsonSerializable(typeof(CalendarOffDaysMarketDTO))]
    [JsonSerializable(typeof(CalendarStockSessionDTO))]
    [JsonSerializable(typeof(CalendarFuturesSessionDTO))]
    [JsonSerializable(typeof(CalendarSessionTypeDTO))]
    [JsonSerializable(typeof(CalendarFortsContractDTO))]
    [JsonSerializable(typeof(CalendarOptionsSeriesDTO))]
    [JsonSerializable(typeof(CalendarSuspendedDTO))]
    [JsonSerializable(typeof(CalendarSuspendedReasonDTO))]
    [JsonSerializable(typeof(CalendarSecurityChangeDTO))]
    [JsonSerializable(typeof(CalendarSecurityAttributeDTO))]

    public partial class AppJsonContext : JsonSerializerContext
    {
    }
}