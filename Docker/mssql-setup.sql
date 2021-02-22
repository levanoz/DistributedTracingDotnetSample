USE [master]
GO

PRINT 'Create SampleDb Database and SampleDbUser user';
GO

IF DB_ID('SampleDb') IS NULL
BEGIN
	CREATE DATABASE [SampleDb];		
END

USE [master]
GO

IF NOT EXISTS 
	(SELECT name  
	 FROM master.sys.server_principals
	 WHERE name = 'SampleDbUser') 
	 AND DB_ID('SampleDb') IS NOT NULL
BEGIN
	CREATE LOGIN [SampleDbUser] WITH PASSWORD=N'Aa123456Qw!@', DEFAULT_DATABASE=[SampleDb], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF;

	USE [SampleDb]

	CREATE USER [SampleDbUser] FOR LOGIN [SampleDbUser];

	ALTER ROLE [db_datareader] ADD MEMBER [SampleDbUser];

	ALTER ROLE [db_datawriter] ADD MEMBER [SampleDbUser];

	ALTER ROLE [db_owner] ADD MEMBER [SampleDbUser];
END