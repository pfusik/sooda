create table SoodaDynamicField (
    class varchar(32) not null,
    field varchar(32) not null,
    type varchar(32) not null,
    nullable int not null,
    fieldsize int null,
    precision int null,
    constraint PK_SoodaDynamicField primary key (class, field)
);

grant create table to soodatest
grant references to soodatest -- SQL Server only
