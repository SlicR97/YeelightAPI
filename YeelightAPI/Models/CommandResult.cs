using System.Collections.Generic;

namespace YeelightAPI.Models
{
    /// <summary>
    /// Extensions for CommandResult
    /// </summary>
    public static class CommandResultExtensions
    {
        /// <summary>
        /// Determine if the result is a classical OK result ({"id":1, "result":["ok"]})
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsOk(this CommandResult<List<string>> @this)
        {
            return @this?.Error == null && @this?.Result?[0] == "ok";
        }
    }

    /// <summary>
    /// Default command result
    /// </summary>
    public class CommandResult
    {
        /// <summary>
        /// Error, null if command is successful
        /// </summary>
        public CommandErrorResult Error { get; set; }

        /// <summary>
        /// Request Id (mirrored from the sent request)
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Error model
        /// </summary>
        public class CommandErrorResult
        {
            /// <summary>
            /// Error code
            /// </summary>
            public int Code { get; set; }

            /// <summary>
            /// Error message
            /// </summary>
            public string Message { get; set; }
            
            /// <summary>
            /// ToString override
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return $"{Code} - {Message}";
            }
        }
    }

    /// <summary>
    /// Result received after a Command has been sent
    /// </summary>
    public class CommandResult<T> : CommandResult
    {
        /// <summary>
        /// Result
        /// </summary>
        public T Result { get; set; }
    }
}