#if !NO_WINFORMS

// Daniel Moth 12/2004

using System;
using System.ComponentModel;

namespace InTheHand.ComponentModel {

	#region EventsArgs classes
	internal class RunWorkerCompletedEventArgs : System.EventArgs {
		// This class should inherit from AsyncCompletedEventArgs but I don't see the point in the CF's case
		private readonly object mResult;
		private readonly bool mCancelled;
		private readonly System.Exception mError;

		public RunWorkerCompletedEventArgs(object aResult, System.Exception aError, bool aCancelled) {
			mResult = aResult;
			mError = aError;
			mCancelled = aCancelled;
		}

		public object Result { 
			get{
				return mResult;
			} 
		}

		public bool Cancelled { 
			get{
				return mCancelled;
			} 
		}

		public System.Exception Error { 
			get{
				return mError;
			}
		}


		#region These are in the help but never seem to get used
		//		private object mUserState;
		//		public object UserState { 
		//			get{
		//				return mUserState;
		//			}
		//		}
		#endregion
	}

	internal class ProgressChangedEventArgs : System.EventArgs {
		private readonly int mProgressPercent;
		private readonly object mUserState;

		public ProgressChangedEventArgs(int aProgressPercent, object aUserState){
			mProgressPercent = aProgressPercent;
			mUserState = aUserState;
		}

		public int ProgressPercentage { 
			get{
				return mProgressPercent;
			}
		}

		public object UserState { 
			get{
				return mUserState;
			}
		}		
	}
 

	internal class DoWorkEventArgs : System.ComponentModel.CancelEventArgs {
		private readonly object mArgument;
		private object mResult;

		public DoWorkEventArgs(object aArgument){
			mArgument = aArgument;
		}

		public object Argument { 
			get{
				return mArgument;
			}
 
		}

		public object Result {       
			get {
				return mResult;
			}
			set {
				mResult = value;
			}
		}
	}
	#endregion

	#region Delegates
	internal delegate void DoWorkEventHandler(object sender, DoWorkEventArgs e);
	internal delegate void ProgressChangedEventHandler(object sender, ProgressChangedEventArgs e);
	internal delegate void RunWorkerCompletedEventHandler(object sender, RunWorkerCompletedEventArgs e);
	#endregion

	#region BackgroundWorker Class
	internal class BackgroundWorker : Component 
	{
		#region Public Interface

		/// <summary>
        /// Occurs when <see cref="M:RunWorkerAsync()"/> / <see cref="M:RunWorkerAsync(System.Object)"/> is called.
		/// </summary>
#if DESIGN
		[Category("Asynchronous"),
		Description("Event handler to run on a different thread when the operation begins.")]
#endif
		public event DoWorkEventHandler DoWork;
		/// <summary>
		/// Occurs when <see cref="ReportProgress(System.Int32)"/> is called.
		/// </summary>
#if DESIGN
		[Category("Asynchronous"),
		Description("Raised when the worker thread indicates that some progress has been made.")]
#endif
		public event ProgressChangedEventHandler ProgressChanged;

		/// <summary>
		/// Occurs when the background operation has completed, has been cancelled, or has raised an exception.
		/// </summary>
#if DESIGN
		[Category("Asynchronous"),
		Description("Raised when the worker has completed (either through success, failure, or cancellation).")]
#endif
		public event RunWorkerCompletedEventHandler RunWorkerCompleted;

		/// <summary>
		/// Initializes a new instance of the BackgroundWorker class.
		/// </summary>
		public BackgroundWorker() : this(new System.Windows.Forms.Control()){			
			/* 
			  ideally we want to call Control.CreateControl()
			  without it, running on the desktop will crash (it is OK on the CF)
			  [on the full fx simply calling a control's constructor does not create the Handle.]
			  
			  The CreateControl method is not supported on the CF so to keep this assembly retargettable
			  I have offered the alternative ctor for desktop clients 
			  (where they can pass in already created controls)
			*/
		}

		/// <summary>
		/// Initializes a new instance of the BackgroundWorker class.
		/// Call from the desktop code as the other ctor is not good enough
		/// Call it passing in a created control e.g. the Form
		/// </summary>
		public BackgroundWorker(System.Windows.Forms.Control c) : base(){			
			mGuiMarshaller = c;	
		}

		/// <summary>
		/// Gets a value indicating whether the application has requested cancellation of a background operation.
		/// </summary>
#if DESIGN
		[Browsable(false)]
#endif
		public bool CancellationPending {
			get{
				return mCancelPending;
			} 
		}
		/// <summary>
		/// Raises the BackgroundWorker.ProgressChanged event.
		/// </summary>
		/// <param name="aProgressPercent">The percentage, from 0 to 100, of the background operation that is complete. </param>
		public void ReportProgress(int aProgressPercent){
			this.ReportProgress(aProgressPercent, null);
		}

		/// <summary>
		/// Raises the BackgroundWorker.ProgressChanged event.
		/// </summary>
		/// <param name="aProgressPercent">The percentage, from 0 to 100, of the background operation that is complete. </param>
		/// <param name="aUserState">The state object passed to BackgroundWorker.RunWorkerAsync(System.Object).</param>
		public void ReportProgress(int aProgressPercent, object aUserState){
			if (!mDoesProgress) {
				throw new System.InvalidOperationException("Doesn't do progress events. You must WorkerReportsProgress=True");
			}
			
			// Send the event to the GUI
			System.Threading.ThreadPool.QueueUserWorkItem(
				new System.Threading.WaitCallback(ProgressHelper),
				new ProgressChangedEventArgs(aProgressPercent, aUserState));
		}

