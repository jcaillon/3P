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
