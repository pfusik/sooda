create table SoodaDynamicField (
    class varchar(32) not null,
    field varchar(32) not null,
    type varchar(32) not null,
    nullable int not null,
    size int null,
    precision int null,
    constraint PK_SoodaDynamicField primary key (class, field)
);
grant create table to soodatest
