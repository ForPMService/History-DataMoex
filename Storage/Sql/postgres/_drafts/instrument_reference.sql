-- Draft DDL. Not executed automatically. Will be converted to migrations later.
-- TODO: при интеграции с ProjectTraiding сверить с целевой таблицей instruments из ТЗ_001.

create table instrument_reference
(
    instrument_id uuid primary key,
    display_code text not null,
    sec_id text not null,
    board_id text not null,
    asset_code text null,
    isin text null,
    instrument_type text not null,
    is_active boolean not null,
    created_at_utc timestamptz not null
);