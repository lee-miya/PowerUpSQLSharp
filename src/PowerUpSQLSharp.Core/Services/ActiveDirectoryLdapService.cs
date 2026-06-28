using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Security.Principal;
using PowerUpSQLSharp.Core.Models;
using PowerUpSQLSharp.Core.Utils;

namespace PowerUpSQLSharp.Core.Services
{
    public sealed class ActiveDirectoryLdapService : IActiveDirectoryLdapService
    {
        private const int DefaultPageSize = 1000;

        public IReadOnlyList<DomainSpnEntry> GetDomainSpns(
            DomainLdapOptions options,
            string spnService,
            out string warning)
        {
            warning = null;
            var entries = new List<DomainSpnEntry>();

            try
            {
                using (var searcher = CreateSearcher(options, BuildFilter(options, spnService)))
                {
                    searcher.PropertiesToLoad.AddRange(new[]
                    {
                        "servicePrincipalName",
                        "samAccountName",
                        "cn",
                        "objectSid",
                        "lastLogon",
                        "description"
                    });

                    using (var results = searcher.FindAll())
                    {
                        foreach (SearchResult result in results)
                        {
                            AppendSpnEntries(result, entries);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                warning = $"Domain LDAP query failed: {ExceptionSanitizer.Sanitize(ex)}";
            }

            return entries;
        }

        private static string BuildFilter(DomainLdapOptions options, string spnService)
        {
            var spnFilter = string.Empty;
            if (!string.IsNullOrWhiteSpace(options?.DomainAccount))
            {
                spnFilter = $"(objectcategory=person)(SamAccountName={EscapeFilterValue(options.DomainAccount)})";
            }

            if (!string.IsNullOrWhiteSpace(options?.ComputerName))
            {
                var computerSearch = $"{options.ComputerName}$";
                spnFilter = $"(objectcategory=computer)(SamAccountName={EscapeFilterValue(computerSearch)})";
            }

            var servicePattern = string.IsNullOrWhiteSpace(spnService) ? "*" : spnService.TrimEnd('*') + "*";
            return $"(&(servicePrincipalName={servicePattern}){spnFilter})";
        }

        private static DirectorySearcher CreateSearcher(DomainLdapOptions options, string ldapFilter)
        {
            var directoryEntry = CreateDirectoryEntry(options);
            var searcher = new DirectorySearcher(directoryEntry)
            {
                Filter = ldapFilter,
                SearchScope = SearchScope.Subtree,
                PageSize = DefaultPageSize
            };

            return searcher;
        }

        private static DirectoryEntry CreateDirectoryEntry(DomainLdapOptions options)
        {
            if (!string.IsNullOrWhiteSpace(options?.DomainController))
            {
                var ldapPath = $"LDAP://{options.DomainController}";
                if (!string.IsNullOrWhiteSpace(options.Username))
                {
                    return new DirectoryEntry(ldapPath, options.Username, options.Password ?? string.Empty);
                }

                return new DirectoryEntry(ldapPath);
            }

            return new DirectoryEntry();
        }

        private static void AppendSpnEntries(SearchResult result, ICollection<DomainSpnEntry> entries)
        {
            if (result?.Properties == null
                || !result.Properties.Contains("servicePrincipalName")
                || result.Properties["servicePrincipalName"].Count == 0)
            {
                return;
            }

            var spnValues = GetPropertyValues(result, "servicePrincipalName");
            var sid = FormatObjectSid(GetPropertyValue(result, "objectSid"));
            var samAccount = GetPropertyValue(result, "samAccountName");
            var cn = GetPropertyValue(result, "cn");
            var lastLogon = FormatLastLogon(GetPropertyValue(result, "lastLogon"));
            var description = GetPropertyValue(result, "description");

            foreach (string spn in spnValues)
            {
                if (string.IsNullOrWhiteSpace(spn))
                {
                    continue;
                }

                entries.Add(new DomainSpnEntry
                {
                    UserSid = sid,
                    User = samAccount,
                    UserCn = cn,
                    Service = DomainSpnParser.ParseServiceFromSpn(spn),
                    ComputerName = DomainSpnParser.ParseComputerNameFromSpn(spn),
                    Spn = spn,
                    LastLogon = lastLogon,
                    Description = description
                });
            }
        }

        private static IEnumerable GetPropertyValues(SearchResult result, string propertyName)
        {
            if (result?.Properties == null || !result.Properties.Contains(propertyName))
            {
                return Array.Empty<object>();
            }

            return result.Properties[propertyName];
        }

        private static string GetPropertyValue(SearchResult result, string propertyName)
        {
            foreach (object value in GetPropertyValues(result, propertyName))
            {
                return value?.ToString() ?? string.Empty;
            }

            return string.Empty;
        }

        private static string FormatObjectSid(object sidValue)
        {
            if (sidValue == null)
            {
                return string.Empty;
            }

            if (sidValue is byte[] sidBytes && sidBytes.Length > 0)
            {
                try
                {
                    return new SecurityIdentifier(sidBytes, 0).ToString();
                }
                catch
                {
                    return string.Empty;
                }
            }

            return sidValue.ToString();
        }

        private static string FormatLastLogon(string rawValue)
        {
            if (!long.TryParse(rawValue, out var fileTime) || fileTime <= 0)
            {
                return string.Empty;
            }

            try
            {
                return DateTime.FromFileTime(fileTime).ToString("g");
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string EscapeFilterValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return value
                .Replace("\\", "\\5c")
                .Replace("*", "\\2a")
                .Replace("(", "\\28")
                .Replace(")", "\\29")
                .Replace("\0", "\\00");
        }
    }
}
