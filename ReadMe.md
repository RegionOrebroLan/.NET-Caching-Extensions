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

## 3 Notes

The target-frameworks are:

	<TargetFrameworks>net5.0;netcoreapp3.1;netstandard2.0</TargetFrameworks>

But integration-testing with net462 - net48 does not work.

	 AddDistributedCache_Sqlite_Test
	   Source: ServiceCollectionExtensionTest.cs line 47
	   Duration: 3,2 sec

	  Message: 
		Test method IntegrationTests.Distributed.DependencyInjection.Extensions.ServiceCollectionExtensionTest.AddDistributedCache_Sqlite_Test threw exception: 
		System.DllNotFoundException: Det gick inte att läsa in DLL-filen e_sqlite3: Det går inte att hitta den angivna modulen. (Undantag från HRESULT: 0x8007007E)
	  Stack Trace: 
		NativeMethods.sqlite3_libversion_number()
		ISQLite3Provider.sqlite3_libversion_number()
		raw.SetProvider(ISQLite3Provider imp)
		Batteries.Init()
		SqliteCache.cctor()
		--- Slut på stackspårningen från föregående plats där ett undantag utlöstes ---
		CallSiteRuntimeResolver.VisitConstructor(ConstructorCallSite constructorCallSite, RuntimeResolverContext context)
		CallSiteVisitor`2.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
		CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
		CallSiteRuntimeResolver.VisitRootCache(ServiceCallSite singletonCallSite, RuntimeResolverContext context)
		CallSiteVisitor`2.VisitCallSite(ServiceCallSite callSite, TArgument argument)
		CallSiteRuntimeResolver.Resolve(ServiceCallSite callSite, ServiceProviderEngineScope scope)
		<>c__DisplayClass1_0.<RealizeService>b__0(ServiceProviderEngineScope scope)
		ServiceProviderEngine.GetService(Type serviceType, ServiceProviderEngineScope serviceProviderEngineScope)
		ServiceProviderEngine.GetService(Type serviceType)
		ServiceProvider.GetService(Type serviceType)
		ServiceProviderServiceExtensions.GetRequiredService(IServiceProvider provider, Type serviceType)
		ServiceProviderServiceExtensions.GetRequiredService[T](IServiceProvider provider)
		<AddDistributedCache_Test>d__7.MoveNext() line 84
		--- Slut på stackspårningen från föregående plats där ett undantag utlöstes ---
		TaskAwaiter.ThrowForNonSuccess(Task task)
		TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
		TaskAwaiter.GetResult()
		<AddDistributedCache_Sqlite_Test>d__5.MoveNext() line 49
		--- Slut på stackspårningen från föregående plats där ett undantag utlöstes ---
		TaskAwaiter.ThrowForNonSuccess(Task task)
		TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
		ThreadOperations.ExecuteWithAbortSafety(Action action)

Maybe we should set the target-frameworks to:

	<TargetFrameworks>net5.0;netcoreapp3.1;netstandard2.1</TargetFrameworks>