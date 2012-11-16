create table KeyGen
(
	key_name varchar(64) primary key,
	key_value int not null
);

create table _Group
(
	id int primary key,
	name varchar(64) null,
	manager int not null
);

create table ContactType
(
	code varchar(16) primary key,
	description varchar(64)
);

create table Contact
(
	id int primary key,
	active int not null,
	last_salary decimal(20,10) null,
	name varchar(64) null,
	type varchar(16) not null references ContactType(code),
	primary_group int null references _Group(id) on delete cascade,
	manager int null references Contact(id)
);

create table _Role
(
	id int primary key,
	name varchar(64) null
);

create table Vehicle
(
	id int primary key,
    type int not null,
	name varchar(64) null,
	abs varchar(64) null,
	four_wheel_drive varchar(64) null,
	owner int null references Contact(id)
);

create table Bike
(
	id int primary key,
    two_wheels int null
);

create table ExtendedBike
(
	id int primary key,
	extended_bike_info varchar(64) null
);

create table ContactRole
(
	contact_id int not null references Contact(id) on delete cascade,
	role_id int not null references _Role(id)
);

create table ContactVehicle
(
	contact_id int not null references Contact(id) on delete cascade,
	vehicle_id int not null references Vehicle(id)
);

create table ContactBike
(
	contact_id int not null references Contact(id) on delete cascade,
	bike_id int not null references Vehicle(id)
);

create table AllDataTypes
(
	id int primary key not null,
	bool_val boolean null,
	nn_bool_val boolean not null,
	int_val int null,
	nn_int_val int not null,
	int64_val bigint null,
	nn_int64_val bigint not null,
	string_val varchar(64) null,
	nn_string_val varchar(64) not null,
	date_val timestamp null,
	nn_date_val timestamp not null,
	float_val real null,
	nn_float_val real not null,
	double_val float null,
	nn_double_val float not null,
	decimal_val decimal null,
	nn_decimal_val decimal not null
);

create table PKInt32
(
	id int primary key not null,
	data varchar(64) null,
	parent int null references PKInt32(id)
);

create table PKInt64
(
	id bigint primary key not null,
	data varchar(64) null,
	parent bigint null references PKInt64(id)
);

create table PKString
(
	id varchar(64) primary key not null,
	data varchar(64) null,
	parent varchar(64) null references PKString(id)
);

create table PKBool
(
	id boolean primary key not null,
	data varchar(64) null,
	parent boolean null references PKBool(id)
);

/*

create table PKGuid
(
	id uniqueidentifier primary key not null,
	data varchar(64) null,
	parent uniqueidentifier null references PKGuid(id)
);

*/

create table PKDateTime
(
	id timestamp primary key not null,
	data varchar(64) null,
	parent timestamp null references PKDateTime(id)
);

create table RelInt32ToString
(
	ll int not null references PKInt32(id),
	rr varchar(64) not null references PKString(id)
);

create table RelInt64ToDateTime
(
	ll bigint not null references PKInt64(id),
	rr timestamp not null references PKDateTime(id)
);

create table RelStringToBool
(
	ll varchar(64) not null references PKString(id),
	rr boolean not null references PKBool(id)
);

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

insert into ExtendedBike values(10,'an extended bike info');

insert into PKInt32 values(7777777,'test data',7777777);
insert into PKInt32 values(7777778,'test data 2',7777777);
insert into PKInt32 values(7777779,'test data 3',7777777);

insert into PKInt64 values(77777777777777,'test data',77777777777777);
insert into PKInt64 values(77777777777778,'test data 2',77777777777777);
insert into PKInt64 values(77777777777779,'test data 3',77777777777777);

insert into PKBool values(true,'test data',true);
insert into PKBool values(false,'test data 2',true);

insert into PKDateTime values('2000/01/01 00:00:00','test data','2000/01/01 00:00:00');
insert into PKDateTime values('2000/01/01 01:00:00','test data 2','2000/01/01 00:00:00');
insert into PKDateTime values('2000/01/01 02:00:00','test data 3','2000/01/01 00:00:00');

insert into PKString values('zzzzzzz','test data','zzzzzzz');
insert into PKString values('xxxxxxx','test data 2','zzzzzzz');
insert into PKString values('yyyyyyy','test data 3','zzzzzzz');
