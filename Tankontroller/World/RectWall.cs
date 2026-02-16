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
            //RectangleShape.LocalRotation = MathHelper.ToRadians(45f);
            mTexture = pTexture;
        }

        public void Draw(SpriteBatch pSpriteBatch)
        {
            RectangleShape.Draw(pSpriteBatch, mTexture, COLOUR);
        }

        public void DrawOutlines(SpriteBatch pSpriteBatch)
        {
            RectangleOrientedShape outlineShape = new(Transform, RectangleShape.Size + new Vector2(2f, 2f), RectangleShape.LocalRotation, RectangleShape.LocalOffset - new Vector2(1f, 1f));
            outlineShape.Draw(pSpriteBatch, mTexture, Color.Black);
        }
    }
}
