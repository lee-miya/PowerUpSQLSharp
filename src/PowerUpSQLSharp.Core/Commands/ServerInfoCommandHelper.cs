using System.Collections.Generic;

using System.Linq;

using PowerUpSQLSharp.Core.Models;



namespace PowerUpSQLSharp.Core.Commands

{

    internal static class ServerInfoCommandHelper

    {

        internal static int ResolveExitCode(int requestedCount, IReadOnlyList<SqlServerInfo> results)

        {

            if (requestedCount <= 0)

            {

                return ExitCodes.Success;

            }



            var successCount = results?.Count(info => info != null) ?? 0;



            if (successCount == 0)

            {

                return ExitCodes.ConnectionFailed;

            }



            if (successCount < requestedCount)

            {

                return ExitCodes.PartialFailure;

            }



            return ExitCodes.Success;

        }

        internal static int ResolveExitCode(int requestedCount, int successCount)

        {

            if (requestedCount <= 0)

            {

                return ExitCodes.Success;

            }



            if (successCount == 0)

            {

                return ExitCodes.ConnectionFailed;

            }



            if (successCount < requestedCount)

            {

                return ExitCodes.PartialFailure;

            }



            return ExitCodes.Success;

        }



        internal static CommandResult BuildServerInfoResult(

            IReadOnlyList<SqlServerInfo> results,

            int requestedCount,

            GlobalOptions options)

        {

            var formatted = CommandResultFormatter.FromRows(

                CommandResultFormatter.ToServerInfoRows(results),

                options);



            formatted.ExitCode = ResolveExitCode(requestedCount, results);

            return formatted;

        }

    }

}

