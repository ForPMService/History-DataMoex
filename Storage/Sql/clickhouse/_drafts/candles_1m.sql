-- Draft DDL. Not executed automatically. Will be converted to ClickHouse migrations later.
-- Naming and columns may be adjusted later when integrating with ProjectTraiding target tables.

create table candles_1m
(
    instrument_id UUID,
    bucket_start_utc DateTime64(3, 'UTC'),
    open Float64,
    high Float64,
    low Float64,
    close Float64,
    volume Float64,
    value Float64,
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