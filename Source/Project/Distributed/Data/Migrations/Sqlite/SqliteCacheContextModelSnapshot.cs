﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RegionOrebroLan.Caching.Distributed.Data;

namespace RegionOrebroLan.Caching.Distributed.Data.Migrations.Sqlite
{
    [DbContext(typeof(SqliteCacheContext))]
    partial class SqliteCacheContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.5");

            modelBuilder.Entity("RegionOrebroLan.Caching.Distributed.Data.Entities.CacheEntry<System.DateTime>", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(449)
                        .HasColumnType("TEXT")
                        .UseCollation("NOCASE");

                    b.Property<DateTime?>("AbsoluteExpiration")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ExpiresAtTime")
                        .HasColumnType("TEXT");

                    b.Property<long?>("SlidingExpirationInSeconds")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("Value")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.HasKey("Id");

                    b.HasIndex("ExpiresAtTime");

                    b.ToTable("Cache");
                });
#pragma warning restore 612, 618
        }
    }
}
