using System.Collections.Generic;

namespace CrystalSharp.Application
{
    public class CommandExecutionResult<TResult>
    {
        public bool Success { get; set; }
        public IEnumerable<Error> Errors { get; set; }
        public TResult Data { get; set; }

        public static CommandExecutionResult<TResult> WithError(IEnumerable<Error> errors)
        {
            return new CommandExecutionResult<TResult>
            {
                Success = false,
                Errors = errors
            };
        }
    }
}
