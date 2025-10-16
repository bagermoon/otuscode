set search_path to main;

-- нужны внешние ключи и индексы на них

create table if not exists t_visitor_profile(
    id_visitor_profile serial not null,
    fio varchar(1000),
    id_user integer not null,
    average_rating numeric(4,2),
    constraint pkt_visitor_profile primary key (id_visitor_profile),
    constraint ut_visitor_profile_id_user unique (id_user),
    constraint fkt_visitor_profile_t_user foreign key (id_user) references t_user(id_user)
    );
    create unique index if not exists it_visitor_profile_id_user on t_visitor_profile(id_user);

commit;
