-- Draft DDL. Not executed automatically. Will be converted to migrations later.

create table jobs
(
    id uuid primary key,
    job_type text not null,
    source_code text null,
    data_need_code text null,
    instrument_id uuid null,
    status text not null,
    parameters_json jsonb not null,
    attempts integer not null,
    progress_percent numeric not null,
    heartbeat_at timestamptz null,
    correlation_id uuid not null,
    created_at_utc timestamptz not null,
    started_at_utc timestamptz null,
    finished_at_utc timestamptz null,
    error_code text null,
    error_message text null
);