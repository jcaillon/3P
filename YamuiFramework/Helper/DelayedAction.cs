#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
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
using System.Threading.Tasks;
using System.Timers;

namespace YamuiFramework.Helper {

    /// <summary>
    /// Simple class to delay an action
    /// </summary>
    public class DelayedAction : IDisposable {

        private Timer _timer;

        private Action _toDo;

        /// <summary>
        /// Use this class to do an action after a given delay
        /// </summary>
        public DelayedAction(int msDelay, Action toDo) {
            _toDo = toDo;
            _timer = new Timer {
                AutoReset = false,
                Interval = msDelay
            };
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs) {
            Task.Factory.StartNew(() => {
                _toDo();
            });
            CleanUp();
        }

        /// <summary>
        /// Stop the recurrent action
        /// </summary>
        public void CleanUp() {
            try {
                if (_timer != null) {
                    _timer.Stop();
                    _timer.Close();
                }
            } catch (Exception) {
                // clean up proc
            }
        }

        public void Dispose() {
            CleanUp();
        }
    }
}
