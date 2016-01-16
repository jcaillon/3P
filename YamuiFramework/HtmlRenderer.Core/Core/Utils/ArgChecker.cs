#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (ArgChecker.cs) is part of YamuiFramework.
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
using System.IO;

namespace YamuiFramework.HtmlRenderer.Core.Core.Utils
{
    /// <summary>
    /// Static class that contains argument-checking methods
    /// </summary>
    public static class ArgChecker
    {
        /// <summary>
        /// Validate given <see cref="condition"/> is true, otherwise throw exception.
        /// </summary>
        /// <typeparam name="TException">Exception type to throw.</typeparam>
        /// <param name="condition">Condition to assert.</param>
        /// <param name="message">Exception message in-case of assert failure.</param>
        public static void AssertIsTrue<TException>(bool condition, string message) where TException : Exception, new()
        {
            // Checks whether the condition is false
            if (!condition)
            {
                // Throwing exception
                throw (TException)Activator.CreateInstance(typeof(TException), message);
            }
        }

        /// <summary>
        /// Validate given argument isn't Null.
        /// </summary>
        /// <param name="arg">argument to validate</param>
        /// <param name="argName">Name of the argument checked</param>
        /// <exception cref="System.ArgumentNullException">if <paramref name="arg"/> is Null</exception>
        public static void AssertArgNotNull(object arg, string argName)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(argName);
            }
        }

        /// <summary>
        /// Validate given argument isn't <see cref="System.IntPtr.Zero"/>.
        /// </summary>
        /// <param name="arg">argument to validate</param>
        /// <param name="argName">Name of the argument checked</param>
        /// <exception cref="System.ArgumentNullException">if <paramref name="arg"/> is <see cref="System.IntPtr.Zero"/></exception>
        public static void AssertArgNotNull(IntPtr arg, string argName)
        {
            if (arg == IntPtr.Zero)
            {
                throw new ArgumentException("IntPtr argument cannot be Zero", argName);
            }
        }

        /// <summary>
        /// Validate given argument isn't Null or empty.
        /// </summary>
        /// <param name="arg">argument to validate</param>
        /// <param name="argName">Name of the argument checked</param>
        /// <exception cref="System.ArgumentNullException">if <paramref name="arg"/> is Null or empty</exception>
        public static void AssertArgNotNullOrEmpty(string arg, string argName)
        {
            if (string.IsNullOrEmpty(arg))
            {
                throw new ArgumentNullException(argName);
            }
        }

        /// <summary>
        /// Validate given argument isn't Null.
        /// </summary>
        /// <typeparam name="T">Type expected of <see cref="arg"/></typeparam>
        /// <param name="arg">argument to validate</param>
        /// <param name="argName">Name of the argument checked</param>
        /// <exception cref="System.ArgumentNullException">if <paramref name="arg"/> is Null</exception>
        /// <returns><see cref="arg"/> cast as <see cref="T"/></returns>
        public static T AssertArgOfType<T>(object arg, string argName)
        {
            AssertArgNotNull(arg, argName);

            if (arg is T)
            {
                return (T)arg;
            }
            throw new ArgumentException(string.Format("Given argument isn't of type '{0}'.", typeof(T).Name), argName);
        }

        /// <summary>
        /// Validate given argument isn't Null or empty AND argument value is the path of existing file.
        /// </summary>
        /// <param name="arg">argument to validate</param>
        /// <param name="argName">Name of the argument checked</param>
        /// <exception cref="System.ArgumentNullException">if <paramref name="arg"/> is Null or empty</exception>
        /// <exception cref="System.IO.FileNotFoundException">if <see cref="arg"/> file-path not exist</exception>
        public static void AssertFileExist(string arg, string argName)
        {
            AssertArgNotNullOrEmpty(arg, argName);

            if (false == File.Exists(arg))
            {
                throw new FileNotFoundException(string.Format("Given file in argument '{0}' not exist.", argName), arg);
            }
        }
    }
}