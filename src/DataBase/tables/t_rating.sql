set search_path to main;

-- нужно ограничение по уникальности + внешние ключи и индексы
create table if not exists t_rating(
    id_rating bigserial not null,
    datetime timestamp with time zone not null,
    value integer,
    id_restaurant integer not null,
    id_user integer not null,
    constraint pkt_rating primary key (id_rating),
    constraint fkt_rating_t_restaurant foreign key (id_restaurant) references t_restaurant(id_restaurant),
    constraint fkt_rating_t_user foreign key (id_user) references t_user(id_user),
    constraint ut_rating_id_restaurant_id_user unique (id_restaurant,id_user)
    );
    create index if not exists it_rating_id_restaurant on t_rating(id_restaurant);
    create index if not exists it_rating_id_user on t_rating(id_user);
    create unique index if not exists it_rating_id_restaurant_id_user on t_rating(id_restaurant,id_user);

commit;
