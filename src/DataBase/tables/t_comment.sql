set search_path to main;

-- нужны внешние ключи и индексы на них

create table if not exists t_comment(
    id_comment bigserial not null,
    dtm_create timestamp with time zone not null,
    comment text,
    id_restaurant integer not null,
    id_user integer not null,
    constraint pkt_comment primary key (id_comment),
    constraint fkt_comment_t_restaurant foreign key (id_restaurant) references t_restaurant(id_restaurant),
    constraint fkt_comment_t_user foreign key (id_user) references t_user(id_user)
    );

    create index if not exists it_comment_id_restaurant on t_comment(id_restaurant);

    create index if not exists it_comment_id_user on t_comment(id_user);


commit;
