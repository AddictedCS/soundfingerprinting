USE FingerprintsDb
GO
IF OBJECT_ID('sp_InsertHashBinMinHash','P') IS NOT NULL
	DROP PROCEDURE sp_InsertHashBinMinHash
GO