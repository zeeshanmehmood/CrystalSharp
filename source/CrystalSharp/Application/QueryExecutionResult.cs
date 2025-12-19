using System.Collections.Generic;

namespace CrystalSharp.Application
{
    public class QueryExecutionResult<TResult>
    {
        public bool Success { get; set; }
        public IEnumerable<Error> Errors { get; set; }
        public TResult Data { get; set; }

        public static QueryExecutionResult<TResult> WithError(IEnumerable<Error> errors)
        {
            return new QueryExecutionResult<TResult>
            {
                Success = false,
                Errors = errors
            };
        }
    }
}
