-- Draft DDL. Not executed automatically. Will be converted to migrations later.

create table trading_calendar
(
    id uuid primary key,
    board_id text not null,
    trade_date date not null,
    is_trading_day boolean not null,
    session_type text null,
    time_from_utc time null,
    time_till_utc time null,
    created_at_utc timestamptz not null
);