# .NET-Caching-Extensions

Additions and extensions for .NET caching.

[![NuGet](https://img.shields.io/nuget/v/RegionOrebroLan.Caching.svg?label=NuGet)](https://www.nuget.org/packages/RegionOrebroLan.Caching)

## 1 Features

### 1.1 Configurable distributed cache

- Configuring data-protection through AppSettings.json

#### 1.1.1 Examples

- [Memory](/Source/Sample/Application/appsettings.Memory.json)
- [Redis (1)](/Source/Sample/Application/appsettings.Redis-1.json) - you need to setup Redis, see below
- [Redis (2)](/Source/Sample/Application/appsettings.Redis-2.json) - || -
- [Redis (3)](/Source/Sample/Application/appsettings.Redis-3.json) - || -
- [Redis (4)](/Source/Sample/Application/appsettings.Redis-4.json) - || -
- [Sqlite](/Source/Sample/Application/appsettings.Sqlite.json)
- [SqlServer (1)](/Source/Sample/Application/appsettings.SqlServer-1.json)
- [SqlServer (2)](/Source/Sample/Application/appsettings.SqlServer-2.json)

##### 1.1.1.1 Redis

Setup Redis locally with Docker:

	docker run --rm -it -p 6379:6379 redis

### 1.2 EntityFramework distributed cache

- IDistributedCache-implementation with EntityFramework

May be used for development and test. May not be suted for production (regarding speed). But the database-context implementations can be used with a custom implementation of a database-driven cache built with sql-queries. That may speed it up. In that scenario the database-context is used for migrations. See the setup of SqlServerCache in this solution:

- [SqlServer (1)](/Source/Sample/Application/appsettings.SqlServer-1.json)
- [SqlServer (2)](/Source/Sample/Application/appsettings.SqlServer-2.json)

#### 1.2.1 Sqlite

- [Implementation](/Source/Project/Distributed/SqliteCache.cs)
- [Example](/Source/Sample/Application/appsettings.Sqlite.json)

#### 1.2.2 Base-classes

- [DateTimeContextCache](/Source/Project/Distributed/DateTimeContextCache.cs)
- [DateTimeOffsetContextCache](/Source/Project/Distributed/DateTimeOffsetContextCache.cs)

## 2 Development

### 2.1 Migrations

We might want to create/recreate migrations. If we can accept data-loss we can recreate the migrations otherwhise we will have to update them.

Copy all the commands below and run them in the Package Manager Console for the affected database-context.

If you want more migration-information you can add the -Verbose parameter:

	Add-Migration TheMigration -Context TheDatabaseContext -OutputDir Data/Migrations -Project Project -StartupProject Application -Verbose;

#### 2.1.1 CacheContext (Distributed)

##### 2.1.1.1 Create migrations

	Write-Host "Removing migrations...";
	Remove-Migration -Context SqliteCacheContext -Force -Project Project -StartupProject Application;
	Remove-Migration -Context SqlServerCacheContext -Force -Project Project -StartupProject Application;
	Write-Host "Removing current migrations-directory...";
	Remove-Item "Project\Distributed\Data\Migrations" -ErrorAction Ignore -Force -Recurse;
	Write-Host "Creating migrations...";
	Add-Migration SqliteCacheContextMigration -Context SqliteCacheContext -OutputDir Distributed/Data/Migrations/Sqlite -Project Project -StartupProject Application;
	Add-Migration SqlServerCacheContextMigration -Context SqlServerCacheContext -OutputDir Distributed/Data/Migrations/SqlServer -Project Project -StartupProject Application;
	Write-Host "Finnished";

##### 2.1.1.2 Update migrations

	Write-Host "Updating migrations...";
	Add-Migration SqliteCacheContextMigrationUpdate -Context SqliteCacheContext -OutputDir Distributed/Data/Migrations/Sqlite -Project Project -StartupProject Application;
	Add-Migration SqlServerCacheContextMigrationUpdate -Context SqlServerCacheContext -OutputDir Distributed/Data/Migrations/SqlServer -Project Project -StartupProject Application;
	Write-Host "Finnished";