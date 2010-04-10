using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using BlueBrick.MapData;

namespace BlueBrick.Actions.Bricks
{
	class RotateBrickOnPivotBrick : RotateBrick
	{
		private LayerBrick.Brick mPivotBrick = null;

		public RotateBrickOnPivotBrick(LayerBrick layer, List<Layer.LayerItem> bricks, float angle, LayerBrick.Brick pivotBrick)
			: base(layer, bricks, angle)
		{
			mPivotBrick = pivotBrick;
		}

		public override void redo()
		{
			// get the center position of the pivot before the rotation
			PointF pivotShift = mPivotBrick.Center;
			// rotate the bricks
			base.redo();
			// compute the movement of the pivot and shift all the bricks
			pivotShift.X -= mPivotBrick.Center.X;
			pivotShift.Y -= mPivotBrick.Center.Y;
			// shift all the bricks
			foreach (LayerBrick.Brick brick in mBricks)
				brick.Center = new PointF(brick.Center.X + pivotShift.X, brick.Center.Y + pivotShift.Y);
			// update the selection rectangle after moving all the parts
			mBrickLayer.updateBoundingSelectionRectangle();
		}

		public override void undo()
		{
			// get the center position of the pivot before the rotation
			PointF pivotShift = mPivotBrick.Center;
			// rotate the bricks
			base.undo();
			// compute the movement of the pivot and shift all the bricks
			pivotShift.X -= mPivotBrick.Center.X;
			pivotShift.Y -= mPivotBrick.Center.Y;
			// shift all the bricks
			foreach (LayerBrick.Brick brick in mBricks)
				brick.Center = new PointF(brick.Center.X + pivotShift.X, brick.Center.Y + pivotShift.Y);
			// update the selection rectangle after moving all the parts
			mBrickLayer.updateBoundingSelectionRectangle();
		}
	}
}
