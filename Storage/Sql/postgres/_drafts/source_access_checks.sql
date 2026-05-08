-- Draft DDL. Not executed automatically. Will be converted to migrations later.

create table source_access_checks
(
    id uuid primary key,
    source_code text not null,
    access_status text not null,
    checked_at_utc timestamptz not null,
    response_details text null,
    created_at_utc timestamptz not null
);