#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (RecurentAction.cs) is part of 3P.
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
using System.Timers;
using Timer = System.Timers.Timer;

namespace _3PA.Lib {
    /// <summary>
    /// Allows to do a given action every XXX ms for XXX times
    /// </summary>
    public class RecurentAction : IDisposable {
        #region Static

        private static List<RecurentAction> _savedReccurentActionStarted = new List<RecurentAction>();

        /// <summary>
        /// Clean all recurrent actions started
        /// </summary>
        public static void CleanAll() {
            foreach (var reccurentAction in _savedReccurentActionStarted.ToList()) {
                reccurentAction.Stop();
            }
        }

        /// <summary>
        /// Start a new recurrent action
        /// </summary>
        public static RecurentAction StartNew(Action actionToDo, long interval, int nbRepeat = 0, bool doActionOnCreate = true) {
            var created = new RecurentAction(actionToDo, interval, nbRepeat, doActionOnCreate);
            _savedReccurentActionStarted.Add(created);
            return created;
        }

        #endregion

        #region private fields

        private Timer _timer;

        private int _nbRepeat;

        private Action _actionToDo;

        private int _repeatCounter;

        private object _lock;

        #endregion

        #region Life and death

        /// <summary>
        /// Executed ASYNCHRONOUSLY, 
        /// Allows to do a given action (in a new task) every XXX ms for XXX times
        /// by default it does the action when creating this instance, set doActionOnCreate = false to not do it immediatly
        /// </summary>
        private RecurentAction(Action actionToDo, long interval, int nbRepeat = 0, bool doActionOnCreate = true) {
            _nbRepeat = nbRepeat;
            _actionToDo = actionToDo;
            _lock = new object();

            if (_actionToDo == null)
                throw new Exception("ReccurentAction > the action can't be null");

            // initiate the timer if needed
            if (_timer == null) {
                _timer = new Timer(interval) {
                    AutoReset = true
                };
                _timer.Elapsed += OnTick;
                _timer.Start();
            }

            // do the recurrent action immediatly?
            if (doActionOnCreate)
                OnTick(null, null);
        }

        /// <summary>
        /// Stop the recurrent action
        /// </summary>
        private void Stop() {
            if (_timer != null) {
                _timer.Stop();
                _timer.Close();
            }
            _savedReccurentActionStarted.Remove(this);
        }

        public void Dispose() {
            Stop();
        }

        #endregion

        #region private methods

        /// <summary>
        /// This method every time the timer ticks
        /// </summary>
        private void OnTick(object sender, ElapsedEventArgs elapsedEventArgs) {
            Task.Factory.StartNew(() => {
                if (Monitor.TryEnter(_lock, 100)) {
                    try {
                        // increase number of already repeated action
                        _repeatCounter++;
                        if (_nbRepeat > 0 && _repeatCounter >= _nbRepeat)
                            Stop();

                        // new task, do the action
                        _actionToDo();
                    } finally {
                        Monitor.Exit(_lock);
                    }
                }
            });
        }

        #endregion
    }
}