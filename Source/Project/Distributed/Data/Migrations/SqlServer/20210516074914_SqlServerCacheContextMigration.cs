using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RegionOrebroLan.Caching.Distributed.Data.Migrations.SqlServer
{
	public partial class SqlServerCacheContextMigration : Migration
	{
		#region Methods

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "Cache");
		}

		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Cache",
				columns: table => new
				{
					Id = table.Column<string>(type: "nvarchar(449)", maxLength: 449, nullable: false),
					AbsoluteExpiration = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
					ExpiresAtTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
					SlidingExpirationInSeconds = table.Column<long>(type: "bigint", nullable: true),
					Value = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Cache", x => x.Id)
						.Annotation("SqlServer:Clustered", true);
				});

			migrationBuilder.CreateIndex(
				name: "IX_Cache_ExpiresAtTime",
				table: "Cache",
				column: "ExpiresAtTime");
		}

		#endregion
	}
}