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
using System.Windows.Forms;

namespace BlueBrick
{
	class MainSplitContainer : SplitContainer
	{
		public MainSplitContainer()
		{
			// set the width for a stupid bug, maybe done by .Net itself:
			// I want to set the minWidth of this.mainSplitContainer.panel2 to 180, but
			// if I do so, I have an exception saying that the splitDistance must be
			// between panel1.MinWidth and Width - panel2.MinWidth.
			// I agree, but for an strange reason the splitcontainer.Width == 150 even 
			// if in the form designer it is not
			this.Width = 800;
		}
	}
}
