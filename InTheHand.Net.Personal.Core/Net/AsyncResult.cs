/******************************************************************************
Module:  AsyncResult.cs
Notices: Written by Jeffrey Richter
******************************************************************************/

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;


///////////////////////////////////////////////////////////////////////////////
namespace InTheHand.Net
{

    [DebuggerNonUserCode] // alanjmcf
    internal class AsyncResult<TResult> : AsyncResultNoResult
    {
       // Field set when operation completes
       private TResult m_result = default(TResult);

       public AsyncResult(AsyncCallback asyncCallback, Object state) : base(asyncCallback, state) { }

       [DebuggerNonUserCode] // alanjmcf
       public void SetAsCompleted(TResult result, Boolean completedSynchronously)
       {
           // Save the asynchronous operation's result
           m_result = result;

           // Tell the base class that the operation completed sucessfully (no exception)
           base.SetAsCompleted(null, completedSynchronously);
       }

       [DebuggerNonUserCode] // alanjmcf
       public void SetAsCompleted(TResult result, AsyncResultCompletion completion)
       {
          // Save the asynchronous operation's result
          m_result = result;

          // Tell the base class that the operation completed sucessfully (no exception)
          base.SetAsCompleted(null, completion);
       }

       [DebuggerNonUserCode] // alanjmcf
       new public TResult EndInvoke()
       {
          base.EndInvoke(); // Wait until operation has completed 
          return m_result;  // Return the result (if above didn't throw)
       }

        #region SetAsCompleted with: Func getResultsOrThrow
#if V2 // Hack to not include in XxxMenuTesting where WidcommPortSingleThreader.Func<> is inaccessible.
        /// <summary>
        /// Get the results of the operation from the specified function
        /// and set the operation as completed,
        /// or if getting the results fails then set the corresponding error
        /// completion.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>The pattern that comes to mind when calling
        /// <see cref="M:SetAsCompleted(TResult,AsyncResultCompletion)"/> is
        /// the incorrect:
        /// <code>try {
        ///    var result = SomeStatementsAndFunctionCallsToGetTheResult(...);
        ///    ar.SetAsCompleted(result, false);
        /// } catch (Exception ex) {
        ///    ar.SetAsCompleted(ex, false);
        /// }
        /// </code>
        /// That is wrong because if the user callback fails with an exception
        /// then we'll catch it and try to call SetAsCompleted a second time!
        /// </para>
        /// <para>We need to instead call SetAsCompleted outside of the try
        /// block.  This method provides that pattern.
        /// </para>
        /// </remarks>
        /// -
        /// <param name="getResultsOrThrow">A delegate containing the function
        /// to call to get the result.
        /// It should throw an exception in error cases.
        /// </param>
        /// <param name="completedSynchronously"></param>
       internal void SetAsCompletedWithResultOf(
           Func<TResult> getResultsOrThrow,
           Boolean completedSynchronously)
       {
           SetAsCompletedWithResultOf(getResultsOrThrow, ConvertCompletion(completedSynchronously));
       }

       [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
           Justification = "SetAsCompleted.")]
       internal void SetAsCompletedWithResultOf(
           Func<TResult> getResultsOrThrow,
           AsyncResultCompletion completion)
       {
           TResult result;
           try {
               result = getResultsOrThrow();
           } catch (Exception ex) {
               this.SetAsCompleted(ex, completion);
               return;
           }
           this.SetAsCompleted(result, completion);
       }
#endif
        #endregion
    }


    // alanjmcf
    [DebuggerNonUserCode] // alanjmcf
    internal class AsyncResult<TResult, TParams> : AsyncResult<TResult>
    {
        readonly TParams m_args;

        public AsyncResult(AsyncCallback asyncCallback, Object state, TParams args)
            : base(asyncCallback, state)
        {
            m_args = args;
        }

        public TParams BeginParameters { get { return m_args; } }
    }

}
//////////////////////////////// End of File //////////////////////////////////
