# EntityFrameworkCore.Cacheable

[![Build status](https://ci.appveyor.com/api/projects/status/8h2kg4gjcv85w6wg?svg=true)](https://ci.appveyor.com/project/SteffenMangold/entityframeworkcore-cacheable)
[![Maintainability](https://api.codeclimate.com/v1/badges/541ce9c419c532bcd292/maintainability)](https://codeclimate.com/github/SteffenMangold/EntityFrameworkCore.Cacheable/maintainability)
[![Total alerts](https://img.shields.io/lgtm/alerts/g/SteffenMangold/EntityFrameworkCore.Cacheable.svg?logo=lgtm&logoWidth=18)](https://lgtm.com/projects/g/SteffenMangold/EntityFrameworkCore.Cacheable/alerts/)

A high performance second level query cache for [Entity Framework Core](https://github.com/aspnet/EntityFrameworkCore).

## What is EF Core Cacheable?

Entity Framework (EF) Core Cacheable is an extention library for the popular Entity Framework data access technology.

It provides caching functionality for all types of query results. Based on expression tree and parameters, the context decide rather to execute query against database or returning result from memory.

## How fast is it?


This a sample result of 1,000 uncached and cached queries, called agains a really good performing MSSQL-database.

```
Average database query duration [+00:00:00.1698972].
Average cache query duration [+00:00:00.0000650].
Cached queries are x2,611 times faster.
```

Even with a InMemory test database, the results are significant faster.

```
Average database query duration [+00:00:00.0026076].
Average cache query duration [+00:00:00.0000411].
Cached queries are x63 times faster.
```

The performance gain can be even higher, depending on the database performance.


Install via NuGet
-----------------

You can view package page on NuGet.

[![Version Status](https://img.shields.io/nuget/v/EntityFrameworkCore.Cacheable.svg)](https://www.nuget.org/packages/EntityFrameworkCore.Cacheable/)

To install `EntityFrameworkCore.Cacheable`, run the following command in the Package Manager Console:

```
PM> Install-Package EntityFrameworkCore.Cacheable
```


This library also uses the [Data.HashFunction](https://github.com/brandondahler/Data.HashFunction/) and [aspnet.Extensions](https://github.com/aspnet/Extensions) as InMemory cache.


## Usage


### Configuration

You can either override the `OnModelCreating` method in your derived context and use the ModelBuilder API to configure your model and add cachable support.

```csharp
public partial class CacheableContext : DbContext
{
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		if (!optionsBuilder.IsConfigured)
		{
			optionsBuilder.UseSecondLevelMemoryCache();
		}
	}

    [...]
```

Or use the `DbContextOptions` parameter overload of the `DbContext` constructor.

```csharp
var options = new DbContextOptionsBuilder<BloggingContext>()  
	.UseSecondLevelMemoryCache()
	.Options;

using (var cacheableContext = new CacheableContext(options))
{

    [...]
```

### Usage

To get in use of result caching, you simply need to add `.Cacheable(...` to your query and define a TTL parameter.


```csharp
var cacheableQuery = cacheableContext.Books
	.Include(d => d.Pages)
	.ThenInclude(d => d.Lines)
	.Where(d => d.ID == 200)
	.Cacheable(TimeSpan.FromSeconds(60));
```

-----


## Contributors

The following contributors have either created (thats only me :stuck_out_tongue_winking_eye:) the project, have contributed
code, are actively maintaining it (including documentation), or in other ways
being helpfull contributors to this project. 


|                                                                                    | Name                  | GitHub                                                  |
| :--------------------------------------------------------------------------------: | --------------------- | ------------------------------------------------------- |
| <img src="https://avatars.githubusercontent.com/u/20702171?size=72" width="72"/>   | Steffen Mangold       | [@SteffenMangold](https://github.com/SteffenMangold)    |
| <img src="https://avatars.githubusercontent.com/u/1528107?size=72" width="72"/>    | Smit Patel            | [@smitpatel](https://github.com/smitpatel)              |
