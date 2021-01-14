using System;
using AutoMapper;
using Memester.Core;
using Memester.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Memester.UnitTests
{
    public class UnitTestBench
    {
        private static readonly InMemoryDatabaseRoot DatabaseRoot = new InMemoryDatabaseRoot();

        public static IServiceCollection Create()
        {
            var serviceCollection = new ServiceCollection();
            // serviceCollection.AddAutoMapper(typeof(MappingAssemblyMarker));
            serviceCollection.AddDbContext<DatabaseContext>(options => options.UseInMemoryDatabase("test_db", DatabaseRoot));
            serviceCollection.AddDistributedMemoryCache();
            serviceCollection.AddScoped(typeof(OperationContext), _ => new OperationContext());
            return serviceCollection;
        }

        public static IServiceProvider CreateAndBuild() => Create().BuildServiceProvider();
    }
}