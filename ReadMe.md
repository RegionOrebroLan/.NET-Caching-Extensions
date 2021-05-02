# .NET-Caching-Extensions

Additions and extensions for .NET caching.

[![NuGet](https://img.shields.io/nuget/v/RegionOrebroLan.Caching.svg?label=NuGet)](https://www.nuget.org/packages/RegionOrebroLan.Caching)

## 1 Configurable distributed cache

### 1.1 Examples

- [Memory](/Source/Sample/Application/appsettings.Memory-DistributedCache.json)
- [Redis](/Source/Sample/Application/appsettings.Redis-DistributedCache.json)
- [Sqlite (1)](/Source/Sample/Application/appsettings.Sqlite-DistributedCache-1.json)
- [Sqlite (2)](/Source/Sample/Application/appsettings.Sqlite-DistributedCache-2.json)
- [SqlServer (1)](/Source/Sample/Application/appsettings.SqlServer-DistributedCache-1.json)
- [SqlServer (2)](/Source/Sample/Application/appsettings.SqlServer-DistributedCache-2.json)

## 2 Development

### 2.1 Migrations

We might want to create/recreate migrations. If we can accept data-loss we can recreate the migrations otherwhise we will have to update them.

Copy all the commands below and run them in the Package Manager Console for the affected database-context.

If you want more migration-information you can add the -Verbose parameter:

	Add-Migration TheMigration -Context TheDatabaseContext -OutputDir Data/Migrations -Project Project -Verbose;

**Important!** Before running the commands below you need to ensure the "Project"-project is set as startup-project. 

#### 2.1.1 CacheContext (Distributed)

##### 2.1.1.1 Create migrations

	Write-Host "Removing migrations...";
	Remove-Migration -Context SqlServerCacheContext -Force -Project Project;
	Write-Host "Removing current migrations-directory...";
	Remove-Item "Project\Distributed\Data\Migrations" -ErrorAction Ignore -Force -Recurse;
	Write-Host "Creating migrations...";
	Add-Migration SqlServerCacheContextMigration -Context SqlServerCacheContext -OutputDir Distributed/Data/Migrations/SqlServer -Project Project;
	Write-Host "Finnished";

##### 2.1.1.2 Update migrations

	Write-Host "Updating migrations...";
	Add-Migration SqlServerCacheContextMigrationUpdate -Context SqlServerCacheContext -OutputDir Distributed/Data/Migrations/SqlServer -Project Project;
	Write-Host "Finnished";