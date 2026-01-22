using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTMapEditor.Managers;
using TTMapEditor.Maps;
using TTMapEditor.Objects;

namespace TTMapEditor.Scenes
{
    internal class MapEditingScene : IScene
    {
        GraphicsDevice mGraphicsDevice;
        IGame mGameInstance = TTMapEditor.Instance();
        MainMenuScene mStartScene;
        String mName;
        static readonly SpriteFont mTitleFont = TTMapEditor.Instance().GetContentManager().Load<SpriteFont>("TitleFont");
        static readonly Texture2D mBackgroundTexture = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("background_01");
        static readonly Texture2D mPixelTexture = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("white_pixel");
        Rectangle mBackgroundRectangle;
        Rectangle mPlayArea;
        Rectangle mPlayAreaOutline;
        RectWall mWall;
        MapPreview mPreview;
        bool mIsNewMap;
        private int mSelectedItems = 0;

        // Selected object (any Object-derived)
        SceneObject mSelectedObject;
        Rectangle mSelectedObjectPreviousRect;

        // New fields to support dragging the template wall to create a new wall
        bool mIsDraggingTemplate = false;
        Rectangle mTemplateOriginalRect;
        Vector2 mTemplateDragOffset;

        // drag offset for selected existing object
        Vector2 mSelectedDragOffset;

        public MapEditingScene(MainMenuScene pStartScene, string pMapFile, bool pIsNewMap)
        {
            mGraphicsDevice = TTMapEditor.Instance().GetGraphicsDeviceManager().GraphicsDevice;
            mStartScene = pStartScene;
            mName = pMapFile;
            mPreview = new MapPreview(pFilePath: pMapFile);
            mPlayArea = mPreview.GetPlayArea();
            mPlayAreaOutline = new Rectangle(mPlayArea.X - 5, mPlayArea.Y - 5, mPlayArea.Width + 10, mPlayArea.Height + 10);
            mSpriteBatch = new SpriteBatch(mGraphicsDevice);
            mBackgroundRectangle = new Rectangle(0, 0, mGameInstance.GetGraphicsDeviceManager().GraphicsDevice.Viewport.Width, mGameInstance.GetGraphicsDeviceManager().GraphicsDevice.Viewport.Height);
            mWall = new RectWall(mPixelTexture, new Rectangle(100, 100, 50, 50));
            mIsNewMap = pIsNewMap;

            // store the template's default rect so we can reset it after dragging
            mTemplateOriginalRect = mWall.mRectangle;
        }

        void DeselectAll()
        {
            foreach (var w in mPreview.GetWalls()) w.SetSelected(false);
            foreach (var t in mPreview.GetTanks()) t.SetSelected(false);
            foreach (var p in mPreview.GetPickups()) p.SetSelected(false);
            mSelectedObject = null;
        }

        public override void Draw(float pSeconds)
        {

            mGraphicsDevice.Clear(Color.CornflowerBlue);
            mSpriteBatch.Begin();
            mSpriteBatch.Draw(mBackgroundTexture, mBackgroundRectangle, Color.White);
            mSpriteBatch.DrawString(mTitleFont, mName, new Vector2(100, 100), Color.Black);
            mSpriteBatch.Draw(mPixelTexture, mPlayAreaOutline, Color.Black);
            mSpriteBatch.Draw(mPixelTexture, mPlayArea, Color.Wheat);
            if (!mIsNewMap)
            {
                foreach (RectWall wall in mPreview.GetWalls())
                {
                    wall.DrawOutline(mSpriteBatch);
                    wall.Draw(mSpriteBatch);
                }
                foreach (Tank tank in mPreview.GetTanks())
                {
                    tank.DrawOutline(mSpriteBatch);
                    tank.Draw(mSpriteBatch);
                }
                foreach (Pickup pickup in mPreview.GetPickups())
                {
                    pickup.DrawOutline(mSpriteBatch);
                    pickup.Draw(mSpriteBatch);
                }
            }

            // draw the template wall (while dragging it will follow the mouse)
            mWall.DrawOutline(mSpriteBatch);
            mWall.Draw(mSpriteBatch);
            mSpriteBatch.End();
        }

