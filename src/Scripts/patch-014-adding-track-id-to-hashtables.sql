use FingerprintsDb
go
if not exists(select * from sys.columns 
   where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_1'))
begin
   	alter table HashTable_1 add TrackId int
end
go
if not exists(select * from sys.columns 
   where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_2'))
begin
   	alter table HashTable_2 add TrackId int
end
go
if not exists(select * from sys.columns 
   where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_3'))
begin
   	alter table HashTable_3 add TrackId int
end
go
if not exists(select * from sys.columns 
   where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_4'))
begin
   	alter table HashTable_4 add TrackId int
end
go
if not exists(select * from sys.columns 
   where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_5'))
begin
   	alter table HashTable_5 add TrackId int
end
go
if not exists(select * from sys.columns 
   where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_6'))
begin
   	alter table HashTable_6 add TrackId int
end
go
if not exists(select * from sys.columns 
   where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_7'))
begin
   	alter table HashTable_7 add TrackId int
end
go
if not exists(select * from sys.columns 
   where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_8'))
begin
   	alter table HashTable_8 add TrackId int
end
go
if not exists(select * from sys.columns 
   where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_9'))
begin
   	alter table HashTable_9 add TrackId int
end
go
if not exists(select * from sys.columns 
   where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_10'))
begin
   	alter table HashTable_10 add TrackId int
end
go
if not exists(select * from sys.columns 
   where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_11'))
begin
   	alter table HashTable_11 add TrackId int
end
go
if not exists(select * from sys.columns 
   where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_12'))
begin
   	alter table HashTable_12 add TrackId int
end
go
if not exists(select * from sys.columns 
   where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_13'))
begin
   	alter table HashTable_13 add TrackId int
end
go
if not exists(select * from sys.columns 
   where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_14'))
begin
   	alter table HashTable_14 add TrackId int
end
go
if not exists(select * from sys.columns 
   where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_15'))
begin
   	alter table HashTable_15 add TrackId int
end
go
if not exists(select * from sys.columns 
   where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_16'))
begin
   	alter table HashTable_16 add TrackId int
end
go
if not exists(select * from sys.columns 
   where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_17'))
begin
   	alter table HashTable_17 add TrackId int
end
go
if not exists(select * from sys.columns 
   where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_18'))
begin
   	alter table HashTable_18 add TrackId int
end
go
if not exists(select * from sys.columns 
   where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_19'))
begin
   	alter table HashTable_19 add TrackId int
end
go
if not exists(select * from sys.columns 
   where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_20'))
begin
   	alter table HashTable_20 add TrackId int
end
go
if not exists(select * from sys.columns 
   where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_21'))
begin
   	alter table HashTable_21 add TrackId int
end
go
if not exists(select * from sys.columns 
   where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_22'))
begin
   	alter table HashTable_22 add TrackId int
end
go
if not exists(select * from sys.columns 
   where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_23'))
begin
   	alter table HashTable_23 add TrackId int
end
go
if not exists(select * from sys.columns 
   where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_24'))
begin
   	alter table HashTable_24 add TrackId int
end
go
if not exists(select * from sys.columns 
   where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_25'))
begin
   	alter table HashTable_25 add TrackId int
end
go
update HashTable_1
	set TrackId = SubFingerprints.TrackId
	from HashTable_1 
	join SubFingerprints on HashTable_1.SubFingerprintId = SubFingerprints.Id
go
update HashTable_2
	set TrackId = SubFingerprints.TrackId
	from HashTable_2 
	join SubFingerprints on HashTable_2.SubFingerprintId = SubFingerprints.Id
go
update HashTable_3
	set TrackId = SubFingerprints.TrackId
	from HashTable_3 
	join SubFingerprints on HashTable_3.SubFingerprintId = SubFingerprints.Id
go
update HashTable_4
	set TrackId = SubFingerprints.TrackId
	from HashTable_4
	join SubFingerprints on HashTable_4.SubFingerprintId = SubFingerprints.Id
go
update HashTable_5
	set TrackId = SubFingerprints.TrackId
	from HashTable_5
	join SubFingerprints on HashTable_5.SubFingerprintId = SubFingerprints.Id
go
update HashTable_6
	set TrackId = SubFingerprints.TrackId
	from HashTable_6
	join SubFingerprints on HashTable_6.SubFingerprintId = SubFingerprints.Id
go
update HashTable_7
	set TrackId = SubFingerprints.TrackId
	from HashTable_7
	join SubFingerprints on HashTable_7.SubFingerprintId = SubFingerprints.Id
go
update HashTable_8
	set TrackId = SubFingerprints.TrackId
	from HashTable_8
	join SubFingerprints on HashTable_8.SubFingerprintId = SubFingerprints.Id
go
update HashTable_9
	set TrackId = SubFingerprints.TrackId
	from HashTable_9
	join SubFingerprints on HashTable_9.SubFingerprintId = SubFingerprints.Id
go
update HashTable_10
	set TrackId = SubFingerprints.TrackId
	from HashTable_10 
	join SubFingerprints on HashTable_10.SubFingerprintId = SubFingerprints.Id
go
update HashTable_11
	set TrackId = SubFingerprints.TrackId
	from HashTable_11 
	join SubFingerprints on HashTable_11.SubFingerprintId = SubFingerprints.Id
go
update HashTable_12
	set TrackId = SubFingerprints.TrackId
	from HashTable_12
	join SubFingerprints on HashTable_12.SubFingerprintId = SubFingerprints.Id
go
update HashTable_13
	set TrackId = SubFingerprints.TrackId
	from HashTable_13 
	join SubFingerprints on HashTable_13.SubFingerprintId = SubFingerprints.Id
go
update HashTable_14
	set TrackId = SubFingerprints.TrackId
	from HashTable_14 
	join SubFingerprints on HashTable_14.SubFingerprintId = SubFingerprints.Id
go
update HashTable_15
	set TrackId = SubFingerprints.TrackId
	from HashTable_15 
	join SubFingerprints on HashTable_15.SubFingerprintId = SubFingerprints.Id
go
update HashTable_16
	set TrackId = SubFingerprints.TrackId
	from HashTable_16
	join SubFingerprints on HashTable_16.SubFingerprintId = SubFingerprints.Id
go
update HashTable_17
	set TrackId = SubFingerprints.TrackId
	from HashTable_17 
	join SubFingerprints on HashTable_17.SubFingerprintId = SubFingerprints.Id
go
update HashTable_18
	set TrackId = SubFingerprints.TrackId
	from HashTable_18 
	join SubFingerprints on HashTable_18.SubFingerprintId = SubFingerprints.Id
go
update HashTable_19
	set TrackId = SubFingerprints.TrackId
	from HashTable_19
	join SubFingerprints on HashTable_19.SubFingerprintId = SubFingerprints.Id
go
update HashTable_20
	set TrackId = SubFingerprints.TrackId
	from HashTable_20
	join SubFingerprints on HashTable_20.SubFingerprintId = SubFingerprints.Id
go
update HashTable_21
	set TrackId = SubFingerprints.TrackId
	from HashTable_21
	join SubFingerprints on HashTable_21.SubFingerprintId = SubFingerprints.Id
go
update HashTable_22
	set TrackId = SubFingerprints.TrackId
	from HashTable_22 
	join SubFingerprints on HashTable_22.SubFingerprintId = SubFingerprints.Id
go
update HashTable_23
	set TrackId = SubFingerprints.TrackId
	from HashTable_23
	join SubFingerprints on HashTable_23.SubFingerprintId = SubFingerprints.Id
go
update HashTable_24
	set TrackId = SubFingerprints.TrackId
	from HashTable_24
	join SubFingerprints on HashTable_24.SubFingerprintId = SubFingerprints.Id
go
update HashTable_25
	set TrackId = SubFingerprints.TrackId
	from HashTable_25 
	join SubFingerprints on HashTable_25.SubFingerprintId = SubFingerprints.Id
go
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_1_Track')
begin
	alter table HashTable_1 add constraint FK_HashTable_1_Track foreign key(TrackId) references dbo.Tracks(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_2_Track')
begin
	alter table HashTable_2 add constraint FK_HashTable_2_Track foreign key(TrackId) references dbo.Tracks(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_3_Track')
begin
	alter table HashTable_3 add constraint FK_HashTable_3_Track foreign key(TrackId) references dbo.Tracks(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_4_Track')
begin
	alter table HashTable_4 add constraint FK_HashTable_4_Track foreign key(TrackId) references dbo.Tracks(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_5_Track')
begin
	alter table HashTable_5 add constraint FK_HashTable_5_Track foreign key(TrackId) references dbo.Tracks(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_6_Track')
begin
	alter table HashTable_6 add constraint FK_HashTable_6_Track foreign key(TrackId) references dbo.Tracks(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_7_Track')
begin
	alter table HashTable_7 add constraint FK_HashTable_7_Track foreign key(TrackId) references dbo.Tracks(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_8_Track')
begin
	alter table HashTable_8 add constraint FK_HashTable_8_Track foreign key(TrackId) references dbo.Tracks(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_9_Track')
begin
	alter table HashTable_9 add constraint FK_HashTable_9_Track foreign key(TrackId) references dbo.Tracks(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_10_Track')
begin
	alter table HashTable_10 add constraint FK_HashTable_10_Track foreign key(TrackId) references dbo.Tracks(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_11_Track')
begin
	alter table HashTable_11 add constraint FK_HashTable_11_Track foreign key(TrackId) references dbo.Tracks(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_12_Track')
begin
	alter table HashTable_12 add constraint FK_HashTable_12_Track foreign key(TrackId) references dbo.Tracks(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_13_Track')
begin
	alter table HashTable_13 add constraint FK_HashTable_13_Track foreign key(TrackId) references dbo.Tracks(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_14_Track')
begin
	alter table HashTable_14 add constraint FK_HashTable_14_Track foreign key(TrackId) references dbo.Tracks(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_15_Track')
begin
	alter table HashTable_15 add constraint FK_HashTable_15_Track foreign key(TrackId) references dbo.Tracks(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_16_Track')
begin
	alter table HashTable_16 add constraint FK_HashTable_16_Track foreign key(TrackId) references dbo.Tracks(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_17_Track')
begin
	alter table HashTable_17 add constraint FK_HashTable_17_Track foreign key(TrackId) references dbo.Tracks(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_18_Track')
begin
	alter table HashTable_18 add constraint FK_HashTable_18_Track foreign key(TrackId) references dbo.Tracks(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_19_Track')
begin
	alter table HashTable_19 add constraint FK_HashTable_19_Track foreign key(TrackId) references dbo.Tracks(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_20_Track')
begin
	alter table HashTable_20 add constraint FK_HashTable_20_Track foreign key(TrackId) references dbo.Tracks(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_21_Track')
begin
	alter table HashTable_21 add constraint FK_HashTable_21_Track foreign key(TrackId) references dbo.Tracks(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_22_Track')
begin
	alter table HashTable_22 add constraint FK_HashTable_22_Track foreign key(TrackId) references dbo.Tracks(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_23_Track')
begin
	alter table HashTable_23 add constraint FK_HashTable_23_Track foreign key(TrackId) references dbo.Tracks(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_24_Track')
begin
	alter table HashTable_24 add constraint FK_HashTable_24_Track foreign key(TrackId) references dbo.Tracks(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_25_Track')
begin
	alter table HashTable_25 add constraint FK_HashTable_25_Track foreign key(TrackId) references dbo.Tracks(Id)
end
---------------------------------------------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_1_SubFingerprints')
begin
	alter table HashTable_1 add constraint FK_HashTable_1_SubFingerprints foreign key(SubFingerprintId) references dbo.SubFingerprints(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_2_SubFingerprints')
begin
	alter table HashTable_2 add constraint FK_HashTable_2_SubFingerprints foreign key(SubFingerprintId) references dbo.SubFingerprints(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_3_SubFingerprints')
begin
	alter table HashTable_3 add constraint FK_HashTable_3_SubFingerprints foreign key(SubFingerprintId) references dbo.SubFingerprints(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_4_SubFingerprints')
begin
	alter table HashTable_4 add constraint FK_HashTable_4_SubFingerprints foreign key(SubFingerprintId) references dbo.SubFingerprints(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_5_SubFingerprints')
begin
	alter table HashTable_5 add constraint FK_HashTable_5_SubFingerprints foreign key(SubFingerprintId) references dbo.SubFingerprints(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_6_SubFingerprints')
begin
	alter table HashTable_6 add constraint FK_HashTable_6_SubFingerprints foreign key(SubFingerprintId) references dbo.SubFingerprints(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_7_SubFingerprints')
begin
	alter table HashTable_7 add constraint FK_HashTable_7_SubFingerprints foreign key(SubFingerprintId) references dbo.SubFingerprints(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_8_SubFingerprints')
begin
	alter table HashTable_8 add constraint FK_HashTable_8_SubFingerprints foreign key(SubFingerprintId) references dbo.SubFingerprints(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_9_SubFingerprints')
begin
	alter table HashTable_9 add constraint FK_HashTable_9_SubFingerprints foreign key(SubFingerprintId) references dbo.SubFingerprints(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_10_SubFingerprints')
begin
	alter table HashTable_10 add constraint FK_HashTable_10_SubFingerprints foreign key(SubFingerprintId) references dbo.SubFingerprints(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_11_SubFingerprints')
begin
	alter table HashTable_11 add constraint FK_HashTable_11_SubFingerprints foreign key(SubFingerprintId) references dbo.SubFingerprints(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_12_SubFingerprints')
begin
	alter table HashTable_12 add constraint FK_HashTable_12_SubFingerprints foreign key(SubFingerprintId) references dbo.SubFingerprints(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_13_SubFingerprints')
begin
	alter table HashTable_13 add constraint FK_HashTable_13_SubFingerprints foreign key(SubFingerprintId) references dbo.SubFingerprints(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_14_SubFingerprints')
begin
	alter table HashTable_14 add constraint FK_HashTable_14_SubFingerprints foreign key(SubFingerprintId) references dbo.SubFingerprints(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_15_SubFingerprints')
begin
	alter table HashTable_15 add constraint FK_HashTable_15_SubFingerprints foreign key(SubFingerprintId) references dbo.SubFingerprints(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_16_SubFingerprints')
begin
	alter table HashTable_16 add constraint FK_HashTable_16_SubFingerprints foreign key(SubFingerprintId) references dbo.SubFingerprints(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_17_SubFingerprints')
begin
	alter table HashTable_17 add constraint FK_HashTable_17_SubFingerprints foreign key(SubFingerprintId) references dbo.SubFingerprints(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_18_SubFingerprints')
begin
	alter table HashTable_18 add constraint FK_HashTable_18_SubFingerprints foreign key(SubFingerprintId) references dbo.SubFingerprints(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_19_SubFingerprints')
begin
	alter table HashTable_19 add constraint FK_HashTable_19_SubFingerprints foreign key(SubFingerprintId) references dbo.SubFingerprints(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_20_SubFingerprints')
begin
	alter table HashTable_20 add constraint FK_HashTable_20_SubFingerprints foreign key(SubFingerprintId) references dbo.SubFingerprints(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_21_SubFingerprints')
begin
	alter table HashTable_21 add constraint FK_HashTable_21_SubFingerprints foreign key(SubFingerprintId) references dbo.SubFingerprints(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_22_SubFingerprints')
begin
	alter table HashTable_22 add constraint FK_HashTable_22_SubFingerprints foreign key(SubFingerprintId) references dbo.SubFingerprints(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_23_SubFingerprints')
begin
	alter table HashTable_23 add constraint FK_HashTable_23_SubFingerprints foreign key(SubFingerprintId) references dbo.SubFingerprints(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_24_SubFingerprints')
begin
	alter table HashTable_24 add constraint FK_HashTable_24_SubFingerprints foreign key(SubFingerprintId) references dbo.SubFingerprints(Id)
end
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_HashTable_25_SubFingerprints')
begin
	alter table HashTable_25 add constraint FK_HashTable_25_SubFingerprints foreign key(SubFingerprintId) references dbo.SubFingerprints(Id)
end
--------------------------------------------------------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_Fingerprints_Tracks')
begin
	alter table Fingerprints add constraint FK_Fingerprints_Tracks foreign key(TrackId) references dbo.Tracks(Id)
end









