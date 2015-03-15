use FingerprintsDb
go
if not exists(select * from sys.columns 
   where Name = N'SequenceNumber' and Object_ID = Object_ID(N'SubFingerprints'))
begin
   	alter table SubFingerprints add SequenceNumber int
end
go
IF OBJECT_ID('sp_InsertSubFingerprint','P') IS NOT NULL
	DROP PROCEDURE sp_InsertSubFingerprint
GO
CREATE PROCEDURE sp_InsertSubFingerprint
	@Signature VARBINARY(100),
	@TrackId INT,
	@SequenceNumber INT
AS
BEGIN
INSERT INTO SubFingerprints (
	Signature,
	TrackId,
	SequenceNumber
	) OUTPUT inserted.Id
VALUES
(
	@Signature, @TrackId, @SequenceNumber
);
END
GO
IF OBJECT_ID('sp_ReadFingerprintsByHashBinHashTableAndThreshold','P') IS NOT NULL
	DROP PROCEDURE sp_ReadFingerprintsByHashBinHashTableAndThreshold
GO
CREATE PROCEDURE sp_ReadFingerprintsByHashBinHashTableAndThreshold
	@HashBin_1 BIGINT, @HashBin_2 BIGINT, @HashBin_3 BIGINT, @HashBin_4 BIGINT, @HashBin_5 BIGINT, 
	@HashBin_6 BIGINT, @HashBin_7 BIGINT, @HashBin_8 BIGINT, @HashBin_9 BIGINT, @HashBin_10 BIGINT,
	@HashBin_11 BIGINT, @HashBin_12 BIGINT, @HashBin_13 BIGINT, @HashBin_14 BIGINT, @HashBin_15 BIGINT, 
	@HashBin_16 BIGINT, @HashBin_17 BIGINT, @HashBin_18 BIGINT, @HashBin_19 BIGINT, @HashBin_20 BIGINT,
	@HashBin_21 BIGINT, @HashBin_22 BIGINT, @HashBin_23 BIGINT, @HashBin_24 BIGINT, @HashBin_25 BIGINT,
	@Threshold INT
AS
SELECT SubFingerprints.Id, SubFingerprints.TrackId, SubFingerprints.Signature, SubFingerprints.SequenceNumber
FROM SubFingerprints, 
	( SELECT Hashes.SubFingerprintId as SubFingerprintId FROM 
	   (
		SELECT * FROM HashTable_1 WHERE HashBin = @HashBin_1
		UNION ALL
		SELECT * FROM HashTable_2 WHERE HashBin = @HashBin_2
		UNION ALL
		SELECT * FROM HashTable_3 WHERE HashBin = @HashBin_3
		UNION ALL
		SELECT * FROM HashTable_4 WHERE HashBin = @HashBin_4
		UNION ALL
		SELECT * FROM HashTable_5 WHERE HashBin = @HashBin_5
		UNION ALL
		SELECT * FROM HashTable_6 WHERE HashBin = @HashBin_6
		UNION ALL
		SELECT * FROM HashTable_7 WHERE HashBin = @HashBin_7
		UNION ALL
		SELECT * FROM HashTable_8 WHERE HashBin = @HashBin_8
		UNION ALL
		SELECT * FROM HashTable_9 WHERE HashBin = @HashBin_9
		UNION ALL
		SELECT * FROM HashTable_10 WHERE HashBin = @HashBin_10
		UNION ALL
		SELECT * FROM HashTable_11 WHERE HashBin = @HashBin_11
		UNION ALL
		SELECT * FROM HashTable_12 WHERE HashBin = @HashBin_12
		UNION ALL
		SELECT * FROM HashTable_13 WHERE HashBin = @HashBin_13
		UNION ALL
		SELECT * FROM HashTable_14 WHERE HashBin = @HashBin_14
		UNION ALL
		SELECT * FROM HashTable_15 WHERE HashBin = @HashBin_15
		UNION ALL
		SELECT * FROM HashTable_16 WHERE HashBin = @HashBin_16
		UNION ALL
		SELECT * FROM HashTable_17 WHERE HashBin = @HashBin_17
		UNION ALL
		SELECT * FROM HashTable_18 WHERE HashBin = @HashBin_18
		UNION ALL
		SELECT * FROM HashTable_19 WHERE HashBin = @HashBin_19
		UNION ALL
		SELECT * FROM HashTable_20 WHERE HashBin = @HashBin_20
		UNION ALL
		SELECT * FROM HashTable_21 WHERE HashBin = @HashBin_21
		UNION ALL
		SELECT * FROM HashTable_22 WHERE HashBin = @HashBin_22
		UNION ALL
		SELECT * FROM HashTable_23 WHERE HashBin = @HashBin_23
		UNION ALL
		SELECT * FROM HashTable_24 WHERE HashBin = @HashBin_24
		UNION ALL
		SELECT * FROM HashTable_25 WHERE HashBin = @HashBin_25
	  ) AS Hashes
	 GROUP BY Hashes.SubFingerprintId
	 HAVING COUNT(Hashes.SubFingerprintId) >= @Threshold
	) AS Thresholded
