﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Utilities
{
    /// <summary>
    /// Class help read/write program INI file
    /// </summary>
    public class IniHelper
    {
        [DllImport("KERNEL32.DLL", EntryPoint = "GetPrivateProfileStringW",
            SetLastError = true,
            CharSet = CharSet.Unicode, ExactSpelling = true,
            CallingConvention = CallingConvention.StdCall)]
        private static extern int GetPrivateProfileString(
            string lpAppName,
            string lpKeyName,
            string lpDefault,
            string lpReturnString,
            int nSize,
            string lpFilename);

        [DllImport("KERNEL32.DLL", EntryPoint = "WritePrivateProfileStringW",
            SetLastError = true,
            CharSet = CharSet.Unicode, ExactSpelling = true,
            CallingConvention = CallingConvention.StdCall)]
        private static extern int WritePrivateProfileString(
            string lpAppName,
            string lpKeyName,
            string lpString,
            string lpFilename);

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <param name="iniFile">The ini file.</param>
        /// <param name="category">The category.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static string GetIniFileString(string iniFile, string section, string key, string defaultValue)
        {
            string returnString = new string(' ', 1024);
            GetPrivateProfileString(section, key, defaultValue, returnString, 1024, iniFile);
            return returnString.Split('\0')[0];
        }

        /// <summary>
        /// Gets the keys.
        /// </summary>
        /// <param name="iniFile">The ini file.</param>
        /// <param name="category">The category.</param>
        public static List<string> GetKeys(string iniFile, string section)
        {
            string returnString = new string(' ', 32768);
            GetPrivateProfileString(section, null, null, returnString, 32768, iniFile);
            List<string> result = new List<string>(returnString.Split('\0'));
            result.RemoveRange(result.Count - 2, 2);
            return result;
        }

        /// <summary>
        /// Gets the categories.
        /// </summary>
        /// <param name="iniFile">The ini file.</param>
        /// <returns></returns>
        public static List<string> GetSections(string iniFile)
        {
            string returnString = new string(' ', 65536);
            GetPrivateProfileString(null, null, null, returnString, 65536, iniFile);
            List<string> result = new List<string>(returnString.Split('\0'));
            result.RemoveRange(result.Count - 2, 2);
            return result;
        }

        public static void WriteKey(string iniFile, string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, iniFile);
        }
    }
}
