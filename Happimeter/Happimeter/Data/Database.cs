using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Happimeter.Data
{
    public class Database
    {
        readonly SQLiteAsyncConnection _database;

        public Database(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<MeasurementPoint>().Wait();
            _database.CreateTableAsync<Config>().Wait();
        }

        public Task<List<T>> GetEntitesAsync<T>(int limit = -1) where T : IEntity, new()
        {
            if (limit < 1)
            {
                return _database.Table<T>().ToListAsync();
            }
            return _database.Table<T>().Take(limit).ToListAsync();
        }

        public Task<List<T>> GetEntitesAsync<T>(Expression<Func<T,bool>> whereStatement) where T : IEntity, new()
        {
            return _database.Table<T>().Where(whereStatement).ToListAsync();
        }

        public Task<T> GetEntityAsync<T>(int id) where T : IEntity, new()
        {
            return _database.Table<T>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

        public Task<T> FindEntityAsync<T>(Expression<Func<T, bool>> whereStatement) where T : IEntity, new()
        {
            return _database.Table<T>().Where(whereStatement).FirstOrDefaultAsync();
        }

        public event Action<object> OnSaveItem;
        public async Task<int> SaveItemAsync<T>(T item) where T : IEntity, new()
        {
            if (item.Id != 0)
            {
                var resultCode = await _database.UpdateAsync(item);
                OnSaveItem?.Invoke(item);
                return resultCode;
            }
            else
            {
                var resultCode = await _database.InsertAsync(item);
                OnSaveItem?.Invoke(item);
                return resultCode;
            }
        }

        public Task<int> DeleteItemAsync<T>(T item) where T : IEntity, new()
        {
            return _database.DeleteAsync(item);
        }
    }
}
