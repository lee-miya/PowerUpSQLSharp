using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerUpSQLSharp.Core.Utils
{
    internal static class SqlDefaultPasswordDatabase
    {
        internal sealed class Entry
        {
            public Entry(string instanceName, string username, string password)
            {
                InstanceName = instanceName;
                Username = username;
                Password = password;
            }

            public string InstanceName { get; }
            public string Username { get; }
            public string Password { get; }
        }

        private static readonly Entry[] Entries =
        {
            new Entry("ACS", "ej", "ej"),
            new Entry("ACT7", "sa", "sage"),
            new Entry("AOM2", "admin", "ca_admin"),
            new Entry("ARIS", "ARIS9", "*ARIS!1dm9n#"),
            new Entry("AutodeskVault", "sa", "AutodeskVault@26200"),
            new Entry("BOSCHSQL", "sa", "RPSsql12345"),
            new Entry("BPASERVER9", "sa", "AutoMateBPA9"),
            new Entry("CDRDICOM", "sa", "CDRDicom50!"),
            new Entry("CODEPAL", "sa", "Cod3p@l"),
            new Entry("CODEPAL08", "sa", "Cod3p@l"),
            new Entry("CounterPoint", "sa", "CounterPoint8"),
            new Entry("CSSQL05", "ELNAdmin", "ELNAdmin"),
            new Entry("CSSQL05", "sa", "CambridgeSoft_SA"),
            new Entry("CADSQL", "CADSQLAdminUser", "Cr41g1sth3M4n!"),
            new Entry("DHLEASYSHIP", "sa", "DHLadmin@1"),
            new Entry("DPM", "admin", "ca_admin"),
            new Entry("DVTEL", "sa", string.Empty),
            new Entry("EASYSHIP", "sa", "DHLadmin@1"),
            new Entry("ECC", "sa", "Webgility2011"),
            new Entry("ECOPYDB", "e+C0py2007_@x", "e+C0py2007_@x"),
            new Entry("ECOPYDB", "sa", "ecopy"),
            new Entry("Emerson2012", "sa", "42Emerson42Eme"),
            new Entry("HDPS", "sa", "sa"),
            new Entry("HPDSS", "sa", "Hpdsdb000001"),
            new Entry("HPDSS", "sa", "hpdss"),
            new Entry("INSERTGT", "msi", "keyboa5"),
            new Entry("INSERTGT", "sa", string.Empty),
            new Entry("INTRAVET", "sa", "Webster#1"),
            new Entry("MYMOVIES", "sa", "t9AranuHA7"),
            new Entry("PCAMERICA", "sa", "pcAmer1ca"),
            new Entry("PCAMERICA", "sa", "PCAmerica"),
            new Entry("PRISM", "sa", "SecurityMaster08"),
            new Entry("RMSQLDATA", "Super", "Orange"),
            new Entry("RTCLOCAL", "sa", "mypassword"),
            new Entry("RBAT", "sa", "34TJ4@#$"),
            new Entry("RIT", "sa", "34TJ4@#$"),
            new Entry("RCO", "sa", "34TJ4@#$"),
            new Entry("REDBEAM", "sa", "34TJ4@#$"),
            new Entry("SALESLOGIX", "sa", "SLXMaster"),
            new Entry("SIDEXIS_SQL", "sa", "2BeChanged"),
            new Entry("SQL2K5", "ovsd", "ovsd"),
            new Entry("SQLEXPRESS", "admin", "ca_admin"),
            new Entry("STANDARDDEV2014", "test", "test"),
            new Entry("TEW_SQLEXPRESS", "tew", "tew"),
            new Entry("vocollect", "vocollect", "vocollect"),
            new Entry("VSDOTNET", "sa", string.Empty),
            new Entry("VSQL", "sa", "111"),
            new Entry("CASEWISE", "sa", string.Empty),
            new Entry("VANTAGE", "sa", "vantage12!"),
            new Entry("BCM", "bcmdbuser", "Bcmuser@06"),
            new Entry("BCM", "bcmdbuser", "Numara@06"),
            new Entry("DEXIS_DATA", "sa", "dexis"),
            new Entry("DEXIS_DATA", "dexis", "dexis"),
            new Entry("SMTKINGDOM", "SMTKINGDOM", "$ei$micMicro"),
            new Entry("RE7_MS", "Supervisor", "Supervisor"),
            new Entry("RE7_MS", "Admin", "Admin"),
            new Entry("OHD", "sa", "ohdusa@123"),
            new Entry("UPC", "serviceadmin", "Password.0"),
            new Entry("Hirsh", "Velocity", "i5X9FG42"),
            new Entry("Hirsh", "sa", "i5X9FG42"),
            new Entry("SPSQL", "sa", "SecurityMaster08"),
            new Entry("CAREWARE", "sa", string.Empty)
        };

        public static IReadOnlyList<Entry> GetEntriesForInstanceName(string instanceName)
        {
            if (string.IsNullOrWhiteSpace(instanceName))
            {
                return Array.Empty<Entry>();
            }

            return Entries
                .Where(entry => string.Equals(entry.InstanceName, instanceName, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }
    }
}
