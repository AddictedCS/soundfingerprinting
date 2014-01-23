USE FingerprintsDb
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
					 INNER JOIN HashTable_2 ON HashTable_1.SubFingerprintId = HashTable_2.SubFingerprintId AND HashTable_1.TrackId = @TrackId
					 INNER JOIN HashTable_3 ON HashTable_1.SubFingerprintId = HashTable_3.SubFingerprintId AND HashTable_1.TrackId = @TrackId
					 INNER JOIN HashTable_4 ON HashTable_1.SubFingerprintId = HashTable_4.SubFingerprintId AND HashTable_1.TrackId = @TrackId
					 INNER JOIN HashTable_5 ON HashTable_1.SubFingerprintId = HashTable_5.SubFingerprintId AND HashTable_1.TrackId = @TrackId
					 INNER JOIN HashTable_6 ON HashTable_1.SubFingerprintId = HashTable_6.SubFingerprintId AND HashTable_1.TrackId = @TrackId
					 INNER JOIN HashTable_7 ON HashTable_1.SubFingerprintId = HashTable_7.SubFingerprintId AND HashTable_1.TrackId = @TrackId
					 INNER JOIN HashTable_8 ON HashTable_1.SubFingerprintId = HashTable_8.SubFingerprintId AND HashTable_1.TrackId = @TrackId
					 INNER JOIN HashTable_9 ON HashTable_1.SubFingerprintId = HashTable_9.SubFingerprintId AND HashTable_1.TrackId = @TrackId
					 INNER JOIN HashTable_10 ON HashTable_1.SubFingerprintId = HashTable_10.SubFingerprintId AND HashTable_1.TrackId = @TrackId
					 INNER JOIN HashTable_11 ON HashTable_1.SubFingerprintId = HashTable_11.SubFingerprintId AND HashTable_1.TrackId = @TrackId
					 INNER JOIN HashTable_12 ON HashTable_1.SubFingerprintId = HashTable_12.SubFingerprintId AND HashTable_1.TrackId = @TrackId
					 INNER JOIN HashTable_13 ON HashTable_1.SubFingerprintId = HashTable_13.SubFingerprintId AND HashTable_1.TrackId = @TrackId
					 INNER JOIN HashTable_14 ON HashTable_1.SubFingerprintId = HashTable_14.SubFingerprintId AND HashTable_1.TrackId = @TrackId
					 INNER JOIN HashTable_15 ON HashTable_1.SubFingerprintId = HashTable_15.SubFingerprintId AND HashTable_1.TrackId = @TrackId
					 INNER JOIN HashTable_16 ON HashTable_1.SubFingerprintId = HashTable_16.SubFingerprintId AND HashTable_1.TrackId = @TrackId 
					 INNER JOIN HashTable_17 ON HashTable_1.SubFingerprintId = HashTable_17.SubFingerprintId AND HashTable_1.TrackId = @TrackId
					 INNER JOIN HashTable_18 ON HashTable_1.SubFingerprintId = HashTable_18.SubFingerprintId AND HashTable_1.TrackId = @TrackId
					 INNER JOIN HashTable_19 ON HashTable_1.SubFingerprintId = HashTable_19.SubFingerprintId AND HashTable_1.TrackId = @TrackId
					 INNER JOIN HashTable_20 ON HashTable_1.SubFingerprintId = HashTable_20.SubFingerprintId AND HashTable_1.TrackId = @TrackId
					 INNER JOIN HashTable_21 ON HashTable_1.SubFingerprintId = HashTable_21.SubFingerprintId AND HashTable_1.TrackId = @TrackId
					 INNER JOIN HashTable_22 ON HashTable_1.SubFingerprintId = HashTable_22.SubFingerprintId AND HashTable_1.TrackId = @TrackId
					 INNER JOIN HashTable_23 ON HashTable_1.SubFingerprintId = HashTable_23.SubFingerprintId AND HashTable_1.TrackId = @TrackId
					 INNER JOIN HashTable_24 ON HashTable_1.SubFingerprintId = HashTable_24.SubFingerprintId AND HashTable_1.TrackId = @TrackId
					 INNER JOIN HashTable_25 ON HashTable_1.SubFingerprintId = HashTable_25.SubFingerprintId AND HashTable_1.TrackId = @TrackId
					 INNER JOIN SubFingerprints ON HashTable_1.SubFingerprintId = SubFingerprints.Id
END					 
					 