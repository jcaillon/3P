#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (CompletionItem.cs) is part of 3P.
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
using _3PA.MainFeatures.FilteredLists;
using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures.AutoCompletion {
    /// <summary>
    /// class used in the auto completion feature
    /// </summary>
    internal class CompletionItem : FilteredItem {

        /// <summary>
        /// Type of completion
        /// </summary>
        public CompletionType Type { get; set; }

        /// <summary>
        /// Allows to display small "tag" picture on the left of a completionData in the autocomp list,
        /// see the ParseFlag enumeration for all the possibilities
        /// It works as a Flag, call HasFlag() method to if a certain flag is set and use
        /// Flag = Flag | ParseFlag.Reserved to set a flag!
        /// </summary>
        public ParseFlag Flag { get; set; }

        /// <summary>
        /// Used for sorting the autocompletion list, the higher the ranking, the higher in the list
        /// the item is
        /// </summary>
        public int Ranking { get; set; }

        /// <summary>
        /// A free to use string, can contain :
        /// - keyword = type of keyword
        /// - table = name of the owner database
        /// - field = type
        /// </summary>
        public string SubString { get; set; }

        /// <summary>
        /// Indicates whether or not this completionData is created by the parser Visitor
        /// </summary>
        public bool FromParser { get; set; }

        /// <summary>
        /// When the FromParser is true, contains the ParsedItem extracted by the parser
        /// </summary>
        public ParsedItem ParsedItem { get; set; }

        /// <summary>
        /// This field is only used when Type == CompletionType.Keyword, it contains the keyword type...
        /// </summary>
        public KeywordType KeywordType { get; set; }

        /// <summary>
        /// Use this method to do an action for each flag of the item...
        /// </summary>
        /// <param name="toApplyOnFlag"></param>
        public void DoForEachFlag(Action<string, ParseFlag> toApplyOnFlag) {
            foreach (var name in Enum.GetNames(typeof(ParseFlag))) {
                ParseFlag flag = (ParseFlag)Enum.Parse(typeof(ParseFlag), name);
                if (flag == 0 || !Flag.HasFlag(flag)) continue;
                toApplyOnFlag(name, flag);
            }
        }

    }

    internal enum CompletionType {
        FieldPk = 0,
        Field = 1,
        Snippet = 2,
        TempTable = 3,
        VariablePrimitive = 4,
        VariableComplex = 5,
        Table = 6,
        Function = 7,
        Procedure = 8,
        Preprocessed = 9,
        Keyword = 10,
        Database = 11,
        Widget = 12,
        KeywordObject = 13,
        Label = 14,
        Sequence = 15
    }
}
