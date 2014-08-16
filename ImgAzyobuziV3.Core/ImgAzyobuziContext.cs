using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using Influxdb;
using StackExchange.Redis;

namespace ImgAzyobuziV3.Core
{
    public class ImgAzyobuziContext
    {
        public ImgAzyobuziContext(ImgAzyobuziSettings settings)
        {
            this.settings = settings;
            var i = settings.InfluxDb;
            if (i != null && !string.IsNullOrEmpty(i.Uri))
                this.InfluxDbClient = new InfluxDb(i.Uri, i.UserName, i.Password, i.Database);
            this.redisConn = ConnectionMultiplexer.Connect(settings.Redis.Configuration);

            new CompositionContainer(new AssemblyCatalog(Assembly.GetExecutingAssembly())).ComposeParts(this);
        }

        private readonly ImgAzyobuziSettings settings;

        [ImportMany(typeof(IResolver))]
        private List<IResolver> resolvers = null;

        public IReadOnlyList<IResolver> Resolvers
        {
            get
            {
                return this.resolvers;
            }
        }

        public InfluxDb InfluxDbClient { get; private set; }

        private readonly ConnectionMultiplexer redisConn;

        public IDatabase GetRedisDatabase()
        {
            return this.redisConn.GetDatabase(this.settings.Redis.Database);
        }
    }
}
