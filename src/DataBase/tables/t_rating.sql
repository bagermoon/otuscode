set search_path to main, public;

-- нужно ограничение по уникальности + внешние ключи и индексы
create table if not exists t_rating(
    id_rating bigserial not null,
    datetime timestamp with time zone not null,
    value integer,
    id_restaurant integer not null,
    id_user integer not null,
    constraint pkt_rating primary key (id_rating));

commit;
