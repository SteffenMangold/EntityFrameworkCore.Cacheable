# EntityFrameworkCore.Cacheable

[![Build status](https://ci.appveyor.com/api/projects/status/8h2kg4gjcv85w6wg?svg=true)](https://ci.appveyor.com/project/SteffenMangold/entityframeworkcore-cacheable)

EntityFrameworkCore second level cache.

Install via NuGet
-----------------
To install EntityFrameworkCore.Cacheable, run the following command in the Package Manager Console:

```
PM> Install-Package EntityFrameworkCore.Cacheable
```

You can also view the package page on NuGet. [![Version Status](https://img.shields.io/nuget/v/EntityFrameworkCore.Cacheable.svg)](https://www.nuget.org/packages/EntityFrameworkCore.Cacheable/)

This library also uses the [Data.HashFunction](https://github.com/brandondahler/Data.HashFunction/) and [aspnet.Extensions](https://github.com/aspnet/Extensions) as InMemory cache.


Usage
-----
Init...

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

Usage...
```csharp
var cacheableQuery = cacheableContext.Books
	.Include(d => d.Pages)
	.ThenInclude(d => d.Lines)
	.Where(d => d.ID == 200)
	.Cacheable(TimeSpan.FromSeconds(60));
```

TODO
-----

- Add Async support
- Extend options to support more kinds of caching duration (sliding windows, absolut...)
- Replaceable ICacheProvider
- Add Testing