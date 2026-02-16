using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TTMapEditor.Managers;
using TTMapEditor.Maps;

namespace TTMapEditor.Objects
{
    public class TemplatePalette
    {

        private readonly SpriteFont mTitleFont;
        private readonly Texture2D mPixelTexture;
        private readonly Texture2D mCircleTexture;
        private readonly MapPreview mMapPreview;
        private readonly MapBoundaryValidator mMapBoundaryValidator;

        private readonly RectWall mWallTemplateVisual;
        private readonly Tank mTankTemplateVisual;
        private readonly Pickup mPickupTemplateVisual;

        private readonly DraggableTemplate<RectWall> mWallTemplate;
        private readonly DraggableTemplate<Tank> mTankTemplate;
        private readonly DraggableTemplate<Pickup> mPickupTemplate;

        private readonly int mMaxTanks;

        public bool IsDraggingAny
        {
            get
            {
                return mWallTemplate.mIsDragging || mTankTemplate.mIsDragging || mPickupTemplate.mIsDragging;
            }
        }

        public TemplatePalette(SpriteFont pTitleFont, Texture2D pPixelTexture, Texture2D pCircleTexture, MapPreview pMapPreview, MapBoundaryValidator pBoundaryValidatorm, int pViewPortWidth, int pMaxTanks)
        {
            mTitleFont = pTitleFont;
            mPixelTexture = pPixelTexture;
            mCircleTexture = pCircleTexture;
            mMapPreview = pMapPreview;
            mMapBoundaryValidator = pBoundaryValidatorm;
            mMaxTanks = pMaxTanks;

            mWallTemplateVisual = new RectWall(mPixelTexture, new Rectangle(pViewPortWidth - 5 * pViewPortWidth / 8, 200,200, 50));

            mTankTemplateVisual = new Tank(mPixelTexture, new Rectangle(pViewPortWidth - pViewPortWidth / 8, 200, 14, 14));

            mPickupTemplateVisual = new Pickup(mCircleTexture, new Rectangle(pViewPortWidth - pViewPortWidth / 3, 200, 14, 14));

            mWallTemplate = new DraggableTemplate<RectWall>(mWallTemplateVisual);
            mTankTemplate = new DraggableTemplate<Tank>(mTankTemplateVisual);
            mPickupTemplate = new DraggableTemplate<Pickup>(mPickupTemplateVisual);
        }

        public void Update(Vector2 pMousePosition)
        {
            HandleWallTemplateDragging(pMousePosition);
            HandleTankTemplateDragging(pMousePosition);
            HandlePickupTemplateDragging(pMousePosition);
        }

        public void Draw(SpriteBatch pSpriteBatch)
        {
            DrawWallTemplate(pSpriteBatch);
            DrawTankTemplate(pSpriteBatch);
            DrawPickupTemplate(pSpriteBatch);
        }

        private void HandleWallTemplateDragging(Vector2 pMousePosition)
        {
            if(!mWallTemplate.mIsDragging && mWallTemplate.mTemplate.IsPointWithin(pMousePosition) && InputManager.isLeftMouseClicked())
            {
                mWallTemplate.BeginDrag(pMousePosition);
            }

            if(mWallTemplate.mIsDragging && !InputManager.isLeftMouseReleased())
            {
                mWallTemplate.Update(pMousePosition);
            }

            if(mWallTemplate.mIsDragging && InputManager.isLeftMouseReleased())
            { 
                mWallTemplate.EndDrag(pResetToOriginal: false);

                if (mMapBoundaryValidator.IsRectWithinPlayArea(mWallTemplate.mTemplate.mRectangle))
                {
                    RectWall newWall = new RectWall(mPixelTexture, mWallTemplate.mTemplate.mRectangle);
                    mMapPreview.AddObject(newWall);
                }

                mWallTemplate.Reset();
            }
        }

