using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTMapEditor.Objects
{
    internal class RectWall : SceneObject
    {
        private static readonly Color COLOUR = Color.DarkGray;

        public RectWall(Texture2D pTexture, Rectangle pRectangle) : base(pTexture, pRectangle) { }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            // use selection state from base class
            pSpriteBatch.Draw(mTexture, mRectangle, GetIsSelected() ? Color.Red : COLOUR);
        }

        public override void DrawOutline(SpriteBatch pSpriteBatch) => base.DrawOutline(pSpriteBatch);

        public void SetWallRectangle(Rectangle pRectangle) => SetRectangle(pRectangle);

        public void ScaleHeight(float pScale)
        {
            float scaled = mRectangle.Height * pScale;
            int newHeight = (int)Math.Ceiling(scaled);
            newHeight = Math.Max(newHeight, 1);
            SetRectangle(new Rectangle(mRectangle.X, mRectangle.Y, mRectangle.Width, newHeight));
        }

        public void ScaleWidth(float pScale)
        {
            float scaled = mRectangle.Width * pScale;
            int newWidth = (int)Math.Ceiling(scaled);
            newWidth = Math.Max(newWidth, 1);
            SetRectangle(new Rectangle(mRectangle.X, mRectangle.Y, newWidth, mRectangle.Height));
        }
    }
}
