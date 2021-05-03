﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RegionOrebroLan.Caching.Distributed.Data;

namespace RegionOrebroLan.Caching.Distributed.Data.Migrations.SqlServer
{
    [DbContext(typeof(SqlServerCacheContext))]
    partial class SqlServerCacheContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.14")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("RegionOrebroLan.Caching.Distributed.Data.Entities.Cache", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(449)")
                        .HasMaxLength(449);

                    b.Property<DateTimeOffset?>("AbsoluteExpiration")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset>("ExpiresAtTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<long?>("SlidingExpirationInSeconds")
                        .HasColumnType("bigint");

                    b.Property<byte[]>("Value")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.HasKey("Id")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.HasIndex("ExpiresAtTime");

                    b.ToTable("Cache");
                });
#pragma warning restore 612, 618
        }
    }
}
