-- Draft DDL. Not executed automatically. Will be converted to ClickHouse migrations later.
-- Naming and columns may be adjusted later when integrating with ProjectTraiding target tables.

create table orderstats_5m
(
    instrument_id UUID,
    bucket_start_utc DateTime64(3, 'UTC'),
    put_orders_buy Nullable(Int64),
    put_orders_sell Nullable(Int64),
    cancel_orders_buy Nullable(Int64),
    cancel_orders_sell Nullable(Int64),
    put_volume_buy Nullable(Int64),
    put_volume_sell Nullable(Int64),
    cancel_volume_buy Nullable(Int64),
    cancel_volume_sell Nullable(Int64),
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