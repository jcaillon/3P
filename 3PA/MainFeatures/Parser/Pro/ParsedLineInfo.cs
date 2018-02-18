using System.Collections.Generic;
using System.Linq;
using _3PA.Lib;

namespace _3PA.MainFeatures.Parser.Pro {
    internal class ParsedLineInfo {
        
        public Stack<ParsedScope> BlockStack { get; set; }

        /// <summary>
        /// If, for the line that this object describes, we are not in the middle of a statement then
        /// this number is simply equals to the line this object describes
        /// </summary>
        public int StatementStartLine { get; set; }

        /// <summary>
        /// Would be the same as BlockStack.Peek() but only returns the topmost block of given type
        /// </summary>
        /// <returns></returns>
        public T GetTopMostBlock<T>() where T : ParsedScope {
            T output = BlockStack.Peek() as T;
            var i = 0;
            while (output == null) {
                i++;
                if (i >= BlockStack.Count)
                    break;
                output = BlockStack.ElementAt(i) as T;
            }
            return output;
        }

        public ParsedLineInfo(Stack<ParsedScope> blockStack, int statementStartLine) {
            BlockStack = blockStack.CloneStack();
            StatementStartLine = statementStartLine;
        }
    }
}