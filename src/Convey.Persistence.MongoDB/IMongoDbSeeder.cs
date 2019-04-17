using System.Threading.Tasks;

namespace Convey.Persistence.MongoDB
{
    public interface IMongoDbSeeder
    {
        Task SeedAsync();
    }
}