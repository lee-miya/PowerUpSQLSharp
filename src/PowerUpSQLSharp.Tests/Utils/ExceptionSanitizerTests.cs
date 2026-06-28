using PowerUpSQLSharp.Core.Utils;
using Xunit;

namespace PowerUpSQLSharp.Tests.Utils
{
    public class ExceptionSanitizerTests
    {
        [Fact]
        public void RedactConnectionString_HidesPassword()
        {
            var input = "Server=.;User Id=sa;Password=Secret123;";
            var result = ExceptionSanitizer.RedactConnectionString(input);
            Assert.Contains("Password=[REDACTED]", result);
            Assert.DoesNotContain("Secret123", result);
        }

        [Fact]
        public void Sanitize_ReplacesUserPath()
        {
            var ex = new System.Exception(@"Failed at C:\Users\developer\project\file.cs");
            var result = ExceptionSanitizer.Sanitize(ex);
            Assert.Contains(@"C:\Users\<user>", result);
            Assert.DoesNotContain("developer", result);
        }

        [Fact]
        public void Sanitize_UnwrapsAggregateException()
        {
            var inner = new System.InvalidOperationException("connection failed");
            var aggregate = new System.AggregateException(inner);
            var result = ExceptionSanitizer.Sanitize(aggregate);
            Assert.Contains("InvalidOperationException", result);
            Assert.Contains("connection failed", result);
            Assert.DoesNotContain("AggregateException", result);
        }
    }
}
