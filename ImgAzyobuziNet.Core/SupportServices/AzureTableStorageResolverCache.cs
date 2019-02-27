using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;

namespace ImgAzyobuziNet.Core.SupportServices
{
    internal class AzureTableStorageResolverCache : IResolverCache
    {
        #region Logging

        private static readonly Action<ILogger, Exception> s_storingError =
            LoggerMessage.Define(
                Microsoft.Extensions.Logging.LogLevel.Error,
                new EventId(140, "AzureTableStorageResolverCacheStoringError"),
                "An exception was thrown in storing the data to Azure Table Storage.");

        private static readonly Func<ILogger, DateTimeOffset, IDisposable> s_beginCleaningScope =
            LoggerMessage.DefineScope<DateTimeOffset>("Cleanup entries before {TargetTimestamp}");

        private static readonly Action<ILogger, int, int, Exception> s_cleanedUp =
            LoggerMessage.Define<int, int>(
                Microsoft.Extensions.Logging.LogLevel.Information,
                new EventId(141, "AzureTableStorageResolverCacheCleanedUp"),
                "Deleted entries (Input = {InputCount}, Success = {SuccessCount})");

        private static readonly Action<ILogger, Exception> s_cleaningError =
            LoggerMessage.Define(
                Microsoft.Extensions.Logging.LogLevel.Error,
                new EventId(142, "AzureTableStorageResolverClearningError"),
                "An exception was thrown in cleaning up the cache.");

        #endregion

        private ResolverCacheOptions _options;
        private readonly CloudTable _table;
        private readonly IResolverCacheLogger _resolverCacheLogger;
        private readonly ILogger _logger;

        public AzureTableStorageResolverCache(
            IOptionsSnapshot<ResolverCacheOptions> optionsSnapshot,
            IResolverCacheLogger<AzureTableStorageResolverCache> resolverCacheLogger,
            ILogger<AzureTableStorageResolverCache> logger)
        {
            this._options = optionsSnapshot?.Value ?? new ResolverCacheOptions();
            var connectionString = this._options.AzureTableStorageConnectionString;
            var tableName = this._options.AzureTableStorageTableName;

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

        public async Task DeleteExpiredEntries()
        {
            var expirationSeconds = this._options.ExpirationSeconds;

            if (!expirationSeconds.HasValue
                || !await this._table.ExistsAsync().ConfigureAwait(false))
            {
                return;
            }

            var targetTimestamp = DateTimeOffset.Now.AddSeconds(-expirationSeconds.Value);

            using (this._logger != null ? s_beginCleaningScope(this._logger, targetTimestamp) : null)
            {
                var query = new TableQuery<AzureTableStorageCacheEntity>().Where(
                    TableQuery.GenerateFilterConditionForDate(
                        nameof(AzureTableStorageCacheEntity.Timestamp),
                        QueryComparisons.LessThan,
                        targetTimestamp));

                var tasks = new List<(Task<IList<TableResult>>, int)>();
                TableContinuationToken tableContinuationToken = null;

                do
                {
                    var result = await this._table.ExecuteQuerySegmentedAsync(query, tableContinuationToken).ConfigureAwait(false);

                    tasks.AddRange(
                        result.Results
                            .ToLookup(x => x.PartitionKey)
                            .SelectMany(ToBatchOperations)
                    );

                    tableContinuationToken = result.ContinuationToken;
                } while (tableContinuationToken != null);

                var inputCount = 0;
                var successCount = 0;

                foreach (var (task, count) in tasks)
                {
                    inputCount += count;

                    try
                    {
                        var results = await task.ConfigureAwait(false);
                        successCount += results.Count(x => x.HttpStatusCode >= 200 && x.HttpStatusCode <= 299);
                    }
                    catch (Exception ex)
                    {
                        if (this._logger != null)
                            s_cleaningError(this._logger, ex);
                    }
                }

                if (this._logger != null)
                    s_cleanedUp(this._logger, inputCount, successCount, null);
            }

            IEnumerable<(Task<IList<TableResult>>, int)> ToBatchOperations(IGrouping<string, AzureTableStorageCacheEntity> g)
            {
                using (var enumerator = g.GetEnumerator())
                {
                    while (true)
                    {
                        var batch = new TableBatchOperation();

                        for (var i = 0; i < 100 && enumerator.MoveNext(); i++)
                            batch.Delete(enumerator.Current);

                        if (batch.Count == 0) break;

                        yield return (this._table.ExecuteBatchAsync(batch), batch.Count);
                    }
                }
            }
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
                    s_storingError(this._logger, ex);
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
