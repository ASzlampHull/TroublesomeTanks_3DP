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
    /// <summary>
    /// Represents a rectangular wall object in the map editor.
    /// 
    /// The wall is drawn using a texture scaled to fit a rectangle and can be
    /// rotated around its center. It supports switching between rotation and
    /// scaling modes, and provides hit-testing that respects the current rotation.
    /// </summary>
    public class RectWall : SceneObject
    {

        private static readonly Color COLOUR = DGS.Instance.GetColour("COLOUR_WALL");

        private static readonly SpriteFont mFont = TTMapEditor.Instance().GetContentManager().Load<SpriteFont>("TitleFont");

        private bool mIsRotating = false;

        private bool mIsScaling = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="RectWall"/> class.
        /// </summary>
        /// <param name="pTexture">Texture used to render the wall.</param>
        /// <param name="pRectangle">Rectangle defining the wall's position and size.</param>
        /// <param name="pRotation">Initial rotation in radians (currently ignored and set to 0).</param>
        public RectWall(Texture2D pTexture, Rectangle pRectangle, float pRotation = 0f) : base(pTexture, pRectangle)
        {
            // Rotation is always initialized to 0; the incoming pRotation is not used.
            mRotation = 0f;
        }


        public float mRotation { get; set; }

        /// <summary>
        /// Draws the wall using its texture, applying scaling to fit the rectangle
        /// and rotation around its center. When selected, the tint changes and a
        /// small status label is drawn above it.
        /// </summary>
        /// <param name="pSpriteBatch">Sprite batch used for drawing.</param>
        public override void Draw(SpriteBatch pSpriteBatch)
        {
            Color tint = GetIsSelected() ? Color.Yellow : COLOUR;

            // Draw at the center of the rectangle so rotation behaves as expected.
            Vector2 position = new Vector2(mRectangle.Center.X, mRectangle.Center.Y);
            Vector2 origin = new Vector2(mTexture.Width / 2f, mTexture.Height / 2f);

            // Scale the texture to exactly fill the rectangle.
            Vector2 scale = new Vector2(
                mRectangle.Width / (float)mTexture.Width,
                mRectangle.Height / (float)mTexture.Height);

            pSpriteBatch.Draw(mTexture, position, null, tint, mRotation, origin, scale, SpriteEffects.None, 0f);

            // When selected, display the current edit mode ("Rotating" or "Scaling").
            if (GetIsSelected())
            {
                pSpriteBatch.DrawString(
                    mFont,
                    mIsRotating ? "Rotating" : "Scaling",
                    new Vector2(mRectangle.X, mRectangle.Y - 20),
                    Color.Black);
            }
        }

        /// <summary>
        /// Draws an outline representation of the wall using the outline rectangle.
        /// The same rotation logic is applied, but the tint is always black.
        /// </summary>
        /// <param name="pSpriteBatch">Sprite batch used for drawing the outline.</param>
        public override void DrawOutline(SpriteBatch pSpriteBatch)
        {
            Vector2 position = new Vector2(mOutlineRectangle.Center.X, mOutlineRectangle.Center.Y);
            Vector2 origin = new Vector2(mTexture.Width / 2f, mTexture.Height / 2f);
            Vector2 scale = new Vector2(
                mOutlineRectangle.Width / (float)mTexture.Width,
                mOutlineRectangle.Height / (float)mTexture.Height);

            pSpriteBatch.Draw(mTexture, position, null, Color.Black, mRotation, origin, scale, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Sets the underlying rectangle of the wall.
        /// </summary>
        /// <param name="pRectangle">New rectangle for the wall.</param>
        public void SetWallRectangle(Rectangle pRectangle) => SetRectangle(pRectangle);

        /// <summary>
        /// Scales the height of the wall by a factor, preserving its X, Y and width.
        /// Ensures the resulting height is at least 1 pixel.
        /// </summary>
        /// <param name="pScale">Scale factor to apply to the current height.</param>
        public void ScaleHeight(float pScale)
        {
            float scaled = mRectangle.Height * pScale;
            int newHeight = (int)Math.Ceiling(scaled);

            // Prevent zero or negative size.
            newHeight = Math.Max(newHeight, 1);

            SetRectangle(new Rectangle(mRectangle.X, mRectangle.Y, mRectangle.Width, newHeight));
        }

        /// <summary>
        /// Scales the width of the wall by a factor, preserving its X, Y and height.
        /// Ensures the resulting width is at least 1 pixel.
        /// </summary>
        /// <param name="pScale">Scale factor to apply to the current width.</param>
        public void ScaleWidth(float pScale)
        {
            float scaled = mRectangle.Width * pScale;
            int newWidth = (int)Math.Ceiling(scaled);

            // Prevent zero or negative size.
            newWidth = Math.Max(newWidth, 1);

            SetRectangle(new Rectangle(mRectangle.X, mRectangle.Y, newWidth, mRectangle.Height));
        }

        /// <summary>
        /// Adjusts the wall's rotation by the specified delta.
        /// </summary>
        /// <param name="pDelta">Amount to add to the current rotation, in radians.</param>
        public void Rotate(float pDelta)
        {
            mRotation += pDelta;
        }

        /// <summary>
        /// Toggles between rotation and scaling modes.
        /// Only one of <see cref="mIsRotating"/> or <see cref="mIsScaling"/> is true at a time.
        /// </summary>
        public void SwitchRotationScaling()
        {
            mIsRotating = !mIsRotating;
            mIsScaling = !mIsScaling;
        }

        /// <summary>
        /// Returns whether the wall is currently in rotation mode.
        /// </summary>
        public bool GetIsRotating() => mIsRotating;

        /// <summary>
        /// Returns whether the wall is currently in scaling mode.
        /// </summary>
        public bool GetIsScaling() => mIsScaling;

        /// <summary>
        /// Determines whether a given point lies within the wall's bounds,
        /// taking the current rotation into account.
        /// 
        /// The point is transformed into the wall's local, unrotated space and
        /// tested against the axis-aligned rectangle.
        /// </summary>
        /// <param name="point">World-space point to test.</param>
        /// <returns><c>true</c> if the point is inside the rotated rectangle; otherwise <c>false</c>.</returns>
        public override bool IsPointWithin(Vector2 point)
        {
            // Center of the rectangle in world space.
            Vector2 center = new Vector2(mRectangle.Center.X, mRectangle.Center.Y);

            // Translate the point so that the rectangle center becomes the origin.
            Vector2 local = point - center;

            // Rotate the point by the inverse of mRotation so that we can
            // test it against the unrotated axis-aligned rectangle.
            float cos = (float)Math.Cos(-mRotation);
            float sin = (float)Math.Sin(-mRotation);

            Vector2 rotated = new Vector2(
                local.X * cos - local.Y * sin,
                local.X * sin + local.Y * cos);

            float halfW = mRectangle.Width / 2f;
            float halfH = mRectangle.Height / 2f;

            // Standard AABB check in the wall's local space.
            return rotated.X >= -halfW && rotated.X <= halfW &&
                   rotated.Y >= -halfH && rotated.Y <= halfH;
        }
    }
}
