// BlueBrick, a LEGO(c) layout editor.
// Copyright (C) 2008 Alban NANTY
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// see http://www.fsf.org/licensing/licenses/gpl.html
// and http://www.gnu.org/licenses/
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace BlueBrick.SaveLoad
{
    class LDrawReadWrite
    {
        private static char[] mSeparator = { '|' };

        #region commands
        public static readonly string COMMON_COMMAND_GROUP = "0 GROUP ";
        public static readonly string MLCAD_COMMAND_HIDE = "0 MLCAD HIDE ";
        public static readonly string MLCAD_COMMAND_BTG = "0 MLCAD BTG ";
        // BB Commands all starts this way: "0 !BLUEBRICK <command> <version> "
        public static readonly string BB_COMMAND_HEADER = "0 !BLUEBRICK ";
        public static readonly string BB_COMMAND_RULER = BB_COMMAND_HEADER + "RULER 1 ";
        #endregion

        #region simple type
        public static float readFloat(string line)
        {
            return float.Parse(line, System.Globalization.CultureInfo.InvariantCulture);
        }

        public static void writeFloat(ref string line, float number)
        {
            line += number.ToString(System.Globalization.CultureInfo.InvariantCulture) + " ";
        }

        public static int readInteger(string line)
        {
            return int.Parse(line);
        }

        public static void writeInteger(ref string line, int number)
        {
            line += number.ToString(System.Globalization.CultureInfo.InvariantCulture) + " ";
        }

        public static bool readBoolean(string line)
        {
            return line.Equals("true");
        }

        public static float[] readFloatArray(string line)
        {
            List<float> result = new List<float>();
            string[] floatArray = line.Split(mSeparator, StringSplitOptions.RemoveEmptyEntries);
            foreach (string number in floatArray)
                result.Add(float.Parse(number, System.Globalization.CultureInfo.InvariantCulture));
            return result.ToArray();
        }

        public static void writeFloatArray(ref string line, float[] array)
        {
            foreach (float number in array)
                line += number.ToString(System.Globalization.CultureInfo.InvariantCulture) + "|";
            // replace the last pipe by a space separator
            if (array.Length > 0)
                line = line.Remove(line.Length - 1) + ' ';
        }

        public static void writeBoolean(ref string line, bool flag)
        {
            line += flag.ToString(System.Globalization.CultureInfo.InvariantCulture).ToLower() + " ";
        }

        public static int readItemId(string line)
        {
            return int.Parse(line);
        }

        public static void writeItemId(ref string line, object obj)
        {
            if (obj != null)
                line += obj.GetHashCode().ToString(System.Globalization.CultureInfo.InvariantCulture) + " ";
            else
                line += "0 ";
        }
        #endregion
        #region color
        public static Color readColor(string line)
        {
            if (line.StartsWith("0x"))
                return Color.FromArgb(int.Parse(line, System.Globalization.NumberStyles.HexNumber));
            else
                return Color.FromName(line);
        }

        public static void writeColor(ref string line, Color color)
        {
            line += "\"" + color.Name + "\" ";
        }
        #endregion
        #region font
        public static Font readFont(string line)
        {
            // split in 3 parts
            string[] fontDefinition = line.Split(mSeparator, StringSplitOptions.RemoveEmptyEntries);
            // size
            float size = 8.0f;
            if (fontDefinition.Length > 0)
                size = float.Parse(fontDefinition[1], System.Globalization.CultureInfo.InvariantCulture);
            // the style (italic, bold, etc...)
            FontStyle style = FontStyle.Regular;
            if (fontDefinition.Length > 1)
                style = (FontStyle)Enum.Parse(typeof(FontStyle), fontDefinition[2]);
            // end of Font
            return new Font(fontDefinition[0], size, style);
        }

        public static void writeFont(ref string line, Font font)
        {
            line += "\"" + font.FontFamily.Name + "|";
            line += font.Size.ToString(System.Globalization.CultureInfo.InvariantCulture) + "|";
            line += font.Style.ToString() + "\" ";
        }
        #endregion
        #region point
        public static Point readPoint(string line)
        {
            string[] xy = line.Split(mSeparator, StringSplitOptions.RemoveEmptyEntries);
            int x = int.Parse(xy[0]);
            int y = int.Parse(xy[1]);
            return new Point(x, y);
        }

        public static void writePoint(ref string line, Point point)
        {
            line += point.X.ToString() + "|";
            line += point.Y.ToString() + " ";
        }
        #endregion
        #region point F
        public static PointF readPointF(string line)
        {
            string[] xy = line.Split(mSeparator, StringSplitOptions.RemoveEmptyEntries);
            float x = float.Parse(xy[0], System.Globalization.CultureInfo.InvariantCulture);
            float y = float.Parse(xy[1], System.Globalization.CultureInfo.InvariantCulture);
            return new PointF(x, y);
        }

        public static void writePointF(ref string line, PointF point)
        {
            line += point.X.ToString(System.Globalization.CultureInfo.InvariantCulture) + "|";
            line += point.Y.ToString(System.Globalization.CultureInfo.InvariantCulture) + " ";
        }
        #endregion

    }
}
