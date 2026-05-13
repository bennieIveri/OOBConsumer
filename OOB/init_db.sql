DECLARE @DBNAME varchar(128) = 'OOBResponse';
DECLARE @SQL nvarchar(max);

SET @SQL = N'CREATE DATABASE ' + QUOTENAME(@DBNAME);
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET ANSI_NULL_DEFAULT OFF';
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET ANSI_NULLS OFF';
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET ANSI_PADDING OFF';
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET ANSI_WARNINGS OFF';
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET ARITHABORT OFF';
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET AUTO_CLOSE OFF';
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET AUTO_SHRINK OFF';
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET AUTO_UPDATE_STATISTICS ON';
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET CURSOR_CLOSE_ON_COMMIT OFF';
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET CURSOR_DEFAULT GLOBAL';
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET CONCAT_NULL_YIELDS_NULL OFF';
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET NUMERIC_ROUNDABORT OFF';
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET QUOTED_IDENTIFIER OFF';
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET RECURSIVE_TRIGGERS OFF';
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET DISABLE_BROKER';
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET AUTO_UPDATE_STATISTICS_ASYNC OFF';
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET DATE_CORRELATION_OPTIMIZATION OFF';
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET TRUSTWORTHY OFF';
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET ALLOW_SNAPSHOT_ISOLATION OFF';
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET PARAMETERIZATION SIMPLE';
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET READ_COMMITTED_SNAPSHOT OFF';
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET HONOR_BROKER_PRIORITY OFF';
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET RECOVERY SIMPLE';
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET MULTI_USER';
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET PAGE_VERIFY CHECKSUM';
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET DB_CHAINING OFF';
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF )';
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET TARGET_RECOVERY_TIME = 60 SECONDS';
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET DELAYED_DURABILITY = DISABLED';
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET ACCELERATED_DATABASE_RECOVERY = OFF';
EXEC sp_executesql @SQL;

EXEC sys.sp_db_vardecimal_storage_format @DBNAME, N'ON';

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET QUERY_STORE = OFF';
EXEC sp_executesql @SQL;

SET @SQL = N'USE ' + QUOTENAME(@DBNAME) + N';
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

CREATE TABLE [dbo].[ApplicationConfig](
    [AC_ApplicationID] [uniqueidentifier] NOT NULL,
    [AC_KeyName] [varchar](128) NOT NULL,
    [AC_Key] [varchar](512) NOT NULL,
    [AC_Value] [nvarchar](2048) NOT NULL,
 CONSTRAINT [PK_ApplicationConfig] PRIMARY KEY CLUSTERED
(
    [AC_ApplicationID] ASC,
    [AC_KeyName] ASC,
    [AC_Key] ASC
));

CREATE TABLE [dbo].[ProcessingStatus](
    [PS_Id] [int] NOT NULL,
    [PS_Description] [varchar](64) NOT NULL,
 CONSTRAINT [PK_ProcessingStatus] PRIMARY KEY CLUSTERED
(
    [PS_Id] ASC
));

CREATE TABLE [dbo].[TransactionResponse](
    [TR_Id] [bigint] IDENTITY(1,1) NOT NULL,
    [TR_RequestID] [uniqueidentifier] NOT NULL,
    [PS_Id] [int] NOT NULL,
    [TR_ProcessingTime] [datetime] NOT NULL,
    [TR_ApplicationID] [uniqueidentifier] NOT NULL,
    [TR_Mode] [varchar](4) NOT NULL,
    [TR_TransactionIndex] [uniqueidentifier] NULL,
    [TR_Acquirer] [varchar](64) NULL,
    [TR_AcquirerCycle] [varchar](8) NULL,
    [TR_Command] [varchar](64) NULL,
    [TR_OriginalRequestID] [uniqueidentifier] NULL,
 CONSTRAINT [PK_TransactionResponse] PRIMARY KEY CLUSTERED
(
    [TR_Id] ASC
));