        private void HandleTankTemplateDragging(Vector2 pMousePosition)
        {
            if(!mTankTemplate.mIsDragging && mTankTemplate.mTemplate.IsPointWithin(pMousePosition) && InputManager.isLeftMouseClicked())
            {
                mTankTemplate.BeginDrag(pMousePosition);
            }

            if(mTankTemplate.mIsDragging && !InputManager.isLeftMouseReleased())
            {
                mTankTemplate.Update(pMousePosition);
            }

            if(mTankTemplate.mIsDragging && InputManager.isLeftMouseReleased())
            {
                mTankTemplate.EndDrag(pResetToOriginal: false);

                if(mMapBoundaryValidator.IsRectWithinPlayArea(mTankTemplate.mTemplate.mRectangle) && mMapPreview.GetTanks().Count < mMaxTanks)
                {
                    Tank newTank = new Tank(mPixelTexture, mTankTemplate.mTemplate.mRectangle);
                    mMapPreview.AddObject(newTank);
                }

                mTankTemplate.Reset();
            }
        }

        private void HandlePickupTemplateDragging(Vector2 pMousePosition)
        {
            if(!mPickupTemplate.mIsDragging && mPickupTemplate.mTemplate.IsPointWithin(pMousePosition) && InputManager.isLeftMouseClicked())
            {
                mPickupTemplate.BeginDrag(pMousePosition);
            }

            if(mPickupTemplate.mIsDragging && !InputManager.isLeftMouseReleased())
            {
                mPickupTemplate.Update(pMousePosition);
            }

            if(mPickupTemplate.mIsDragging && InputManager.isLeftMouseReleased())
            {
                mPickupTemplate.EndDrag(pResetToOriginal: false);
                if(mMapBoundaryValidator.IsRectWithinPlayArea(mPickupTemplate.mTemplate.mRectangle))
                {
                    Pickup newPickup = new Pickup(mCircleTexture, mPickupTemplate.mTemplate.mRectangle);
                    mMapPreview.AddObject(newPickup);
                }
                mPickupTemplate.Reset();
            }
        }

        private void DrawWallTemplate(SpriteBatch pSpriteBatch)
        {
            mWallTemplateVisual.DrawOutline(pSpriteBatch);
            mWallTemplateVisual.Draw(pSpriteBatch);

            float labelWidth = mTitleFont.MeasureString("Wall").X;
            float labelHeight = mTitleFont.MeasureString("Wall").Y;

            Vector2 labelPosition = new Vector2(
                mWallTemplateVisual.mRectangle.X + labelWidth / 4f,
                mWallTemplateVisual.mRectangle.Y - labelHeight);

            pSpriteBatch.DrawString(mTitleFont, "Wall", labelPosition, Color.Black);
        }

        private void DrawTankTemplate(SpriteBatch pSpriteBatch)
        {
            mTankTemplateVisual.DrawOutline(pSpriteBatch);
            mTankTemplateVisual.Draw(pSpriteBatch);

            float labelWidth = mTitleFont.MeasureString("Tank").X;
            float labelHeight = mTitleFont.MeasureString("Tank").Y;

            Vector2 labelPosition = new Vector2(
                mTankTemplateVisual.mRectangle.X - labelWidth / 2f,
                mTankTemplateVisual.mRectangle.Y - labelHeight);

            pSpriteBatch.DrawString(mTitleFont, "Tank", labelPosition, Color.Black);
        }

        private void DrawPickupTemplate(SpriteBatch pSpriteBatch)
        {
            mPickupTemplateVisual.DrawOutline(pSpriteBatch);
            mPickupTemplateVisual.Draw(pSpriteBatch);

            float labelWidth = mTitleFont.MeasureString("Pickup").X;
            float labelHeight = mTitleFont.MeasureString("Pickup").Y;

            Vector2 labelPosition = new Vector2(
                mPickupTemplateVisual.mRectangle.X - labelWidth / 2f,
                mPickupTemplateVisual.mRectangle.Y - labelHeight);

            pSpriteBatch.DrawString(mTitleFont, "Pickup", labelPosition, Color.Black);
        }

    }
}
