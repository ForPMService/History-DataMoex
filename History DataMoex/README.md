# History-DataMoex

## Что это

History-DataMoex — .NET 10 модуль для получения исторических рыночных данных из MOEX ISS, MOEX ALGOPACK и MOEX Calendar.

Модуль запрашивает данные у MOEX, парсит ответы и возвращает структурированные объекты. Это исследовательский модуль для фиксации source contract MOEX — не витрина данных. Хранения и фоновой загрузки нет. Целевой контур витрины — ProjectTraiding.

## Что работает

### Данные

- Справочные данные MOEX ISS (акции TQBR, фьючерсы RFUD)
- Свечи MOEX ALGOPACK (минутные, акции и фьючерсы)
- Расширенная статистика MOEX ALGOPACK (TradeStats, OBStats, OrderStats — акции и фьючерсы)
- FUTOI (открытый интерес по фьючерсам в разрезе групп клиентов)
- HI2 (индекс концентрации рынка, акции и фьючерсы)
- Mega Alerts (акции и фьючерсы)
- Календарь MOEX (выходные, сессии, контракты FORTS, опционные серии, приостановки, изменения инструментов)

### Парсинг

Все 22 парсера работают через `Utf8JsonReader` без аллокаций (`ParsingAlgUtf8`, `ParsingIssUtf8`, `ParsingCalendarUtf8`). Каждый парсер принимает `ReadOnlySpan<byte>` и использует `ExpectedSchema` для валидации `columns[]` — при несовпадении имени или количества колонок бросается `MoexSchemaMismatchException`. Старые `JsonDocument`-парсеры закомментированы с тегом `HISTORICAL` и из живого кода не вызываются.

### Streaming

Все endpoint'ы возвращают `IAsyncEnumerable<DTO>`. ASP.NET сериализует поэлементно через `AppJsonContext` (source-generated). Клиент получает первые байты (TTFB) за 30–500 мс вне зависимости от объёма данных. Working set стабилен на протяжении всего прогона.

### ArrayPool

HTTP body читается через `RentedBuffer` — `IDisposable`-обёртка над `ArrayPool<byte>.Shared`. Это убирает LOH-аллокации на больших ответах. Результат замеров (прогон #5 vs #4): LOH с 5.4 МБ до 64 байт, gen2 collections -72%, total_allocated -24%, ускорение endpoint'ов со страницами >85 КБ на 10–49%.

### Пагинация

Три стратегии пагинации MOEX:

- **Cursor** (`data.cursor`, `suspended.cursor`, `securities.cursor`) — единый helper `MoexCursorPagination.Next` с тремя причинами остановки: `empty_cursor`, `range_exhausted`, `safety_cap_hit`. Защита от бесконечного цикла через `MaxPagesPerLoad = 10000`.
- **FixedPage500** — свечи ISS (страница 500 строк).
- **Без пагинации** — FUTOI (лимит 1000 строк, `start`/`offset` игнорируются MOEX). Решение: подневная разбивка диапазона в `StreamFutoi`.

### HTTP-инфраструктура

- `SocketsHttpHandler` (decompression, connection pooling, `MaxConnectionsPerServer = 32`)
- `StandardResilienceHandler` через Polly (TotalRequestTimeout = 10 мин, AttemptTimeout = 2 мин, CircuitBreaker = 5 мин)
- `HttpCompletionOption.ResponseHeadersRead` во всех клиентах
- `CancellationToken` во всех клиентах и endpoint'ах
- JSON-сериализация через source-generated `AppJsonContext`
- Настройки для Native AOT (`PublishAot`, `JsonSerializerIsReflectionEnabledByDefault = false`)

## Что подготовлено, но не реализовано

Архитектурный каркас — интерфейсы, модели и черновые SQL-файлы:

- `Normalization/Models` — внутренние модели витрины (Candle1m, TradeStats5m и др.)
- `Normalization/Mappers` — интерфейс преобразования DTO во внутренние модели
- `Ingestion/Pipeline` — интерфейсы загрузки
- `Storage/Abstractions` — интерфейсы для PostgreSQL и ClickHouse
- `Storage/Sql/postgres/_drafts` — черновые SQL для PostgreSQL
- `Storage/Sql/clickhouse/_drafts` — черновые SQL для ClickHouse
- `Queue/Abstractions` — интерфейсы очереди для Redis Streams
- `Contracts/Ingestion` — модели задач загрузки

## Что не закрыто

- Typed errors (классификация 429/401/5xx, `MoexHttpException` не бросается клиентами)
- Структурированное логирование (нет `ILogger` в клиентах)
- Маппер DTO → canonical model (реализация)
- Raw object store (реализация)
- Документы source contract (E1–E7)
- Запись в PostgreSQL / ClickHouse
- Фоновая загрузка данных
- API витрины `/api/v1`

## Ручки

- `Endpoints/ReferenceEndpoints.cs` — справочники инструментов (акции, фьючерсы)
- `Endpoints/AlgopackEndpoints.cs` — свечи, расширенная статистика, FUTOI, HI2, Mega Alerts
- `Endpoints/CalendarEndpoints.cs` — календарь MOEX
- `Endpoints/DebugEndpoints.cs` — отладка columns-map и FUTOI raw

Все ручки идут в MOEX напрямую при каждом вызове и возвращают DTO MOEX. Это не готовый API витрины.

## Замеры производительности

5 прогонов 08–16.05.2026 зафиксированы в `performance-summary-all-runs.md`. Ключевые результаты последнего прогона (#5):

- 99% времени тяжёлых endpoint'ов — сетевой round-trip к MOEX
- TTFB 30–500 мс на любом объёме
- LOH: 64 байта (было 5.4 МБ)
- gen2 collections: 62 (было 225)
- Total allocated: 2.97 ГБ (было 3.9 ГБ)
- Working set: стабильный ~130–175 МБ

## Как запустить

```powershell
dotnet build ".\History DataMoex.csproj"
dotnet run --project ".\History DataMoex.csproj"
```

Адрес по умолчанию: `http://localhost:5025`

## Настройки

- `MoexIss:BaseUrl` — задаётся в `appsettings.json`
- `MoexAlg:BaseUrl` — задаётся в `appsettings.json`
- `MoexAlg:Key` — через user-secrets или переменные окружения; не коммитить

### Ключ MOEX ALGOPACK

```powershell
dotnet user-secrets set "MoexAlg:Key" "YOUR_MOEX_ALGOPACK_KEY"
```

Или переменная окружения:

```powershell
$env:MoexAlg__Key="YOUR_MOEX_ALGOPACK_KEY"
```

Справочные ISS-ручки работают без ключа. ALGOPACK и Calendar ручки требуют ключ.

## Архитектурное решение

См. [docs/adr/0001-architecture-tracks.md](docs/adr/0001-architecture-tracks.md)
