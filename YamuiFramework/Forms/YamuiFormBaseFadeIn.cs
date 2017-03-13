#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiFormBaseFadeIn.cs) is part of YamuiFramework.
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
using System.ComponentModel;
using System.Windows.Forms;
using YamuiFramework.Animations.Transitions;

namespace YamuiFramework.Forms {
    /// <summary>
    /// Form class that adds a fade in/out animation on form show/close
    /// </summary>
    public class YamuiFormBaseFadeIn : YamuiFormBase {
        #region Private

        private bool _closingAnimationOnGoing;

        protected int _animationDuration = 200;

        #endregion

        #region Field

        /// <summary>
        /// Milliseconds duration for the fade in/fade out animation
        /// </summary>
        public int AnimationDuration {
            get { return _animationDuration; }
            set { _animationDuration = value; }
        }

        /// <summary>
        /// This field is used for the fade in/out animation, shouldn't be used by the user
        /// </summary>
        public virtual double AnimationOpacity {
            get { return Opacity; }
            set {
                if (value < 0) {
                    try {
                        Close();
                    } catch (Exception) {
                        // ignored
                    }
                    return;
                }
                Opacity = value;
            }
        }

        #endregion

        #region On closing

        protected override void OnClosing(CancelEventArgs e) {
            // cancel initialise close to run an animation, after that allow it
            if (!_closingAnimationOnGoing) {
                _closingAnimationOnGoing = true;
                e.Cancel = true;
                if (AnimationDuration > 0) {
                    Transition.run(this, "AnimationOpacity", 1d, -0.01d, new TransitionType_Acceleration(AnimationDuration), (o, args1) => { Dispose(); });
                } else {
                    Close();
                    Dispose();
                }
            } else {
                base.OnClosing(e);
            }
        }

        #endregion

        #region Forceclose

        public void ForceClose() {
            _closingAnimationOnGoing = true;
            Close();
            Dispose();
        }

        #endregion

        #region Show

        /// <summary>
        /// Call this method to show the notification
        /// </summary>
        public new void Show() {
            if (AnimationDuration > 0)
                Transition.run(this, "AnimationOpacity", 0d, 1d, new TransitionType_Acceleration(AnimationDuration));
            base.Show();
        }

        public new void ShowDialog() {
            if (AnimationDuration > 0)
                Transition.run(this, "AnimationOpacity", 0d, 1d, new TransitionType_Acceleration(AnimationDuration));
            base.ShowDialog();
        }

        public new void Show(IWin32Window owner) {
            if (AnimationDuration > 0)
                Transition.run(this, "AnimationOpacity", 0d, 1d, new TransitionType_Acceleration(AnimationDuration));
            base.Show(owner);
        }

        public new void ShowDialog(IWin32Window owner) {
            if (AnimationDuration > 0)
                Transition.run(this, "AnimationOpacity", 0d, 1d, new TransitionType_Acceleration(AnimationDuration));
            base.ShowDialog(owner);
        }

        #endregion
    }
}