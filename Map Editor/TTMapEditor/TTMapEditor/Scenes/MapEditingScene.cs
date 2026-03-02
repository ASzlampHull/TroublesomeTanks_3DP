using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
using TTMapEditor.Managers;
using TTMapEditor.Maps;
using TTMapEditor.Objects;
using TTMapEditor.Saving;

namespace TTMapEditor.Scenes
{
    /// <summary>
    /// Scene used to edit maps: place walls, tanks and pickups.
    /// Responsibilities:
    /// - Render preview and UI.
    /// - Handle template dragging and creation of new objects.
    /// - Handle selection, dragging and keyboard actions for existing objects.
    /// - Validate map rules (e.g. exact tank count) before saving.
    /// </summary>
    public class MapEditingScene : IScene
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
        MapPreview mPreview;
        bool mIsNewMap;
        bool mFileNameEntered = false;
        bool notEnoughTanks = false;
        float popUpTimer = 0f;
        float timeToShowPopUp = 2f;
        SelectionManager mSelectionManager;
        private FileNamer mFileNamer;
        Rectangle mSaveButtonRect;
        const int MaxTanks = 4;
        private static readonly string MAP_ROOT = DGS.Instance.GetString("MAP_FILE_PATH");
        private MapBoundaryValidator mBoundaryValidator;
        private EditorKeyboardController mKeyboardController;
        private TemplatePalette mTemplatePalette;
        private MapEditingMapService mMapService;

        /// <summary>
        /// Creates a new map editing scene for a given map file.
        /// </summary>
        /// <param name="pStartScene">Scene to return to when exiting the editor.</param>
        /// <param name="pMapFile">Path or name of the map file to edit or create.</param>
        /// <param name="pIsNewMap">True if this should start with a new empty map, false to load an existing one.</param>
        public MapEditingScene(MainMenuScene pStartScene, string pMapFile, bool pIsNewMap)
        {
            mGraphicsDevice = TTMapEditor.Instance().GetGraphicsDeviceManager().GraphicsDevice;
            mStartScene = pStartScene;
            mName = pMapFile;
            mIsNewMap = pIsNewMap;

            int viewPortWidth = mGraphicsDevice.Viewport.Width;
            int viewPortHeight = mGraphicsDevice.Viewport.Height;

            mFileNamer = new FileNamer();
            mMapService = new MapEditingMapService(MAP_ROOT);

            // For new maps, create an empty MapData file so the preview can be initialised.
            // For existing maps, load the current state from disk.
            if (mIsNewMap)
            {
                mPreview = mMapService.CreatePreviewForNewMap(pMapFile);
                mFileNameEntered = false;
            }
            else
            {
                mPreview = mMapService.CreatePreviewForExistingMap(pMapFile);
                mFileNameEntered = true;
            }

            // Set up play area rectangles and render helpers.
            mPlayArea = mPreview.GetPlayArea();
            mPlayAreaOutline = new Rectangle(mPlayArea.X - 5, mPlayArea.Y - 5, mPlayArea.Width + 10, mPlayArea.Height + 10);
            mSpriteBatch = new SpriteBatch(mGraphicsDevice);
            mBackgroundRectangle = new Rectangle(0, 0, viewPortWidth, viewPortHeight);
            mBoundaryValidator = new MapBoundaryValidator(mPlayArea);

            // Set up input and interaction helpers.
            mSelectionManager = new SelectionManager(mPreview, mBoundaryValidator);
            mKeyboardController = new EditorKeyboardController(mPreview, mSelectionManager, mBoundaryValidator);
            mTemplatePalette = new TemplatePalette(mTitleFont, mPixelTexture, mCircleTexture, mPreview, mBoundaryValidator, viewPortWidth, MaxTanks);

            // Position the save button near the top-left, with padding based on font size.
            int saveButtonWidth = (int)(mTitleFont.MeasureString("Save").X + 20);
            int saveButtonHeight = (int)(mTitleFont.MeasureString("Save").Y + 10);
            mSaveButtonRect = new Rectangle(viewPortWidth - viewPortWidth + viewPortWidth / 16, 5, saveButtonWidth, saveButtonHeight);
        }

        /// <summary>
        /// Renders the entire editor scene: background, play area, objects, templates and UI.
        /// </summary>
        /// <param name="pSeconds">Elapsed time in seconds since the last frame.</param>
        public override void Draw(float pSeconds)
        {
            mGraphicsDevice.Clear(Color.CornflowerBlue);
            mSpriteBatch.Begin();

            DrawBackgroundAndTitle();
            DrawPlayAreaAndObjects();
            mTemplatePalette.Draw(mSpriteBatch);
            DrawSaveButton();
            mFileNamer.Draw(mSpriteBatch);

            // Draw the "not enough tanks" popup while it is active and within display duration.
            if (notEnoughTanks)
            {
                if (popUpTimer < timeToShowPopUp)
                {
                    popUpTextBox($"You must have exactly {MaxTanks} tanks to save the map.");
                }
                else
                {
                    notEnoughTanks = false;
                }
            }

            mSpriteBatch.End();
        }

        /// <summary>
        /// Draws the editor background and the current map name (or UNNAMED if not yet set).
        /// </summary>
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

