#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (TortoiseWrapper.cs) is part of 3P.
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
namespace _3PA.Lib {
    public class TortoiseWrapper {
        #region Singleton

        private static TortoiseWrapper _gitInstance;

        public static TortoiseWrapper GitInstance {
            get {
                if (_gitInstance == null)
                    _gitInstance = new TortoiseWrapper(TortoiseType.Git);
                return _gitInstance;
            }
        }

        private static TortoiseWrapper _svnInstance;

        public static TortoiseWrapper SvnInstance {
            get {
                if (_svnInstance == null)
                    _svnInstance = new TortoiseWrapper(TortoiseType.Svn);
                return _svnInstance;
            }
        }

        #endregion

        #region fields

        private TortoiseType _type;

        private string _tortoiseProcPath;

        #endregion

        #region Life and death

        public TortoiseWrapper(TortoiseType type) {
            _type = type;
            var nodeName = @"SOFTWARE\Tortoise" + (_type == TortoiseType.Svn ? "SVN" : "Git");
            _tortoiseProcPath = "";
            //_mergeToolPath = "";
            //_tortoiseDirectoryPath = "";
        }

        #endregion

        #region returns true if tortoise was found

        /// <summary>
        /// Allows to know if commands are ready to be send to tortoise (= if tortoise exists on the computer)
        /// </summary>
        public bool IsReady {
            get { return !string.IsNullOrEmpty(_tortoiseProcPath); }
        }

        #endregion

        #region Tortoise type

        public enum TortoiseType {
            Git,
            Svn
        }

        #endregion
    }
}