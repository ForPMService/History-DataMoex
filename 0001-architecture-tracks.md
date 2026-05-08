# ADR-0001: Архитектурные треки History-DataMoex

## Статус

Принято

## Контекст

History-DataMoex — отдельный .NET 10 модуль, который получает исторические рыночные данные из MOEX ISS, MOEX ALGOPACK и MOEX Calendar.

Что сделано после задач TASK-ARCH-001 — TASK-ARCH-009:

- Три HTTP-клиента (`MoexHttpIssClient`, `MoexHttpAlgClient`, `MoexHttpCalendarClient`) отправляют запросы в MOEX и возвращают DTO — объекты, повторяющие структуру ответа MOEX.
- Ручки в папке `Endpoints/` при каждом вызове идут в MOEX напрямую. Это технические ручки для проверки связи с источником, а не готовый API витрины данных.
- DTO лежат в `Contracts/Dto/`, пагинация в `Contracts/Pagination/`, настройка сериализации в `Contracts/Serialization/AppJsonContext.cs`.
- Модели загрузки в `Contracts/Ingestion/` — LoadJob, RawObject, Instrument, DataNeed, DataSource и другие.
- Внутренние модели витрины в `Normalization/Models/` — Candle1m, TradeStats5m, ObStats5m, OrderStats5m, Futoi, Hi2, Alert, TradingCalendarEntry, InstrumentRef.
- Интерфейс преобразования `IMoexMapper<TDto, TCanonical>` и контекст `MapContext` в `Normalization/Mappers/`.
- Каркас загрузки данных в `Ingestion/Pipeline/` и `Ingestion/Models/`.
- Каркас хранения: 11 интерфейсов в `Storage/Abstractions/`, 9 черновых SQL-файлов для PostgreSQL, 7 черновых SQL-файлов для ClickHouse с колонкой `row_hash`.
- Каркас очереди задач: 4 интерфейса в `Queue/Abstractions/`, 3 модели сообщений в `Queue/Models/` для будущего подключения Redis Streams.
- `MoexColumnIndexResolver` в `Parsing/ColumnIndex/` подготовлен, но существующие парсеры пока на него не переведены.
- `MoexHttpException` и `MoexSchemaMismatchException` описывают ошибки взаимодействия с MOEX.
- PostgreSQL, ClickHouse, Redis и MinIO не подключены. Интерфейсы хранения и очереди не зарегистрированы в DI. Фоновой загрузки нет. API витрины `/api/v1` нет.

## Решение

- History-DataMoex остаётся отдельным модулем на время архитектурного трека.
- Используется один .NET-проект с разделением по папкам.
- Ручки в `Endpoints/*` остаются техническими — «запросить MOEX и вернуть DTO». Строить на них интерфейс витрины нельзя.
- API витрины под `/api/v1` будет проектироваться после того, как заработают фоновая загрузка и запись в базы данных.
- Парсеры только преобразуют JSON от MOEX в DTO. Присвоение внутренних идентификаторов (`instrument_id`, `raw_object_id`, `job_id`), кода источника и UTC-времени будет происходить при реализации загрузки, а не в парсерах.
- PostgreSQL, ClickHouse, Redis и MinIO в архитектурном треке не реализуются. Сейчас есть только интерфейсы и черновые SQL-файлы.
- Каркас нужен, чтобы при переходе к реализации было понятно, куда класть конкретный код.

## Последствия

- Модуль MOEX можно развивать отдельно, не затрагивая будущие слои хранения и API витрины.
- Текущие ручки `/Get*` и `/calendar/*` нельзя использовать как готовый API для интерфейса.
- DTO от MOEX нельзя подставлять вместо внутренних моделей витрины.
- API витрины `/api/v1` нельзя начинать раньше, чем заработают хранение и загрузка.
- Названия таблиц в черновых SQL для ClickHouse (`candles_1m`, `tradestats_5m` и др.) могут отличаться от целевых таблиц ProjectTraiding (`market_candles` и др.) — это будет согласовано при объединении.
- Позже модуль можно будет перенести в общий контур ProjectTraiding через явные контракты.

## Когда пересмотреть

- Когда заработает первая запись свечей `candles_1m` в ClickHouse.
- Когда появятся управляющие таблицы в PostgreSQL.
- Когда появится фоновая загрузка данных.
- Перед проектированием API витрины `/api/v1`.
- Перед переносом модуля в общий репозиторий ProjectTraiding.
