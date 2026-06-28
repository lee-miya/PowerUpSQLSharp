namespace PowerUpSQLSharp.Core.Models
{
    public sealed class CommandResult
    {
        public int ExitCode { get; set; }
        public string Output { get; set; }
        public string Error { get; set; }
        public string Warning { get; set; }

        public static CommandResult Success(string output = null, string warning = null)
        {
            return new CommandResult { ExitCode = ExitCodes.Success, Output = output, Warning = warning };
        }

        public static CommandResult Failure(int exitCode, string error)
        {
            return new CommandResult { ExitCode = exitCode, Error = error };
        }
    }

    public static class ExitCodes
    {
        public const int Success = 0;
        public const int GeneralError = 1;
        public const int ArgumentError = 2;
        public const int ConnectionFailed = 3;
        public const int PermissionDenied = 4;
        public const int PartialFailure = 5;
    }
}
