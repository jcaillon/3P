#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (ReccurentAction.cs) is part of 3P.
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
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace _3PA.Lib {
    /// <summary>
    /// Allows to do a given action every XXX ms for XXX times
    /// </summary>
    class ReccurentAction {

        #region private fields

        private Timer _timer;

        private int _nbRepeat;

        private Action _actionToDo;

        private int _repeatCounter;

        private ReaderWriterLockSlim _lock;

        #endregion


        #region Life and death

        /// <summary>
        /// Allows to do a given action (in a new task) every XXX ms for XXX times
        /// by default it does the action when creating this instance, set doActionOnCreate = false to not do it immediatly
        /// </summary>
        public ReccurentAction(Action actionToDo, long timeLapse, int nbRepeat = 0, bool doActionOnCreate = true) {
            _nbRepeat = nbRepeat;
            _actionToDo = actionToDo;
            _lock = new ReaderWriterLockSlim();

            if (_actionToDo == null)
                throw new Exception("ReccurentAction > the action can't be null");

            // initiate the timer if needed
            if (_timer == null) {
                _timer = new Timer(timeLapse) {
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
        public void Stop() {
            try {
                if (_timer != null) {
                    _timer.Stop();
                    _timer.Close();
                    _timer.Dispose();
                }
            } catch (Exception) {
                // clean up proc
            }
        }

        ~ReccurentAction() {
            Stop();
        }

        #endregion


        #region private methods

        /// <summary>
        /// This method every time the timer ticks
        /// </summary>
        private void OnTick(object sender, ElapsedEventArgs elapsedEventArgs) {
            if (_lock.TryEnterWriteLock(100)) {
                try {
                    // increase number of already repeated action
                    _repeatCounter++;
                    if (_repeatCounter >= _nbRepeat)
                        Stop();

                    // new task, do the action
                    Task.Factory.StartNew(() => {
                        _actionToDo();
                    });
                } finally {
                    _lock.ExitWriteLock();
                }
            }
        }

        #endregion


    }
}
