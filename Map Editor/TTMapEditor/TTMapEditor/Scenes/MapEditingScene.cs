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
    /// Scene used to edit maps: place walls, tanks and pickups
    /// Responsibilities:
    /// - Render preview and UI
    /// - Handle template dragging(create new objects
    /// - Handle selection, dragging and keyboard actions for exisiting objects
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


        // Button for saving map
        Rectangle mSaveButtonRect;

        // max tanks allowed
        const int MaxTanks = 4;

        // Use the requested folder as the maps root. Change this path if you move the maps directory.
        private static readonly string MAP_ROOT = DGS.Instance.GetString("MAP_FILE_PATH");

        private MapBoundaryValidator mBoundaryValidator;
        private EditorKeyboardController mKeyboardController;
        private TemplatePalette mTemplatePalette;
        private MapEditingMapService mMapService;

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

            // If this is a new map request, create an initial empty MapData file so MapPreview can load it.
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

            mPlayArea = mPreview.GetPlayArea();
            mPlayAreaOutline = new Rectangle(mPlayArea.X - 5, mPlayArea.Y - 5, mPlayArea.Width + 10, mPlayArea.Height + 10);
            mSpriteBatch = new SpriteBatch(mGraphicsDevice);
            mBackgroundRectangle = new Rectangle(0, 0, viewPortWidth, viewPortHeight);
            mBoundaryValidator = new MapBoundaryValidator(mPlayArea);

            mSelectionManager = new SelectionManager(mPreview, mBoundaryValidator);
            mKeyboardController = new EditorKeyboardController(mPreview, mSelectionManager, mBoundaryValidator);
            mTemplatePalette = new TemplatePalette(mTitleFont, mPixelTexture, mCircleTexture, mPreview, mBoundaryValidator, viewPortWidth, MaxTanks);


            int saveButtonWidth = (int)(mTitleFont.MeasureString("Save").X + 20);
            int saveButtonHeight = (int)(mTitleFont.MeasureString("Save").Y + 10);
            mSaveButtonRect = new Rectangle(viewPortWidth - viewPortWidth + viewPortWidth / 16, 5, saveButtonWidth, saveButtonHeight);
        }

        public override void Draw(float pSeconds)
        {
            mGraphicsDevice.Clear(Color.CornflowerBlue);
            mSpriteBatch.Begin();
            DrawBackgroundAndTitle();
            DrawPlayAreaAndObjects();
            mTemplatePalette.Draw(mSpriteBatch);
            DrawSaveButton();
            mFileNamer.Draw(mSpriteBatch);
            if(notEnoughTanks)
            {
                if(popUpTimer < timeToShowPopUp)
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
            popUpTimer += pSeconds;

            if (InputManager.isKeyPressed(Keys.Escape))
            {
                mGameInstance.GetSceneManager().Transition(mStartScene);
                return;
            }

            if (mFileNamer.IsActive() && InputManager.isKeyPressed(Keys.Enter))
            {
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

            mTemplatePalette.Update(mousePos);
            // If any template is being dragged, skip interacting with existing objects.
            if (!mTemplatePalette.IsDraggingAny)
            {
                mSelectionManager.HandleInteraction(mousePos);
            }

            mKeyboardController.Update();
        }

        // Validates the map and saves it if valid. If not valid, shows a pop up message for a few seconds.
        void SaveMap()
        {
            if (mPreview.GetTanks().Count != MaxTanks)
            {
                notEnoughTanks = true;
                popUpTimer = 0f;
                return;
            }
            else
            {
                string baseName = Path.GetFileNameWithoutExtension(mName);
                string fileName = baseName + ".json";
                string fullPath = Path.Combine(MAP_ROOT, fileName);
                mMapService.SaveMap(mPreview, fullPath);
            }
        }

        //Draws a text box in the middle of the screen with the given message. Needs to be placed in the draw method and requires a timer to control how long it is shown for.
        void popUpTextBox(string pMessage)
        {
            int height = (int)mTitleFont.MeasureString(pMessage).Y;
            int width = (int)mTitleFont.MeasureString(pMessage).X;
            int viewPortWidth = mGraphicsDevice.Viewport.Width;
            int viewPortHeight = mGraphicsDevice.Viewport.Height;
            int popupX = (viewPortWidth - width) / 2;
            int popupY = (viewPortHeight - height) / 2;
            mSpriteBatch.Draw(mPixelTexture, new Rectangle(popupX, popupY, width, height), Color.White);
            mSpriteBatch.DrawString(mTitleFont, pMessage, new Vector2(popupX, popupY), Color.Red);
        }

    }
}



