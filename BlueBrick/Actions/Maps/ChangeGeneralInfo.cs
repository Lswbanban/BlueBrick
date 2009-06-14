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
using BlueBrick.MapData;

namespace BlueBrick.Actions.Maps
{
	class ChangeGeneralInfo : Action
	{
		private class GeneralMapInfo
		{
			public string mAuthor = null;
			public string mLUG = null;
			public string mShow = null;
			public DateTime mDate;
			public string mComment = null;
		}

		private GeneralMapInfo oldInfo = new GeneralMapInfo();
		private GeneralMapInfo newInfo = new GeneralMapInfo();

		public ChangeGeneralInfo(string author, string lug, string show, DateTime date, string comment)
		{
			// save old data
			oldInfo.mAuthor = Map.Instance.Author.Clone() as string;
			oldInfo.mLUG = Map.Instance.LUG.Clone() as string;
			oldInfo.mShow = Map.Instance.Show.Clone() as string;
			oldInfo.mDate = Map.Instance.Date;
			oldInfo.mComment = Map.Instance.Comment.Clone() as string;
			// save new data
			newInfo.mAuthor = author.Clone() as string;
			newInfo.mLUG = lug.Clone() as string;
			newInfo.mShow = show.Clone() as string;
			newInfo.mDate = date;
			newInfo.mComment = comment.Clone() as string;
		}

		public override string getName()
		{
			return BlueBrick.Properties.Resources.ActionChangeGeneralInfo;
		}

		public override void redo()
		{
			Map.Instance.Author = newInfo.mAuthor;
			Map.Instance.LUG = newInfo.mLUG;
			Map.Instance.Show = newInfo.mShow;
			Map.Instance.Date = newInfo.mDate;
			Map.Instance.Comment = newInfo.mComment;
		}

		public override void undo()
		{
			Map.Instance.Author = oldInfo.mAuthor;
			Map.Instance.LUG = oldInfo.mLUG;
			Map.Instance.Show = oldInfo.mShow;
			Map.Instance.Date = oldInfo.mDate;
			Map.Instance.Comment = oldInfo.mComment;
		}
	}
}
