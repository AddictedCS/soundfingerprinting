USE FingerprintsDb
GO
-- drop foreign key constraint on fingerprints table
IF EXISTS(SELECT * 
    FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
    WHERE CONSTRAINT_NAME ='FK_Fingerprints_Tracks')
BEGIN
	ALTER TABLE Fingerprints drop constraint FK_Fingerprints_Tracks
END
-- drop primary key constraint on fingerprints table
IF EXISTS(SELECT * 
    FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
    WHERE CONSTRAINT_NAME ='PK_FingerprintsId')
BEGIN
	ALTER TABLE Fingerprints drop constraint PK_FingerprintsId
END

-- drop default constraint without name [TotalFingerprintsPerTrack, SongOrder]
declare @table_name nvarchar(256)
declare @col_name nvarchar(256)
declare @Command  nvarchar(1000)

set @table_name = N'Fingerprints'
set @col_name = N'TotalFingerprintsPerTrack'

select @Command = 'ALTER TABLE ' + @table_name + ' drop constraint ' + d.name
 from sys.tables t   
  join    sys.default_constraints d       
   on d.parent_object_id = t.object_id  
  join    sys.columns c      
   on c.object_id = t.object_id      
    and c.column_id = d.parent_column_id
 where t.name = @table_name
  and c.name = @col_name

execute (@Command)

set @table_name = N'Fingerprints'
set @col_name = N'SongOrder'

select @Command = 'ALTER TABLE ' + @table_name + ' drop constraint ' + d.name
 from sys.tables t   
  join    sys.default_constraints d       
   on d.parent_object_id = t.object_id  
  join    sys.columns c      
   on c.object_id = t.object_id      
    and c.column_id = d.parent_column_id
 where t.name = @table_name
  and c.name = @col_name

execute (@Command)
GO
-- drop table Fingerprints
IF EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'Fingerprints')
BEGIN
	DROP TABLE Fingerprints
END
GO
IF OBJECT_ID('sp_InsertFingerprint','P') IS NOT NULL
	DROP PROCEDURE sp_InsertFingerprint
GO
IF OBJECT_ID('sp_ReadFingerprints','P') IS NOT NULL
	DROP PROCEDURE sp_ReadFingerprints
GO
IF OBJECT_ID('sp_ReadFingerprintById','P') IS NOT NULL
	DROP PROCEDURE sp_ReadFingerprintById
GO
IF OBJECT_ID('sp_ReadFingerprintByTrackId','P') IS NOT NULL
	DROP PROCEDURE sp_ReadFingerprintByTrackId
GO
IF OBJECT_ID('sp_ReadTrackByFingerprint','P') IS NOT NULL
	DROP PROCEDURE sp_ReadTrackByFingerprint
GO
IF OBJECT_ID('sp_DeleteTrack','P') IS NOT NULL
	DROP PROCEDURE sp_DeleteTrack
GO
CREATE PROCEDURE sp_DeleteTrack
	@Id INT
