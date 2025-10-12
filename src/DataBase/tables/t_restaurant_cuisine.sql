set search_path to main;

-- нужно ограничение по уникальности + внешние ключи и индексы
create table if not exists t_restaurant_cuisine(
    id_restaurant_cuisine bigserial not null,
    id_restaurant integer not null,
    id_cuisine integer not null,
    constraint pkt_restaurant_cuisine primary key (id_restaurant_cuisine),
    constraint ut_restaurant_cuisine_id_restaurant_id_cuisine unique (id_restaurant,id_cuisine),
    constraint fkt_restaurant_cuisine_t_restaurant foreign key (id_restaurant) references t_restaurant(id_restaurant),
    constraint fkt_restaurant_cuisine_t_cuisine foreign key (id_cuisine) references t_cuisine(id_cuisine)
    );
    create index if not exists it_restaurant_cuisine_id_restaurant on t_restaurant_cuisine(id_restaurant);
    create index if not exists it_restaurant_cuisine_id_cuisine on t_restaurant_cuisine(id_cuisine);
    create unique index if not exists it_restaurant_cuisine_id_restaurant_id_cuisine on t_restaurant_cuisine(id_restaurant,id_cuisine);

commit;
