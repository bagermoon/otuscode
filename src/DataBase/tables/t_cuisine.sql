set search_path to main, public;

-- нужно ограничение по уникальности
create table if not exists t_cuisine(
    id_cuisine serial not null,
    name varchar(500) not null,
    constraint pkt_cuisine primary key (id_cuisine));

commit;

