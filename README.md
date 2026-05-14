# History-DataMoex

## Что это

History-DataMoex — .NET 10 модуль для получения исторических рыночных данных из MOEX ISS, MOEX ALGOPACK и MOEX Calendar.

Сейчас модуль умеет запрашивать данные у MOEX, разбирать ответы и возвращать структурированные объекты. Это ещё не витрина данных — хранения и фоновой загрузки пока нет.

## Что уже работает

- Справочные данные MOEX ISS (акции, фьючерсы)
- Свечи MOEX ALGOPACK (минутные, акции и фьючерсы)
- Расширенная статистика MOEX ALGOPACK (TradeStats, OBStats, OrderStats для акций и фьючерсов)
- FUTOI (открытый интерес по фьючерсам в разрезе групп клиентов)
- HI2 (индекс концентрации рынка для акций и фьючерсов)
- Mega Alerts (акции и фьючерсы)
- Календарь MOEX (выходные, сессии, контракты FORTS, опционные серии, приостановки, изменения инструментов)
- Все парсеры работают через Utf8JsonReader без аллокаций (старые JsonDocument-парсеры закомментированы, сохранены для аудита)
- JSON-сериализация через генератор кода `AppJsonContext`
- Настройки для компиляции в Native AOT

## Что подготовлено, но ещё не подключено

Архитектурный каркас — интерфейсы, модели и черновые SQL-файлы без реализации:

- `Contracts/Ingestion` — модели задач загрузки, сырых объектов, инструментов, потребностей в данных, источников
- `Normalization/Models` — внутренние модели витрины (Candle1m, TradeStats5m, ObStats5m, OrderStats5m, Futoi, Hi2, Alert, TradingCalendarEntry)
- `Normalization/Mappers` — интерфейс преобразования DTO MOEX во внутренние модели
- `Ingestion/Pipeline` — интерфейсы загрузки (IHistoricalLoadJob, ILoadJobRunner, IRawJsonStore, IInstrumentLookup)
- `Storage/Abstractions` — 11 интерфейсов для работы с базами данных (репозитории справочников + записи временных рядов)
- `Storage/Sql/postgres/_drafts` — 9 черновых SQL-файлов для PostgreSQL
- `Storage/Sql/clickhouse/_drafts` — 7 черновых SQL-файлов для ClickHouse с колонкой `row_hash`
- `Queue/Abstractions` — 4 интерфейса очереди задач для будущего подключения Redis Streams
- `Queue/Models` — 3 модели сообщений очереди
- `Parsing/ColumnIndex/MoexColumnIndexResolver` — подготовлен для перевода парсеров на безопасный разбор колонок

## Ручки

- `Endpoints/ReferenceEndpoints.cs` — справочники инструментов (акции, фьючерсы)
- `Endpoints/AlgopackEndpoints.cs` — свечи, расширенная статистика, FUTOI, HI2, Mega Alerts
- `Endpoints/CalendarEndpoints.cs` — календарь MOEX

Эти ручки при каждом вызове идут в MOEX напрямую и возвращают DTO MOEX. Это не готовый API витрины — он появится позже, когда заработают хранение и фоновая загрузка.

## Что ещё не сделано

- Запись данных в PostgreSQL
- Запись временных рядов в ClickHouse
- Очередь задач через Redis Streams
- Хранение сырых ответов в MinIO
- Фоновая загрузка данных
- API витрины `/api/v1`
- Повторные попытки и ограничение частоты запросов к MOEX

## Как запустить

```powershell
dotnet build ".\History DataMoex.csproj"
dotnet run --project ".\History DataMoex.csproj"
```

Адрес по умолчанию: `http://localhost:5025`

## Настройки

- `MoexIss:BaseUrl` — задаётся в `appsettings.json`
- `MoexAlg:BaseUrl` — задаётся в `appsettings.json`
- `MoexAlg:Key` — передавать через user-secrets или переменные окружения; не коммитить настоящие ключи

### Ключ MOEX ALGOPACK

Реальный `MoexAlg:Key` нельзя хранить в `appsettings.json` и коммитить в Git.

Для локальной разработки используй user-secrets:

```powershell
dotnet user-secrets set "MoexAlg:Key" "YOUR_MOEX_ALGOPACK_KEY"
```

Или переменную окружения:

```powershell
$env:MoexAlg__Key="YOUR_MOEX_ALGOPACK_KEY"
```

Справочные ISS-ручки могут работать без `MoexAlg:Key`.

ALGOPACK-ручки и календарные ручки используют `MoexAlg:Key`. Если ключ не задан, при вызове этих ручек будет явная ошибка конфигурации.

## Архитектурное решение

См. [docs/adr/0001-architecture-tracks.md](docs/adr/0001-architecture-tracks.md)
