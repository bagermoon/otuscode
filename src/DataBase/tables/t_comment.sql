set search_path to main, public;

-- нужны внешние ключи и индексы на них

create table if not exists t_comment(
    id_comment bigserial not null,
    dtm_create timestamp with time zone not null,
    comment text,
    id_restaurant integer not null,
    id_user integer not null,
    constraint pkt_comment primary key (id_comment));
commit;
