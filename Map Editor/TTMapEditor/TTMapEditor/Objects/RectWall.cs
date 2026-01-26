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

        public RectWall(Texture2D pTexture, Rectangle pRectangle) : base(pTexture, pRectangle)
        {
            mRotation = 0f;
        }

        
        public float mRotation { get; set; }

        public override void Draw(SpriteBatch pSpriteBatch)
        { 
            Color tint = GetIsSelected() ? Color.Yellow : COLOUR;
            Vector2 position = new Vector2(mRectangle.Center.X, mRectangle.Center.Y);
            Vector2 origin = new Vector2(mTexture.Width / 2f, mTexture.Height / 2f);
            Vector2 scale = new Vector2(mRectangle.Width / (float)mTexture.Width, mRectangle.Height / (float)mTexture.Height);

            pSpriteBatch.Draw(mTexture, position, null, tint, mRotation, origin, scale, SpriteEffects.None, 0f);
        }

        public override void DrawOutline(SpriteBatch pSpriteBatch)
        {

            Vector2 position = new Vector2(mOutlineRectangle.Center.X, mOutlineRectangle.Center.Y);
            Vector2 origin = new Vector2(mTexture.Width / 2f, mTexture.Height / 2f);
            Vector2 scale = new Vector2(mOutlineRectangle.Width / (float)mTexture.Width, mOutlineRectangle.Height / (float)mTexture.Height);

            pSpriteBatch.Draw(mTexture, position, null, Color.Black, mRotation, origin, scale, SpriteEffects.None, 0f);
        }

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

        public void Rotate(float pDelta)
        {
            mRotation += pDelta;
        }
    }
}