		/// <summary>
		/// Starts execution of a background operation.
		/// </summary>
		public void RunWorkerAsync(){
			this.RunWorkerAsync(null);
		}

		/// <summary>
		/// Starts execution of a background operation.
		/// </summary>
		/// <param name="aArgument"> A parameter for use by the background operation to be executed in the BackgroundWorker.DoWork event handler.</param>
		public void RunWorkerAsync(object aArgument){
			if (mInUse) {
				throw new System.InvalidOperationException("Already in use");
			}

			if (DoWork == null){
				throw new System.InvalidOperationException("You must subscribe to the DoWork event.");
			}

			mInUse = true;
			mCancelPending = false;
			
			System.Threading.ThreadPool.QueueUserWorkItem(
				new System.Threading.WaitCallback(DoTheRealWork),aArgument);
		}

		/// <summary>
		/// Requests cancellation of a pending background operation.
		/// </summary>
		public void CancelAsync(){
			if (!mDoesCancel) {
				throw new System.InvalidOperationException("Does not support cancel. You must WorkerSupportsCancellation=true");
			}
			mCancelPending = true;
		}

		/// <summary>
		/// Gets or sets a value indicating whether the BackgroundWorker object can report progress updates.
		/// </summary>
#if DESIGN
		[Category("Asynchronous"),
		Description("Whether the worker will report progress.")]
#endif
		public bool WorkerReportsProgress {       
			get 
		{
				return mDoesProgress;
			}
			set {
				mDoesProgress = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the BackgroundWorker object supports asynchronous cancellation.
		/// </summary>
#if DESIGN
		[Category("Asynchronous"),
		Description("Whether the worker supports cancellation.")]
#endif
		public bool WorkerSupportsCancellation 
		{       
			get {
				return mDoesCancel;
			}
			set {
				mDoesCancel = value;
			}
		}


		/// <summary>
		///  Gets a value indicating whether the System.ComponentModel.BackgroundWorker is running an asynchronous operation.
		/// Returns:
		/// true, if the System.ComponentModel.BackgroundWorker is running an
		/// asynchronous operation; otherwise, false.
		/// </summary>
		public bool IsBusy{
			get{
				return mInUse;
			}
		}
		#endregion

		#region Fields
		//Ensures the component is used only once per session
		private bool mInUse;

		//Stores the cancelation request that the worker thread (user's code) should check via CancellationPending
		private bool mCancelPending; 

		//Whether the object supports cancelling or not (and progress or not)
		private bool mDoesCancel;
		private bool mDoesProgress;

		//Helper objects since Control.Invoke takes no arguments
		private RunWorkerCompletedEventArgs mFinalResult;
		private ProgressChangedEventArgs mProgressArgs;

		// Helper for marshalling execution to GUI thread
		private System.Windows.Forms.Control mGuiMarshaller;
		#endregion

		#region Private Methods
		// Async(ThreadPool) called by ReportProgress for reporting progress
		private void ProgressHelper(object o){
			mProgressArgs = (ProgressChangedEventArgs)o;//TODO put this in a queue to preserve the userState if the client code call ReportProgress in quick succession
			mGuiMarshaller.Invoke(new System.EventHandler(TellThemOnGuiProgress));			
		}
		// ControlInvoked by ProgressHelper for raising progress
		private void TellThemOnGuiProgress(object sender, System.EventArgs e){
			if (ProgressChanged != null){
				ProgressChanged(this,mProgressArgs);
			}
		}

		// Async(ThreadPool) called by RunWorkerAsync [the little engine of this class]
		private void DoTheRealWork(object o){
			// declare/initialise the vars we will pass back to client on completion
			System.Exception er = null;
			bool ca = false;
			object result = null;

			// Raise the event passing the original argument and catching any exceptions
			try{
				DoWorkEventArgs inOut = new DoWorkEventArgs(o);
				DoWork(this,inOut);

				ca = inOut.Cancel;
				result = inOut.Result;
			}catch (System.Exception ex){
				er = ex;
			}

			// store the completed final result in a temp var
			RunWorkerCompletedEventArgs tempResult = new RunWorkerCompletedEventArgs(result,er,ca);

			// return execution to client by going async here
			System.Threading.ThreadPool.QueueUserWorkItem(
				new System.Threading.WaitCallback(RealWorkHelper),tempResult);

			// prepare for next use
			mInUse = false;
			mCancelPending = false;
		}

		// Async(ThreadPool) called by DoTheRealWork [to avoid any rentrancy issues at the client end]
		private void RealWorkHelper(object o){
			mFinalResult = (RunWorkerCompletedEventArgs)o;
			mGuiMarshaller.Invoke(new System.EventHandler(TellThemOnGuiCompleted));			
		}
		// ControlInvoked by RealWorkHelper for raising final completed event
		private void TellThemOnGuiCompleted(object sender, System.EventArgs e){
			if (RunWorkerCompleted != null){
				RunWorkerCompleted(this,mFinalResult);
			}
		}

		#endregion
	}
	#endregion
}
#endif
