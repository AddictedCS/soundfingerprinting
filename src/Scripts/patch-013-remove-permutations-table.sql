USE FingerprintsDb
GO
-- drop primary key constraint on Permutations table
IF EXISTS(SELECT * 
    FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
    WHERE CONSTRAINT_NAME ='PK_PermutationsId')
BEGIN
	ALTER TABLE Permutations drop constraint PK_PermutationsId
END
GO
-- drop table Permutations
IF EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'Permutations')
BEGIN
	DROP TABLE Permutations
END
GO
IF OBJECT_ID('sp_ReadPermutations','P') IS NOT NULL
	DROP PROCEDURE sp_ReadPermutations
GO
IF OBJECT_ID('sp_ReadPermutationById','P') IS NOT NULL
	DROP PROCEDURE sp_ReadPermutationById
GO
