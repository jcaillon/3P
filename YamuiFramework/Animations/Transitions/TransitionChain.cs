#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (TransitionChain.cs) is part of YamuiFramework.
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

using System.Collections.Generic;

namespace YamuiFramework.Animations.Transitions {
    internal class TransitionChain {
        #region Public methods

        public TransitionChain(params Transition[] transitions) {
            // We store the list of transitions...
            foreach (Transition transition in transitions) {
                m_listTransitions.AddLast(transition);
            }

            // We start running them...
            runNextTransition();
        }

        #endregion

        #region Private functions

        /// <summary>
        /// Runs the next transition in the list.
        /// </summary>
        private void runNextTransition() {
            if (m_listTransitions.Count == 0) {
                return;
            }

            // We find the next transition and run it. We also register
            // for its completed event, so that we can start the next transition
            // when this one completes...
            Transition nextTransition = m_listTransitions.First.Value;
            nextTransition.TransitionCompletedEvent += onTransitionCompleted;
            nextTransition.run();
        }

        /// <summary>
        /// Called when the transition we have just run has completed.
        /// </summary>
        private void onTransitionCompleted(object sender, Transition.Args e) {
            // We unregister from the completed event...
            Transition transition = (Transition) sender;
            transition.TransitionCompletedEvent -= onTransitionCompleted;

            // We remove the completed transition from our collection, and
            // run the next one...
            m_listTransitions.RemoveFirst();
            runNextTransition();
        }

        #endregion

        #region Private data

        // The list of transitions in the chain...
        private LinkedList<Transition> m_listTransitions = new LinkedList<Transition>();

        #endregion
    }
}