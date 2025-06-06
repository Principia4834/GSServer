﻿/* Copyright(C) 2019-2025 Rob Morgan (robert.morgan.e@gmail.com)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GS.Shared
{
    public static class Strings
    {
        /// <summary>
        /// Get text between two characters, index
        /// </summary>
        /// <param name="strSource"></param>
        /// <param name="strStart"></param>
        /// <param name="strEnd"></param>
        /// <returns></returns>
        public static string GetTxtBetween(string strSource, string strStart, string strEnd)
        {
            if (!strSource.Contains(strStart) || !strSource.Contains(strEnd)) return "";
            var start = strSource.IndexOf(strStart, 0, StringComparison.Ordinal) + strStart.Length;
            var end = strSource.IndexOf(strEnd, start, StringComparison.Ordinal);
            return strSource.Substring(start, end - start);
        }

        /// <summary>
        /// Get text between two words, Regex
        /// </summary>
        /// <param name="strSource"></param>
        /// <param name="strStart"></param>
        /// <param name="strEnd"></param>
        /// <returns></returns>
        public static string GetTextBetween(string strSource, string strStart, string strEnd)
        {
            return
                Regex.Match(strSource, $@"{strStart}\s(?<words>[\w\s]+)\s{strEnd}",
                    RegexOptions.IgnoreCase).Groups["words"].Value;
        }

        /// <summary>
        /// Pulls a number from a string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int? GetNumberFromString(string str)
        {
            if (string.IsNullOrEmpty(str)) return null;
            var numbers = Regex.Split(str, @"\D+");
            foreach (var value in numbers)
            {
                if (string.IsNullOrEmpty(value)) continue;
                var ok = int.TryParse(value.Trim(), out var i);
                if (ok) { return i; }
            }
            return null;
        }

        /// <summary>
        /// Converts a Mount received hex string to type long
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static long StringToLong(string str)
        {
            long value = 0;
            for (var i = 1; i + 1 < str.Length; i += 2)
            {
                value += (long)(int.Parse(str.Substring(i, 2), NumberStyles.AllowHexSpecifier) * Math.Pow(16, i - 1));
            }
            return value;
        }

        /// <summary>
        /// Converts hex response to integer for new advanced command set
        /// </summary>
        /// <notes>
        /// The stepper motor driver IC runs in 256 micro-step, while the previous motor board runs in 64 micro-step.
        /// Since the CPR value is larger than a 24-bit integer, we have to cheat the host in the old command set.
        /// </notes>
        public static int String32ToInt(string response, bool parseFirst, int divFactor)
        {
            if (parseFirst && response.Length > 0)
            { response = response.Substring(1, response.Length - 1);}
            var parsed = int.Parse(response, NumberStyles.HexNumber);
            var a = parsed / divFactor;
            return a;
        }

        /// <summary>
        /// convert collection to ObservableCollection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static ObservableCollection<T> ToObservableCollection<T>(IEnumerable<T> enumerable)
        {
            var col = new ObservableCollection<T>();
            foreach (var cur in enumerable)
            {
                col.Add(cur);
            }
            return col;
        }

    }
}
