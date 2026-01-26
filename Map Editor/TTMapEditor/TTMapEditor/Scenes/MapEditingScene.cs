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
    /// <summary>
    /// Scene used to edit maps: place walls, tanks and pickups
    /// Responsibilities:
    /// - Render preview and UI
    /// - Handle template dragging(create new objects
    /// - Handle selection, dragging and keyboard actions for exisiting objects
    /// </summary>
    
    internal class MapEditingScene : IScene
    {
        GraphicsDevice mGraphicsDevice;
        IGame mGameInstance = TTMapEditor.Instance();
        MainMenuScene mStartScene;
        string mName;
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
        Vector2 mSelectedDragOffset;

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
        bool mIsDragging = false;

        private DraggableTemplate<RectWall> mWallTemplate;
        private DraggableTemplate<Tank> mTankTemplate;
        private DraggableTemplate<Pickup> mPickupTemplate;


        // Button for saving map
        Rectangle mSaveButtonRect;

        // max tanks allowed
        const int MaxTanks = 4;

        public MapEditingScene(MainMenuScene pStartScene, string pMapFile, bool pIsNewMap)
        {
            mGraphicsDevice = TTMapEditor.Instance().GetGraphicsDeviceManager().GraphicsDevice;
            mStartScene = pStartScene;
            mName = pMapFile;
            mIsNewMap = pIsNewMap;
            int viewPortWidth = mGraphicsDevice.Viewport.Width;
            int viewPortHeight = mGraphicsDevice.Viewport.Height;

            // If this is a new map request, create an initial empty MapData file so MapPreview can load it.
            if (mIsNewMap)
            {
                HandleNewMapCreation(pMapFile);
            }
            else
            {
                mPreview = new MapPreview(pFilePath: pMapFile);
            }

            mPlayArea = mPreview.GetPlayArea();
            mPlayAreaOutline = new Rectangle(mPlayArea.X - 5, mPlayArea.Y - 5, mPlayArea.Width + 10, mPlayArea.Height + 10);
            mSpriteBatch = new SpriteBatch(mGraphicsDevice);
            mBackgroundRectangle = new Rectangle(0, 0, viewPortWidth, viewPortHeight);
            
            mWall = new RectWall(mPixelTexture, new Rectangle(viewPortWidth - 5 * viewPortWidth / 8, 200, 200, 50));
            mTemplateTank = new Tank(mPixelTexture, new Rectangle(viewPortWidth - viewPortWidth / 8, 200, 14, 14));
            mTemplatePickup = new Pickup(mCircleTexture, new Rectangle(viewPortWidth - viewPortWidth / 3, 200, 14, 14));
            
            mWallTemplate = new DraggableTemplate<RectWall>(mWall);
            mTankTemplate = new DraggableTemplate<Tank>(mTemplateTank);
            mPickupTemplate = new DraggableTemplate<Pickup>(mTemplatePickup);


            int saveButtonWidth = (int)(mTitleFont.MeasureString("Save").X + 20);
            int saveButtonHeight = (int)(mTitleFont.MeasureString("Save").Y + 10);
            mSaveButtonRect = new Rectangle(viewPortWidth - viewPortWidth + viewPortWidth / 16, 5, saveButtonWidth, saveButtonHeight);
        }

        void HandleNewMapCreation(string pMapFile)
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

        /// <summary>
        /// Deselect every object in the preview.
        /// </summary>
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

            DrawBackgroundAndTitle();
            DrawPlayAreaAndObjects();
            DrawTemplates();
            DrawSaveButton();

            mSpriteBatch.End();
        }

        void DrawBackgroundAndTitle()
        {
            mSpriteBatch.Draw(mBackgroundTexture, mBackgroundRectangle, Color.White);
            mSpriteBatch.DrawString(mTitleFont, mName, new Vector2(100, 100), Color.Black);
        }

        void DrawPlayAreaAndObjects()
        {
            mSpriteBatch.Draw(mPixelTexture, mPlayAreaOutline, Color.Black);
            mSpriteBatch.Draw(mPixelTexture, mPlayArea, Color.Wheat);

            foreach (RectWall wall in mPreview.GetWalls())
            {
                wall.DrawOutline(mSpriteBatch);
            }
            foreach (RectWall wall in mPreview.GetWalls())
            {
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

        void DrawTemplates()
        {
            // wall template
            mWall.DrawOutline(mSpriteBatch);
            mWall.Draw(mSpriteBatch);
            float wallLabelWidth = mTitleFont.MeasureString("Wall").X;
            float wallLabelHeight = mTitleFont.MeasureString("Wall").Y;
            mSpriteBatch.DrawString(mTitleFont, "Wall", new Vector2(mWall.mRectangle.X + wallLabelWidth / 4, mWall.mRectangle.Y - wallLabelHeight), Color.Black);

            // tank template
            mTemplateTank.DrawOutline(mSpriteBatch);
            mTemplateTank.Draw(mSpriteBatch);
            float tankLabelWidth = mTitleFont.MeasureString("Tank").X;
            float tankLabelHeight = mTitleFont.MeasureString("Tank").Y;
            mSpriteBatch.DrawString(mTitleFont, "Tank", new Vector2(mTemplateTank.mRectangle.X - tankLabelWidth / 2, mTemplateTank.mRectangle.Y - tankLabelHeight), Color.Black);

            // pickup template
            mTemplatePickup.DrawOutline(mSpriteBatch);
            mTemplatePickup.Draw(mSpriteBatch);
            float pickupLabelWidth = mTitleFont.MeasureString("Pickup").X;
            float pickupLabelHeight = mTitleFont.MeasureString("Pickup").Y;
            mSpriteBatch.DrawString(mTitleFont, "Pickup", new Vector2(mTemplatePickup.mRectangle.X - pickupLabelWidth / 2, mTemplatePickup.mRectangle.Y - pickupLabelHeight), Color.Black);
        }

        void handlePickupEnabling()
        {
            if(mSelectedObject is Pickup)
            {
                if(InputManager.isKeyPressed(Keys.D1))
                {
                    ((Pickup)mSelectedObject).TogglePickupType(PickupType.HEALTH);
                }
                if (InputManager.isKeyPressed(Keys.D2))
                {
                    ((Pickup)mSelectedObject).TogglePickupType(PickupType.EMP);
                }
                if (InputManager.isKeyPressed(Keys.D3))
                {
                    ((Pickup)mSelectedObject).TogglePickupType(PickupType.MINE);
                }
                if (InputManager.isKeyPressed(Keys.D4))
                {
                    ((Pickup)mSelectedObject).TogglePickupType(PickupType.BOUNCY_BULLET);
                }
            }
        }

        void DrawSaveButton()
        {
            if (mSaveButtonRect.Contains(InputManager.GetMousePosition()))
            {
                mSpriteBatch.Draw(mPixelTexture, mSaveButtonRect, Color.LightGreen);
            }
            else
            {
                mSpriteBatch.Draw(mPixelTexture, mSaveButtonRect, Color.Green);
            }
            float saveLabelWidth = mTitleFont.MeasureString("Save").X;
            float saveLabelHeight = mTitleFont.MeasureString("Save").Y;
            mSpriteBatch.DrawString(mTitleFont, "Save", new Vector2(mSaveButtonRect.X + (mSaveButtonRect.Width - saveLabelWidth) / 2, mSaveButtonRect.Y + (mSaveButtonRect.Height - saveLabelHeight) / 2), Color.Black);
        }

        public override void Update(float pSeconds)
        {
            InputManager.Update();

            if (InputManager.isKeyPressed(Keys.Escape))
            {
                mGameInstance.GetSceneManager().Transition(mStartScene);
                return;
            }

            Vector2 mousePos = InputManager.GetMousePosition();

            if (mSaveButtonRect.Contains(mousePos) && InputManager.isLeftMouseClicked())
            {
                SaveMap();
            }

            HandleTemplateWallDragging(mousePos);
            HandleTemplateTankDragging(mousePos);
            HandleTemplatePickupDragging(mousePos);

            // If any template is being dragged, skip interacting with existing objects.
            if (!(mIsDraggingTemplate || mIsDraggingTemplateTank || mIsDraggingTemplatePickup))
            {
                HandleExistingObjectInteraction(mousePos);
            }

            HandleKeyboardActions();
            handlePickupEnabling();
        }

        /// <summary>
        /// Manage drag/create lifecycle for wall template.
        /// </summary>
        void HandleTemplateWallDragging(Vector2 mousePos)
        {
            if(!mWallTemplate.mIsDragging && mWallTemplate.mTemplate.IsPointWithin(mousePos) && InputManager.isLeftMouseClicked())
            {
                mWallTemplate.BeginDrag(mousePos);
            }
            if(mWallTemplate.mIsDragging && !InputManager.isLeftMouseReleased())
            {
                mWallTemplate.Update(mousePos);
            }
            if(mWallTemplate.mIsDragging && InputManager.isLeftMouseReleased())
            {
                var final = mWallTemplate.EndDrag(pResetToOriginal: false);
                if (IsRectWithinPlayArea(mWallTemplate.mTemplate.mRectangle))
                {
                    var newWall = new RectWall(mPixelTexture, new Rectangle(mWallTemplate.mTemplate.mRectangle.X, mWallTemplate.mTemplate.mRectangle.Y, mWallTemplate.mTemplate.mRectangle.Width, mWallTemplate.mTemplate.mRectangle.Height));
                    mPreview.AddObject(newWall);
                }
                mWallTemplate.Reset();
            }
        }

        /// <summary>
        /// Manage drag/create lifecycle for tank template.
        /// </summary>
        void HandleTemplateTankDragging(Vector2 mousePos)
        {
            if (!mTankTemplate.mIsDragging && mTankTemplate.mTemplate.IsPointWithin(mousePos) && InputManager.isLeftMouseClicked())
            {
                mTankTemplate.BeginDrag(mousePos);
            }
            if (mTankTemplate.mIsDragging && !InputManager.isLeftMouseReleased())
            {
                mTankTemplate.Update(mousePos);
            }
            if (mTankTemplate.mIsDragging && InputManager.isLeftMouseReleased())
            {
                var final = mTankTemplate.EndDrag(pResetToOriginal: false);
                if (IsRectWithinPlayArea(mTankTemplate.mTemplate.mRectangle) && mPreview.GetTanks().Count < MaxTanks)
                {
                    var newWall = new Tank(mPixelTexture, new Rectangle(mTankTemplate.mTemplate.mRectangle.X, mTankTemplate.mTemplate.mRectangle.Y, mTankTemplate.mTemplate.mRectangle.Width, mTankTemplate.mTemplate.mRectangle.Height));
                    mPreview.AddObject(newWall);
                }
                mTankTemplate.Reset();
            }
        }

        /// <summary>
        /// Manage drag/create lifecycle for pickup template.
        /// </summary>
        void HandleTemplatePickupDragging(Vector2 mousePos)
        {
            if (!mPickupTemplate.mIsDragging && mPickupTemplate.mTemplate.IsPointWithin(mousePos) && InputManager.isLeftMouseClicked())
            {
                mPickupTemplate.BeginDrag(mousePos);
            }
            if (mPickupTemplate.mIsDragging && !InputManager.isLeftMouseReleased())
            {
                mPickupTemplate.Update(mousePos);
            }
            if (mPickupTemplate.mIsDragging && InputManager.isLeftMouseReleased())
            {
                var final = mPickupTemplate.EndDrag(pResetToOriginal: false);
                if (IsRectWithinPlayArea(mPickupTemplate.mTemplate.mRectangle))
                {
                    var newWall = new Pickup(mCircleTexture, new Rectangle(mPickupTemplate.mTemplate.mRectangle.X, mPickupTemplate.mTemplate.mRectangle.Y, mPickupTemplate.mTemplate.mRectangle.Width, mPickupTemplate.mTemplate.mRectangle.Height));
                    mPreview.AddObject(newWall);
                }
                mPickupTemplate.Reset();
            }
        }

        /// <summary>
        /// Handle selection/dragging of existing objects. Top-most priority: pickups -> tanks -> walls.
        /// </summary>
        void HandleExistingObjectInteraction(Vector2 mousePos)
        {
            bool handledClick = false;

            // pick-ups (top-most)
            HandleSelectionFor(mPreview.GetPickups(), ref handledClick, mousePos);

            // tanks
            if (!handledClick) HandleSelectionFor(mPreview.GetTanks(), ref handledClick, mousePos);

            // walls
            if (!handledClick) HandleSelectionFor(mPreview.GetWalls(), ref handledClick, mousePos);

            // On mouse release finalize move: if object outside play area revert
            if (mSelectedObject != null && mSelectedObject.GetIsSelected() && InputManager.isLeftMouseReleased())
            {
                if (mSelectedObject is RectWall rw && !IsWallWithinPlayArea(rw))
                {
                    mSelectedObject.SetRectangle(mSelectedObjectPreviousRect);
                }
                else if (mSelectedObject is Tank || mSelectedObject is Pickup)
                {
                    Rectangle r = mSelectedObject.mRectangle;
                    if (!IsRectWithinPlayArea(r))
                    {
                        mSelectedObject.SetRectangle(mSelectedObjectPreviousRect);
                    }
                }

                mSelectedObject.SetSelected(false);
                mSelectedObject = null;
            }
        }

        /// <summary>
        /// Generic selection/dragging logic for lists of SceneObject-derived types.
        /// </summary>
        void HandleSelectionFor<T>(List<T> list, ref bool handledClick, Vector2 mousePos) where T : SceneObject
        {
            foreach (T obj in list)
            {
                if (handledClick) break;

                if (obj.IsPointWithin(mousePos) && InputManager.isLeftMouseClicked())
                {
                    if (!obj.GetIsSelected())
                    {
                        DeselectAll();
                        obj.SetSelected(true);
                        mSelectedObject = obj;
                        mSelectedObjectPreviousRect = obj.mRectangle;
                        mSelectedDragOffset = new Vector2(mousePos.X - obj.mRectangle.X, mousePos.Y - obj.mRectangle.Y);
                    }
                    else
                    {
                        obj.SetSelected(false);
                        mSelectedObject = null;
                    }
                    handledClick = true;
                }

                if (obj.GetIsSelected() && !InputManager.isLeftMouseReleased())
                {
                    int newX = (int)(mousePos.X - mSelectedDragOffset.X);
                    int newY = (int)(mousePos.Y - mSelectedDragOffset.Y);
                    obj.UpdatePosition(newX, newY);
                }
            }
        }

        /// <summary>
        /// Handle keyboard interactions for the currently selected object (delete, rotate, scale).
        /// </summary>
        void HandleKeyboardActions()
        {
            if (mSelectedObject == null || !mSelectedObject.GetIsSelected()) return;

            // Delete
            if (InputManager.isKeyPressed(Keys.Delete))
            {
                mPreview.RemoveObject(mSelectedObject);
                mSelectedObject = null;
                return;
            }

            // Tank rotation
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
            }

            // Wall scaling
            if (mSelectedObject is RectWall selectedWall)
            {
                float rotationStep = MathHelper.ToRadians(15.0f);
                if (InputManager.isKeyPressed(Keys.Left))
                {
                    selectedWall.Rotate(rotationStep);
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

        /// <summary>
        /// Helper to check simple AABB inclusion for non-wall objects.
        /// </summary>
        bool IsRectWithinPlayArea(Rectangle r)
        {
            return r.Left >= mPlayArea.Left
                && r.Top >= mPlayArea.Top
                && r.Right <= mPlayArea.Right
                && r.Bottom <= mPlayArea.Bottom;
        }

        public bool IsWallWithinPlayArea(RectWall pWall)
        {
            Rectangle r = pWall.mRectangle;
            return r.Left >= mPlayArea.Left
                && r.Top >= mPlayArea.Top
                && r.Right <= mPlayArea.Right
                && r.Bottom <= mPlayArea.Bottom;
        }

        void SaveMap()
        {
            mPreview.SaveMap();
        }
    }
}



