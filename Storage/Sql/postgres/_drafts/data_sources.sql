-- Draft DDL. Not executed automatically. Will be converted to migrations later.

create table data_sources
(
    id uuid primary key,
    source_code text not null,
    display_name text not null,
    base_url text not null,
    auth_type text not null,
    rate_limit_json jsonb not null,
    created_at_utc timestamptz not null,
    unique (source_code)
);