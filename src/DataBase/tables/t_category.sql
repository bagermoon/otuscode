-- create table if not exists "Main"."TCategory" (
--     "CategoryId" bigserial not null,
--     "Name" text not null,
--     constraint "PKTCategory" primary key ("CategoryId")
--     );

set search_path to main;

-- нужно ограничение по уникальности
create table if not exists t_category(
    id_category serial not null,
    name varchar(500) not null,
    constraint pkt_category primary key (id_category),
    constraint ut_category_name unique (name)
    );
    create unique index if not exists it_category_name on t_category(name);

commit;
