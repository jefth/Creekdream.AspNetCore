﻿using Creekdream.Domain.Entities;
using Creekdream.Domain.Repositories;
using Dapper;
using DapperExtensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Creekdream.Orm.Dapper
{
    /// <summary>
    /// Repository extension method
    /// </summary>
    public static class RepositoryExtensions
    {
        /// <summary>
        /// Get entity as enumerable based on criteria
        /// </summary>
        public static async Task<IEnumerable<TEntity>> GetEnumerable<TEntity, TPrimaryKey>(
            this IRepository<TEntity, TPrimaryKey> repository,
            Expression<Func<TEntity, bool>> predicate = null)
            where TEntity : class, IEntity<TPrimaryKey>
        {
            var databaseProvider = repository.GetDatabaseProvider();
            var dbTransaction = databaseProvider.GetDbTransaction();
            var database = databaseProvider.GetDatabase();
            var predicateGroup = predicate.ToPredicateGroup<TEntity, TPrimaryKey>();

            return await Task.FromResult(
                database.GetList<TEntity>(
                    predicateGroup,
                    null,
                    transaction: dbTransaction));
        }

        /// <summary>
        /// Sql statement query result
        /// </summary>
        public static async Task<IEnumerable<TEntity>> QueryAsync<TEntity, TPrimaryKey>(
            this IRepository<TEntity, TPrimaryKey> repository,
            string sql,
            object parameters = null)
            where TEntity : class, IEntity<TPrimaryKey>
        {
            var databaseProvider = repository.GetDatabaseProvider();
            var dbTransaction = databaseProvider.GetDbTransaction();
            var database = databaseProvider.GetDatabase();
            var dbConnection = database.Connection;

            return await dbConnection.QueryAsync<TEntity>(
                sql,
                param: parameters,
                transaction: dbTransaction);
        }

        /// <summary>
        /// Execute sql statement and return the number of affected rows
        /// </summary>
        public static async Task<int> ExecuteNonQueryAsync<TEntity, TPrimaryKey>(
            this IRepository<TEntity, TPrimaryKey> repository,
            string sql,
            object parameters = null)
            where TEntity : class, IEntity<TPrimaryKey>
        {
            var databaseProvider = repository.GetDatabaseProvider();
            var dbTransaction = databaseProvider.GetDbTransaction();
            var database = databaseProvider.GetDatabase();
            var dbConnection = database.Connection;

            return await dbConnection.ExecuteAsync(
                sql,
                param: parameters,
                transaction: dbTransaction);
        }

        /// <summary>
        /// Execute the sql statement and query the first column value of the first row of the result
        /// </summary>
        public static async Task<object> ExecuteScalarAsync<TEntity, TPrimaryKey>(
            this IRepository<TEntity, TPrimaryKey> repository,
            string sql,
            object parameters = null)
            where TEntity : class, IEntity<TPrimaryKey>
        {
            var databaseProvider = repository.GetDatabaseProvider();
            var dbTransaction = databaseProvider.GetDbTransaction();
            var database = databaseProvider.GetDatabase();
            var dbConnection = database.Connection;

            return await dbConnection.ExecuteScalarAsync(
                sql,
                param: parameters,
                transaction: dbTransaction);
        }

        /// <summary>
        /// Paging query
        /// </summary>
        public static async Task<IEnumerable<TEntity>> GetPaged<TEntity, TPrimaryKey>(
            this IRepository<TEntity, TPrimaryKey> repository,
            int pageIndex,
            int pageSize,
            string ordering,
            Expression<Func<TEntity, bool>> predicate = null)
            where TEntity : class, IEntity<TPrimaryKey>
        {
            var databaseProvider = repository.GetDatabaseProvider();
            var dbTransaction = databaseProvider.GetDbTransaction();
            var database = databaseProvider.GetDatabase();

            return await Task.FromResult(
                database.GetPage<TEntity>(
                    predicate.ToPredicateGroup<TEntity, TPrimaryKey>(),
                    ToSortable(ordering),
                    pageIndex,
                    pageSize,
                    transaction: dbTransaction));
        }

        /// <summary>
        /// Get an database provider
        /// </summary>
        private static IDatabaseProvider GetDatabaseProvider<TEntity, TPrimaryKey>(this IRepository<TEntity, TPrimaryKey> repository)
            where TEntity : class, IEntity<TPrimaryKey>
        {
            return repository.ServiceProvider.GetRequiredService<IDatabaseProvider>();
        }

        /// <summary>
        /// The sort string is converted to an ISort array, for example: CreationTime desc,Id asc
        /// </summary>
        private static List<ISort> ToSortable(string ordering)
        {
            var sortList = new List<ISort>();
            if (!string.IsNullOrEmpty(ordering))
            {
                foreach (var sort in ordering.Split(','))
                {
                    var sortItem = sort.Split(' ');
                    var sortName = sortItem[0];
                    var sortIsAsc = sortItem[1].Equals("asc", StringComparison.OrdinalIgnoreCase);
                    sortList.Add(new Sort { PropertyName = sortName, Ascending = sortIsAsc });
                }
            }
            return sortList;
        }
    }
}

