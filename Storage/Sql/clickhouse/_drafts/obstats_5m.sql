-- Draft DDL. Not executed automatically. Will be converted to ClickHouse migrations later.
-- Naming and columns may be adjusted later when integrating with ProjectTraiding target tables.

create table obstats_5m
(
    instrument_id UUID,
    bucket_start_utc DateTime64(3, 'UTC'),
    spread Nullable(Float64),
    imbalance_volume Nullable(Float64),
    imbalance_value Nullable(Float64),
    bid_volume Nullable(Float64),
    ask_volume Nullable(Float64),
    bid_value Nullable(Float64),
    ask_value Nullable(Float64),
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