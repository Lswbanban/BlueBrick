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
using System.Drawing.Drawing2D;

namespace BlueBrick.MapData
{
	class XmlReadWrite
	{
		#region simple type
		public static float readFloat(System.Xml.XmlReader reader)
		{
			return reader.ReadElementContentAsFloat();
		}

		public static void writeFloat(System.Xml.XmlWriter writer, string name, float number)
		{
			writer.WriteElementString(name, number.ToString(System.Globalization.CultureInfo.InvariantCulture));
		}

		public static int readInteger(System.Xml.XmlReader reader)
		{
			return reader.ReadElementContentAsInt();
		}

		public static void writeInteger(System.Xml.XmlWriter writer, string name, int number)
		{
			writer.WriteElementString(name, number.ToString(System.Globalization.CultureInfo.InvariantCulture));
		}

		public static bool readBoolean(System.Xml.XmlReader reader)
		{
			return reader.ReadElementContentAsBoolean();
		}

		public static void writeBoolean(System.Xml.XmlWriter writer, string name, bool flag)
		{
			writer.WriteElementString(name, flag.ToString().ToLower());
		}

		public static float[] readFloatArray(System.Xml.XmlReader reader)
		{
			List<float> result = new List<float>();
			reader.ReadToDescendant("value");
			while (reader.Name.Equals("value"))
				result.Add(reader.ReadElementContentAsFloat());
			reader.ReadEndElement();
			return result.ToArray();
		}

		public static void writeFloatArray(System.Xml.XmlWriter writer, string name, float[] array)
		{
			writer.WriteStartElement(name);
			foreach (float number in array)
				writer.WriteElementString("value", number.ToString(System.Globalization.CultureInfo.InvariantCulture));
			writer.WriteEndElement();
		}

		public static int readItemId(System.Xml.XmlReader reader)
		{
			return reader.ReadElementContentAsInt();
		}

		public static void writeItemId(System.Xml.XmlWriter writer, string name, object obj)
		{
			if (obj != null)
				writer.WriteElementString(name, obj.GetHashCode().ToString(System.Globalization.CultureInfo.InvariantCulture));
			else
				writer.WriteElementString(name, "0");
		}
		#endregion

		#region color
		public static Color readColor(System.Xml.XmlReader reader)
		{
			reader.ReadToDescendant("IsKnownColor");
			bool isKnownColor = reader.ReadElementContentAsBoolean();
			string colorName = reader.ReadElementContentAsString();
			reader.ReadEndElement();
			if (isKnownColor)
				return Color.FromName(colorName);
			else
				return Color.FromArgb(int.Parse(colorName, System.Globalization.NumberStyles.HexNumber));
		}

		public static void writeColor(System.Xml.XmlWriter writer, string name, Color color)
		{
			writer.WriteStartElement(name);
			writer.WriteElementString("IsKnownColor", color.IsKnownColor.ToString().ToLower());
			writer.WriteElementString("Name", color.Name);
			writer.WriteEndElement();
		}
		#endregion

		#region font
		public static Font readFont(System.Xml.XmlReader reader)
		{
			reader.ReadToDescendant("FontFamily");
			string fontFamily = reader.ReadElementContentAsString();
			float size = reader.ReadElementContentAsFloat();
			FontStyle style = FontStyle.Regular;
			// the style (italic, bold, etc...)
			if (Map.DataVersionOfTheFileLoaded >= 6)
				style = (FontStyle)Enum.Parse(typeof(FontStyle), reader.ReadElementContentAsString());
			// end of Font
			reader.ReadEndElement();
			return new Font(fontFamily, size, style);
		}

		public static void writeFont(System.Xml.XmlWriter writer, string name, Font font)
		{
			writer.WriteStartElement(name);
			writer.WriteElementString("FontFamily", font.FontFamily.Name);
			writer.WriteElementString("Size", font.Size.ToString(System.Globalization.CultureInfo.InvariantCulture));
			writer.WriteElementString("Style", font.Style.ToString());
			writer.WriteEndElement();
		}
		#endregion

		#region point
		public static Point readPoint(System.Xml.XmlReader reader)
		{
			reader.ReadToDescendant("X");
			int x = reader.ReadElementContentAsInt();
			int y = reader.ReadElementContentAsInt();
			reader.ReadEndElement();
			return new Point(x, y);
		}

		public static void writePoint(System.Xml.XmlWriter writer, string name, Point point)
		{
			writer.WriteStartElement(name);
			writer.WriteElementString("X", point.X.ToString());
			writer.WriteElementString("Y", point.Y.ToString());
			writer.WriteEndElement();
		}
		#endregion

		#region point F
		public static PointF readPointF(System.Xml.XmlReader reader)
		{
			reader.ReadToDescendant("X");
			float x = reader.ReadElementContentAsFloat();
			float y = reader.ReadElementContentAsFloat();
			reader.ReadEndElement();
			return new PointF(x, y);
		}

		public static void writePointF(System.Xml.XmlWriter writer, string name, PointF point)
		{
			writer.WriteStartElement(name);
			writer.WriteElementString("X", point.X.ToString(System.Globalization.CultureInfo.InvariantCulture));
			writer.WriteElementString("Y", point.Y.ToString(System.Globalization.CultureInfo.InvariantCulture));
			writer.WriteEndElement();
		}

		public static void writePointFLowerCase(System.Xml.XmlWriter writer, string name, PointF point)
		{
			writer.WriteStartElement(name);
			writer.WriteElementString("x", point.X.ToString(System.Globalization.CultureInfo.InvariantCulture));
			writer.WriteElementString("y", point.Y.ToString(System.Globalization.CultureInfo.InvariantCulture));
			writer.WriteEndElement();
		}
		#endregion

		#region rectangle F
		public static RectangleF readRectangleF(System.Xml.XmlReader reader)
		{
			reader.ReadToDescendant("X");
			float x = reader.ReadElementContentAsFloat();
			float y = reader.ReadElementContentAsFloat();
			float width = reader.ReadElementContentAsFloat();
			float height = reader.ReadElementContentAsFloat();
			reader.ReadEndElement();
			return (new RectangleF(x, y, width, height));
		}

		public static void writeRectangleF(System.Xml.XmlWriter writer, string name, RectangleF rect)
		{
			writer.WriteStartElement(name);
			writer.WriteElementString("X", rect.X.ToString(System.Globalization.CultureInfo.InvariantCulture));
			writer.WriteElementString("Y", rect.Y.ToString(System.Globalization.CultureInfo.InvariantCulture));
			writer.WriteElementString("Width", rect.Width.ToString(System.Globalization.CultureInfo.InvariantCulture));
			writer.WriteElementString("Height", rect.Height.ToString(System.Globalization.CultureInfo.InvariantCulture));
			writer.WriteEndElement();
		}
		#endregion
	}
}
