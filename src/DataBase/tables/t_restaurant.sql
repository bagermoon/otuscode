set search_path to main;

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
    constraint pkt_restaurant primary key (id_restaurant),
    constraint ut_restaurant_name unique (name),
    constraint fkt_restaurant_t_category foreign key (id_category) references t_category(id_category)
    );

    create index if not exists it_restaurant_id_category on t_restaurant(id_category);

    create unique index if not exists it_restaurant_name on t_restaurant(name);

commit;
