using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
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
        static readonly Texture2D mCircleTexture = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("circle");
        Rectangle mBackgroundRectangle;
        Rectangle mPlayArea;
        Rectangle mPlayAreaOutline;
        RectWall mWall;
        MapPreview mPreview;
        bool mIsNewMap;
        private int mSelectedItems = 0;

        // Selected object (any SceneObject-derived)
        SceneObject mSelectedObject;

        Rectangle mSelectedObjectPreviousRect;

        // Template wall
        bool mIsDraggingTemplate = false;
        Rectangle mTemplateOriginalRect;
        Vector2 mTemplateDragOffset;

        // Templates for tank and pickup
        Tank mTemplateTank;
        Pickup mTemplatePickup;
        bool mIsDraggingTemplateTank = false;
        bool mIsDraggingTemplatePickup = false;
        Rectangle mTemplateTankOriginalRect;
        Rectangle mTemplatePickupOriginalRect;
        Vector2 mTemplateTankDragOffset;
        Vector2 mTemplatePickupDragOffset;

        // drag offset for selected existing object
        Vector2 mSelectedDragOffset;

        //Button for saving map
        Rectangle mSaveButtonRect;

        // max tanks allowed
        const int MaxTanks = 4;

        public MapEditingScene(MainMenuScene pStartScene, string pMapFile, bool pIsNewMap)
        {
            mGraphicsDevice = TTMapEditor.Instance().GetGraphicsDeviceManager().GraphicsDevice;
            mStartScene = pStartScene;
            mName = pMapFile;
            mIsNewMap = pIsNewMap;

            // If this is a new map request, create an initial empty MapData file so MapPreview can load it.
            if (mIsNewMap)
            {
                string mapsRoot = Path.Combine(Environment.CurrentDirectory, "Maps");
                Directory.CreateDirectory(mapsRoot);

                // Normalize incoming path (strip leading "Maps\" if present)
                string relative = pMapFile;
                string mapsPrefix1 = "Maps" + Path.DirectorySeparatorChar;
                string mapsPrefix2 = "Maps" + Path.AltDirectorySeparatorChar;
                if (relative.StartsWith(mapsPrefix1) || relative.StartsWith(mapsPrefix2))
                {
                    relative = relative.Substring(5);
                }

                string candidate = Path.Combine(mapsRoot, relative);

                // If caller supplied just a name (no extension) or a folder, create folder + "map.json"
                string targetPath;
                if (Directory.Exists(candidate) || !Path.HasExtension(candidate))
                {
                    Directory.CreateDirectory(candidate);
                    targetPath = Path.Combine(candidate, "map.json");
                }
                else
                {
                    // candidate is a file path
                    string? dir = Path.GetDirectoryName(candidate);
                    if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                    targetPath = candidate;
                }

                // Create an empty MapData file only if it doesn't already exist
                if (!File.Exists(targetPath))
                {
                    var emptyMap = new MapData
                    {
                        Walls = new List<WallData>(),
                        Tanks = new List<TankData>(),
                        Pickups = new List<PickupData>()
                    };
                    var opts = new JsonSerializerOptions { WriteIndented = true };
                    File.WriteAllText(targetPath, JsonSerializer.Serialize(emptyMap, opts));
                }

                // Use the resolved absolute path when creating the preview so LoadMapPreview picks it up directly
                mPreview = new MapPreview(pFilePath: Path.GetFullPath(targetPath));
            }
            else
            {
                mPreview = new MapPreview(pFilePath: pMapFile);
            }

            mPlayArea = mPreview.GetPlayArea();
            mPlayAreaOutline = new Rectangle(mPlayArea.X - 5, mPlayArea.Y - 5, mPlayArea.Width + 10, mPlayArea.Height + 10);
            mSpriteBatch = new SpriteBatch(mGraphicsDevice);
            mBackgroundRectangle = new Rectangle(0, 0, mGameInstance.GetGraphicsDeviceManager().GraphicsDevice.Viewport.Width, mGameInstance.GetGraphicsDeviceManager().GraphicsDevice.Viewport.Height);
            mWall = new RectWall(mPixelTexture, new Rectangle(100, 100, 50, 50));

            // store the template's default rect so we can reset it after dragging
            mTemplateOriginalRect = mWall.mRectangle;

            // create simple templates for tank/pickup (position them in a small toolbar area)
            mTemplateTank = new Tank(mPixelTexture, new Rectangle(60, 200, 14, 14));
            mTemplatePickup = new Pickup(mCircleTexture, new Rectangle(60, 230, 12, 12));
            mTemplateTankOriginalRect = mTemplateTank.mRectangle;
            mTemplatePickupOriginalRect = mTemplatePickup.mRectangle;
            mSaveButtonRect = new Rectangle(50, 50, 100, 40);
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
            // draw templates (wall, tank, pickup). Templates sit in the UI and can be dragged into the play area.
            mWall.DrawOutline(mSpriteBatch);
            mWall.Draw(mSpriteBatch);

            mTemplateTank.DrawOutline(mSpriteBatch);
            mTemplateTank.Draw(mSpriteBatch);

            mTemplatePickup.DrawOutline(mSpriteBatch);
            mTemplatePickup.Draw(mSpriteBatch);

            // draw save button
            if(mSaveButtonRect.Contains(InputManager.GetMousePosition()))
            {
                mSpriteBatch.Draw(mPixelTexture, mSaveButtonRect, Color.LightGreen);
            }
            else
            {
                mSpriteBatch.Draw(mPixelTexture, mSaveButtonRect, Color.Green);
            }

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
            if (mSaveButtonRect.Contains(mousePos) && InputManager.isLeftMouseClicked())
            {
                saveMap();
            }

            // ---------- Template dragging (create new wall by dragging mWall) ----------
            // Start dragging the template wall if clicked
            if (!mIsDraggingTemplate && !mIsDraggingTemplateTank && !mIsDraggingTemplatePickup && mWall.IsPointWithin(mousePos) && InputManager.isLeftMouseClicked())
            {
                mIsDraggingTemplate = true;
                mTemplateOriginalRect = mWall.mRectangle;
                mTemplateDragOffset = new Vector2(mousePos.X - mWall.mRectangle.X, mousePos.Y - mWall.mRectangle.Y);
            }

            // drag wall template
            if (mIsDraggingTemplate && !InputManager.isLeftMouseReleased())
            {
                int newX = (int)(mousePos.X - mTemplateDragOffset.X);
                int newY = (int)(mousePos.Y - mTemplateDragOffset.Y);
                mWall.UpdatePosition(newX, newY);
            }

            if (mIsDraggingTemplate && InputManager.isLeftMouseReleased())
            {
                if (IsWallWithinPlayArea(mWall))
                {
                    var newRectWall = new RectWall(mPixelTexture, new Rectangle(mWall.mRectangle.X, mWall.mRectangle.Y, mWall.mRectangle.Width, mWall.mRectangle.Height));
                    mPreview.AddWall(newRectWall);
                }

                mWall.SetWallRectangle(mTemplateOriginalRect);
                mIsDraggingTemplate = false;
            }

            // ---------- Template dragging for tank ----------
            if (!mIsDraggingTemplateTank && !mIsDraggingTemplate && !mIsDraggingTemplatePickup && mTemplateTank.IsPointWithin(mousePos) && InputManager.isLeftMouseClicked())
            {
                mIsDraggingTemplateTank = true;
                mTemplateTankOriginalRect = mTemplateTank.mRectangle;
                mTemplateTankDragOffset = new Vector2(mousePos.X - mTemplateTank.mRectangle.X, mousePos.Y - mTemplateTank.mRectangle.Y);
            }

            if (mIsDraggingTemplateTank && !InputManager.isLeftMouseReleased())
            {
                int newX = (int)(mousePos.X - mTemplateTankDragOffset.X);
                int newY = (int)(mousePos.Y - mTemplateTankDragOffset.Y);
                mTemplateTank.UpdatePosition(newX, newY);
            }

            if (mIsDraggingTemplateTank && InputManager.isLeftMouseReleased())
            {
                // Only create tank if within play area and tank count < MaxTanks
                if (mTemplateTank.mRectangle.Left >= mPlayArea.Left
                    && mTemplateTank.mRectangle.Top >= mPlayArea.Top
                    && mTemplateTank.mRectangle.Right <= mPlayArea.Right
                    && mTemplateTank.mRectangle.Bottom <= mPlayArea.Bottom
                    && mPreview.GetTanks().Count < MaxTanks)
                {
                    var newTank = new Tank(mPixelTexture, new Rectangle(mTemplateTank.mRectangle.X, mTemplateTank.mRectangle.Y, mTemplateTank.mRectangle.Width, mTemplateTank.mRectangle.Height));
                    mPreview.AddTank(newTank);
                }

                // reset template
                mTemplateTank.SetRectangle(mTemplateTankOriginalRect);
                mIsDraggingTemplateTank = false;
            }

            // ---------- Template dragging for pickup ----------
            if (!mIsDraggingTemplatePickup && !mIsDraggingTemplate && !mIsDraggingTemplateTank && mTemplatePickup.IsPointWithin(mousePos) && InputManager.isLeftMouseClicked())
            {
                mIsDraggingTemplatePickup = true;
                mTemplatePickupOriginalRect = mTemplatePickup.mRectangle;
                mTemplatePickupDragOffset = new Vector2(mousePos.X - mTemplatePickup.mRectangle.X, mousePos.Y - mTemplatePickup.mRectangle.Y);
            }

            if (mIsDraggingTemplatePickup && !InputManager.isLeftMouseReleased())
            {
                int newX = (int)(mousePos.X - mTemplatePickupDragOffset.X);
                int newY = (int)(mousePos.Y - mTemplatePickupDragOffset.Y);
                mTemplatePickup.UpdatePosition(newX, newY);
            }

            if (mIsDraggingTemplatePickup && InputManager.isLeftMouseReleased())
            {
                // create pickup if inside play area
                Rectangle pr = mTemplatePickup.mRectangle;
                if (pr.Left >= mPlayArea.Left && pr.Top >= mPlayArea.Top && pr.Right <= mPlayArea.Right && pr.Bottom <= mPlayArea.Bottom)
                {
                    var newPickup = new Pickup(mCircleTexture, new Rectangle(pr.X, pr.Y, pr.Width, pr.Height));
                    mPreview.AddPickup(newPickup);
                }

                // reset template
                mTemplatePickup.SetRectangle(mTemplatePickupOriginalRect);
                mIsDraggingTemplatePickup = false;
            }

            // ---------- Existing objects selection / dragging (pick top-most) ----------
            bool isAnyTemplateDragging = mIsDraggingTemplate || mIsDraggingTemplateTank || mIsDraggingTemplatePickup;

            if (!isAnyTemplateDragging)
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
                // Tank rotation: left/right rotate by 15 degrees per key press
                if (mSelectedObject is Tank selectedTank)
                {
                    float rotationStep = MathHelper.ToRadians(15.0f);
                    if (InputManager.isKeyPressed(Keys.Left))
                    {
                        selectedTank.Rotate(-rotationStep);
                    }
                    if (InputManager.isKeyPressed(Keys.Right))
                    {
                        selectedTank.Rotate(rotationStep);
                    }

                    // optional: clamp or wrap rotation if you prefer a 0..2π range
                    // selectedTank.Rotation = MathHelper.WrapAngle(selectedTank.Rotation);
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

        public void saveMap()
        {
            mPreview.SaveMap();
        }
    }
}



