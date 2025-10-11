set search_path to main, public;

-- нужны внешние ключи и индексы на них

create table if not exists t_viewlog(
    id_viewlog bigserial not null,
    datetime timestamp with time zone not null,
    id_restaurant integer not null,
    id_user integer not null,
    constraint pkt_viewlog primary key (id_viewlog));
    
commit;
