using System;
using System.Collections.Generic;
using PowerUpSQLSharp.Core.Models;

namespace PowerUpSQLSharp.Core.Services
{
    public interface IActiveDirectoryLdapService
    {
        IReadOnlyList<DomainSpnEntry> GetDomainSpns(
            DomainLdapOptions options,
            string spnService,
            out string warning);
    }
}
