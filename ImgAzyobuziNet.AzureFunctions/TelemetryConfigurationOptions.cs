using System.Collections.Generic;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Options;

namespace ImgAzyobuziNet.AzureFunctions
{
    internal class TelemetryConfigurationOptions : IOptions<TelemetryConfiguration>
    {
        public TelemetryConfiguration Value { get; }

        public TelemetryConfigurationOptions(IEnumerable<IConfigureOptions<TelemetryConfiguration>> configureOptions)
        {
            this.Value = TelemetryConfiguration.CreateDefault();

            foreach (var c in configureOptions)
            {
                c.Configure(this.Value);
            }
        }
    }
}
