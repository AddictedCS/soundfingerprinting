use FingerprintsDb
go
-- artist name is not mandatory anymore
alter table Tracks alter column Artist varchar(255) null
go
-- title is not mandatory anymore
alter table Tracks alter column Title varchar(255) null
go
-- add IRSC to Tracks
if not exists(select * from sys.columns 
   where Name = N'ISRC' and Object_ID = Object_ID(N'Tracks'))
begin
	alter table tracks add ISRC varchar(50) null
end
------------
-- Album table is denormalized.
-- add Album name to tracks table
if not exists(select * from sys.columns 
            where Name = N'Album' and Object_ID = Object_ID(N'Tracks'))
begin
   alter table tracks add Album varchar(255) null
end
-- add ReleaseYear to tracks table
if not exists(select * from sys.columns 
            where Name = N'ReleaseYear' and Object_ID = Object_ID(N'Tracks'))
begin
   alter table tracks add ReleaseYear INT DEFAULT 0
end
go
-- update tracks and set previously defined album on tracks    
IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'Albums'))
BEGIN
	update tracks
	set Album = Albums.Name
	from Albums inner join Tracks on Albums.Id = Tracks.AlbumId;
	-- update release year
	update tracks
	set ReleaseYear = Albums.ReleaseYear
	from Albums inner join Tracks on Albums.Id = Tracks.AlbumId;
END
-- drop foreign key constraing on albums
if exists(select * 
    from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
    where CONSTRAINT_NAME ='FK_Tracks_Albums')
begin
	alter table tracks drop constraint FK_Tracks_Albums
end

-- drop default constraint without name :( never defined constraints without name

declare @table_name nvarchar(256)
declare @col_name nvarchar(256)
declare @Command  nvarchar(1000)

set @table_name = N'Tracks'
set @col_name = N'AlbumId'

select @Command = 'ALTER TABLE ' + @table_name + ' drop constraint ' + d.name
 from sys.tables t   
  join    sys.default_constraints d       
   on d.parent_object_id = t.object_id  
  join    sys.columns c      
   on c.object_id = t.object_id      
    and c.column_id = d.parent_column_id
 where t.name = @table_name
  and c.name = @col_name

--print @Command

execute (@Command)

-- drop albumid column as it is not required anymore
if exists(select * from sys.columns 
            where Name = N'AlbumId' and Object_ID = Object_ID(N'Tracks'))
begin
   alter table tracks drop column AlbumId
end
-- drop primary key constraint 
if exists(select * 
    from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
    where CONSTRAINT_NAME ='PK_AlbumsId')
begin
	alter table albums drop constraint PK_AlbumsId
end

if not exists(select * from sys.check_constraints where name = 'CK_ISRC_NotEmpty')
begin
	alter table tracks add constraint CK_ISRC_NotEmpty check(ISRC <> N'')
end
-- drop table
IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'Albums'))
BEGIN
	drop table albums
END
go
IF OBJECT_ID('sp_InsertAlbum','P') IS NOT NULL
	DROP PROCEDURE sp_InsertAlbum
GO
IF OBJECT_ID('sp_ReadAlbums','P') IS NOT NULL
	DROP PROCEDURE sp_ReadAlbums
GO
IF OBJECT_ID('sp_ReadAlbumUnknown','P') IS NOT NULL
	DROP PROCEDURE sp_ReadAlbumUnknown
GO
IF OBJECT_ID('sp_ReadAlbumById','P') IS NOT NULL
	DROP PROCEDURE sp_ReadAlbumById
GO
IF OBJECT_ID('sp_InsertTrack','P') IS NOT NULL
	DROP PROCEDURE sp_InsertTrack
GO
CREATE PROCEDURE sp_InsertTrack
	@Id INT,
	@ISRC VARCHAR(50),
	@Artist VARCHAR(255),
	@Title VARCHAR(255),
	@Album VARCHAR(255),
	@ReleaseYear INT,
	@TrackLengthSec INT
AS
INSERT INTO Tracks (
	ISRC,
	Artist,
	Title,
	Album,
	ReleaseYear,
	TrackLengthSec
	) OUTPUT inserted.Id
VALUES
(
 	@ISRC, @Artist, @Title, @Album, @ReleaseYear, @TrackLengthSec
);
GO

IF OBJECT_ID('sp_ReadTracks','P') IS NOT NULL
	DROP PROCEDURE sp_ReadTracks
GO
CREATE PROCEDURE sp_ReadTracks
AS
SELECT * FROM Tracks
GO
IF OBJECT_ID('sp_ReadTrackById','P') IS NOT NULL
	DROP PROCEDURE sp_ReadTrackById
GO
CREATE PROCEDURE sp_ReadTrackById
	@Id INT
AS
SELECT * FROM Tracks WHERE Tracks.Id = @Id
GO
IF OBJECT_ID('sp_ReadTrackByArtistAndSongName','P') IS NOT NULL
	DROP PROCEDURE sp_ReadTrackByArtistAndSongName
GO
CREATE PROCEDURE sp_ReadTrackByArtistAndSongName
	@Artist VARCHAR(255),
	@Title VARCHAR(255) 
AS
SELECT * FROM Tracks
WHERE Tracks.Title = @Title AND Tracks.Artist = @Artist
GO
IF OBJECT_ID('sp_DeleteTrack','P') IS NOT NULL
	DROP PROCEDURE sp_DeleteTrack
GO
CREATE PROCEDURE sp_DeleteTrack
	@Id INT
AS
BEGIN
	DELETE FROM HashBins WHERE HashBins.TrackId = @Id				-- CASCADE DELETE OF HASHBINSNEURALHASHER
	DELETE FROM Fingerprints WHERE Fingerprints.TrackId = @Id		-- CASCADE DELETE
	DELETE FROM SubFingerprints WHERE SubFingerprints.TrackId = @Id -- CASCADE DELETE
	DELETE FROM Tracks WHERE Tracks.Id = @Id						-- CASCADE DELETE
END
GO
IF OBJECT_ID('sp_ReadDuplicatedTracks','P') IS NOT NULL
	DROP PROCEDURE sp_ReadDuplicatedTracks
GO
IF OBJECT_ID('sp_ReadTrackByFingerprint','P') IS NOT NULL
	DROP PROCEDURE sp_ReadTrackByFingerprint
GO
CREATE PROCEDURE sp_ReadTrackByFingerprint
	@Id INT -- Fingerprint ID
AS
SELECT Tracks.Id, Tracks.Artist, Tracks.Title, Tracks.Album, Tracks.ISRC, Tracks.ReleaseYear, Tracks.TrackLengthSec FROM Tracks, Fingerprints
WHERE Tracks.Id = Fingerprints.TrackId AND Fingerprints.Id = @Id
GO
IF OBJECT_ID('sp_ReadTrackISRC','P') IS NOT NULL
	DROP PROCEDURE sp_ReadTrackISRC
GO
CREATE PROCEDURE sp_ReadTrackISRC
	@ISRC VARCHAR(50)
AS
SELECT * FROM Tracks WHERE Tracks.ISRC = @ISRC
GO


