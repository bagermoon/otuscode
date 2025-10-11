set search_path to main, public;

-- нужны внешние ключи и индексы на них

create table if not exists t_restaurantinterior(
    id_restaurantinterior serial not null,
    image varchar(500),
    id_restaurant integer not null,
    constraint pkt_restaurantinterior primary key (id_restaurantinterior));
commit;
