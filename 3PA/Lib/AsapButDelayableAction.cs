using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using _3PA.MainFeatures;
using Timer = System.Timers.Timer;

namespace _3PA.Lib {
    internal class AsapButDelayableAction : IDisposable {
        #region Static

        private static List<AsapButDelayableAction> _savedActions = new List<AsapButDelayableAction>();

        /// <summary>
        /// Clean all delayed actions started
        /// </summary>
        public static void CleanAll() {
            foreach (var action in _savedActions.ToList()) {
                action.Cancel();
            }
        }

        #endregion

        #region event

        /// <summary>
        /// Event published when the parser starts doing its job
        /// </summary>
        public event Action BeforeAction;

        /// <summary>
        /// Event published when the parser has done its job and it's time to get the results
        /// </summary>
        public event Action AfterAction;

        #endregion

        #region private

        private object _lock = new object();
        private object _timerLock = new object();
        private Timer _timer;
        private Action _toDo;
        private Task _task;
        private CancellationTokenSource _cancelSource;
        private int _msDelay;
        private int _msToDoTimeout = 2000;
        private volatile bool _timerOnGoing;

        #endregion

        #region Constructor

        public AsapButDelayableAction(int msDelay, Action toDo) {
            _msDelay = msDelay;
            _toDo = toDo;
            _cancelSource = new CancellationTokenSource();
            _savedActions.Add(this);
        }

        #endregion

        #region Public

        /// <summary>
        /// Max time to wait (in ms) when trying to do the action that has already been started
        /// (this should be set roughly to the time needed to do the action)..
        /// </summary>
        public int MsToDoTimeout {
            get { return _msToDoTimeout; }
            set { _msToDoTimeout = value; }
        }

        /// <summary>
        /// Start the action with a delay, delay that can be extended if this method is called again within the
        /// delay
        /// </summary>
        public void DoDelayable() {
            // do on delay, can be delayed event more if this method is called again
            if (Monitor.TryEnter(_timerLock, 50)) {
                try {
                    if (_timer == null) {
                        _timer = new Timer {
                            AutoReset = false,
                            Interval = _msDelay
                        };
                        _timer.Elapsed += (sender, args) => TimerTick();
                        _timer.Start();
                    } else {
                        // reset timer
                        _timer.Stop();
                        _timer.Start();
                    }
                    _timerOnGoing = true;
                } finally {
                    Monitor.Exit(_timerLock);
                }
            }
        }

        /// <summary>
        /// Wait for the latest task to be completed (but for a max of ms)
        /// If a timer was already set, it triggers the to do method immediately and returns true
        /// </summary>
        public bool WaitLatestTask(int maxMsWait = -1) {
            if (maxMsWait == -1)
                maxMsWait = MsToDoTimeout;
            bool timerOnGoing = false;
            if (Monitor.TryEnter(_timerLock, 50)) {
                timerOnGoing = _timerOnGoing;
                Monitor.Exit(_timerLock);
            }
            if (timerOnGoing) {
                // a timer was set, trigger it now
                TimerTick();
            }
            if (_task != null)
                _task.Wait(maxMsWait);
            return timerOnGoing;
        }

        /// <summary>
        /// Forces to do the action now, if one is already ongoing then we wait for the end and do another
        /// </summary>
        public void DoSync(int maxMsWait = -1) {
            WaitLatestTask(maxMsWait);
            ToDo();
        }

        /// <summary>
        /// Forces to do the action now but still async
        /// </summary>
        public void DoTaskNow(int maxMsWait = -1) {
            TimerTick();
        }

        #endregion

        #region Private

        /// <summary>
        /// Called when the _parserTimer ticks
        /// refresh the Items list with all the static items
        /// as well as the dynamic items found by the parser
        /// </summary>
        private void TimerTick() {
            if (_task != null && !_task.IsCompleted) {
                return;
            }
            if (Monitor.TryEnter(_timerLock, 500)) {
                _timerOnGoing = false;
                _task = Task.Factory.StartNew(ToDo, _cancelSource.Token);
                Monitor.Exit(_timerLock);
            }
        }

        private void ToDo() {
            if (Monitor.TryEnter(_lock, MsToDoTimeout)) {
                try {
                    if (BeforeAction != null)
                        BeforeAction();
                    if (!_cancelSource.IsCancellationRequested)
                        _toDo();
                    if (AfterAction != null)
                        AfterAction();
                } catch (Exception e) {
                    ErrorHandler.ShowErrors(e, "Error in AsapButDelayableAction.ToDo");
                } finally {
                    Monitor.Exit(_lock);
                }
            }
        }

        #endregion

        #region Stop

        /// <summary>
        /// Stop the recurrent action
        /// </summary>
        public void Cancel() {
            try {
                if (_task != null) {
                    _cancelSource.Cancel();
                }
                if (_timer != null) {
                    _timer.Stop();
                    _timer.Close();
                }
            } catch (Exception) {
                // clean up proc
            } finally {
                _savedActions.Remove(this);
            }
        }

        public void Dispose() {
            Cancel();
        }

        #endregion
    }
}