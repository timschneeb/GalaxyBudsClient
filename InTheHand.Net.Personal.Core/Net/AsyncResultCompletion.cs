// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.AsyncResultCompletion
// 
// Copyright (c) 2010 In The Hand Ltd, All rights reserved.
// Copyright (c) 2010 Alan J McFarlane, All rights reserved.
// This source code is licensed under the MIT License

namespace InTheHand.Net
{
    /// <summary>
    /// Used with
    /// <see cref="M:InTheHand.Net.AsyncResultNoResult.SetAsCompleted(System.Exception,InTheHand.Net.AsyncResultCompletion)">
    /// AsyncResultNoResult.SetAsCompleted</see> and 
    /// <see cref="M:InTheHand.Net.AsyncResult{TResult}.SetAsCompleted(TResult,AsyncResultNoResult.AsyncResultCompletion)">
    /// AsyncResult&lt;TResult&gt;.SetAsCompleted</see>.
    /// </summary>
    internal enum AsyncResultCompletion
    {
        /// <summary>
        /// Equivalent to <c>true</c> for the <see cref="T:System.Boolean"/>
        /// #x201C;completedSynchronously&#x201D; parameter.
        /// </summary>
        IsSync,
        /// <summary>
        /// Equivalent to <c>false</c> for the <see cref="T:System.Boolean"/>
        /// #x201C;completedSynchronously&#x201D; parameter.
        /// </summary>
        IsAsync,
        /// <summary>
        /// Forces the callback to run on a thread-pool thread.
        /// </summary>
        MakeAsync
    }

}