AS
BEGIN
	DELETE HashTable_1 FROM HashTable_1 INNER JOIN SubFingerprints ON HashTable_1.SubFingerprintId = SubFingerprints.Id AND SubFingerprints.TrackId = @Id
	DELETE HashTable_2 FROM HashTable_2 INNER JOIN SubFingerprints ON HashTable_2.SubFingerprintId = SubFingerprints.Id AND SubFingerprints.TrackId = @Id
	DELETE HashTable_3 FROM HashTable_3 INNER JOIN SubFingerprints ON HashTable_3.SubFingerprintId = SubFingerprints.Id AND SubFingerprints.TrackId = @Id
	DELETE HashTable_4 FROM HashTable_4 INNER JOIN SubFingerprints ON HashTable_4.SubFingerprintId = SubFingerprints.Id AND SubFingerprints.TrackId = @Id
	DELETE HashTable_5 FROM HashTable_5 INNER JOIN SubFingerprints ON HashTable_5.SubFingerprintId = SubFingerprints.Id AND SubFingerprints.TrackId = @Id
	DELETE HashTable_6 FROM HashTable_6 INNER JOIN SubFingerprints ON HashTable_6.SubFingerprintId = SubFingerprints.Id AND SubFingerprints.TrackId = @Id
	DELETE HashTable_7 FROM HashTable_7 INNER JOIN SubFingerprints ON HashTable_7.SubFingerprintId = SubFingerprints.Id AND SubFingerprints.TrackId = @Id
	DELETE HashTable_8 FROM HashTable_8 INNER JOIN SubFingerprints ON HashTable_8.SubFingerprintId = SubFingerprints.Id AND SubFingerprints.TrackId = @Id
	DELETE HashTable_9 FROM HashTable_9 INNER JOIN SubFingerprints ON HashTable_9.SubFingerprintId = SubFingerprints.Id AND SubFingerprints.TrackId = @Id
	DELETE HashTable_10 FROM HashTable_10 INNER JOIN SubFingerprints ON HashTable_10.SubFingerprintId = SubFingerprints.Id AND SubFingerprints.TrackId = @Id
	DELETE HashTable_11 FROM HashTable_11 INNER JOIN SubFingerprints ON HashTable_11.SubFingerprintId = SubFingerprints.Id AND SubFingerprints.TrackId = @Id
	DELETE HashTable_12 FROM HashTable_12 INNER JOIN SubFingerprints ON HashTable_12.SubFingerprintId = SubFingerprints.Id AND SubFingerprints.TrackId = @Id
	DELETE HashTable_13 FROM HashTable_13 INNER JOIN SubFingerprints ON HashTable_13.SubFingerprintId = SubFingerprints.Id AND SubFingerprints.TrackId = @Id
	DELETE HashTable_14 FROM HashTable_14 INNER JOIN SubFingerprints ON HashTable_14.SubFingerprintId = SubFingerprints.Id AND SubFingerprints.TrackId = @Id
	DELETE HashTable_15 FROM HashTable_15 INNER JOIN SubFingerprints ON HashTable_15.SubFingerprintId = SubFingerprints.Id AND SubFingerprints.TrackId = @Id
	DELETE HashTable_16 FROM HashTable_16 INNER JOIN SubFingerprints ON HashTable_16.SubFingerprintId = SubFingerprints.Id AND SubFingerprints.TrackId = @Id
	DELETE HashTable_17 FROM HashTable_17 INNER JOIN SubFingerprints ON HashTable_17.SubFingerprintId = SubFingerprints.Id AND SubFingerprints.TrackId = @Id
	DELETE HashTable_18 FROM HashTable_18 INNER JOIN SubFingerprints ON HashTable_18.SubFingerprintId = SubFingerprints.Id AND SubFingerprints.TrackId = @Id
	DELETE HashTable_19 FROM HashTable_19 INNER JOIN SubFingerprints ON HashTable_19.SubFingerprintId = SubFingerprints.Id AND SubFingerprints.TrackId = @Id
	DELETE HashTable_20 FROM HashTable_20 INNER JOIN SubFingerprints ON HashTable_20.SubFingerprintId = SubFingerprints.Id AND SubFingerprints.TrackId = @Id
	DELETE HashTable_21 FROM HashTable_21 INNER JOIN SubFingerprints ON HashTable_21.SubFingerprintId = SubFingerprints.Id AND SubFingerprints.TrackId = @Id
	DELETE HashTable_22 FROM HashTable_22 INNER JOIN SubFingerprints ON HashTable_22.SubFingerprintId = SubFingerprints.Id AND SubFingerprints.TrackId = @Id
	DELETE HashTable_23 FROM HashTable_23 INNER JOIN SubFingerprints ON HashTable_23.SubFingerprintId = SubFingerprints.Id AND SubFingerprints.TrackId = @Id
	DELETE HashTable_24 FROM HashTable_24 INNER JOIN SubFingerprints ON HashTable_24.SubFingerprintId = SubFingerprints.Id AND SubFingerprints.TrackId = @Id
	DELETE HashTable_25 FROM HashTable_25 INNER JOIN SubFingerprints ON HashTable_25.SubFingerprintId = SubFingerprints.Id AND SubFingerprints.TrackId = @Id
	DELETE FROM SubFingerprints WHERE SubFingerprints.TrackId = @Id
	DELETE FROM Tracks WHERE Tracks.Id = @Id
END
GO


