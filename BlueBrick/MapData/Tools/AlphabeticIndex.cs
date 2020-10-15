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

namespace BlueBrick.MapData.Tools
{
	class AlphabeticIndex
	{
		/// <summary>
		/// This function return a number converted more or less into the alphabetic base of 26 character, but not quite so.
		/// Normally in a 26 character base, the A is the 0 of the base, and B is 1, so that the numbers should be:
		/// A, B, C, .... Z, BA, BB, BC, .... BZ, CA, ... BAA, BAB, BAC...
		/// This function will instead return the more human friendly comprehension serie of numbers:
		/// A, B, C, .... Z, AA, AB, AC, .... AZ, BA, ... AAA, AAB, AAC...
		/// This serie is equivalent to the column name of an Excel Calc Sheet.
		/// This function return an empty string if the specified number is negative or equals 0.
		/// </summary>
		/// <param name="num">The number you want to convert into a pseudo alphabetic base</param>
		/// <returns>An empty string if the number is less or equals to 0, otherwise a string representing more or less its value in an alphabetic base</returns>
		public static string GetNumberInAlphabicIndexStyle(int num)
		{
			// the function doesn't support negative number and 0 that is in therory "A" doesn't exist alone by itself.
			if (num <= 0)
				return string.Empty;

			string result = "";

			// Compute a number in base 26 (26 letters in the alphabet), but with a twist because A is both 0 and 1
			const string numbase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			int rest = num;
			int modulo;
			do
			{
				modulo = (rest - 1) % 26;
				result = numbase[modulo] + result;
				if ((rest % 26) == 0)
				{
					rest /= 26;
					rest--;
				}
				else
				{
					rest /= 26;
				}
			} while (rest > 0);

			return result;
		}

		public static string ConvertIndexToHumanFriendlyIndex(int num, bool convertIntoLetters)
		{
			if (num <= 0)
				return string.Empty;
			else if (convertIntoLetters)
				return GetNumberInAlphabicIndexStyle(num);
			else
				return num.ToString();
		}
	}
}
