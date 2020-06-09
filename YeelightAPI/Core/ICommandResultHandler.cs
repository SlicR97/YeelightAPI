using System;
using YeelightAPI.Models;

namespace YeelightAPI.Core
{
    /// <summary>
    /// Handler interface for CommandResult
    /// </summary>
    internal interface ICommandResultHandler
    {
        /// <summary>
        /// Type of the result
        /// </summary>
        Type ResultType { get; }

        /// <summary>
        /// Sets the error
        /// </summary>
        /// <param name="commandResultError"></param>
        void SetError(CommandResult.CommandErrorResult commandResultError);

        /// <summary>
        /// Sets the result
        /// </summary>
        /// <param name="commandResult"></param>
        void SetResult(CommandResult commandResult);

        /// <summary>
        /// Try to cancel
        /// </summary>
        void TrySetCanceled();
    }
}