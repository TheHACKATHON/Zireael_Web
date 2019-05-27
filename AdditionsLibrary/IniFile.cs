using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AdditionsLibrary
{
    public class IniFile
    {
        private readonly string _path;

        public IniFile(string iniPath)
        {
            _path = new FileInfo(iniPath).FullName;
        }

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string value, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string Default,
            StringBuilder retVal, int size, string filePath);

        //Читаем ini-файл и возвращаем значение указного ключа из заданной секции.
        public string ReadINI(string section, string key)
        {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(section, key, "", RetVal, 255, _path);
            return RetVal.ToString();
        }

        //Записываем в ini-файл. Запись происходит в выбранную секцию в выбранный ключ.
        public void Write(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, _path);
        }

        //Удаляем ключ из выбранной секции.
        public void DeleteKey(string key, string section = null)
        {
            Write(section, key, null);
        }

        //Удаляем выбранную секцию
        public void DeleteSection(string section = null)
        {
            Write(section, null, null);
        }

        //Проверяем, есть ли такой ключ, в этой секции
        public bool KeyExists(string key, string section = null)
        {
            return ReadINI(section, key).Length > 0;
        }
    }
}