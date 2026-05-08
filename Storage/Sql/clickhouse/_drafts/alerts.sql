-- Draft DDL. Not executed automatically. Will be converted to ClickHouse migrations later.
-- Naming and columns may be adjusted later when integrating with ProjectTraiding target tables.

create table alerts
(
    instrument_id UUID,
    event_time_utc DateTime64(3, 'UTC'),
    alert_type String,
    severity Nullable(String),
    description Nullable(String),
    source_code LowCardinality(String),
    row_hash String,
    raw_object_id UUID,
    load_job_id UUID,
    version UInt64,
    inserted_at DateTime64(3, 'UTC')
)
engine = ReplacingMergeTree(inserted_at)
partition by toYYYYMM(event_time_utc)
order by (instrument_id, event_time_utc, alert_type, source_code, version);