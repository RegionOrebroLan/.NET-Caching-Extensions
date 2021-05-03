using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RegionOrebroLan.Caching.Distributed.Data.Migrations.SqlServer
{
	[CLSCompliant(false)]
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
					Id = table.Column<string>(maxLength: 449, nullable: false),
					AbsoluteExpiration = table.Column<DateTimeOffset>(nullable: true),
					ExpiresAtTime = table.Column<DateTimeOffset>(nullable: false),
					SlidingExpirationInSeconds = table.Column<long>(nullable: true),
					Value = table.Column<byte[]>(nullable: false)
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