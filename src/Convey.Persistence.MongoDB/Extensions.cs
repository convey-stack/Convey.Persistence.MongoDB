using System;
using Convey.Persistence.MongoDB.Builders;
using Convey.Persistence.MongoDB.Initializers;
using Convey.Persistence.MongoDB.Repositories;
using Convey.Persistence.MongoDB.Seeders;
using Convey.Types;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Convey.Persistence.MongoDB
{
    public static class Extensions
    {
        private const string SectionName = "mongo";
        private const string RegistryName = "periosistence.MngoDb";

        public static IConveyBuilder AddMongo(this IConveyBuilder builder, string sectionName = SectionName,
            IMongoDbSeeder seeder = null)
        {
            var mongoOptions = builder.GetOptions<MongoDbOptions>(sectionName);
            return builder.AddMongo(mongoOptions, seeder);
        }
        
        public static IConveyBuilder AddMongo(this IConveyBuilder builder, Func<IMongoDbOptionsBuilder, 
                IMongoDbOptionsBuilder> buildOptions, IMongoDbSeeder seeder = null)
        {
            var mongoOptions = buildOptions(new MongoDbOptionsBuilder()).Build();
            return builder.AddMongo(mongoOptions, seeder);
        }

        public static IConveyBuilder AddMongo(this IConveyBuilder builder, MongoDbOptions mongoOptions, 
            IMongoDbSeeder seeder = null)
        {
            if (!builder.TryRegister(RegistryName))
            {
                return builder;
            }
            
            builder.Services.AddSingleton(mongoOptions);

            builder.Services.AddSingleton(sp =>
            {
                var options = sp.GetService<MongoDbOptions>();
                return new MongoClient(options.ConnectionString);
            });

            builder.Services.AddTransient(sp =>
            {
                var options = sp.GetService<MongoDbOptions>();
                var client = sp.GetService<MongoClient>();
                return client.GetDatabase(options.Database);
            });

            builder.Services.AddTransient<IMongoDbInitializer, MongoDbInitializer>();

            if (seeder is null)
            {
                builder.Services.AddSingleton<IMongoDbSeeder, MongoDbSeeder>();
            }
            else
            {
                builder.Services.AddSingleton(seeder);
            }
            
            builder.AddInitializer<IMongoDbInitializer>();

            return builder;
        }

        public static void AddMongoRepository<TEntity>(this IConveyBuilder builder, string collectionName)
            where TEntity : IIdentifiable
            => builder.Services.AddTransient<IMongoRepository<TEntity>>(sp =>
            {
                var database = sp.GetService<IMongoDatabase>();
                return new MongoRepository<TEntity>(database, collectionName);
            });
    }
}