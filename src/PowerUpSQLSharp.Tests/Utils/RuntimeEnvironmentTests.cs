using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Utils;
using Xunit;

namespace PowerUpSQLSharp.Tests.Utils
{
    public class RuntimeEnvironmentTests
    {
        [Fact]
        public void ShouldUseColor_RespectsNoColorFlag()
        {
            var options = new GlobalOptions { NoColor = true };
            Assert.False(RuntimeEnvironment.ShouldUseColor(options));
        }

        [Fact]
        public void ShouldUseQuiet_RespectsExplicitQuiet()
        {
            var options = new GlobalOptions { Quiet = true, Verbose = true };
            Assert.True(RuntimeEnvironment.ShouldUseQuiet(options));
        }

        [Fact]
        public void ShouldUseQuiet_AllowsVerboseOverrideForC2Default()
        {
            var options = new GlobalOptions { Verbose = true };
            Assert.False(RuntimeEnvironment.ShouldUseQuiet(options));
        }

        [Fact]
        public void ShouldUseQuiet_DefaultsToFalseWithoutOptions()
        {
            var options = new GlobalOptions();
            Assert.False(RuntimeEnvironment.ShouldUseQuiet(options));
        }
    }
}
