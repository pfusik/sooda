create database SoodaUnitTests 
/*
on primary (name='SoodaUnitTests', 
filename='***insert_path_here***\SoodaUnitTests.mdf', 
size=10MB) 
log on (name='SoodaUnitTests_log',
filename='***insert_path_here***\SoodaUnitTests_log.ldf',
size=10MB)
*/
go

-- create user for web application, giving it default access to SoodaUnitTests
-- database
exec sp_addlogin 'soodatest','PASS',SoodaUnitTests
go

use SoodaUnitTests;
go

create table KeyGen
(
	key_name varchar(64) primary key,
	key_value int not null
)
go

create table _Group
(
	id int primary key,
	name varchar(64) null,
	manager int not null
)
go

create table ContactType
(
	code varchar(16) primary key,
	description varchar(64)
)
go

create table Contact
(
	id int primary key,
	active int not null,
	last_salary decimal(20,10) null,
	name varchar(64) null,
	type varchar(16) not null references ContactType(code),
	primary_group int null references _Group(id),
	manager int null references Contact(id)
)
go

create table _Role
(
	id int primary key,
	name varchar(64) null
)
go

create table MultiKey
(
	contact_id int not null,
	group_id int not null,
    value int not null,
    value2 int not null,
    value3 int not null,
)
go

create table Vehicle
(
	id int primary key,
    type int not null,
	name varchar(64) null,
	abs varchar(64) null,
	four_wheel_drive varchar(64) null,
	owner int null references Contact(id)
)
go

create table Bike
(
	id int primary key,
    two_wheels int null
)
go

create table ExtendedBike
(
	id int primary key,
	extended_bike_info varchar(64) null
)
go

create table ContactRole
(
	contact_id int not null references Contact(id),
	role_id int not null references _Role(id)
)
go

create table ContactVehicle
(
	contact_id int not null references Contact(id),
	vehicle_id int not null references Vehicle(id)
)
go

create table ContactBike
(
	contact_id int not null references Contact(id),
	bike_id int not null references Vehicle(id)
)
go

create table AllDataTypes
(
	id int primary key not null,
	bool_val bit null,
	nn_bool_val bit not null,
	int_val int null,
	nn_int_val int not null,
	int64_val bigint null,
	nn_int64_val bigint not null,
	string_val varchar(64) null,
	nn_string_val varchar(64) not null,
	date_val datetime null,
	nn_date_val datetime not null,
	float_val real null,
	nn_float_val real not null,
	double_val float null,
	nn_double_val float not null,
	decimal_val decimal null,
	nn_decimal_val decimal not null
)
go

create table PKInt32
(
	id int primary key not null,
	data varchar(64) null,
	parent int null references PKInt32(id)
)
go

create table PKInt64
(
	id bigint primary key not null,
	data varchar(64) null,
	parent bigint null references PKInt64(id)
)
go

create table PKString
(
	id varchar(64) primary key not null,
	data varchar(64) null,
	parent varchar(64) null references PKString(id)
)
go

create table PKBool
(
	id bit primary key not null,
	data varchar(64) null,
	parent bit null references PKBool(id)
)
go

create table PKGuid
(
	id uniqueidentifier primary key not null,
	data varchar(64) null,
	parent uniqueidentifier null references PKGuid(id)
)
go

create table PKDateTime
(
	id datetime primary key not null,
	data varchar(64) null,
	parent datetime null references PKDateTime(id)
)
go

create table RelInt32ToString
(
	ll int not null references PKInt32(id),
	rr varchar(64) not null references PKString(id)
)
go

create table RelInt64ToDateTime
(
	ll bigint not null references PKInt64(id),
	rr datetime not null references PKDateTime(id)
)
go

create table RelStringToBool
(
	ll varchar(64) not null references PKString(id),
	rr bit not null references PKBool(id)
)
go

create table EightFields
(
	id int primary key not null,
	parent int null references EightFields(id),
	timespan int not null,
	timespan2 int null,
	guid uniqueidentifier not null,
	guid2 uniqueidentifier null,
	blob varbinary(max) not null,
	blob2 varbinary(max) null
)
go

print 'Inserting sample data...'
set nocount on

insert into ContactType values('Employee','Internal Employee');
insert into ContactType values('Manager','Internal Manager');
insert into ContactType values('Customer','External Contact');

insert into _Group values(10,'Group1',1);
insert into _Group values(11,'Group2',2);

insert into Contact values(1,1,123.123456789,'Mary Manager','Manager',10,null);
insert into Contact values(2,1,234.0,'Ed Employee','Employee',10,1);
insert into Contact values(3,1,345.0,'Eva Employee','Employee',11,1);

insert into Contact values(50,1,99.123,'Catie Customer','Customer',null,null);
insert into Contact values(51,1,3.14159,'Caroline Customer','Customer',10,null);
insert into Contact values(52,1,123,'Chris Customer','Customer',null,null);
insert into Contact values(53,1,-1,'Chuck Customer','Customer',10,null);

