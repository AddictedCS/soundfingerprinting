USE FingerprintsDb
GO
IF NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'HashTable_1')
BEGIN
	CREATE TABLE HashTable_1
	(
		HashBin BIGINT NOT NULL,								    
		SubFingerprintId BIGINT NOT NULL,
		CONSTRAINT PK_HashBinsMinHashId_1 PRIMARY KEY(HashBin, SubFingerprintId)
	)

    insert into HashTable_1(HashBin, SubFingerprintId)
	select HashBin, SubFingerprintId from HashBinsMinHash where hashtable = 1
END
go
IF NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'HashTable_2')
BEGIN
	CREATE TABLE HashTable_2
	(
		HashBin BIGINT NOT NULL,								    
		SubFingerprintId BIGINT NOT NULL,
		CONSTRAINT PK_HashBinsMinHashId_2 PRIMARY KEY(HashBin, SubFingerprintId)
	)

    insert into HashTable_2(HashBin, SubFingerprintId)
	select HashBin, SubFingerprintId from HashBinsMinHash where hashtable = 2
END
GO
IF NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'HashTable_3')
BEGIN
	CREATE TABLE HashTable_3
	(
		HashBin BIGINT NOT NULL,								    
		SubFingerprintId BIGINT NOT NULL,
		CONSTRAINT PK_HashBinsMinHashId_3 PRIMARY KEY(HashBin, SubFingerprintId)
	)

    insert into HashTable_3(HashBin, SubFingerprintId)
	select HashBin, SubFingerprintId from HashBinsMinHash where hashtable = 3
END
GO
IF NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'HashTable_4')
BEGIN
	CREATE TABLE HashTable_4
	(
		HashBin BIGINT NOT NULL,								    
		SubFingerprintId BIGINT NOT NULL,
		CONSTRAINT PK_HashBinsMinHashId_4 PRIMARY KEY(HashBin, SubFingerprintId)
	)

    insert into HashTable_4(HashBin, SubFingerprintId)
	select HashBin, SubFingerprintId from HashBinsMinHash where hashtable = 4
END
GO
IF NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'HashTable_5')
BEGIN
	CREATE TABLE HashTable_5
	(
		HashBin BIGINT NOT NULL,								    
		SubFingerprintId BIGINT NOT NULL,
		CONSTRAINT PK_HashBinsMinHashId_5 PRIMARY KEY(HashBin, SubFingerprintId)
	)

    insert into HashTable_5(HashBin, SubFingerprintId)
	select HashBin, SubFingerprintId from HashBinsMinHash where hashtable = 5
END
GO
IF NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'HashTable_6')
BEGIN
	CREATE TABLE HashTable_6
	(
		HashBin BIGINT NOT NULL,								    
		SubFingerprintId BIGINT NOT NULL,
		CONSTRAINT PK_HashBinsMinHashId_6 PRIMARY KEY(HashBin, SubFingerprintId)
	)

    insert into HashTable_6(HashBin, SubFingerprintId)
	select HashBin, SubFingerprintId from HashBinsMinHash where hashtable = 6
END
GO
IF NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'HashTable_7')
BEGIN
	CREATE TABLE HashTable_7
	(
		HashBin BIGINT NOT NULL,								    
		SubFingerprintId BIGINT NOT NULL,
		CONSTRAINT PK_HashBinsMinHashId_7 PRIMARY KEY(HashBin, SubFingerprintId)
	)

    insert into HashTable_7(HashBin, SubFingerprintId)
	select HashBin, SubFingerprintId from HashBinsMinHash where hashtable = 7
END
GO
IF NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'HashTable_8')
BEGIN
	CREATE TABLE HashTable_8
	(
		HashBin BIGINT NOT NULL,								    
		SubFingerprintId BIGINT NOT NULL,
		CONSTRAINT PK_HashBinsMinHashId_8 PRIMARY KEY(HashBin, SubFingerprintId)
	)

    insert into HashTable_8(HashBin, SubFingerprintId)
	select HashBin, SubFingerprintId from HashBinsMinHash where hashtable = 8
