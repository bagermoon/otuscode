set search_path to main, public;

-- нужны внешние ключи и индексы на них

create table if not exists t_restaurant(
    id_restaurant serial not null,
    name text not null,
    logo_image varchar(500),
    description text not null,
    address text not null,
    average_rating numeric(4,2),
    id_category integer,
    price_range numeric(10,2),
    constraint pkt_restaurant primary key (id_restaurant));
commit;
