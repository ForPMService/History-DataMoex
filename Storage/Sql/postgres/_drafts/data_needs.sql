-- Draft DDL. Not executed automatically. Will be converted to migrations later.

create table data_needs
(
    id uuid primary key,
    data_need_code text not null,
    business_purpose text not null,
    priority text not null,
    research_status text not null,
    storage_target text not null,
    canonical_model text not null,
    created_at_utc timestamptz not null,
    unique (data_need_code)
);