using System.Threading.Tasks;
using ImgAzyobuziNet.Core;
using ImgAzyobuziNet.Core.SupportServices;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImgAzyobuziNet.AzureFunctions
{
    public static class DeleteExpiredCacheFunction
    {
        [FunctionName("DeleteExpiredCache")]
        public static async Task Run([TimerTrigger("01:00:00")] TimerInfo myTimer, ILogger log)
        {
            var resolverCacheOptions = FunctionsEnvironment.ServiceProvider
                .GetService<IOptionsMonitor<ResolverCacheOptions>>()
                ?.CurrentValue
                ?? new ResolverCacheOptions();

            if (resolverCacheOptions.Type != ResolverCacheType.AzureTableStorage)
            {
                log.LogWarning(
                    "ResolverCache:Type が {ResolverCacheType} に設定されています。 DeleteExpiredCache は不要なため、関数を無効化することをおすすめします。",
                    resolverCacheOptions.Type);
                return;
            }

            if (!resolverCacheOptions.ExpirationSeconds.HasValue)
            {
                log.LogWarning("ResolverCache:ExpirationSeconds が null なため、キャッシュは削除されません。");
                return;
            }

            await FunctionsEnvironment.ServiceProvider.GetRequiredService<IResolverCache>()
                .DeleteExpiredEntries().ConfigureAwait(false);
        }
    }
}
