using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTMapEditor.Objects
{
    enum PickupType
    {
        HEALTH,
        EMP,
        MINE,
        BOUNCY_BULLET,
    }

    internal class Pickup : SceneObject
    {
        private static readonly Color COLOUR = Color.Red;
        private static readonly Texture2D mEmpIcon = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("EMP");
        private static readonly Texture2D mHealthIcon = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("healthpickup");
        private static readonly Texture2D mMineIcon = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("MinePickup");
        private static readonly Texture2D mBouncyIcon = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("BouncyBulletPickup");
        private static readonly SpriteFont mFont = TTMapEditor.Instance().GetContentManager().Load<SpriteFont>("TitleFont");


        public Pickup(Texture2D pTexture, Rectangle pRectangle) : base(pTexture, pRectangle) { }

        private Dictionary<PickupType, bool> mActivatedPickups = new Dictionary<PickupType, bool>()
        {
            { PickupType.HEALTH, true },
            { PickupType.EMP, true },
            { PickupType.MINE, true },
            { PickupType.BOUNCY_BULLET, true },
        };

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            // highlight when selected
            pSpriteBatch.Draw(mTexture, mRectangle, GetIsSelected() ? Color.Yellow : COLOUR);
            if (GetIsSelected())
            {
                // draw icons for activated pickups
                int iconSize = Math.Min(mRectangle.Width, mRectangle.Height) * 5;
                int iconX = mRectangle.X + 2;
                int iconY = mRectangle.Y + 2;

                int index = 1;
                foreach (var pickup in mActivatedPickups)
                {
                    Texture2D iconTexture = pickup.Key switch
                    {
                        PickupType.HEALTH => mHealthIcon,
                        PickupType.EMP => mEmpIcon,
                        PickupType.MINE => mMineIcon,
                        PickupType.BOUNCY_BULLET => mBouncyIcon,
                        _ => null
                    };
                    if (iconTexture != null)
                    {
                        Rectangle iconRect = new Rectangle(iconX, iconY, iconSize, iconSize);
                        if (pickup.Value)
                        {
                            pSpriteBatch.Draw(iconTexture, iconRect, Color.White);
                        }
                        else
                        {
                            pSpriteBatch.Draw(iconTexture, iconRect, Color.Black);
                        }

                        // Draw the number centered on the icon with a 1px shadow for readability.
                        string number = index.ToString();
                        Vector2 textSize = mFont.MeasureString(number);
                        Vector2 textPos = new Vector2(iconRect.X + iconRect.Width / 2f, iconRect.Y + iconRect.Height / 2f);

                        // Shadow
                        pSpriteBatch.DrawString(mFont, number, textPos + new Vector2(1f, 1f), Color.Black, 0f, textSize / 2f, 1.0f, SpriteEffects.None, 1f);
                        // Foreground color: white when active, light gray when inactive
                        Color fg = pickup.Value ? Color.White : Color.LightGray;
                        pSpriteBatch.DrawString(mFont, number, textPos, fg, 0f, textSize / 2f, 1.0f, SpriteEffects.None, 1f);

                        iconX += iconSize + 2; // move to the right for the next icon
                        index++;
                    }
                }
            }
        }

        public override void DrawOutline(SpriteBatch pSpriteBatch) => pSpriteBatch.Draw(mTexture, mOutlineRectangle, Color.Black);

        public void ActivatePickupType(PickupType type)
        {
            if (mActivatedPickups.ContainsKey(type))
            {
                mActivatedPickups[type] = true;
            }
        }

        public void TogglePickupType(PickupType type)
        {
            if (mActivatedPickups.ContainsKey(type))
            {
                mActivatedPickups[type] = !mActivatedPickups[type];
            }
        }

        public void DeactivatePickupType(PickupType type)
        {
            if (mActivatedPickups.ContainsKey(type))
            {
                mActivatedPickups[type] = false;
            }
        }

    }
}
