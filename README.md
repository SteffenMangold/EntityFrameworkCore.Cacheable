

<p align="center">
  <img src="https://raw.githubusercontent.com/SteffenMangold/EntityFrameworkCore.Cacheable/master/nuget_icon_light.png?size=72" width="96"/>
</p>

<h3 align="center">
  EntityFrameworkCore.Cacheable
</h3>

<p align="center">
  A high performance second level query cache for <a href="https://github.com/aspnet/EntityFrameworkCore">Entity Framework Core</a>.
</p>

<p align="center">
  <a href="https://ci.appveyor.com/project/SteffenMangold/entityframeworkcore-cacheable"><img src="https://ci.appveyor.com/api/projects/status/8h2kg4gjcv85w6wg?svg=true"></a>
  <a href="https://codeclimate.com/github/SteffenMangold/EntityFrameworkCore.Cacheable/maintainability"><img src="https://api.codeclimate.com/v1/badges/541ce9c419c532bcd292/maintainability"></a>
  <a href="https://lgtm.com/projects/g/SteffenMangold/EntityFrameworkCore.Cacheable/alerts/"><img src="https://img.shields.io/lgtm/alerts/g/SteffenMangold/EntityFrameworkCore.Cacheable.svg?logo=lgtm&logoWidth=18"></a>
  <a href="https://www.nuget.org/packages/EntityFrameworkCore.Cacheable/"><img src="https://buildstats.info/nuget/EntityFrameworkCore.Cacheable"></a>
</p>

<br/>
<br/>

## What is EF Core Cacheable?

Entity Framework (EF) Core Cacheable is an extension library for the popular Entity Framework data access technology.

It provides caching functionality for all types of query results. Based on the expression tree and parameters, the context decides whether to execute the query against the database or return the result from memory.

## How caching affects performance


This a sample result of 1,000 iterations of an uncached and cached query, called agains a good performing MSSQL-database.

```
Average database query duration [+00:00:00.1698972].
Average cache query duration [+00:00:00.0000650].
Cached queries are x2,611 times faster.
```

Even with an InMemory test database, the results are significantly faster.

```
Average database query duration [+00:00:00.0026076].
Average cache query duration [+00:00:00.0000411].
Cached queries are x63 times faster.
```

The performance gain can be even higher, depending on the database performance.


## Install via NuGet

You can view the [package page on NuGet](https://www.nuget.org/packages/EntityFrameworkCore.Cacheable/).

To install `EntityFrameworkCore.Cacheable`, run the following command in the Package Manager Console:

```
PM> Install-Package EntityFrameworkCore.Cacheable
```


This library also uses the [Data.HashFunction](https://github.com/brandondahler/Data.HashFunction/) and [aspnet.Extensions](https://github.com/aspnet/Extensions) as InMemory cache.


## Configuring a DbContext

There are three types of configurations for the DbContext to support `Cachable`.
Each sample uses `UseSqlite` as an option only for showing the pattern.

For more information about this, please read [configuring DbContextOptions](https://docs.microsoft.com/de-de/ef/core/miscellaneous/configuring-dbcontext#configuring-dbcontextoptions).

### Constructor argument

Application code to initialize from constructor argument:

```csharp
var optionsBuilder = new DbContextOptionsBuilder<CacheableBloggingContext>();
optionsBuilder
    .UseSqlite("Data Source=blog.db")
    .UseSecondLevelCache();

using (var context = new CacheableBloggingContext(optionsBuilder.Options))
{
    // do stuff
}
```

### OnConfiguring

Context code with `OnConfiguring`:

```csharp
public partial class CacheableBloggingContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=blog.db");
            optionsBuilder.UseSecondLevelCache();
        }
    }
}
```

### Using DbContext with dependency injection

Adding the Dbcontext to dependency injection:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddDbContext<CacheableBloggingContext>(options => options
        .UseSqlite("Data Source=blog.db"))
        .UseSecondLevelCache();
}
```


This requires [adding a constructor argument](https://docs.microsoft.com/de-de/ef/core/miscellaneous/configuring-dbcontext#using-dbcontext-with-dependency-injection) to your DbContext type that accepts DbContextOptions<TContext>.


## Usage

To get in use of result caching, you simply need to add `.Cacheable(...` to your query and define a TTL parameter.


```csharp
var cacheableQuery = cacheableContext.Books
	.Include(d => d.Pages)
	.ThenInclude(d => d.Lines)
	.Where(d => d.ID == 200)
	.Cacheable(TimeSpan.FromSeconds(60));
```

### Custom Cache Provider


Alternatively, you can provide a custom implementation of `ICacheProvider` (default is `MemoryCacheProvider`).
This provides an easy option for supporting other caching systems like [![](https://redis.io/images/favicon.png) redis](https://redis.io/) or [Memcached](https://memcached.org/).

```csharp
optionsBuilder.UseSecondLevelCache(new MyCachingProvider());
```


-----------------


## Contributors

The following contributors have either created (that only me :stuck_out_tongue_winking_eye:) the project, have contributed
code, are actively maintaining it (including documentation), or in other ways
being helpful contributors to this project. 


|                                                                                    | Name                  | GitHub                                                  |
| :--------------------------------------------------------------------------------: | --------------------- | ------------------------------------------------------- |
| <img src="https://avatars.githubusercontent.com/u/20702171?size=72" width="72"/>   | Steffen Mangold       | [@SteffenMangold](https://github.com/SteffenMangold)    |
| <img src="https://avatars.githubusercontent.com/u/1528107?size=72" width="72"/>    | Smit Patel            | [@smitpatel](https://github.com/smitpatel)              |
