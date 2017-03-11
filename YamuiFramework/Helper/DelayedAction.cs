#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (DelayedAction.cs) is part of YamuiFramework.
// 
// YamuiFramework is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// YamuiFramework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with YamuiFramework. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace YamuiFramework.Helper {
    /// <summary>
    /// Simple class to delay an action
    /// </summary>
    public class DelayedAction : IDisposable {
        #region private fields

        private Timer _timer;

        private Action _toDo;

        private static List<DelayedAction> _savedDelayedActions = new List<DelayedAction>();

        #endregion

        #region Life and death

        /// <summary>
        /// Use this class to do an action after a given delay
        /// </summary>
        public DelayedAction(int msDelay, Action toDo) {
            _savedDelayedActions.Add(this);
            _toDo = toDo;
            _timer = new Timer {AutoReset = false, Interval = msDelay};
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }

        /// <summary>
        /// Stop the recurrent action
        /// </summary>
        public void Stop() {
            try {
                if (_timer != null) {
                    _timer.Stop();
                    _timer.Close();
                }
            } catch (Exception) {
                // clean up proc
            } finally {
                _savedDelayedActions.Remove(this);
            }
        }

        public void Dispose() {
            Stop();
        }

        #endregion

        #region public

        /// <summary>
        /// Clean all delayed actions started
        /// </summary>
        public static void CleanAll() {
            foreach (var action in _savedDelayedActions.ToList()) {
                action.Stop();
            }
        }

        #endregion

        #region private

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs) {
            Task.Factory.StartNew(() => { _toDo(); });
            Stop();
        }

        #endregion
    }
}