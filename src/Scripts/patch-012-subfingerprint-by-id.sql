USE FingerprintsDb
GO
IF OBJECT_ID('sp_ReadSubFingerprintById','P') IS NOT NULL
	DROP PROCEDURE sp_ReadSubFingerprintById
GO
CREATE PROCEDURE sp_ReadSubFingerprintById
	@Id INT
AS
BEGIN
	SELECT * FROM SubFingerprints WHERE Id = @Id
END