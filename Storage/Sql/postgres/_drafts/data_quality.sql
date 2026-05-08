-- Draft DDL. Not executed automatically. Will be converted to migrations later.

create table data_quality
(
    id uuid primary key,
    job_id uuid null,
    instrument_id uuid null,
    source_code text null,
    data_need_code text null,
    quality_status text not null,
    issue_code text null,
    details text null,
    created_at_utc timestamptz not null
);