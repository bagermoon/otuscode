set search_path to main;

-- нужно ограничение по уникальности
create table if not exists t_cuisine(
    id_cuisine serial not null,
    name varchar(500) not null,
    constraint pkt_cuisine primary key (id_cuisine),
    constraint ut_cuisine_name unique (name)
    );
    create unique index if not exists it_cuisine_name on t_cuisine(name);

commit;

