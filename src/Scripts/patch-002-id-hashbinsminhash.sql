use FingerprintsDb
go
if exists(SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_TYPE = 'PRIMARY KEY'
	AND TABLE_NAME = 'HashBinsMinHash' AND CONSTRAINT_NAME = 'PK_HashBinsMinHashId')
begin
	alter table hashbinsminhash drop constraint PK_HashBinsMinHashId
end
go
if exists(select * from sys.columns 
   where Name = N'Id' and Object_ID = Object_ID(N'HashBinsMinHash'))
begin
	alter table HashBinsMinHash drop column Id
end