        public override void Update(float pSeconds)
        {
            InputManager.Update();
            if (InputManager.isKeyPressed(Keys.Escape))
            {
                mGameInstance.GetSceneManager().Transition(mStartScene);
            }

            Vector2 mousePos = InputManager.GetMousePosition();

            // ---------- Template dragging (create new wall by dragging mWall) ----------
            // Start dragging the template if the mouse clicks it
            if (!mIsDraggingTemplate && mWall.IsPointWithin(mousePos) && InputManager.isLeftMouseClicked())
            {
                mIsDraggingTemplate = true;
                mTemplateOriginalRect = mWall.mRectangle;
                // remember offset so the wall doesn't "jump" when you start dragging
                mTemplateDragOffset = new Vector2(mousePos.X - mWall.mRectangle.X, mousePos.Y - mWall.mRectangle.Y);
            }

            // While dragging the template (mouse down), move the template with the mouse
            if (mIsDraggingTemplate && !InputManager.isLeftMouseReleased())
            {
                int newX = (int)(mousePos.X - mTemplateDragOffset.X);
                int newY = (int)(mousePos.Y - mTemplateDragOffset.Y);
                mWall.UpdatePosition(newX, newY);
            }

            // On mouse release, if we were dragging the template, either create the new wall if valid or discard it.
            if (mIsDraggingTemplate && InputManager.isLeftMouseReleased())
            {
                if (IsWallWithinPlayArea(mWall))
                {
                    // create a new wall at the template's current position
                    var newRectWall = new RectWall(mPixelTexture, new Rectangle(mWall.mRectangle.X, mWall.mRectangle.Y, mWall.mRectangle.Width, mWall.mRectangle.Height));
                    mPreview.AddWall(newRectWall);
                }

                // reset template to its original position and clear dragging state
                mWall.SetWallRectangle(mTemplateOriginalRect);
                mIsDraggingTemplate = false;
            }

            // ---------- Existing objects selection / dragging (pick top-most) ----------
            if (!mIsDraggingTemplate)
            {
                // Check pickups first (drawn last = topmost), then tanks, then walls
                bool handledClick = false;

                // pick-ups
                foreach (Pickup pickup in mPreview.GetPickups())
                {
                    if (handledClick) break;
                    if (pickup.IsPointWithin(mousePos) && InputManager.isLeftMouseClicked())
                    {
                        if (!pickup.GetIsSelected())
                        {
                            DeselectAll();
                            pickup.SetSelected(true);
                            mSelectedObject = pickup;
                            mSelectedObjectPreviousRect = pickup.mRectangle;
                            mSelectedDragOffset = new Vector2(mousePos.X - pickup.mRectangle.X, mousePos.Y - pickup.mRectangle.Y);
                        }
                        else
                        {
                            pickup.SetSelected(false);
                            mSelectedObject = null;
                        }
                        handledClick = true;
                    }

                    if (pickup.GetIsSelected() && !InputManager.isLeftMouseReleased())
                    {
                        int newX = (int)(mousePos.X - mSelectedDragOffset.X);
                        int newY = (int)(mousePos.Y - mSelectedDragOffset.Y);
                        pickup.UpdatePosition(newX, newY);
                    }
                }

                // tanks
                foreach (Tank tank in mPreview.GetTanks())
                {
                    if (handledClick) break;
                    if (tank.IsPointWithin(mousePos) && InputManager.isLeftMouseClicked())
                    {
                        if (!tank.GetIsSelected())
                        {
                            DeselectAll();
                            tank.SetSelected(true);
                            mSelectedObject = tank;
                            mSelectedObjectPreviousRect = tank.mRectangle;
                            mSelectedDragOffset = new Vector2(mousePos.X - tank.mRectangle.X, mousePos.Y - tank.mRectangle.Y);
                        }
                        else
                        {
                            tank.SetSelected(false);
                            mSelectedObject = null;
                        }
                        handledClick = true;
                    }

                    if (tank.GetIsSelected() && !InputManager.isLeftMouseReleased())
                    {
                        int newX = (int)(mousePos.X - mSelectedDragOffset.X);
                        int newY = (int)(mousePos.Y - mSelectedDragOffset.Y);
                        tank.UpdatePosition(newX, newY);
                    }
                }

                // walls
                foreach (RectWall wall in mPreview.GetWalls())
                {
                    if (handledClick) break;
                    if (wall.IsPointWithin(mousePos) && InputManager.isLeftMouseClicked())
                    {
                        if (!wall.GetIsSelected())
                        {
                            DeselectAll();
                            wall.SetSelected(true);
                            mSelectedObject = wall;
                            mSelectedObjectPreviousRect = wall.mRectangle;
                            mSelectedDragOffset = new Vector2(mousePos.X - wall.mRectangle.X, mousePos.Y - wall.mRectangle.Y);
                        }
                        else
                        {
                            wall.SetSelected(false);
                            mSelectedObject = null;
                        }
                        handledClick = true;
                    }

                    if (wall.GetIsSelected() && !InputManager.isLeftMouseReleased())
                    {
                        int newX = (int)(mousePos.X - mSelectedDragOffset.X);
                        int newY = (int)(mousePos.Y - mSelectedDragOffset.Y);
                        wall.UpdatePosition(newX, newY);
                    }
                }

                // On mouse release finalize move: if object outside play area revert
                if (mSelectedObject != null && mSelectedObject.GetIsSelected() && InputManager.isLeftMouseReleased())
                {
                    if (mSelectedObject is RectWall rw && !IsWallWithinPlayArea(rw))
                    {
                        mSelectedObject.SetRectangle(mSelectedObjectPreviousRect);
                    }
                    // For tanks/pickups ensure they remain inside play area (simple clamp)
                    if (mSelectedObject is Tank || mSelectedObject is Pickup)
                    {
                        Rectangle r = mSelectedObject.mRectangle;
                        if (r.Left < mPlayArea.Left || r.Top < mPlayArea.Top || r.Right > mPlayArea.Right || r.Bottom > mPlayArea.Bottom)
                        {
                            mSelectedObject.SetRectangle(mSelectedObjectPreviousRect);
                        }
                    }

                    mSelectedObject.SetSelected(false);
                    mSelectedObject = null;
                }
            }

            // Keyboard actions: delete or scale (scaling only applies to walls)
            if (mSelectedObject != null && mSelectedObject.GetIsSelected())
            {
                if (InputManager.isKeyPressed(Keys.Delete))
                {
                    if (mSelectedObject is RectWall rw)
                    {
                        mPreview.RemoveWall(rw);
                    }
                    else if (mSelectedObject is Tank t)
                    {
                        mPreview.RemoveTank(t);
                    }
                    else if (mSelectedObject is Pickup p)
                    {
                        mPreview.RemovePickup(p);
                    }
                    mSelectedObject = null;
                    return;
                }

                if (mSelectedObject is RectWall selectedWall)
                {
                    if (InputManager.isKeyPressed(Keys.Left))
                    {
                        selectedWall.ScaleWidth(0.75f);
                    }
                    if (InputManager.isKeyPressed(Keys.Right))
                    {
                        selectedWall.ScaleWidth(1.25f);
                    }
                    if (InputManager.isKeyPressed(Keys.Up))
                    {
                        selectedWall.ScaleHeight(1.25f);
                    }
                    if (InputManager.isKeyPressed(Keys.Down))
                    {
                        selectedWall.ScaleHeight(0.75f);
                    }

                    if (!IsWallWithinPlayArea(selectedWall))
                    {
                        selectedWall.SetWallRectangle(mSelectedObjectPreviousRect);
                    }
                    else
                    {
                        mSelectedObjectPreviousRect = selectedWall.mRectangle;
                    }
                }
            }
        }

        public bool IsWallWithinPlayArea(RectWall pWall)
        {
            Rectangle r = pWall.mRectangle;
            return r.Left >= mPlayArea.Left
                && r.Top >= mPlayArea.Top
                && r.Right <= mPlayArea.Right
                && r.Bottom <= mPlayArea.Bottom;
        }
    }
}



