#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (IniFile.cs) is part of 3P.
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
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace _3PA.Lib
{
    public class IniFile
    {
        [DllImport("kernel32")]
        static extern int GetPrivateProfileString(string section, string key, string defaultValue, StringBuilder retval, int size, string file);

        [DllImport("kernel32")]
        static extern long WritePrivateProfileString(string section, string key, string value, string file);

        protected string File;

        public IniFile()
        {
        }

        public IniFile(string file)
        {
            File = file;
        }

        public void SetValue<T>(string section, string key, T value)
        {
            try
            {
                WritePrivateProfileString(section, key, value.ToString(), File);
                MessageBox.Show(section + " > " + key + " = " + value.ToString());
            }
            catch { }
        }

        public T GetValue<T>(string section, string key, T defaultValue, int size = 255)
        {
            try
            {
                var retval = new StringBuilder(size);
                GetPrivateProfileString(section, key, defaultValue.ToString(), retval, size, File);
                MessageBox.Show(section + " > " + key + " = " + retval.ToString());
                return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFrom(retval.ToString());
            }
            catch
            {
                return defaultValue;
            }
        }
    }

}
