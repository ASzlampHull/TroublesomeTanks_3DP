using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TTMapEditor.Objects
{
    internal class Tank : SceneObject
    {
        private static readonly Color COLOUR = Color.Blue;

        // small pixel texture used for drawing the front indicator
        private static readonly Texture2D sPixel = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("white_pixel");

        // Rotation in radians; public getter so other code can read it if needed.
        public float Rotation { get; set; }

        public Tank(Texture2D pTexture, Rectangle pRectangle) : base(pTexture, pRectangle)
        {
            Rotation = 0f;
        }

        public void Rotate(float delta) => Rotation += delta;

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            // tint when selected
            Color tint = GetIsSelected() ? Color.Yellow : COLOUR;

            // draw centered at rectangle center with rotation and scale to rectangle size
            Vector2 position = new Vector2(mRectangle.Center.X, mRectangle.Center.Y);
            Vector2 origin = new Vector2(mTexture.Width / 2f, mTexture.Height / 2f);
            Vector2 scale = new Vector2(mRectangle.Width / (float)mTexture.Width, mRectangle.Height / (float)mTexture.Height);

            pSpriteBatch.Draw(mTexture, position, null, tint, Rotation, origin, scale, SpriteEffects.None, 0f);

            // Draw a front indicator: a short line in front of the tank that rotates with it.
            // Compute direction vector from rotation, length and thickness of the indicator.
            float length = Math.Max(mRectangle.Width, mRectangle.Height) * 0.6f;
            float thickness = Math.Max(2f, Math.Min(mRectangle.Width, mRectangle.Height) * 0.12f);

            Vector2 dir = new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation));
            // position the indicator so it's centered along its length just in front of the tank center
            Vector2 indicatorCenter = position + dir * (length / 2f + Math.Min(mRectangle.Width, mRectangle.Height) * 0.1f);

            // origin at pixel center (pixel is 1x1), scale to desired length/thickness
            Vector2 pixelOrigin = new Vector2(0.5f, 0.5f);
            Vector2 pixelScale = new Vector2(length, thickness);

            // draw the line
            pSpriteBatch.Draw(sPixel, indicatorCenter, null, Color.Red, Rotation, pixelOrigin, pixelScale, SpriteEffects.None, 0f);

            // draw a small tip (a short square) at the front-most point
            Vector2 tipPos = position + dir * (length + Math.Min(mRectangle.Width, mRectangle.Height) * 0.1f + thickness / 2f);
            Vector2 tipScale = new Vector2(thickness * 1.2f, thickness * 1.2f);
            pSpriteBatch.Draw(sPixel, tipPos, null, Color.OrangeRed, Rotation, pixelOrigin, tipScale, SpriteEffects.None, 0f);
        }

        // Outline remains unrotated (keeps existing behaviour)
        public override void DrawOutline(SpriteBatch pSpriteBatch) => pSpriteBatch.Draw(mTexture, mOutlineRectangle, Color.Black);
    }
}