CREATE TABLE [dbo].[TransactionResponseNameValue](
    [TRNV_Id] [bigint] IDENTITY(1,1) NOT NULL,
    [TR_Id] [bigint] NOT NULL,
    [TRNV_Name] [varchar](128) NOT NULL,
    [TRNV_Value] [nvarchar](2048) NOT NULL,
    [TRNV_ValueIndexedLeft384] AS (left([TRNV_Value],(384))) PERSISTED,
    [TRNV_ValueIndexedSha256] AS (hashbytes(''SHA2_256'',[TRNV_Value])) PERSISTED,
 CONSTRAINT [PK_TransactionResponseNameValue] PRIMARY KEY CLUSTERED
(
    [TRNV_Id] ASC
));

CREATE NONCLUSTERED INDEX [IX_TransactionResponse_TR_AcquirerCycle]
ON [dbo].[TransactionResponse]([TR_AcquirerCycle] ASC)
INCLUDE([TR_Acquirer]);

CREATE NONCLUSTERED INDEX [IX_TransactionResponse_TR_ApplicationID]
ON [dbo].[TransactionResponse]([TR_ApplicationID] ASC);

CREATE NONCLUSTERED INDEX [IX_TransactionResponse_TR_ProcessingTime]
ON [dbo].[TransactionResponse]([TR_ProcessingTime] ASC);

CREATE UNIQUE NONCLUSTERED INDEX [IX_TransactionResponse_TR_RequestID]
ON [dbo].[TransactionResponse]([TR_RequestID] ASC)
WITH (FILLFACTOR = 90);

CREATE NONCLUSTERED INDEX [IX_TransactionResponse_TR_TransactionIndex]
ON [dbo].[TransactionResponse]([TR_TransactionIndex] ASC)
WITH (FILLFACTOR = 90);

CREATE NONCLUSTERED INDEX [IX_TransactionResponseNameValue_TR_Id]
ON [dbo].[TransactionResponseNameValue]([TR_Id] ASC)
WITH (FILLFACTOR = 90);

CREATE NONCLUSTERED INDEX [IX_TransactionResponseNameValue_TRNV_Name]
ON [dbo].[TransactionResponseNameValue]([TRNV_Name] ASC)
INCLUDE([TRNV_Value])
WITH (FILLFACTOR = 90);

CREATE NONCLUSTERED INDEX [IX_TransactionResponseNameValue_TRNV_ValueIndexedLeft384]
ON [dbo].[TransactionResponseNameValue]([TRNV_ValueIndexedLeft384] ASC)
WITH (FILLFACTOR = 90);

CREATE NONCLUSTERED INDEX [IX_TransactionResponseNameValue_TRNV_ValueIndexedSha256]
ON [dbo].[TransactionResponseNameValue]([TRNV_ValueIndexedSha256] ASC)
WITH (FILLFACTOR = 90);

ALTER TABLE [dbo].[TransactionResponse]
ADD CONSTRAINT [DF_TransactionResponse_TR_ProcessingTime]
DEFAULT (getdate()) FOR [TR_ProcessingTime];

ALTER TABLE [dbo].[TransactionResponse]
ADD CONSTRAINT [FK_TransactionResponse_ProcessingStatus]
FOREIGN KEY([PS_Id]) REFERENCES [dbo].[ProcessingStatus] ([PS_Id]);

ALTER TABLE [dbo].[TransactionResponseNameValue]
ADD CONSTRAINT [FK_TransactionResponseNameValue_TransactionResponse]
FOREIGN KEY([TR_Id]) REFERENCES [dbo].[TransactionResponse] ([TR_Id]);

CREATE PROCEDURE [dbo].[spCleanup]
    @daysToKeep int
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TR_Id bigint;

    SELECT TOP 100 @TR_Id = TR_Id
    FROM dbo.TransactionResponse WITH (NOLOCK)
    WHERE TR_ProcessingTime < CONVERT(date, DATEADD(dd, -@daysToKeep, GETDATE()))
    ORDER BY TR_Id;

    IF @TR_Id IS NOT NULL
    BEGIN
        BEGIN TRANSACTION;
            DELETE FROM dbo.TransactionResponseNameValue WHERE TR_Id <= @TR_Id;
            DELETE FROM dbo.TransactionResponse WHERE TR_Id <= @TR_Id;
        COMMIT TRANSACTION;
    END
END;';
EXEC sp_executesql @SQL;

SET @SQL = N'ALTER DATABASE ' + QUOTENAME(@DBNAME) + N' SET READ_WRITE';
EXEC sp_executesql @SQL;