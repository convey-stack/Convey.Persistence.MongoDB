using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Convey.CQRS.Queries;
using Convey.Types;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Convey.Persistence.MongoDB.Repositories
{
	internal class MongoRepository<TEntity, TIdentifiable> : IMongoRepository<TEntity, TIdentifiable>
		where TEntity : IIdentifiable<TIdentifiable>
	{
		private readonly IMongoCollection<TEntity> _collection;

		public MongoRepository(IMongoDatabase database, string collectionName)
		{
			_collection = database.GetCollection<TEntity>(collectionName);
		}

		public async Task<TEntity> GetAsync(TIdentifiable id)
			=> await GetAsync(e => e.Id.Equals(id));

		public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate)
			=> await _collection.Find(predicate).SingleOrDefaultAsync();

		public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
			=> await _collection.Find(predicate).ToListAsync();

		public async Task<PagedResult<TEntity>> BrowseAsync<TQuery>(Expression<Func<TEntity, bool>> predicate,
			TQuery query) where TQuery : IPagedQuery
			=> await _collection.AsQueryable().Where(predicate).PaginateAsync(query);

		public async Task AddAsync(TEntity entity)
			=> await _collection.InsertOneAsync(entity);

		public async Task UpdateAsync(TEntity entity)
			=> await _collection.ReplaceOneAsync(e => e.Id.Equals(entity.Id), entity);

		public async Task DeleteAsync(TIdentifiable id)
			=> await _collection.DeleteOneAsync(e => e.Id.Equals(id));

		public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
			=> await _collection.Find(predicate).AnyAsync();
	}
}