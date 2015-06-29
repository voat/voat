namespace Command.Tests
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using EntityFramework.Batch;
    using EntityFramework.Mapping;

    /// <summary>
    /// Batch runner for EntityFramework.Extended that works with Effort using in-memory data store.
    /// </summary>
    public class EffortBatchRunner : IBatchRunner
    {
        /// <summary>
        /// Create and runs a batch delete statement.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam><param name="objectContext">The <see cref="T:System.Data.Entity.Core.Objects.ObjectContext"/> to get connection and metadata information from.</param><param name="entityMap">The <see cref="T:EntityFramework.Mapping.EntityMap"/> for <typeparamref name="TEntity"/>.</param><param name="query">The query to create the where clause from.</param>
        /// <returns>
        /// The number of rows deleted.
        /// </returns>
        public int Delete<TEntity>(ObjectContext objectContext, EntityMap entityMap, ObjectQuery<TEntity> query) where TEntity : class
        {
            return DeleteAsync(objectContext, entityMap, query).Result;
        }

        /// <summary>
        /// Create and runs a batch delete statement asynchronously.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam><param name="objectContext">The <see cref="T:System.Data.Entity.Core.Objects.ObjectContext"/> to get connection and metadata information from.</param><param name="entityMap">The <see cref="T:EntityFramework.Mapping.EntityMap"/> for <typeparamref name="TEntity"/>.</param><param name="query">The query to create the where clause from.</param>
        /// <returns>
        /// The number of rows deleted.
        /// </returns>
        public async Task<int> DeleteAsync<TEntity>(ObjectContext objectContext, EntityMap entityMap,
            ObjectQuery<TEntity> query) where TEntity : class
        {
            var objs = await query.ToListAsync().ConfigureAwait(false);
            foreach (var obj in objs)
            {
                objectContext.DeleteObject(obj);
            }

            return objs.Count;
        }

        /// <summary>
        /// Create and runs a batch update statement.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam><param name="objectContext">The <see cref="T:System.Data.Entity.Core.Objects.ObjectContext"/> to get connection and metadata information from.</param><param name="entityMap">The <see cref="T:EntityFramework.Mapping.EntityMap"/> for <typeparamref name="TEntity"/>.</param><param name="query">The query to create the where clause from.</param><param name="updateExpression">The update expression.</param>
        /// <returns>
        /// The number of rows updated.
        /// </returns>
        public int Update<TEntity>(ObjectContext objectContext, EntityMap entityMap, ObjectQuery<TEntity> query, Expression<Func<TEntity, TEntity>> updateExpression) where TEntity : class
        {
            return UpdateAsync(objectContext, entityMap, query, updateExpression).Result;
        }

        /// <summary>
        /// Create and runs a batch update statement asynchronously.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam><param name="objectContext">The <see cref="T:System.Data.Entity.Core.Objects.ObjectContext"/> to get connection and metadata information from.</param><param name="entityMap">The <see cref="T:EntityFramework.Mapping.EntityMap"/> for <typeparamref name="TEntity"/>.</param><param name="query">The query to create the where clause from.</param><param name="updateExpression">The update expression.</param>
        /// <returns>
        /// The number of rows updated.
        /// </returns>
        public async Task<int> UpdateAsync<TEntity>(ObjectContext objectContext, EntityMap entityMap, ObjectQuery<TEntity> query,
            Expression<Func<TEntity, TEntity>> updateExpression) where TEntity : class
        {
            var objs = await query.ToListAsync().ConfigureAwait(false);
            var names = ((MemberInitExpression)updateExpression.Body).Bindings.Select(x => x.Member.Name).ToArray();
            var update = updateExpression.Compile();
            var properties = typeof (TEntity).GetProperties().ToDictionary(v => v.Name, v => v);
            foreach (var obj in objs)
            {
                var updated = update(obj);
                foreach (var name in names)
                {
                    var property = properties[name];
                    property.SetValue(obj, property.GetValue(updated));
                }
                objectContext.ApplyCurrentValues(entityMap.StoreSet.Name, obj);
            }

            return objs.Count;
        }
    }
}