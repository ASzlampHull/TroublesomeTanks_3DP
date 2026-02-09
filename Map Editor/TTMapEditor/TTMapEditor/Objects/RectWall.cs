using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;

namespace TTMapEditor.Objects
{
    internal class RectWall : SceneObject
    {
        private static readonly Color COLOUR = DGS.Instance.GetColour("COLOUR_WALL");
        private static readonly SpriteFont mFont = TTMapEditor.Instance().GetContentManager().Load<SpriteFont>("TitleFont");
        bool mIsRotating = false;
        bool mIsScaling = true;

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
            if(GetIsSelected())
            {
                pSpriteBatch.DrawString(mFont, mIsRotating ? "Rotating" : "Scaling", new Vector2(mRectangle.X, mRectangle.Y - 20), Color.Black);
            }
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

        public void SwitchRotationScaling()
        {
            mIsRotating = !mIsRotating;
            mIsScaling = !mIsScaling;
        }

        public bool GetIsRotating() => mIsRotating;

        public bool GetIsScaling() => mIsScaling;

        public override bool IsPointWithin(Vector2 point)
        {
            // center of the rectangle
            Vector2 center = new Vector2(mRectangle.Center.X, mRectangle.Center.Y);

            // translate to origin
            Vector2 local = point - center;

            // rotate by -mRotation
            float cos = (float)Math.Cos(-mRotation);
            float sin = (float)Math.Sin(-mRotation);
            Vector2 rotated = new Vector2(local.X * cos - local.Y * sin, local.X * sin + local.Y * cos);

            float halfW = mRectangle.Width / 2f;
            float halfH = mRectangle.Height / 2f;

            return rotated.X >= -halfW && rotated.X <= halfW && rotated.Y >= -halfH && rotated.Y <= halfH;
        }
    }
}
