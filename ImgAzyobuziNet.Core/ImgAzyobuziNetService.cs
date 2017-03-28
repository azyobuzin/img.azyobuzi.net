using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImgAzyobuziNet.Core
{
    public class ImgAzyobuziNetService
    {
        private readonly IEnumerable<IPatternProvider> _patternProviders;
        private readonly IServiceProvider _serviceProvider;

        public ImgAzyobuziNetService(IEnumerable<IPatternProvider> patternProviders, IServiceProvider serviceProvider)
        {
            this._patternProviders = patternProviders;
            this._serviceProvider = serviceProvider;
        }

        public IEnumerable<IPatternProvider> GetPatternProviders()
        {
            return this._patternProviders;
        }

        public async Task<ResolveResult> Resolve(string uri)
        {
            foreach (var p in this._patternProviders)
            {
                var m = p.GetRegex().Match(uri);
                if (m.Success)
                {
                    try
                    {
                        var resolver = p.GetResolver(this._serviceProvider);
                        return new ResolveResult(p, await resolver.GetImages(m).ConfigureAwait(false));
                    }
                    catch (Exception ex)
                    {
                        return new ResolveResult(p, ex);
                    }
                }
            }

            return null;
        }

        public object Select(Func<object, object> p)
        {
            throw new NotImplementedException();
        }
    }
}
