USE FingerprintsDb
GO
if exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_HashTable_1_Track')
begin
	alter table HashTable_1 drop constraint FK_HashTable_1_Track
end
go
if exists(select * from sys.columns where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_1'))
begin
	alter table HashTable_1 DROP COLUMN TrackId
end
GO
if exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_HashTable_2_Track')
begin
	alter table HashTable_2 drop constraint FK_HashTable_2_Track
end
go
if exists(select * from sys.columns where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_2'))
begin
	alter table HashTable_2 DROP COLUMN TrackId
end
GO
if exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_HashTable_3_Track')
begin
	alter table HashTable_3 drop constraint FK_HashTable_3_Track
end
go
if exists(select * from sys.columns where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_3'))
begin
	alter table HashTable_3 DROP COLUMN TrackId
end
GO
if exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_HashTable_4_Track')
begin
	alter table HashTable_4 drop constraint FK_HashTable_4_Track
end
go
if exists(select * from sys.columns where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_4'))
begin
	alter table HashTable_4 DROP COLUMN TrackId
end
GO
if exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_HashTable_5_Track')
begin
	alter table HashTable_5 drop constraint FK_HashTable_5_Track
end
go
if exists(select * from sys.columns where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_5'))
begin
	alter table HashTable_5 DROP COLUMN TrackId
end
GO
if exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_HashTable_6_Track')
begin
	alter table HashTable_6 drop constraint FK_HashTable_6_Track
end
go
if exists(select * from sys.columns where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_6'))
begin
	alter table HashTable_6 DROP COLUMN TrackId
end
GO
if exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_HashTable_7_Track')
begin
	alter table HashTable_7 drop constraint FK_HashTable_7_Track
end
go
if exists(select * from sys.columns where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_7'))
begin
	alter table HashTable_7 DROP COLUMN TrackId
end
GO
if exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_HashTable_8_Track')
begin
	alter table HashTable_8 drop constraint FK_HashTable_8_Track
end
go
if exists(select * from sys.columns where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_8'))
begin
	alter table HashTable_8 DROP COLUMN TrackId
end
GO
if exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_HashTable_9_Track')
begin
	alter table HashTable_9 drop constraint FK_HashTable_9_Track
end
go
if exists(select * from sys.columns where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_9'))
begin
	alter table HashTable_9 DROP COLUMN TrackId
end
GO
if exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_HashTable_10_Track')
begin
	alter table HashTable_10 drop constraint FK_HashTable_10_Track
end
go
if exists(select * from sys.columns where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_10'))
begin
	alter table HashTable_10 DROP COLUMN TrackId
end
GO
if exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_HashTable_11_Track')
begin
	alter table HashTable_11 drop constraint FK_HashTable_11_Track
end
go
if exists(select * from sys.columns where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_11'))
begin
	alter table HashTable_11 DROP COLUMN TrackId
end
GO
if exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_HashTable_12_Track')
begin
	alter table HashTable_12 drop constraint FK_HashTable_12_Track
end
go
if exists(select * from sys.columns where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_12'))
begin
	alter table HashTable_12 DROP COLUMN TrackId
end
GO
if exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_HashTable_13_Track')
begin
	alter table HashTable_13 drop constraint FK_HashTable_13_Track
end
go
if exists(select * from sys.columns where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_13'))
begin
	alter table HashTable_13 DROP COLUMN TrackId
end
GO
if exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_HashTable_14_Track')
begin
	alter table HashTable_14 drop constraint FK_HashTable_14_Track
end
go
if exists(select * from sys.columns where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_14'))
begin
	alter table HashTable_14 DROP COLUMN TrackId
end
GO
if exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_HashTable_15_Track')
begin
	alter table HashTable_15 drop constraint FK_HashTable_15_Track
end
go
if exists(select * from sys.columns where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_15'))
begin
	alter table HashTable_15 DROP COLUMN TrackId
end
GO
if exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_HashTable_16_Track')
begin
	alter table HashTable_16 drop constraint FK_HashTable_16_Track
end
go
if exists(select * from sys.columns where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_16'))
begin
	alter table HashTable_16 DROP COLUMN TrackId
end
GO
if exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_HashTable_17_Track')
begin
	alter table HashTable_17 drop constraint FK_HashTable_17_Track
end
go
if exists(select * from sys.columns where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_17'))
begin
	alter table HashTable_17 DROP COLUMN TrackId
end
GO
if exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_HashTable_18_Track')
begin
	alter table HashTable_18 drop constraint FK_HashTable_18_Track
end
go
if exists(select * from sys.columns where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_18'))
begin
	alter table HashTable_18 DROP COLUMN TrackId
end
GO
if exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_HashTable_19_Track')
begin
	alter table HashTable_19 drop constraint FK_HashTable_19_Track
end
go
if exists(select * from sys.columns where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_19'))
begin
	alter table HashTable_19 DROP COLUMN TrackId
end
GO
if exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_HashTable_20_Track')
begin
	alter table HashTable_20 drop constraint FK_HashTable_20_Track
end
go
if exists(select * from sys.columns where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_20'))
begin
	alter table HashTable_20 DROP COLUMN TrackId
end
GO
if exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_HashTable_21_Track')
begin
	alter table HashTable_21 drop constraint FK_HashTable_21_Track
end
go
if exists(select * from sys.columns where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_21'))
begin
	alter table HashTable_21 DROP COLUMN TrackId
end
GO
if exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_HashTable_22_Track')
begin
	alter table HashTable_22 drop constraint FK_HashTable_22_Track
end
go
if exists(select * from sys.columns where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_22'))
begin
	alter table HashTable_22 DROP COLUMN TrackId
end
GO
if exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_HashTable_23_Track')
begin
	alter table HashTable_23 drop constraint FK_HashTable_23_Track
end
go
if exists(select * from sys.columns where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_23'))
begin
	alter table HashTable_23 DROP COLUMN TrackId
end
GO
if exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_HashTable_24_Track')
begin
	alter table HashTable_24 drop constraint FK_HashTable_24_Track
end
go
if exists(select * from sys.columns where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_24'))
begin
	alter table HashTable_24 DROP COLUMN TrackId
end
GO
if exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_HashTable_25_Track')
begin
	alter table HashTable_25 drop constraint FK_HashTable_25_Track
end
go
if exists(select * from sys.columns where Name = N'TrackId' and Object_ID = Object_ID(N'HashTable_25'))
begin
	alter table HashTable_25 DROP COLUMN TrackId
end
GO
IF OBJECT_ID('sp_ReadHashDataByTrackId','P') IS NOT NULL
	DROP PROCEDURE sp_ReadHashDataByTrackId
GO
CREATE PROCEDURE sp_ReadHashDataByTrackId
	@TrackId INT
AS
BEGIN
SELECT SubFingerprints.Signature as Signature, HashTable_1.SubFingerprintId as SubFingerprintId, HashTable_1.HashBin as HashBin_1, HashTable_2.HashBin as HashBin_2, HashTable_3.HashBin as HashBin_3, HashTable_4.HashBin as HashBin_4, HashTable_5.HashBin as HashBin_5,
       HashTable_6.HashBin as HashBin_6, HashTable_7.HashBin as HashBin_7, HashTable_8.HashBin as HashBin_8, HashTable_9.HashBin as HashBin_9, HashTable_10.HashBin as HashBin_10,
       HashTable_11.HashBin as HashBin_11, HashTable_12.HashBin as HashBin_12, HashTable_13.HashBin as HashBin_13, HashTable_14.HashBin as HashBin_14, HashTable_15.HashBin as HashBin_15,
       HashTable_16.HashBin as HashBin_16, HashTable_17.HashBin as HashBin_17, HashTable_18.HashBin as HashBin_18, HashTable_19.HashBin as HashBin_19, HashTable_20.HashBin as HashBin_20,
       HashTable_21.HashBin as HashBin_21, HashTable_22.HashBin as HashBin_22, HashTable_23.HashBin as HashBin_23, HashTable_24.HashBin as HashBin_24, HashTable_25.HashBin as HashBin_25
	   FROM HashTable_1 
					 INNER JOIN HashTable_2 ON HashTable_1.SubFingerprintId = HashTable_2.SubFingerprintId
					 INNER JOIN HashTable_3 ON HashTable_1.SubFingerprintId = HashTable_3.SubFingerprintId
					 INNER JOIN HashTable_4 ON HashTable_1.SubFingerprintId = HashTable_4.SubFingerprintId 
					 INNER JOIN HashTable_5 ON HashTable_1.SubFingerprintId = HashTable_5.SubFingerprintId
					 INNER JOIN HashTable_6 ON HashTable_1.SubFingerprintId = HashTable_6.SubFingerprintId
					 INNER JOIN HashTable_7 ON HashTable_1.SubFingerprintId = HashTable_7.SubFingerprintId
					 INNER JOIN HashTable_8 ON HashTable_1.SubFingerprintId = HashTable_8.SubFingerprintId
					 INNER JOIN HashTable_9 ON HashTable_1.SubFingerprintId = HashTable_9.SubFingerprintId
					 INNER JOIN HashTable_10 ON HashTable_1.SubFingerprintId = HashTable_10.SubFingerprintId
					 INNER JOIN HashTable_11 ON HashTable_1.SubFingerprintId = HashTable_11.SubFingerprintId
					 INNER JOIN HashTable_12 ON HashTable_1.SubFingerprintId = HashTable_12.SubFingerprintId
					 INNER JOIN HashTable_13 ON HashTable_1.SubFingerprintId = HashTable_13.SubFingerprintId
					 INNER JOIN HashTable_14 ON HashTable_1.SubFingerprintId = HashTable_14.SubFingerprintId
					 INNER JOIN HashTable_15 ON HashTable_1.SubFingerprintId = HashTable_15.SubFingerprintId
					 INNER JOIN HashTable_16 ON HashTable_1.SubFingerprintId = HashTable_16.SubFingerprintId
					 INNER JOIN HashTable_17 ON HashTable_1.SubFingerprintId = HashTable_17.SubFingerprintId
					 INNER JOIN HashTable_18 ON HashTable_1.SubFingerprintId = HashTable_18.SubFingerprintId
					 INNER JOIN HashTable_19 ON HashTable_1.SubFingerprintId = HashTable_19.SubFingerprintId
					 INNER JOIN HashTable_20 ON HashTable_1.SubFingerprintId = HashTable_20.SubFingerprintId
					 INNER JOIN HashTable_21 ON HashTable_1.SubFingerprintId = HashTable_21.SubFingerprintId
					 INNER JOIN HashTable_22 ON HashTable_1.SubFingerprintId = HashTable_22.SubFingerprintId
					 INNER JOIN HashTable_23 ON HashTable_1.SubFingerprintId = HashTable_23.SubFingerprintId
					 INNER JOIN HashTable_24 ON HashTable_1.SubFingerprintId = HashTable_24.SubFingerprintId
					 INNER JOIN HashTable_25 ON HashTable_1.SubFingerprintId = HashTable_25.SubFingerprintId
					 INNER JOIN SubFingerprints ON HashTable_1.SubFingerprintId = SubFingerprints.Id
					 WHERE SubFingerprints.TrackId = @TrackId
END					 
