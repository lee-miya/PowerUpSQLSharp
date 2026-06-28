using System.Collections.Generic;
using PowerUpSQLSharp.Core.Utils;
using Xunit;

namespace PowerUpSQLSharp.Tests.Utils
{
    public class SqlInstanceFileReaderTests
    {
        [Fact]
        public void ReadInstances_ParsesSupportedFormats()
        {
            var filePath = System.IO.Path.GetTempFileName();
            try
            {
                System.IO.File.WriteAllLines(filePath, new[]
                {
                    "HOST-01",
                    @"HOST-02\INSTANCE",
                    "HOST-03,1433"
                });

                var instances = SqlInstanceFileReader.ReadInstances(filePath);

                Assert.Equal(3, instances.Count);
                Assert.Equal("HOST-01", instances[0].ComputerName);
                Assert.Equal("HOST-01", instances[0].ServerInstance);
                Assert.Equal(@"HOST-02\INSTANCE", instances[1].ServerInstance);
                Assert.Equal("INSTANCE", instances[1].InstanceName);
                Assert.Equal("HOST-03,1433", instances[2].ServerInstance);
            }
            finally
            {
                System.IO.File.Delete(filePath);
            }
        }

        [Fact]
        public void ReadInstances_MissingFile_ReturnsEmpty()
        {
            var instances = SqlInstanceFileReader.ReadInstances("missing-file.txt");
            Assert.Empty(instances);
        }
    }
}
