set search_path to main, public;

-- нужны внешние ключи и индексы на них

create table if not exists t_visitor_profile(
    id_visitor_profile serial not null,
    fio varchar(1000),
    id_user integer not null,
    average_rating numeric(4,2),
    constraint pkt_visitor_profile primary key (id_visitor_profile));
commit;
