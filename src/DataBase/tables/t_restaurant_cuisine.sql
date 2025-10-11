set search_path to main, public;

-- нужно ограничение по уникальности + внешние ключи и индексы
create table if not exists t_restaurant_cuisine(
    id_restaurant_cuisine bigserial not null,
    id_restaurant integer not null,
    id_cuisine integer not null,
    constraint pkt_restaurant_cuisine primary key (id_restaurant_cuisine));

commit;
