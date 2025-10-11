-- create table if not exists "Main"."TCategory" (
--     "CategoryId" bigserial not null,
--     "Name" text not null,
--     constraint "PKTCategory" primary key ("CategoryId")
--     );

set search_path to main, public;

-- нужно ограничение по уникальности
create table if not exists t_category(
    id_category serial not null,
    name varchar(500) not null,
    constraint pkt_category primary key (id_category));

commit;
