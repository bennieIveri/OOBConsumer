using Microsoft.EntityFrameworkCore.Migrations;

namespace OOB.Data.Migrations
{
    public partial class AddSpCleanup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"CREATE OR ALTER PROCEDURE [dbo].[spCleanup]
	@daysToKeep int
AS
BEGIN
	SET NOCOUNT ON;

	declare @TR_Id bigint;
	select top 100 @TR_Id=TR_Id 
	from TransactionResponse with (nolock)
	where TR_ProcessingTime<convert(date,dateadd(dd,-@daysToKeep,getdate()))
	order by TR_Id;
	if @TR_Id is not null
	begin
		begin transaction;
			delete from TransactionResponseNameValue where TR_Id<=@TR_Id;
			delete from TransactionResponse where TR_Id<=@TR_Id;
		commit transaction;
	end
END";

            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[spCleanup];");
        }
    }
}
