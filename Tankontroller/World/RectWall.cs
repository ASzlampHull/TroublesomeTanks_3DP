using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tankontroller.World.Shapes;
using Tankontroller.World.WorldObject;

namespace Tankontroller.World
{
    public class RectWall : IWorldObject
    {
        private static readonly Color COLOUR = DGS.Instance.GetColour("COLOUR_WALLS");

        public Transform Transform { get; private set; } = new Transform();
        public CollisionShape CollisionShape => RectangleShape;
        public RectangleOrientedShape RectangleShape { get; private set; }

        private Texture2D mTexture;

        public RectWall(Transform pTransform, Vector2 pSize, Texture2D pTexture)
        {
            Transform = pTransform;
            RectangleShape = new RectangleOrientedShape(Transform, pSize);
            mTexture = pTexture;
        }

        public void Draw(SpriteBatch pSpriteBatch)
        {
            Rectangle rectangle = RectangleShape.ToRectangle();
            pSpriteBatch.Draw(mTexture, rectangle, null, COLOUR, Transform.Rotation, new Vector2(0f,0f), SpriteEffects.None, 0f);
        }

        public void DrawOutlines(SpriteBatch pSpriteBatch)
        {
            Rectangle rectangle = RectangleShape.ToRectangle();
            rectangle.Inflate(2, 2);
            pSpriteBatch.Draw(mTexture, rectangle, null, Color.Black, Transform.Rotation, new Vector2(0f, 0f), SpriteEffects.None, 0f);
        }
    }
}
