using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace ImgAzyobuziNet.Core.SupportServices
{
    internal class AzureTableStorageResolverCache : IResolverCache
    {
        #region Logging

        private static readonly Action<ILogger, Exception> s_azureTableStorageStoringError =
            LoggerMessage.Define(Microsoft.Extensions.Logging.LogLevel.Error, new EventId(140, "AzureTableStorageStoringError"), "An exception was thrown in storing the data to Azure Table Storage.");

        #endregion

        private readonly CloudTable _table;
        private readonly IResolverCacheLogger _resolverCacheLogger;
        private readonly ILogger _logger;

        public AzureTableStorageResolverCache(
            IOptionsSnapshot<ResolverCacheOptions> optionsSnapshot,
            IResolverCacheLogger<AzureTableStorageResolverCache> resolverCacheLogger,
            ILogger<AzureTableStorageResolverCache> logger)
        {
            var options = optionsSnapshot?.Value;
            var connectionString = options?.AzureTableStorageConnectionString;
            var tableName = options?.AzureTableStorageTableName;

            if (string.IsNullOrEmpty(connectionString))
                throw new NotConfiguredException(nameof(ImgAzyobuziNetOptions.ResolverCache) + ":" + nameof(ResolverCacheOptions.AzureTableStorageConnectionString));
            if (string.IsNullOrEmpty(tableName))
                throw new NotConfiguredException(nameof(ImgAzyobuziNetOptions.ResolverCache) + ":" + nameof(ResolverCacheOptions.AzureTableStorageTableName));

            this._table = CloudStorageAccount.Parse(connectionString).CreateCloudTableClient().GetTableReference(tableName);
            this._resolverCacheLogger = resolverCacheLogger;
            this._logger = logger;
        }

        public Task Set(string key, object value)
        {
            // fire and forget
            Task.Run(() => this.SetCore(key, value));
            return Task.CompletedTask;
        }

        public async ValueTask<(bool Exists, T Value)> TryGetValue<T>(string key)
        {
            Exception exception = null;

            try
            {
                if (await this._table.ExistsAsync().ConfigureAwait(false))
                {
                    var (partitionKey, rowKey) = ToTableKey(key);
                    var operation = TableOperation.Retrieve<AzureTableStorageCacheEntity>(partitionKey, rowKey);
                    var result = await this._table.ExecuteAsync(operation).ConfigureAwait(false);

                    if (result.Result is AzureTableStorageCacheEntity entity)
                    {
                        this._resolverCacheLogger?.LogCacheHit(key);
                        return (true, JsonConvert.DeserializeObject<T>(entity.Data));
                    }
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            this._resolverCacheLogger?.LogCacheMiss(key, exception);
            return (false, default);
        }

        private async void SetCore(string key, object value)
        {
            try
            {
                var entity = new AzureTableStorageCacheEntity(key, JsonConvert.SerializeObject(value));

                await this._table.CreateIfNotExistsAsync().ConfigureAwait(false);
                await this._table.ExecuteAsync(TableOperation.InsertOrReplace(entity)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // 保存に失敗しても動作上問題ないので、ログだけ記録しておく
                if (this._logger != null)
                    s_azureTableStorageStoringError(this._logger, ex);
            }
        }

        internal static (string PartitionKey, string RowKey) ToTableKey(string key)
        {
            var splitIndex = key.IndexOf('-');
            return splitIndex >= 0
                ? (key.Substring(0, splitIndex), key.Substring(splitIndex + 1))
                : ("", key);
        }
    }

    public class AzureTableStorageCacheEntity : TableEntity
    {
        public AzureTableStorageCacheEntity(string key, string data)
        {
            (this.PartitionKey, this.RowKey) = AzureTableStorageResolverCache.ToTableKey(key);
            this.Data = data;
        }

        public AzureTableStorageCacheEntity() { }

        public string Data { get; set; }
    }
}