insert into _Role values(1,'Employee');
insert into _Role values(2,'Manager');
insert into _Role values(3,'Customer');

insert into ContactRole values(1,1);
insert into ContactRole values(1,2);

insert into ContactRole values(2,1);
insert into ContactRole values(2,2);

insert into ContactRole values(3,1);

insert into ContactRole values(50,3);
insert into ContactRole values(51,3);
insert into ContactRole values(52,3);
insert into ContactRole values(53,3);

insert into KeyGen values('Contact',100);
insert into KeyGen values('Group',100);
insert into KeyGen values('Vehicle',100);

insert into Vehicle values(1,1,'some vehicle',null,null,null);
insert into Vehicle values(2,1,'a car',null,null,1);
insert into Vehicle values(3,2,'a bike',null,null,2);
insert into Vehicle values(4,3,'a super-bike',null,null,null);
insert into Vehicle values(5,3,'another super-bike',null,null,null);
insert into Vehicle values(6,4,'mega super-bike',null,null,null);
insert into Vehicle values(10,7,'an extended bike',null,null,null);
insert into Vehicle values(11,5,'concrete bike 1',null,null,null);
insert into Vehicle values(12,6,'concrete bike 2',null,null,null);

insert into Bike values(3,1);
insert into Bike values(4,1);
insert into Bike values(5,1);
insert into Bike values(6,1);
insert into Bike values(10,1);

insert into MultiKey values (1,1,11,22,33);
insert into MultiKey values (2,3,456,789,123);
insert into MultiKey values (7,8,901,111,222);

insert into ExtendedBike values(10,'an extended bike info');

insert into AllDataTypes (id, nn_bool_val, nn_int_val, nn_int64_val, nn_string_val, nn_date_val, nn_float_val, nn_double_val, nn_decimal_val) values (1, 1, 42, 1234567890123, 'foo', '2014-07-23 13:05:47.123', 10.5, 20.5, 1337);

insert into PKInt32 values(7777777,'test data',7777777);
insert into PKInt32 values(7777778,'test data 2',7777777);
insert into PKInt32 values(7777779,'test data 3',7777777);

insert into PKInt64 values(77777777777777,'test data',77777777777777);
insert into PKInt64 values(77777777777778,'test data 2',77777777777777);
insert into PKInt64 values(77777777777779,'test data 3',77777777777777);

insert into PKBool values(1,'test data',1);
insert into PKBool values(0,'test data 2',1);

insert into PKDateTime values('2000/01/01 00:00:00','test data','2000/01/01 00:00:00');
insert into PKDateTime values('2000/01/01 01:00:00','test data 2','2000/01/01 00:00:00');
insert into PKDateTime values('2000/01/01 02:00:00','test data 3','2000/01/01 00:00:00');

insert into PKString values('zzzzzzz','test data','zzzzzzz');
insert into PKString values('xxxxxxx','test data 2','zzzzzzz');
insert into PKString values('yyyyyyy','test data 3','zzzzzzz');

insert into EightFields values (1, null, 3600, null, '757a29af-2bb2-4974-829a-a944cf741265', null, 0xf, null);
go

exec sp_grantdbaccess 'soodatest','soodatest'
go

print 'Granting table permissions...'

grant select,insert,update on KeyGen to soodatest
grant select,insert,update,delete on Contact to soodatest
grant select,insert,update,delete on _Group to soodatest
grant select,insert,update,delete on _Role to soodatest
grant select,insert,update,delete on ContactRole to soodatest
grant select,insert,update,delete on ContactBike to soodatest
grant select,insert,update,delete on ContactVehicle to soodatest
grant select,insert,update,delete on ContactType to soodatest
grant select,insert,update,delete on AllDataTypes to soodatest
grant select,insert,update,delete on PKInt32 to soodatest
grant select,insert,update,delete on PKInt64 to soodatest
grant select,insert,update,delete on PKBool to soodatest
grant select,insert,update,delete on PKGuid to soodatest
grant select,insert,update,delete on PKDateTime to soodatest
grant select,insert,update,delete on PKString to soodatest
grant select,insert,update,delete on Vehicle to soodatest
grant select,insert,update,delete on Bike to soodatest
grant select,insert,update,delete on ExtendedBike to soodatest
grant select,insert,update,delete on MultiKey to soodatest
grant select,insert,update,delete on EightFields to soodatest

go

print 'Enabling Dynamic Fields...'

create table SoodaDynamicField (
    class varchar(32) not null,
    field varchar(32) not null,
    type varchar(32) not null,
    nullable int not null,
    fieldsize int null,
    precision int null,
    constraint PK_SoodaDynamicField primary key (class, field)
);
grant select,insert,update,delete on SoodaDynamicField to soodatest
grant create table, references to soodatest

print 'Finished.'
