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

        private static List<AsapButDelayableAction> _savedDelayedActions = new List<AsapButDelayableAction>();

        /// <summary>
        /// Clean all delayed actions started
        /// </summary>
        public static void CleanAll() {
            foreach (var action in _savedDelayedActions.ToList()) {
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

        private static volatile bool _parseRequestedWhenBusy;

        private static volatile bool _parsing;

        private ReaderWriterLockSlim _timerLock = new ReaderWriterLockSlim();

        private static ReaderWriterLockSlim _parserLock = new ReaderWriterLockSlim();

        private Timer _timer;

        private Action _toDo;

        private Task _task;

        private CancellationTokenSource _cancelSource;

        private int _msDelay;

        private int _msToDoTimeout = 1000;

        #endregion

        #region Constructor

        public AsapButDelayableAction(int msDelay, Action toDo) {
            _msDelay = msDelay;
            _toDo = toDo;
            _cancelSource = new CancellationTokenSource();
            _savedDelayedActions.Add(this);
        }

        #endregion
        
        #region Public

        public void DoOnTimerDelayable() {
            // do on delay, can be delayed event more if this method is called again
            if (_timerLock.TryEnterWriteLock(50)) {
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
                } finally {
                    _timerLock.ExitWriteLock();
                }
            }
        }

        public void DoSynchronized() {
            DoParse();
        }

        #endregion

        #region Private

        /// <summary>
        /// Called when the _parserTimer ticks
        /// refresh the Items list with all the static items
        /// as well as the dynamic items found by the parser
        /// </summary>
        private void TimerTick() {
            if (_parsing) {
                _parseRequestedWhenBusy = true;
                return;
            }
            _parseRequestedWhenBusy = false;
            _task = Task.Factory.StartNew(DoParse, _cancelSource.Token);
        }

        private void DoParse() {
            _parsing = true;
            try {
                if (_parserLock.TryEnterWriteLock(_msToDoTimeout)) {
                    try {
                        if (BeforeAction != null)
                            BeforeAction();
                        if (!_cancelSource.IsCancellationRequested)
                            _toDo();
                        if (AfterAction != null)
                            AfterAction();
                    } finally {
                        _parserLock.ExitWriteLock();
                    }
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in ParseCurrentDocumentTick ");
            } finally {
                _parsing = false;
                if (_parseRequestedWhenBusy)
                    TimerTick();
            }
        }

        #endregion

        #region Stop

        /// <summary>
        /// Stop the recurrent action
        /// </summary>
        public void Cancel() {
            try {
                if (_timer != null) {
                    _timer.Stop();
                    _timer.Close();
                }
                if (_task != null) {
                    _cancelSource.Cancel();
                }
            } catch (Exception) {
                // clean up proc
            } finally {
                _savedDelayedActions.Remove(this);
            }
        }

        public void Dispose() {
            Cancel();
        }

        #endregion
    }
}
