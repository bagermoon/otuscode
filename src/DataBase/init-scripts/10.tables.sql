\c "Restaurants"

set search_path to main;

create table if not exists t_user(
    id_user serial not null,
    name varchar(500) not null,
    constraint pkt_user primary key (id_user),
    constraint ut_user_name unique (name)
    );
create unique index if not exists it_user_name on t_user(name);

alter table t_user owner to master;

create table if not exists t_category(
    id_category serial not null,
    name varchar(500) not null,
    constraint pkt_category primary key (id_category),
    constraint ut_category_name unique (name)
    );
create unique index if not exists it_category_name on t_category(name);

alter table t_category owner to master;

create table if not exists t_cuisine(
    id_cuisine serial not null,
    name varchar(500) not null,
    constraint pkt_cuisine primary key (id_cuisine),
    constraint ut_cuisine_name unique (name)
    );
create unique index if not exists it_cuisine_name on t_cuisine(name);

alter table t_cuisine owner to master;

create table if not exists t_restaurant(
    id_restaurant serial not null,
    name text not null,
    logo_image varchar(500),
    description text not null,
    address text not null,
    average_rating numeric(4,2),
    id_category integer,
    price_range numeric(10,2),
    constraint pkt_restaurant primary key (id_restaurant),
    constraint ut_restaurant_name unique (name),
    constraint fkt_restaurant_t_category foreign key (id_category) references t_category(id_category)
    );
alter table t_restaurant owner to master;

create index if not exists it_restaurant_id_category on t_restaurant(id_category);

create unique index if not exists it_restaurant_name on t_restaurant(name);

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

alter table t_comment owner to master;

create index if not exists it_comment_id_restaurant on t_comment(id_restaurant);

create index if not exists it_comment_id_user on t_comment(id_user);

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
alter table t_rating owner to master;

create index if not exists it_rating_id_restaurant on t_rating(id_restaurant);
create index if not exists it_rating_id_user on t_rating(id_user);
create unique index if not exists it_rating_id_restaurant_id_user on t_rating(id_restaurant,id_user);

create table if not exists t_restaurantinterior(
    id_restaurantinterior serial not null,
    image varchar(500),
    id_restaurant integer not null,
    constraint pkt_restaurantinterior primary key (id_restaurantinterior),
    constraint ut_restaurantinterior_id_restaurant_image unique (id_restaurant,image),
    constraint fkt_restaurantinterior_t_restaurant foreign key (id_restaurant) references t_restaurant(id_restaurant)
    );

alter table t_restaurantinterior owner to master;

create index if not exists it_restaurantinterior_id_restaurant on t_restaurantinterior(id_restaurant);
create unique index if not exists it_restaurantinterior_id_restaurant_image on t_restaurantinterior(id_restaurant,image);

create table if not exists t_restaurant_cuisine(
    id_restaurant_cuisine bigserial not null,
    id_restaurant integer not null,
    id_cuisine integer not null,
    constraint pkt_restaurant_cuisine primary key (id_restaurant_cuisine),
    constraint ut_restaurant_cuisine_id_restaurant_id_cuisine unique (id_restaurant,id_cuisine),
    constraint fkt_restaurant_cuisine_t_restaurant foreign key (id_restaurant) references t_restaurant(id_restaurant),
    constraint fkt_restaurant_cuisine_t_cuisine foreign key (id_cuisine) references t_cuisine(id_cuisine)
    );
alter table t_restaurant_cuisine owner to master;
create index if not exists it_restaurant_cuisine_id_restaurant on t_restaurant_cuisine(id_restaurant);
create index if not exists it_restaurant_cuisine_id_cuisine on t_restaurant_cuisine(id_cuisine);
create unique index if not exists it_restaurant_cuisine_id_restaurant_id_cuisine on t_restaurant_cuisine(id_restaurant,id_cuisine);

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
alter table t_viewlog owner to master;
create index if not exists it_viewlog_id_restaurant on t_viewlog(id_restaurant);
create index if not exists it_viewlog_id_user on t_viewlog(id_user);
create unique index if not exists it_viewlog_id_restaurant_id_user on t_viewlog(id_restaurant,id_user);

create table if not exists t_visitor_profile(
    id_visitor_profile serial not null,
    fio varchar(1000),
    id_user integer not null,
    average_rating numeric(4,2),
    constraint pkt_visitor_profile primary key (id_visitor_profile),
    constraint ut_visitor_profile_id_user unique (id_user),
    constraint fkt_visitor_profile_t_user foreign key (id_user) references t_user(id_user)
    );
alter table t_visitor_profile owner to master;
create unique index if not exists it_visitor_profile_id_user on t_visitor_profile(id_user);