END
GO
IF NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'HashTable_9')
BEGIN
	CREATE TABLE HashTable_9
	(
		HashBin BIGINT NOT NULL,								    
		SubFingerprintId BIGINT NOT NULL,
		CONSTRAINT PK_HashBinsMinHashId_9 PRIMARY KEY(HashBin, SubFingerprintId)
	)

    insert into HashTable_9(HashBin, SubFingerprintId)
	select HashBin, SubFingerprintId from HashBinsMinHash where hashtable = 9
END
GO
IF NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'HashTable_10')
BEGIN
	CREATE TABLE HashTable_10
	(
		HashBin BIGINT NOT NULL,								    
		SubFingerprintId BIGINT NOT NULL,
		CONSTRAINT PK_HashBinsMinHashId_10 PRIMARY KEY(HashBin, SubFingerprintId)
	)

    insert into HashTable_10(HashBin, SubFingerprintId)
	select HashBin, SubFingerprintId from HashBinsMinHash where hashtable = 10
END
GO
IF NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'HashTable_11')
BEGIN
	CREATE TABLE HashTable_11
	(
		HashBin BIGINT NOT NULL,								    
		SubFingerprintId BIGINT NOT NULL,
		CONSTRAINT PK_HashBinsMinHashId_11 PRIMARY KEY(HashBin, SubFingerprintId)
	)

    insert into HashTable_11(HashBin, SubFingerprintId)
	select HashBin, SubFingerprintId from HashBinsMinHash where hashtable = 11
END
GO
IF NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'HashTable_12')
BEGIN
	CREATE TABLE HashTable_12
	(
		HashBin BIGINT NOT NULL,								    
		SubFingerprintId BIGINT NOT NULL,
		CONSTRAINT PK_HashBinsMinHashId_12 PRIMARY KEY(HashBin, SubFingerprintId)
	)

    insert into HashTable_12(HashBin, SubFingerprintId)
	select HashBin, SubFingerprintId from HashBinsMinHash where hashtable = 12
END
GO
IF NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'HashTable_13')
BEGIN
	CREATE TABLE HashTable_13
	(
		HashBin BIGINT NOT NULL,								    
		SubFingerprintId BIGINT NOT NULL,
		CONSTRAINT PK_HashBinsMinHashId_13 PRIMARY KEY(HashBin, SubFingerprintId)
	)

    insert into HashTable_13(HashBin, SubFingerprintId)
	select HashBin, SubFingerprintId from HashBinsMinHash where hashtable = 13
END
GO
IF NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'HashTable_14')
BEGIN
	CREATE TABLE HashTable_14
	(
		HashBin BIGINT NOT NULL,								    
		SubFingerprintId BIGINT NOT NULL,
		CONSTRAINT PK_HashBinsMinHashId_14 PRIMARY KEY(HashBin, SubFingerprintId)
	)

    insert into HashTable_14(HashBin, SubFingerprintId)
	select HashBin, SubFingerprintId from HashBinsMinHash where hashtable = 14
END
GO
IF NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'HashTable_15')
BEGIN
	CREATE TABLE HashTable_15
	(
		HashBin BIGINT NOT NULL,								    
		SubFingerprintId BIGINT NOT NULL,
		CONSTRAINT PK_HashBinsMinHashId_15 PRIMARY KEY(HashBin, SubFingerprintId)
	)

    insert into HashTable_15(HashBin, SubFingerprintId)
	select HashBin, SubFingerprintId from HashBinsMinHash where hashtable = 15
END
GO
IF NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'HashTable_16')
BEGIN
	CREATE TABLE HashTable_16
	(
		HashBin BIGINT NOT NULL,								    
		SubFingerprintId BIGINT NOT NULL,
		CONSTRAINT PK_HashBinsMinHashId_16 PRIMARY KEY(HashBin, SubFingerprintId)
	)

    insert into HashTable_16(HashBin, SubFingerprintId)
	select HashBin, SubFingerprintId from HashBinsMinHash where hashtable = 16
END
GO
IF NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'HashTable_17')
BEGIN
	CREATE TABLE HashTable_17
	(
		HashBin BIGINT NOT NULL,								    
		SubFingerprintId BIGINT NOT NULL,
		CONSTRAINT PK_HashBinsMinHashId_17 PRIMARY KEY(HashBin, SubFingerprintId)
	)

    insert into HashTable_17(HashBin, SubFingerprintId)
	select HashBin, SubFingerprintId from HashBinsMinHash where hashtable = 17
