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
using System.Runtime.InteropServices;
using TTMapEditor.Saving;


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
        static readonly Color BACKGROUND_COLOUR = DGS.Instance.GetColour("COLOUR_BACKGROUND");
        Rectangle mBackgroundRectangle;
        Rectangle mPlayArea;
        Rectangle mPlayAreaOutline;
        RectWall mWall;
        MapPreview mPreview;
        bool mIsNewMap;
        bool mFileNameEntered = false;

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

        private FileNamer mFileNamer;


        // Button for saving map
        Rectangle mSaveButtonRect;

        // max tanks allowed
        const int MaxTanks = 4;

        // Use the requested folder as the maps root. Change this path if you move the maps directory.
        private static readonly string MAP_ROOT = DGS.Instance.GetString("MAP_FILE_PATH");

        public MapEditingScene(MainMenuScene pStartScene, string pMapFile, bool pIsNewMap)
        {
            mGraphicsDevice = TTMapEditor.Instance().GetGraphicsDeviceManager().GraphicsDevice;
            mStartScene = pStartScene;
            mName = pMapFile;
            mIsNewMap = pIsNewMap;
            int viewPortWidth = mGraphicsDevice.Viewport.Width;
            int viewPortHeight = mGraphicsDevice.Viewport.Height;
            mFileNamer = new FileNamer();

            // If this is a new map request, create an initial empty MapData file so MapPreview can load it.
            if (mIsNewMap)
            {
                HandleNewMapCreation(pMapFile);
                mFileNameEntered = false;
            }
            else
            {
                // Resolve the incoming path relative to the configured maps root when appropriate.
                string resolved = ResolveMapPath(pMapFile);
                mPreview = new MapPreview(pFilePath: resolved);
                mFileNameEntered = true;
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

        /// <summary>
        /// Resolve a supplied map path into an absolute file path using sMapsRoot when the incoming value is a name or relative path.
        /// If the provided value is already rooted it will be normalized and returned.
        /// </summary>
        string ResolveMapPath(string pMapFile)
        {
            if (string.IsNullOrWhiteSpace(pMapFile))
            {
                // default to a map.json inside the maps root
                Directory.CreateDirectory(MAP_ROOT);
                return Path.GetFullPath(Path.Combine(MAP_ROOT, "map.json"));
            }

            // If absolute path was provided, normalize and return (if it's a directory, return map.json inside it)
            if (Path.IsPathRooted(pMapFile))
            {
                if (Directory.Exists(pMapFile) || !Path.HasExtension(pMapFile))
                {
                    Directory.CreateDirectory(pMapFile);
                    return Path.GetFullPath(Path.Combine(pMapFile, "map.json"));
                }
                return Path.GetFullPath(pMapFile);
            }

            // Remove leading "Maps\" if present in the supplied value to avoid double "Maps\Maps\..."
            string relative = pMapFile;
            string mapsPrefix1 = "Maps" + Path.DirectorySeparatorChar;
            string mapsPrefix2 = "Maps" + Path.AltDirectorySeparatorChar;
            if (relative.StartsWith(mapsPrefix1) || relative.StartsWith(mapsPrefix2))
            {
                relative = relative.Substring(5);
            }

            // Combine with configured maps root
            string candidate = Path.Combine(MAP_ROOT, relative);

            // If caller supplied just a name (no extension) or a folder, create folder + "map.json"
            if (Directory.Exists(candidate) || !Path.HasExtension(candidate))
            {
                Directory.CreateDirectory(candidate);
                return Path.GetFullPath(Path.Combine(candidate, "map.json"));
            }
            else
            {
                // candidate is a file path
                string? dir = Path.GetDirectoryName(candidate);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                return Path.GetFullPath(candidate);
            }
        }

        void HandleNewMapCreation(string pMapFile)
        {
            // Use the configured maps root directory as a fallback only
            string mapsRoot = MAP_ROOT;

            // Normalize incoming path (strip leading "Maps\" if present)
            string relative = pMapFile ?? string.Empty;
            string mapsPrefix1 = "Maps" + Path.DirectorySeparatorChar;
            string mapsPrefix2 = "Maps" + Path.AltDirectorySeparatorChar;
            if (relative.StartsWith(mapsPrefix1) || relative.StartsWith(mapsPrefix2))
            {
                relative = relative.Substring(5);
            }

            // If caller provided an absolute path or a path that contains separators,
            // treat it as an explicit path. Only prepend mapsRoot for simple names.
            string candidate;
            if (string.IsNullOrWhiteSpace(relative))
            {
                candidate = mapsRoot;
            }
            else if (Path.IsPathRooted(relative)
                     || relative.Contains(Path.DirectorySeparatorChar)
                     || relative.Contains(Path.AltDirectorySeparatorChar))
            {
                candidate = relative;
            }
            else
            {
                candidate = Path.Combine(mapsRoot, relative);
            }

            string targetPath;
            if (Directory.Exists(candidate) || !Path.HasExtension(candidate))
            {
                // Do NOT create the directory here — just resolve the target path
                targetPath = Path.Combine(candidate, "map.json");
            }
            else
            {
                // candidate is a file path; do not create parent here
                targetPath = candidate;
            }

            // DO NOT create directories here. Initialize the preview with the resolved path.
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
            mFileNamer.Draw(mSpriteBatch);
            mSpriteBatch.End();
        }

        void DrawBackgroundAndTitle()
        {
            mSpriteBatch.Draw(mBackgroundTexture, mBackgroundRectangle, Color.White);

            // Show only the final path segment (file or folder name).
            string displayName = mName ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(displayName))
            {
                // Trim any trailing separators then get the last segment.
                displayName = Path.GetFileName(displayName.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

                // If Path.GetFileName returned empty (e.g. input was a root or ended with a separator),
                // fall back to DirectoryInfo.Name to try to get the last folder name.
                if (string.IsNullOrEmpty(displayName))
                {
                    try
                    {
                        displayName = new DirectoryInfo(mName).Name;
                    }
                    catch
                    {
                        // keep original if DirectoryInfo fails
                        displayName = mName;
                    }
                }
            }

            mSpriteBatch.DrawString(mTitleFont, displayName, new Vector2(100, 100), Color.Black);
        }

        void DrawPlayAreaAndObjects()
        {
            mSpriteBatch.Draw(mPixelTexture, mPlayAreaOutline, Color.Black);
            mSpriteBatch.Draw(mPixelTexture, mPlayArea, BACKGROUND_COLOUR);

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
            if (mSelectedObject is Pickup)
            {
                if (InputManager.isKeyPressed(Keys.D1))
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
            mFileNamer.Update(pSeconds);

            if (InputManager.isKeyPressed(Keys.Escape))
            {
                mGameInstance.GetSceneManager().Transition(mStartScene);
                return;
            }

            if (mFileNamer.IsActive() && InputManager.isKeyPressed(Keys.Enter))
            {
                mName = mFileNamer.ReturnName();
                SaveMap();
                mFileNameEntered = true;
            }

            Vector2 mousePos = InputManager.GetMousePosition();

            if (mSaveButtonRect.Contains(mousePos) && InputManager.isLeftMouseClicked())
            {
                if (!mFileNameEntered)
                {
                    mFileNamer.StartTyping();
                }
                else
                {
                    SaveMap();
                }
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
            if (!mWallTemplate.mIsDragging && mWallTemplate.mTemplate.IsPointWithin(mousePos) && InputManager.isLeftMouseClicked())
            {
                mWallTemplate.BeginDrag(mousePos);
            }
            if (mWallTemplate.mIsDragging && !InputManager.isLeftMouseReleased())
            {
                mWallTemplate.Update(mousePos);
            }
            if (mWallTemplate.mIsDragging && InputManager.isLeftMouseReleased())
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

                    // Store previous position before updating
                    Rectangle previousRect = obj.mRectangle;

                    obj.UpdatePosition(newX, newY);

                    // Check if the new position is valid
                    bool isValid = true;
                    if (obj is RectWall wall)
                    {
                        isValid = IsWallWithinPlayArea(wall);
                    }
                    else
                    {
                        isValid = IsRectWithinPlayArea(obj.mRectangle);
                    }

                    // If invalid, revert to previous position
                    if (!isValid)
                    {
                        obj.SetRectangle(previousRect);
                    }
                    else
                    {
                        // Update the stored previous rect for successful moves
                        mSelectedObjectPreviousRect = obj.mRectangle;
                    }
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
                if(InputManager.isKeyPressed(Keys.LeftControl) || InputManager.isKeyPressed(Keys.RightControl))
                {
                    selectedWall.SwitchRotationScaling();
                }
                switch (selectedWall.GetIsRotating())
                {
                    case true:
                        float rotationStep = MathHelper.ToRadians(15.0f);
                        // Store current state before attempting rotation
                        float previousRotation = selectedWall.mRotation;
                        Vector2 previousPositon = selectedWall.mRectangle.Location.ToVector2();

                        if (InputManager.isKeyPressed(Keys.Left))
                        {
                            selectedWall.Rotate(rotationStep);
                            if (!IsWallWithinPlayArea(selectedWall))
                            {
                                // Revert rotation
                                selectedWall.mRotation = previousRotation;
                            }
                            else
                            {
                                mSelectedObjectPreviousRect = selectedWall.mRectangle;
                            }
                        }
                        if (InputManager.isKeyPressed(Keys.Right))
                        {
                            selectedWall.Rotate(-rotationStep);
                            if (!IsWallWithinPlayArea(selectedWall))
                            {
                                // Revert rotation
                                selectedWall.mRotation = previousRotation;
                            }
                            else
                            {
                                mSelectedObjectPreviousRect = selectedWall.mRectangle;
                            }
                        }
                        break;
                    case false:
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
                        break;
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

        /// <summary>
        /// Rotates the wall's corners and checks if any are outside the play area.
        /// </summary>
        /// <param name="pWall"></param>
        /// <returns>
        /// True if all corners of wall within play area, false if any corner outside.
        /// </returns>
        public bool IsWallWithinPlayArea(RectWall pWall)
        {
            Vector2 center = new Vector2(pWall.mRectangle.Center.X,pWall.mRectangle.Center.Y);

            Vector2[] corners = new Vector2[4]
            {
                new Vector2(-pWall.mRectangle.Width / 2f, -pWall.mRectangle.Height / 2f),
                new Vector2(pWall.mRectangle.Width / 2f, -pWall.mRectangle.Height / 2f),
                new Vector2(pWall.mRectangle.Width / 2f, pWall.mRectangle.Height / 2f),
                new Vector2(-pWall.mRectangle.Width / 2f, pWall.mRectangle.Height / 2f)
            };

            float cos = MathF.Cos(pWall.mRotation);
            float sin = MathF.Sin(pWall.mRotation);

            for(int i = 0; i < corners.Length; i++)
            {
                // Rotate corner
                float rotatedX = corners[i].X * cos - corners[i].Y * sin;
                float rotatedY = corners[i].X * sin + corners[i].Y * cos;
                // Translate back to world position
                Vector2 worldPos = new Vector2(center.X + rotatedX, center.Y + rotatedY);
                if (worldPos.X < mPlayArea.Left || worldPos.X > mPlayArea.Right || worldPos.Y < mPlayArea.Top || worldPos.Y > mPlayArea.Bottom)
                {
                    return false;
                }
            }

            return true;
        }

        void SaveMap()
        {
            mPreview.SaveMap(mName);
        }
    }
}



