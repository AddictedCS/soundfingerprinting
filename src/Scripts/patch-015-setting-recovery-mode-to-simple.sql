USE FingerprintsDb
GO
ALTER DATABASE FingerprintsDb SET RECOVERY SIMPLE;
GO
CHECKPOINT;
GO
CHECKPOINT; -- run twice to ensure file wrap-around
GO
DBCC SHRINKFILE(FingerprintsDb_log, 1024);
GO