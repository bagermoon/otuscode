set search_path to main;

-- нужны внешние ключи и индексы на них

create table if not exists t_viewlog(
    id_viewlog bigserial not null,
    datetime timestamp with time zone not null,
    id_restaurant integer not null,
    id_user integer not null,
    constraint pkt_viewlog primary key (id_viewlog),
    constraint ut_viewlog_id_restaurant_id_user unique (id_restaurant,id_user),
    constraint fkt_viewlog_t_restaurant foreign key (id_restaurant) references t_restaurant(id_restaurant),
    constraint fkt_viewlog_t_user foreign key (id_user) references t_user(id_user)
    );
    create index if not exists it_viewlog_id_restaurant on t_viewlog(id_restaurant);
    create index if not exists it_viewlog_id_user on t_viewlog(id_user);
    create unique index if not exists it_viewlog_id_restaurant_id_user on t_viewlog(id_restaurant,id_user);
    
commit;