WHERE SubFingerprints.Id = Thresholded.SubFingerprintId	
GO
IF OBJECT_ID('sp_ReadSubFingerprintsByHashBinHashTableAndThresholdWithGroupId','P') IS NOT NULL
	DROP PROCEDURE sp_ReadSubFingerprintsByHashBinHashTableAndThresholdWithGroupId
GO
CREATE PROCEDURE sp_ReadSubFingerprintsByHashBinHashTableAndThresholdWithGroupId
	@HashBin_1 BIGINT, @HashBin_2 BIGINT, @HashBin_3 BIGINT, @HashBin_4 BIGINT, @HashBin_5 BIGINT, 
	@HashBin_6 BIGINT, @HashBin_7 BIGINT, @HashBin_8 BIGINT, @HashBin_9 BIGINT, @HashBin_10 BIGINT,
	@HashBin_11 BIGINT, @HashBin_12 BIGINT, @HashBin_13 BIGINT, @HashBin_14 BIGINT, @HashBin_15 BIGINT, 
	@HashBin_16 BIGINT, @HashBin_17 BIGINT, @HashBin_18 BIGINT, @HashBin_19 BIGINT, @HashBin_20 BIGINT,
	@HashBin_21 BIGINT, @HashBin_22 BIGINT, @HashBin_23 BIGINT, @HashBin_24 BIGINT, @HashBin_25 BIGINT,
	@Threshold INT, @GroupId VARCHAR(20)
AS
SELECT SubFingerprints.Id, SubFingerprints.TrackId, SubFingerprints.Signature, SubFingerprints.SequenceNumber
FROM SubFingerprints INNER JOIN
	( SELECT Hashes.SubFingerprintId as SubFingerprintId FROM 
	   (
	    SELECT * FROM HashTable_1 WHERE HashBin = @HashBin_1
		UNION ALL
		SELECT * FROM HashTable_2 WHERE HashBin = @HashBin_2
		UNION ALL
		SELECT * FROM HashTable_3 WHERE HashBin = @HashBin_3
		UNION ALL
		SELECT * FROM HashTable_4 WHERE HashBin = @HashBin_4
		UNION ALL
		SELECT * FROM HashTable_5 WHERE HashBin = @HashBin_5
		UNION ALL
		SELECT * FROM HashTable_6 WHERE HashBin = @HashBin_6
		UNION ALL
		SELECT * FROM HashTable_7 WHERE HashBin = @HashBin_7
		UNION ALL
		SELECT * FROM HashTable_8 WHERE HashBin = @HashBin_8
		UNION ALL
		SELECT * FROM HashTable_9 WHERE HashBin = @HashBin_9
		UNION ALL
		SELECT * FROM HashTable_10 WHERE HashBin = @HashBin_10
		UNION ALL
		SELECT * FROM HashTable_11 WHERE HashBin = @HashBin_11
		UNION ALL
		SELECT * FROM HashTable_12 WHERE HashBin = @HashBin_12
		UNION ALL
		SELECT * FROM HashTable_13 WHERE HashBin = @HashBin_13
		UNION ALL
		SELECT * FROM HashTable_14 WHERE HashBin = @HashBin_14
		UNION ALL
		SELECT * FROM HashTable_15 WHERE HashBin = @HashBin_15
		UNION ALL
		SELECT * FROM HashTable_16 WHERE HashBin = @HashBin_16
		UNION ALL
		SELECT * FROM HashTable_17 WHERE HashBin = @HashBin_17
		UNION ALL
		SELECT * FROM HashTable_18 WHERE HashBin = @HashBin_18
		UNION ALL
		SELECT * FROM HashTable_19 WHERE HashBin = @HashBin_19
		UNION ALL
		SELECT * FROM HashTable_20 WHERE HashBin = @HashBin_20
		UNION ALL
		SELECT * FROM HashTable_21 WHERE HashBin = @HashBin_21
		UNION ALL
		SELECT * FROM HashTable_22 WHERE HashBin = @HashBin_22
		UNION ALL
		SELECT * FROM HashTable_23 WHERE HashBin = @HashBin_23
		UNION ALL
		SELECT * FROM HashTable_24 WHERE HashBin = @HashBin_24
		UNION ALL
		SELECT * FROM HashTable_25 WHERE HashBin = @HashBin_25
	  ) AS Hashes
	 GROUP BY Hashes.SubFingerprintId
	 HAVING COUNT(Hashes.SubFingerprintId) >= @Threshold
	) AS Thresholded ON SubFingerprints.Id = Thresholded.SubFingerprintId	
INNER JOIN Tracks ON SubFingerprints.TrackId = Tracks.Id AND Tracks.GroupId = @GroupId
GO
IF OBJECT_ID('sp_ReadHashDataByTrackId','P') IS NOT NULL
	DROP PROCEDURE sp_ReadHashDataByTrackId
GO
CREATE PROCEDURE sp_ReadHashDataByTrackId
	@TrackId INT
AS
BEGIN
SELECT SubFingerprints.Signature as Signature, SubFingerprints.SequenceNumber as SequenceNumber, HashTable_1.SubFingerprintId as SubFingerprintId, HashTable_1.HashBin as HashBin_1, HashTable_2.HashBin as HashBin_2, HashTable_3.HashBin as HashBin_3, HashTable_4.HashBin as HashBin_4, HashTable_5.HashBin as HashBin_5,
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
GO