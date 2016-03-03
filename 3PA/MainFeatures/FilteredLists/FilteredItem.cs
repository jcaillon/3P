#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (FilteredItem.cs) is part of 3P.
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
using System.Collections.Generic;
using System.Drawing;

namespace _3PA.MainFeatures.FilteredLists {
    internal class FilteredItem {

        /// <summary>
        /// The piece of text displayed in the completion list
        /// </summary>
        public string DisplayText { get; set; }

        #region Filter

        /// <summary>
        /// The dispertion level with which the lowerCaseFilterString matches the DisplayText
        /// </summary>
        public int FilterDispertionLevel { get; set; }

        /// <summary>
        /// True of the lowerCaseFilterString fully matches DisplayText
        /// </summary>
        public bool FilterFullyMatch { get; set; }

        /// <summary>
        /// The way lowerCaseFilterString matches DisplayText
        /// </summary>
        public List<CharacterRange> FilterMatchedRanges { get; set; }

        /// <summary>
        /// Call this method to compute the value of
        /// FilterDispertionLevel, FilterFullyMatch, FilterMatchedRanges
        /// </summary>
        /// <param name="lowerCaseFilterString"></param>
        public void FilterApply(string lowerCaseFilterString) {

            FilterMatchedRanges = new List<CharacterRange>();
            FilterFullyMatch = true;
            FilterDispertionLevel = 0;

            if (string.IsNullOrEmpty(lowerCaseFilterString) || string.IsNullOrEmpty(DisplayText))
                return;

            var lcText = DisplayText.ToLower();
            var textLenght = lcText.Length;
            var filterLenght = lowerCaseFilterString.Length;

            int pos = 0;
            int posFilter = 0;
            bool matching = false;
            int startMatch = 0;

            while (pos < textLenght) {
                // remember matching state at the beginning of the loop
                bool wasMatching = matching;
                // we match the current char of the filter
                if (lcText[pos] == lowerCaseFilterString[posFilter]) {
                    if (!matching) {
                        matching = true;
                        startMatch = pos;
                    }
                    posFilter++;
                    // we matched the entire filter
                    if (posFilter >= filterLenght) {
                        FilterMatchedRanges.Add(new CharacterRange(startMatch, pos - startMatch + 1));
                        break;
                    }
                } else {
                    matching = false;

                    // gap between match mean more penalty than finding the match later in the string
                    if (posFilter > 0) {
                        FilterDispertionLevel += 900;
                    } else {
                        FilterDispertionLevel += 30;
                    }
                }
                // we stopped matching, remember matching range
                if (!matching && wasMatching)
                    FilterMatchedRanges.Add(new CharacterRange(startMatch, pos - startMatch));
                pos++;
            }

            // put the exact matches first
            if (filterLenght != textLenght)
                FilterDispertionLevel += 1;

            // we reached the end of the input, if we were matching stuff, remember matching range
            if (pos >= textLenght && matching)
                FilterMatchedRanges.Add(new CharacterRange(startMatch, pos - 1 - startMatch));

            // we didn't match the entire filter
            if (posFilter < filterLenght)
                FilterFullyMatch = false;
        }

        #endregion
    }
}
