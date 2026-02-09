using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tankontroller.World.Shapes;
using Tankontroller.World.WorldObject;

namespace Tankontroller.World.Pickups
{
    public abstract class Pickup : IWorldObject
    {
        public Transform Transform { get; protected set; } = new Transform();
        public CollisionShape CollisionShape => throw new System.NotImplementedException();
        public RectangleAxisAlignedShape RectangleShape { get; protected set; } = null;
        public Rectangle PickupRect { get; protected set; }
        public Texture2D Texture { get; protected set; }


        public int screenWidth = Tankontroller.Instance().GDM().GraphicsDevice.Viewport.Width;
        public int screenHeight = Tankontroller.Instance().GDM().GraphicsDevice.Viewport.Height;
        public float mScalerX;
        public float mScalerY;

        protected Pickup(Texture2D pTexture, Rectangle pRectangle, Vector2 pPosition) {
            mScalerX = ((float)screenWidth / 200f);
            mScalerY = ((float)screenHeight / 200f);
            pRectangle = new Rectangle((int)((pRectangle.X / 10) * mScalerX), (int)((pRectangle.Y / 10) * mScalerY), (int)(pRectangle.Width/10  * mScalerX), (int)(pRectangle.Height/ 10 * mScalerY));
            PickupRect = pRectangle;
            Texture = pTexture;
            Transform.Position = pPosition;
        }

        public virtual void Draw(SpriteBatch pSpriteBatch)
        {
            pSpriteBatch.Draw(Texture, PickupRect, Color.White);
        }

        public virtual bool PickUpCollision(Tank pTank) { return false; }
    }
}
