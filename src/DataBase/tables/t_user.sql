set search_path to main;

-- нужно ограничение по уникальности
create table if not exists t_user(
    id_user serial not null,
    name varchar(500) not null,
    constraint pkt_user primary key (id_user),
    constraint ut_user_name unique (name)
    );
    create unique index if not exists it_user_name on t_user(name);

commit;
