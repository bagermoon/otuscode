set search_path to main;

-- нужны внешние ключи и индексы на них

create table if not exists t_restaurantinterior(
    id_restaurantinterior serial not null,
    image varchar(500),
    id_restaurant integer not null,
    constraint pkt_restaurantinterior primary key (id_restaurantinterior),
    constraint ut_restaurantinterior_id_restaurant_image unique (id_restaurant,image),
    constraint fkt_restaurantinterior_t_restaurant foreign key (id_restaurant) references t_restaurant(id_restaurant)
    );
    create index if not exists it_restaurantinterior_id_restaurant on t_restaurantinterior(id_restaurant);
    create unique index if not exists it_restaurantinterior_id_restaurant_image on t_restaurantinterior(id_restaurant,image);

commit;
