using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace TTMapEditor.Objects
{
    internal abstract class SceneObject
    {
        protected Texture2D mTexture;
        public Rectangle mRectangle { get; protected set; }
        protected Rectangle mOutlineRectangle;
        private const int OutlinePad = 2;

        // selection state shared by all objects
        private bool mIsSelected = false;

        protected SceneObject(Texture2D texture, Rectangle rectangle)
        {
            mTexture = texture;
            SetRectangle(rectangle);
        }

        // Default colour for Draw ; derived classes can override or override Draw entirely.
        protected virtual Color Colour => Color.White;

        public virtual void Draw(SpriteBatch spriteBatch) => spriteBatch.Draw(mTexture, mRectangle, Colour);

        public virtual void DrawOutline(SpriteBatch spriteBatch) => spriteBatch.Draw(mTexture, mOutlineRectangle, Color.Black);

        public bool IsPointWithin(Vector2 point) => mRectangle.Contains(point);

        public void UpdatePosition(int x, int y) => SetRectangle(new Rectangle(x, y, mRectangle.Width, mRectangle.Height));

        public void SetRectangle(Rectangle rectangle)
        {
            mRectangle = rectangle;
            mOutlineRectangle = new Rectangle(rectangle.X - OutlinePad, rectangle.Y - OutlinePad, rectangle.Width + OutlinePad * 2, rectangle.Height + OutlinePad * 2);
        }

        // Selection helpers
        public void ToggleSelected() => mIsSelected = !mIsSelected;
        public void SetSelected(bool selected) => mIsSelected = selected;
        public bool GetIsSelected() => mIsSelected;
    }
}