            mSpriteBatch.DrawString(mTitleFont, mFileNameEntered ? displayName : "UNNAMED", new Vector2(100, 100), Color.Black);
        }

        /// <summary>
        /// Draws the play area border and all placed objects (walls, tanks and pickups).
        /// </summary>
        void DrawPlayAreaAndObjects()
        {
            // Draw play area border and fill.
            mSpriteBatch.Draw(mPixelTexture, mPlayAreaOutline, Color.Black);
            mSpriteBatch.Draw(mPixelTexture, mPlayArea, BACKGROUND_COLOUR);

            // Draw walls.
            foreach (RectWall wall in mPreview.GetWalls())
            {
                wall.DrawOutline(mSpriteBatch);
            }
            foreach (RectWall wall in mPreview.GetWalls())
            {
                wall.Draw(mSpriteBatch);
            }

            // Draw tanks.
            foreach (Tank tank in mPreview.GetTanks())
            {
                tank.DrawOutline(mSpriteBatch);
                tank.Draw(mSpriteBatch);
            }

            // Draw pickups.
            foreach (Pickup pickup in mPreview.GetPickups())
            {
                pickup.DrawOutline(mSpriteBatch);
                pickup.Draw(mSpriteBatch);
            }
        }

        /// <summary>
        /// Draws the clickable save button and its label, with hover feedback.
        /// </summary>
        void DrawSaveButton()
        {
            // Highlight the button when the mouse is over it.
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

            // Center the "Save" label inside the button rectangle.
            mSpriteBatch.DrawString(
                mTitleFont,
                "Save",
                new Vector2(
                    mSaveButtonRect.X + (mSaveButtonRect.Width - saveLabelWidth) / 2,
                    mSaveButtonRect.Y + (mSaveButtonRect.Height - saveLabelHeight) / 2),
                Color.Black);
        }

        /// <summary>
        /// Updates input handling, object interaction, keyboard shortcuts and popup timers.
        /// </summary>
        /// <param name="pSeconds">Elapsed time in seconds since the last frame.</param>
        public override void Update(float pSeconds)
        {
            InputManager.Update();
            mFileNamer.Update(pSeconds);
            popUpTimer += pSeconds;

            // Escape returns to the start scene without saving.
            if (InputManager.isKeyPressed(Keys.Escape))
            {
                mGameInstance.GetSceneManager().Transition(mStartScene);
                return;
            }

            // Handle confirming a file name when the FileNamer is active.
            if (mFileNamer.IsActive() && InputManager.isKeyPressed(Keys.Enter))
            {
                // Enforce exactly MaxTanks before allowing save.
                if (mPreview.GetTanks().Count != MaxTanks)
                {
                    notEnoughTanks = true;
                    popUpTimer = 0f;
                    mFileNamer.ReturnName();
                    return;
                }
                else
                {
                    mName = mFileNamer.ReturnName();
                    SaveMap();
                    mFileNameEntered = true;
                }
            }

            Vector2 mousePos = InputManager.GetMousePosition();

            // Clicking the save button either opens the FileNamer or saves immediately.
            if (mSaveButtonRect.Contains(mousePos) && InputManager.isLeftMouseClicked())
            {
                if (!mFileNameEntered)
                {
                    // First time saving: ask user for a file name.
                    mFileNamer.StartTyping();
                }
                else
                {
                    // File name already known: save directly.
                    SaveMap();
                }
            }

            // Update template palette and, if nothing is being dragged from it,
            // allow interaction with existing objects in the map.
            mTemplatePalette.Update(mousePos);
            if (!mTemplatePalette.IsDraggingAny)
            {
                mSelectionManager.HandleInteraction(mousePos);
            }

            // Apply keyboard shortcuts (delete, move, rotate, etc.).
            mKeyboardController.Update();
        }

        /// <summary>
        /// Validates the map and saves it to disk if valid. Shows a popup if validation fails.
        /// </summary>
        void SaveMap()
        {
            // Validation: the map must contain exactly MaxTanks tanks.
            if (mPreview.GetTanks().Count != MaxTanks)
            {
                notEnoughTanks = true;
                popUpTimer = 0f;
                return;
            }
            else
            {
                // Normalise the file name and save as JSON under the map root.
                string baseName = Path.GetFileNameWithoutExtension(mName);
                string fileName = baseName + ".json";
                string fullPath = Path.Combine(MAP_ROOT, fileName);
                mMapService.SaveMap(mPreview, fullPath);
            }
        }

        /// <summary>
        /// Draws a simple centered popup text box with the given message.
        /// Intended to be called from <see cref="Draw(float)"/> while a timer controls visibility.
        /// </summary>
        /// <param name="pMessage">Message to display to the user.</param>
        void popUpTextBox(string pMessage)
        {
            int height = (int)mTitleFont.MeasureString(pMessage).Y;
            int width = (int)mTitleFont.MeasureString(pMessage).X;

            int viewPortWidth = mGraphicsDevice.Viewport.Width;
            int viewPortHeight = mGraphicsDevice.Viewport.Height;

            int popupX = (viewPortWidth - width) / 2;
            int popupY = (viewPortHeight - height) / 2;

            // Draw white background rectangle then the red message text.
            mSpriteBatch.Draw(mPixelTexture, new Rectangle(popupX, popupY, width, height), Color.White);
            mSpriteBatch.DrawString(mTitleFont, pMessage, new Vector2(popupX, popupY), Color.Red);
        }
    }
}