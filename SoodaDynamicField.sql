create table SoodaDynamicField (
    class varchar(32) not null,
    field varchar(32) not null,
    type varchar(32) not null,
    nullable int not null,
    fieldsize int null,
    precision int null,
    constraint PK_SoodaDynamicField primary key (class, field)
);

-- permissions for SQL Server user "soodatest"
grant create table, references to soodatest
