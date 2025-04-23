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
using System.Windows.Media.Media3D;

namespace GS.Shared
{
    public static class Numbers
    {
        public static double GetRandomNumber(double minimum, double maximum)
        {
            var random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        public static string LongToHex(long number)
        {
            // 31 -> 0F0000
            var a = ((int)number & 0xFF).ToString("X").ToUpper();
            var b = (((int)number & 0xFF00) / 256).ToString("X").ToUpper();
            var c = (((int)number & 0xFF0000) / 256 / 256).ToString("X").ToUpper();

            if (a.Length == 1)
                a = "0" + a;
            if (b.Length == 1)
                b = "0" + b;
            if (c.Length == 1)
                c = "0" + c;
            return a + b + c;
        }

        public static IEnumerable<double> InclusiveRange(double start, double end, double step = .1, int round = 1)
        {
            while (start <= end)
            {
                yield return start;
                start += step;
                start = Math.Round(start, round);
            }
        }

        public static IEnumerable<int> InclusiveIntRange(int start, int end, int step = 1)
        {
            while (start <= end)
            {
                yield return start;
                start += step;
            }
        }

        public static double TruncateD(double value, int digits)
        {
            var factor = Math.Pow(10.0, digits);
            return Math.Truncate(value * factor) / factor;
        }

        public static bool IsBetween<T>(this T value, T min, T max) where T : IComparable<T>
        {
            return (min.CompareTo(value) <= 0) && (value.CompareTo(max) <= 0);
        }

        public static bool IsEqualTo(this double a, double b, double margin)
        {
            return Math.Abs(a - b) < margin;
        }

        public static bool IsEqualTo(this double a, double b)
        {
            return Math.Abs(a - b) < double.Epsilon;
        }

        public static bool IsNaNVector3D(Vector3D vec)
        {
            return double.IsNaN(vec.X) || double.IsNaN(vec.Y) || double.IsNaN(vec.Z);
        }

        public static bool Is0Vector3D(Vector3D vec)
        {
            return vec.X == 0 || vec.Y == 0 || vec.Z == 0;
        }

        public static bool IsNaNPoint3D(Point3D pnt)
        {
            return double.IsNaN(pnt.X) || double.IsNaN(pnt.Y) || double.IsNaN(pnt.Z);
        }

        public static bool Is0Point3D(Point3D pnt)
        {
            return Math.Abs(pnt.X) < 0.00001 || Math.Abs(pnt.Y) < 0.00001 || Math.Abs(pnt.Z) < 0.00001;
        }
    }
}
