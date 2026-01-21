using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTMapEditor.Objects
{
    internal class RectWall
    {
        private static readonly Color COLOUR = Color.DarkGray;
        public Rectangle mRectangle { get; private set; }
        private Rectangle mOutlineRectangle;
        private Texture2D mTexture;
        
        public RectWall(Texture2D pTexture, Rectangle pRectangle)
        {
            mRectangle = pRectangle;
            mOutlineRectangle = new Rectangle(pRectangle.X - 2, pRectangle.Y - 2, pRectangle.Width + 4, pRectangle.Height + 4);
            mTexture = pTexture;
        }

        public void Draw(SpriteBatch pSpriteBatch)
        {
            pSpriteBatch.Draw(mTexture, mRectangle, COLOUR);
        }

        public void DrawOutline(SpriteBatch pSpriteBatch)
        {
            pSpriteBatch.Draw(mTexture, mOutlineRectangle, Color.Black);
        }
    }
}
