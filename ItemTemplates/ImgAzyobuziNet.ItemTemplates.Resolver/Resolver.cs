using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.TestFramework;

namespace $rootnamespace$
{
	public class $safeitemrootname$Provider : PatternProviderBase<$safeitemrootname$Resolver>
	{
        public override string ServiceId => "";

        public override string ServiceName => "";

        public override string Pattern => @"";

        #region Tests

        [TestMethod(TestCategory.Static)]
        private void RegexTest()
        {
            var match = this.GetRegex().Match("");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("");
        }

        #endregion
    }

    public class $safeitemrootname$Resolver : IResolver
    {
        public ValueTask<ImageInfo[]> GetImages(Match match)
        {
            var id = match.Groups[1].Value;
            // TODO
        }
    }
}
