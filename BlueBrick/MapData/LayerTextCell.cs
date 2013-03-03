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
	public partial class LayerText : Layer
	{
		[Serializable]
		public class TextCell : LayerItem
		{
			private StringFormat mTextStringFormat = new StringFormat();
			private Font mTextFont = Properties.Settings.Default.DefaultTextFont;
			private SolidBrush mTextBrush = new SolidBrush(Properties.Settings.Default.DefaultTextColor);
			private string mText = "";
			private Bitmap mImage = new Bitmap(1, 1);	// image representing the text to draw in the correct orientation

			#region get/set
			public string Text
			{
				get { return mText; }
				set { mText = value; updateBitmap(); }
			}

			public override float Orientation
			{
				set { mOrientation = value; updateBitmap(); }
			}

			public Color FontColor
			{
				get { return mTextBrush.Color; }
				set { mTextBrush.Color = value; updateBitmap(); }
			}

			public StringAlignment TextAlignment
			{
				get { return mTextStringFormat.Alignment; }
				set { mTextStringFormat.Alignment = value; updateBitmap(); }
			}

			public Font Font
			{
				get { return mTextFont; }
				set { mTextFont = value; updateBitmap(); }
			}

			public FontStyle FontStyle
			{
				get { return mTextFont.Style; }
				set { mTextFont = new Font(mTextFont, value); updateBitmap(); }
			}

			public Bitmap Image
			{
				get { return mImage; }
			}
			#endregion

			#region constructor/copy
			/// <summary>
			/// The paramererless constructor is used for serialization, it should not be used by the program
			/// </summary>
			public TextCell()
			{
				mTextStringFormat.LineAlignment = StringAlignment.Center;
			}

			public TextCell(string text, Font font, Color color, StringAlignment alignment)
			{
				init(text, font, color, alignment);
			}

			/// <summary>
			/// Clone this TextCell
			/// </summary>
			/// <returns>a new TextCell which is a conform copy of this</returns>
			public override LayerItem Clone()
			{
				TextCell result = new TextCell();
				result.mDisplayArea = this.mDisplayArea;
				result.mOrientation = this.mOrientation;
				// call the init after setting the orientation to compute the image in the right orientation
				// the init method will initialize mImage, mConnectionPoints and mSnapToGridOffsetFromTopLeftCorner
				result.init(this.Text, this.Font, this.FontColor, this.TextAlignment);
				// return the cloned value
				return result;
			}

			private void init(string text, Font font, Color color, StringAlignment alignment)
			{
				mTextStringFormat.Alignment = alignment;
				mTextStringFormat.LineAlignment = StringAlignment.Center;
				// set parameter directly to avoid calling several time the rebuild of picture
				mText = text;
				mTextBrush.Color = color;
				// then finally use an accessor in order to create the picture
				this.Font = font;
			}

			#endregion

			#region IXmlSerializable Members

			public override void ReadXml(System.Xml.XmlReader reader)
			{
				base.ReadXml(reader);
				// avoid using the accessor to reduce the number of call of updateBitmap
				mText = reader.ReadElementContentAsString();
				mOrientation = reader.ReadElementContentAsFloat();
				mTextBrush.Color = XmlReadWrite.readColor(reader);
				mTextFont = XmlReadWrite.readFont(reader);
				// for the last use the accessor to recreate the bitmap
				string alignment = reader.ReadElementContentAsString();
				if (alignment.Equals("Near"))
					TextAlignment = StringAlignment.Near;
				else if (alignment.Equals("Far"))
					TextAlignment = StringAlignment.Far;
				else
					TextAlignment = StringAlignment.Center;
				// read the end element of the brick
				reader.ReadEndElement();
			}

			public override void WriteXml(System.Xml.XmlWriter writer)
			{
				writer.WriteStartElement("TextCell");
				base.WriteXml(writer);
				writer.WriteElementString("Text", mText);
				writer.WriteElementString("Orientation", mOrientation.ToString(System.Globalization.CultureInfo.InvariantCulture));
				XmlReadWrite.writeColor(writer, "FontColor", FontColor);
				XmlReadWrite.writeFont(writer, "Font", mTextFont);
				writer.WriteElementString("TextAlignment", TextAlignment.ToString());
				writer.WriteEndElement(); // end of TextCell
			}

			#endregion

			#region method
			private void updateBitmap()
			{
				// create a bitmap if the text is not empty
				if (mText != "")
				{
					// create a font to mesure the text
					Font textFont = new Font(mTextFont.FontFamily, mTextFont.Size, mTextFont.Style);

					Graphics graphics = Graphics.FromImage(mImage);
					SizeF textFontSize = graphics.MeasureString(mText, textFont);
					float halfWidth = textFontSize.Width * 0.5f;
					float halfHeight = textFontSize.Height * 0.5f;

					Matrix rotation = new Matrix();
					rotation.Rotate(mOrientation);
					// compute the rotated corners
					PointF[] corners = new PointF[] { new PointF(-halfWidth, -halfHeight), new PointF(-halfWidth, halfHeight), new PointF(halfWidth, halfHeight), new PointF(halfWidth, -halfHeight) };
					rotation.TransformVectors(corners);

					PointF min = corners[0];
					PointF max = corners[0];
					for (int i = 1; i < 4; ++i)
					{
						if (corners[i].X < min.X)
							min.X = corners[i].X;
						if (corners[i].Y < min.Y)
							min.Y = corners[i].Y;
						if (corners[i].X > max.X)
							max.X = corners[i].X;
						if (corners[i].Y > max.Y)
							max.Y = corners[i].Y;
					}
					// adjust the display area and selection area
					mDisplayArea.Width = Math.Abs(max.X - min.X);
					mDisplayArea.Height = Math.Abs(max.Y - min.Y);

					// adjust the selection area (after adjusting the display area such as the center properties is correct)
					Matrix translation = new Matrix();
					translation.Translate(Center.X, Center.Y);
					translation.TransformPoints(corners);

					// then create the new selection area
					mSelectionArea = new Tools.Polygon(corners);

					// now create a scaled font from the current one, to avoid aliasing
					const float FONT_SCALE = 4.0f;
					Font scaledTextFont = new Font(mTextFont.FontFamily, mTextFont.Size * FONT_SCALE, mTextFont.Style);
					mImage = new Bitmap(mImage, new Size((int)(mDisplayArea.Width * FONT_SCALE), (int)(mDisplayArea.Height * FONT_SCALE)));

					// compute the position where to draw according to the alignment (if centered == 0)
					float posx = 0;
					if (this.TextAlignment == StringAlignment.Far)
						posx = halfWidth;
					else if (this.TextAlignment == StringAlignment.Near)
						posx = -halfWidth;

					graphics = Graphics.FromImage(mImage);
					rotation.Translate(mImage.Width / 2, mImage.Height / 2, MatrixOrder.Append);
					graphics.Transform = rotation;
					graphics.Clear(Color.Transparent);
					graphics.SmoothingMode = SmoothingMode.HighQuality;
					graphics.DrawString(mText, scaledTextFont, mTextBrush, posx * FONT_SCALE, 0, mTextStringFormat);
					graphics.Flush();
				}
			}
			#endregion
		}
	}
}
