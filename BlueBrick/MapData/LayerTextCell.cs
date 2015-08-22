﻿// BlueBrick, a LEGO(c) layout editor.
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
            public const float ANTI_ALIASING_FONT_SCALE = 4.0f;

			private StringFormat mTextStringFormat = new StringFormat();
			private Font mTextFont = Properties.Settings.Default.DefaultTextFont;
			private SolidBrush mTextBrush = new SolidBrush(Properties.Settings.Default.DefaultTextColor);
			private string mText = "";
			private Bitmap mImage = new Bitmap(1, 1);	// image representing the text to draw in the correct orientation

			#region get/set
			public string Text
			{
				get { return mText; }
				set { mText = value; updateBitmap(true); }
			}

			public override float Orientation
			{
				set { mOrientation = value; updateBitmap(false); }
			}

			public Color FontColor
			{
				get { return mTextBrush.Color; }
				set { mTextBrush.Color = value; updateBitmap(true); }
			}

			public StringAlignment TextAlignment
			{
				get { return mTextStringFormat.Alignment; }
				set { mTextStringFormat.Alignment = value; updateBitmap(true); }
			}

			public Font Font
			{
				get { return mTextFont; }
				set { mTextFont = value; updateBitmap(true); }
			}

			public FontStyle FontStyle
			{
				get { return mTextFont.Style; }
				set { mTextFont = new Font(mTextFont, value); updateBitmap(true); }
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

			/// <summary>
			/// Copy constructor
			/// </summary>
			/// <param name="model">the model of the copy</param>
			public TextCell(TextCell model)
				: base(model)
			{
				// call the init after setting the orientation (in the base copy constructor)
				// to compute the image in the right orientation
				// the init method will initialize mImage because setting the Font will call an update of the image
				init(model.Text, model.Font, model.FontColor, model.TextAlignment);
			}

			/// <summary>
			/// Construct a text cell with the specified parameters
			/// </summary>
			/// <param name="text">the text to display</param>
			/// <param name="font">the font to used for displaying the text</param>
			/// <param name="color">the color of the text</param>
			/// <param name="alignment">the alignment of the text</param>
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
				// call the copy constructor
				return new TextCell(this);
			}

			/// <summary>
			/// Fonction used to factorise code in 2 different constructor
			/// </summary>
			/// <param name="text">the text to display</param>
			/// <param name="font">the font to used for displaying the text</param>
			/// <param name="color">the color of the text</param>
			/// <param name="alignment">the alignment of the text</param>
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
                // for some strange reason, the new lines are read "/n" no matter the environnement,
                // so reset it as the environnement wants it, such as the edit text form works correctly
				mText = reader.ReadElementContentAsString().Replace("\n", Environment.NewLine);
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
			private void updateBitmap(bool redrawImage)
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

                    if (redrawImage)
                    {
                        // now create a scaled font from the current one, to avoid aliasing
                        Font scaledTextFont = new Font(mTextFont.FontFamily, mTextFont.Size * ANTI_ALIASING_FONT_SCALE, mTextFont.Style);
                        mImage = new Bitmap(mImage, new Size((int)(textFontSize.Width * ANTI_ALIASING_FONT_SCALE), (int)(textFontSize.Height * ANTI_ALIASING_FONT_SCALE)));

                        // compute the position where to draw according to the alignment (if centered == 0)
                        float posx = 0;
                        if (this.TextAlignment == StringAlignment.Far)
                            posx = halfWidth;
                        else if (this.TextAlignment == StringAlignment.Near)
                            posx = -halfWidth;

                        graphics = Graphics.FromImage(mImage);
                        rotation = new Matrix();
                        rotation.Translate(mImage.Width / 2, mImage.Height / 2, MatrixOrder.Append);
                        graphics.Transform = rotation;
                        graphics.Clear(Color.Transparent);
                        graphics.SmoothingMode = SmoothingMode.HighQuality;
                        graphics.DrawString(mText, scaledTextFont, mTextBrush, posx * ANTI_ALIASING_FONT_SCALE, 0, mTextStringFormat);
                        graphics.Flush();
                    }
				}
			}
			#endregion
		}
	}
}
