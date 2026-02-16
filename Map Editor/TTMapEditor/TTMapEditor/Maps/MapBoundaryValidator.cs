using Microsoft.Xna.Framework;
using System;
using TTMapEditor.Objects;

namespace TTMapEditor.Maps
{
    public class MapBoundaryValidator
    {
        private readonly Rectangle mPlayArea;

        public MapBoundaryValidator(Rectangle pPlayArea)
        {
            mPlayArea = pPlayArea;
        }

        public bool IsRectWithinPlayArea(Rectangle pRect)
        {
            return pRect.Left >= mPlayArea.Left
            && pRect.Top >= mPlayArea.Top
            && pRect.Right <= mPlayArea.Right
            && pRect.Bottom <= mPlayArea.Bottom;
        }

        public bool IsWallWithinPlayArea(RectWall pWall)
        {
            Vector2 center = new Vector2(pWall.mRectangle.Center.X, pWall.mRectangle.Center.Y);

            Vector2[] corners =
            {
                new Vector2(-pWall.mRectangle.Width / 2f, -pWall.mRectangle.Height / 2f),
                new Vector2(pWall.mRectangle.Width / 2f, -pWall.mRectangle.Height / 2f),
                new Vector2(pWall.mRectangle.Width / 2f, pWall.mRectangle.Height / 2f),
                new Vector2(-pWall.mRectangle.Width / 2f, pWall.mRectangle.Height / 2f)
            };

            float cos = MathF.Cos(pWall.mRotation);
            float sin = MathF.Sin(pWall.mRotation);

            for (int i = 0; i < corners.Length; i++)
            {
                float rotatedX = corners[i].X * cos - corners[i].Y * sin;
                float rotatedY = corners[i].X * sin + corners[i].Y * cos;

                Vector2 worldPos = new Vector2(center.X + rotatedX, center.Y + rotatedY);

                if (worldPos.X < mPlayArea.Left
                    || worldPos.X > mPlayArea.Right
                    || worldPos.Y < mPlayArea.Top
                    || worldPos.Y > mPlayArea.Bottom)
                {
                    return false;
                }
            }

            return true;
        }



    }
}
