-- Draft DDL. Not executed automatically. Will be converted to ClickHouse migrations later.
-- Naming and columns may be adjusted later when integrating with ProjectTraiding target tables.

create table tradestats_5m
(
    instrument_id UUID,
    bucket_start_utc DateTime64(3, 'UTC'),
    price_open Nullable(Float64),
    price_high Nullable(Float64),
    price_low Nullable(Float64),
    price_close Nullable(Float64),
    price_std Nullable(Float64),
    volume Nullable(Float64),
    value Nullable(Float64),
    trades Nullable(Int32),
    source_code LowCardinality(String),
    row_hash String,
    raw_object_id UUID,
    load_job_id UUID,
    version UInt64,
    inserted_at DateTime64(3, 'UTC')
)
engine = ReplacingMergeTree(inserted_at)
partition by toYYYYMM(bucket_start_utc)
order by (instrument_id, bucket_start_utc, source_code, version);