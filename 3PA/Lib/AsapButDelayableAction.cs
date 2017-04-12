#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (AsapButDelayableAction.cs) is part of 3P.
// 
// 3P is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// 3P is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with 3P. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        #region private

        private Timer _timer;
        private Action _toDo;
        private Task _task;
        private CancellationTokenSource _cancelSource;
        private int _msDelay;

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
        /// Start the action with a delay, delay that can be extended if this method is called again within the
        /// delay
        /// </summary>
        public void DoDelayable() {
            // do on delay, can be delayed event more if this method is called again
            if (_timer == null) {
                _timer = new Timer(_msDelay) {
                    AutoReset = false
                };
                _timer.Elapsed += (sender, args) => TimerTick();
                _timer.Start();
            } else {
                // reset timer
                _timer.Stop();
                _timer.Start();
            }
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
            if (_timer != null) {
                _timer.Stop();
                _timer.Close();
                _timer = null;
            }
            if (_task != null && !_task.IsCompleted)
                return;
            _task = Task.Factory.StartNew(ToDo, _cancelSource.Token);
        }

        private void ToDo() {
            if (!_cancelSource.IsCancellationRequested)
                _toDo();
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
                    _timer = null;
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