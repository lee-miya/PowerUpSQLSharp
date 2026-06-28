using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Services;
using Xunit;

namespace PowerUpSQLSharp.Tests.Services
{
    public class SqlConnectionFactoryTests
    {
        private readonly SqlConnectionFactory _factory = new SqlConnectionFactory();

        [Fact]
        public void BuildConnectionString_IntegratedSecurity_UsesSspi()
        {
            var connectionString = _factory.BuildConnectionString(new SqlConnectionOptions
            {
                Instance = "SQL01",
                ConnectTimeoutSeconds = 15
            });

            Assert.Contains("Integrated Security=True", connectionString);
            Assert.Contains("Data Source=SQL01", connectionString);
            Assert.Contains("Initial Catalog=Master", connectionString);
        }

        [Fact]
        public void BuildConnectionString_SqlLogin_UsesUserId()
        {
            var connectionString = _factory.BuildConnectionString(new SqlConnectionOptions
            {
                Instance = "SQL01",
                Username = "sa",
                Password = "secret"
            });

            Assert.Contains("User ID=sa", connectionString);
            Assert.Contains("Password=secret", connectionString);
            Assert.DoesNotContain("Integrated Security=True", connectionString);
        }

        [Fact]
        public void BuildConnectionString_DomainUser_UsesIntegratedSecurityWithUid()
        {
            var connectionString = _factory.BuildConnectionString(new SqlConnectionOptions
            {
                Instance = "SQL01",
                Username = @"DOMAIN\user",
                Password = "secret"
            });

            Assert.Contains("Integrated Security=True", connectionString);
            Assert.Contains(@"User ID=DOMAIN\user", connectionString);
            Assert.Contains("Password=secret", connectionString);
        }

        [Fact]
        public void BuildConnectionString_Dac_PrefixesAdmin()
        {
            var connectionString = _factory.BuildConnectionString(new SqlConnectionOptions
            {
                Instance = "SQL01",
                Dac = true
            });

            Assert.Contains("Data Source=ADMIN:SQL01", connectionString);
        }
    }
}
