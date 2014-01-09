USE FingerprintsDb
GO
IF OBJECT_ID('sp_ReadSubFingerprintById','P') IS NOT NULL
	DROP PROCEDURE sp_ReadSubFingerprintById
GO
CREATE PROCEDURE sp_ReadSubFingerprintById
	@Id BIGINT
AS
BEGIN
	SELECT * FROM SubFingerprints WHERE SubFingerprints.Id = @Id
END
GO
IF OBJECT_ID('sp_ReadHashBinsByHashTable','P') IS NOT NULL
	DROP PROCEDURE sp_ReadHashBinsByHashTable
GO
CREATE PROCEDURE sp_ReadHashBinsByHashTable
	@HashTable INT
AS
BEGIN
	DECLARE @STR_TO_EXECUTE NVARCHAR(2000)
	SELECT @HASH_TABLE = CAST (@HashTable AS VARCHAR(2))
	SELECT @STR_TO_EXECUTE = N'SELECT * FROM HashTable_' + @HASH_TABLE
	
	EXEC sp_executesql @STR_TO_EXECUTE
END