END
GO
IF NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'HashTable_18')
BEGIN
	CREATE TABLE HashTable_18
	(
		HashBin BIGINT NOT NULL,								    
		SubFingerprintId BIGINT NOT NULL,
		CONSTRAINT PK_HashBinsMinHashId_18 PRIMARY KEY(HashBin, SubFingerprintId)
	)

    insert into HashTable_18(HashBin, SubFingerprintId)
	select HashBin, SubFingerprintId from HashBinsMinHash where hashtable = 18
END
GO
IF NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'HashTable_19')
BEGIN
	CREATE TABLE HashTable_19
	(
		HashBin BIGINT NOT NULL,								    
		SubFingerprintId BIGINT NOT NULL,
		CONSTRAINT PK_HashBinsMinHashId_19 PRIMARY KEY(HashBin, SubFingerprintId)
	)

    insert into HashTable_19(HashBin, SubFingerprintId)
	select HashBin, SubFingerprintId from HashBinsMinHash where hashtable = 19
END
GO
IF NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'HashTable_20')
BEGIN
	CREATE TABLE HashTable_20
	(
		HashBin BIGINT NOT NULL,								    
		SubFingerprintId BIGINT NOT NULL,
		CONSTRAINT PK_HashBinsMinHashId_20 PRIMARY KEY(HashBin, SubFingerprintId)
	)

    insert into HashTable_20(HashBin, SubFingerprintId)
	select HashBin, SubFingerprintId from HashBinsMinHash where hashtable = 20
END
GO
IF NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'HashTable_21')
BEGIN
	CREATE TABLE HashTable_21
	(
		HashBin BIGINT NOT NULL,								    
		SubFingerprintId BIGINT NOT NULL,
		CONSTRAINT PK_HashBinsMinHashId_21 PRIMARY KEY(HashBin, SubFingerprintId)
	)

    insert into HashTable_21(HashBin, SubFingerprintId)
	select HashBin, SubFingerprintId from HashBinsMinHash where hashtable = 21
END
GO
IF NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'HashTable_22')
BEGIN
	CREATE TABLE HashTable_22
	(
		HashBin BIGINT NOT NULL,								    
		SubFingerprintId BIGINT NOT NULL,
		CONSTRAINT PK_HashBinsMinHashId_22 PRIMARY KEY(HashBin, SubFingerprintId)
	)

    insert into HashTable_22(HashBin, SubFingerprintId)
	select HashBin, SubFingerprintId from HashBinsMinHash where hashtable = 22
END
GO
IF NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'HashTable_23')
BEGIN
	CREATE TABLE HashTable_23
	(
		HashBin BIGINT NOT NULL,								    
		SubFingerprintId BIGINT NOT NULL,
		CONSTRAINT PK_HashBinsMinHashId_23 PRIMARY KEY(HashBin, SubFingerprintId)
	)

    insert into HashTable_23(HashBin, SubFingerprintId)
	select HashBin, SubFingerprintId from HashBinsMinHash where hashtable = 23
END
GO
IF NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'HashTable_24')
BEGIN
	CREATE TABLE HashTable_24
	(
		HashBin BIGINT NOT NULL,								    
		SubFingerprintId BIGINT NOT NULL,
		CONSTRAINT PK_HashBinsMinHashId_24 PRIMARY KEY(HashBin, SubFingerprintId)
	)

    insert into HashTable_24(HashBin, SubFingerprintId)
	select HashBin, SubFingerprintId from HashBinsMinHash where hashtable = 24
END

GO
IF NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'HashTable_25')
BEGIN
	CREATE TABLE HashTable_25
	(
		HashBin BIGINT NOT NULL,								    
		SubFingerprintId BIGINT NOT NULL,
		CONSTRAINT PK_HashBinsMinHashId_25 PRIMARY KEY(HashBin, SubFingerprintId)
	)

    insert into HashTable_25(HashBin, SubFingerprintId)
	select HashBin, SubFingerprintId from HashBinsMinHash where hashtable = 25
END

GO

IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND TABLE_NAME = 'HashBinsMinHash'))
BEGIN
	drop table HashBinsMinHash
END




























