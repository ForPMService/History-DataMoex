-- Draft DDL. Not executed automatically. Will be converted to migrations later.

create table raw_objects
(
    raw_object_id uuid primary key,
    source_code text not null,
    data_need_code text not null,
    instrument_id uuid null,
    request_json jsonb not null,
    response_meta_json jsonb not null,
    sha256 text not null,
    storage_uri text not null,
    size_bytes bigint not null,
    received_at_utc timestamptz not null,
    created_at_utc timestamptz not null
);