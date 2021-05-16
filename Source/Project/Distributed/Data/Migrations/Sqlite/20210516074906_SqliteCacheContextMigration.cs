using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RegionOrebroLan.Caching.Distributed.Data.Migrations.Sqlite
{
	public partial class SqliteCacheContextMigration : Migration
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
					Id = table.Column<string>(type: "TEXT", maxLength: 449, nullable: false, collation: "NOCASE"),
					AbsoluteExpiration = table.Column<DateTime>(type: "TEXT", nullable: true),
					ExpiresAtTime = table.Column<DateTime>(type: "TEXT", nullable: false),
					SlidingExpirationInSeconds = table.Column<long>(type: "INTEGER", nullable: true),
					Value = table.Column<byte[]>(type: "BLOB", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Cache", x => x.Id);
				});

			migrationBuilder.CreateIndex(
				name: "IX_Cache_ExpiresAtTime",
				table: "Cache",
				column: "ExpiresAtTime");
		}

		#endregion
	}
}