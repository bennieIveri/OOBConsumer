using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OOB.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialFullDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[ApplicationConfig]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[ApplicationConfig]
                    (
                        [AC_ApplicationID] uniqueidentifier NOT NULL,
                        [AC_KeyName] varchar(128) NOT NULL,
                        [AC_Key] varchar(512) NOT NULL,
                        [AC_Value] nvarchar(2048) NOT NULL,
                        CONSTRAINT [PK_ApplicationConfig] PRIMARY KEY ([AC_ApplicationID], [AC_KeyName], [AC_Key])
                    );
                END;

                IF OBJECT_ID(N'[dbo].[ProcessingStatus]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[ProcessingStatus]
                    (
                        [PS_Id] int NOT NULL,
                        [PS_Description] varchar(64) NOT NULL,
                        CONSTRAINT [PK_ProcessingStatus] PRIMARY KEY ([PS_Id])
                    );
                END;

                IF OBJECT_ID(N'[dbo].[TransactionResponse]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[TransactionResponse]
                    (
                        [TR_Id] bigint IDENTITY(1,1) NOT NULL,
                        [TR_RequestID] uniqueidentifier NOT NULL,
                        [PS_Id] int NOT NULL,
                        [TR_ProcessingTime] datetime2 NOT NULL,
                        [TR_ApplicationID] uniqueidentifier NOT NULL,
                        [TR_Mode] varchar(4) NOT NULL,
                        [TR_TransactionIndex] uniqueidentifier NULL,
                        [TR_Acquirer] varchar(64) NULL,
                        [TR_AcquirerCycle] varchar(8) NULL,
                        [TR_Command] nvarchar(max) NULL,
                        [TR_OriginalRequestID] uniqueidentifier NULL,
                        CONSTRAINT [PK_TransactionResponse] PRIMARY KEY ([TR_Id]),
                        CONSTRAINT [FK_TransactionResponse_ProcessingStatus_PS_Id] FOREIGN KEY ([PS_Id]) REFERENCES [dbo].[ProcessingStatus] ([PS_Id]) ON DELETE NO ACTION
                    );
                END;

                IF OBJECT_ID(N'[dbo].[TransactionResponseNameValue]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[TransactionResponseNameValue]
                    (
                        [TRNV_Id] bigint IDENTITY(1,1) NOT NULL,
                        [TR_Id] bigint NOT NULL,
                        [TRNV_Name] varchar(128) NOT NULL,
                        [TRNV_Value] nvarchar(2048) NOT NULL,
                        CONSTRAINT [PK_TransactionResponseNameValue] PRIMARY KEY ([TRNV_Id]),
                        CONSTRAINT [FK_TransactionResponseNameValue_TransactionResponse_TR_Id] FOREIGN KEY ([TR_Id]) REFERENCES [dbo].[TransactionResponse] ([TR_Id]) ON DELETE CASCADE
                    );
                END;

                IF NOT EXISTS
                (
                    SELECT 1
                    FROM sys.indexes
                    WHERE [name] = N'IX_TransactionResponse_PS_Id'
                      AND [object_id] = OBJECT_ID(N'[dbo].[TransactionResponse]')
                )
                BEGIN
                    CREATE INDEX [IX_TransactionResponse_PS_Id] ON [dbo].[TransactionResponse] ([PS_Id]);
                END;

                IF NOT EXISTS
                (
                    SELECT 1
                    FROM sys.indexes
                    WHERE [name] = N'IX_TransactionResponseNameValue_TR_Id'
                      AND [object_id] = OBJECT_ID(N'[dbo].[TransactionResponseNameValue]')
                )
                BEGIN
                    CREATE INDEX [IX_TransactionResponseNameValue_TR_Id] ON [dbo].[TransactionResponseNameValue] ([TR_Id]);
                END;
                """);

            migrationBuilder.Sql(
                """
                CREATE OR ALTER PROCEDURE [dbo].[spCleanup]
                    @daysToKeep int
                AS
                BEGIN
                    SET NOCOUNT ON;

                    DECLARE @TR_Id bigint;

                    SELECT TOP 100 @TR_Id = [TR_Id]
                    FROM [dbo].[TransactionResponse] WITH (NOLOCK)
                    WHERE [TR_ProcessingTime] < CONVERT(date, DATEADD(day, -@daysToKeep, GETDATE()))
                    ORDER BY [TR_Id];

                    IF @TR_Id IS NOT NULL
                    BEGIN
                        BEGIN TRANSACTION;
                            DELETE FROM [dbo].[TransactionResponseNameValue] WHERE [TR_Id] <= @TR_Id;
                            DELETE FROM [dbo].[TransactionResponse] WHERE [TR_Id] <= @TR_Id;
                        COMMIT TRANSACTION;
                    END
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP PROCEDURE IF EXISTS [dbo].[spCleanup];

                DROP TABLE IF EXISTS [dbo].[TransactionResponseNameValue];
                DROP TABLE IF EXISTS [dbo].[TransactionResponse];
                DROP TABLE IF EXISTS [dbo].[ApplicationConfig];
                DROP TABLE IF EXISTS [dbo].[ProcessingStatus];
                """);
        }
    }
}
