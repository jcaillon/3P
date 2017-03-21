#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (CompletionFilterClass.cs) is part of 3P.
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

using YamuiFramework.Controls.YamuiList;
using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures.AutoCompletionFeature {
    /// <summary>
    /// This class is not a singleton (the constructor is public) but we use a static instance
    /// performances and handiness
    /// </summary>
    internal class CompletionFilterClass {
        #region private

        private int _currentLineNumber = -2;
        private ParsedScopeItem _currentScope;

        #endregion

        #region static instance (not a singleton)

        private static CompletionFilterClass _instance;

        public static CompletionFilterClass Instance {
            get { return _instance ?? (_instance = new CompletionFilterClass()); }
            set { _instance = value; }
        }

        #endregion

        #region public

        /// <summary>
        /// Returns true if the conditions have changed
        /// </summary>
        public bool UpdateConditions(int currentLineNumber, bool dontCheckLine) {
            if (currentLineNumber != _currentLineNumber) {
                _currentLineNumber = currentLineNumber;
                _currentScope = ParserHandler.GetScopeOfLine(currentLineNumber);
                if (dontCheckLine || !Config.Instance.AutoCompleteOnlyShowDefinedVar)
                    _currentLineNumber = -1;
                return true;
            }

            return false;
        }

        /// <summary>
        /// if true, the item isn't filtered
        /// </summary>
        public bool FilterPredicate(ListItem o) {
            var compData = o as CompletionItem;
            if (compData == null)
                return false;

            // if the item isn't a parsed item, it is available no matter where we are in the code
            var parsedItem = compData.ParsedBaseItem as ParsedItem;
            if (!compData.FromParser || parsedItem == null)
                return true;

            return parsedItem.SurvivesFilter(_currentLineNumber, _currentScope);
        }

        #endregion
    }
}