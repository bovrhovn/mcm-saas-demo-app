create table McUsers
(
    McUserId int identity
        constraint McUsers_pk
            primary key,
    Fullname nvarchar(max),
    Email    nvarchar(max) not null,
    Password nvarchar(200) not null,
    IsAdmin  bit default 0
)
    go

create table Packages
(
    PackageId   int identity
        constraint Packages_pk
            primary key,
    Name        nvarchar(max)   not null,
    Description nvarchar(max),
    Price       float default 0 not null
)
    go

create table McUser2Packages
(
    McUserId  int      not null
        constraint McUser2Packages_McUsers_McUserId_fk
            references McUsers,
    PackageId int      not null
        constraint McUser2Packages_Packages_PackageId_fk
            references Packages,
    CreatedAt datetime not null,
    constraint McUser2Packages_pk
        primary key (McUserId, PackageId)
)
    go

