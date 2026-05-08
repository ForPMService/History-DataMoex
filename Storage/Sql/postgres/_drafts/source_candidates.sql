-- Draft DDL. Not executed automatically. Will be converted to migrations later.

create table source_candidates
(
    id uuid primary key,
    data_need_code text not null,
    source_code text not null,
    research_status text not null,
    source_priority integer null,
    notes text null,
    created_at_utc timestamptz not null
);