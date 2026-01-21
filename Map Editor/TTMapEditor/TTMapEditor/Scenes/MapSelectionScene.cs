using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TTMapEditor.Managers;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text.Json;

namespace TTMapEditor.Scenes
{
    internal class MapSelectionScene : IScene
    {

        GraphicsDevice mGraphicsDevice;
        IGame mGameInstance = TTMapEditor.Instance();
        private MainMenuScene mStartScene;
        private static readonly Texture2D mBackgroundTexture = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("background_01");
        private static readonly SpriteFont mSpriteFont = TTMapEditor.Instance().GetContentManager().Load<SpriteFont>("TitleFont");
        private Rectangle mBackgroundRectangle;
        private Vector2 mTitlePosition;
        private List<string> mMapFiles;
        private TTMapEditor mEditorInstance;
        private int mCurrentScrollPosition;

        private List<Texture2D> mThumbnailTextures = new List<Texture2D>();
        Rectangle mCurrentRectangle;
        Rectangle mPreviousRectangle;
        Rectangle mNextRectangle;
        int mThumbnailWidth;
        int mThumbnailHeight;

        public MapSelectionScene(MainMenuScene pStartScene)
        {
            mStartScene = pStartScene;
            mGameInstance = (TTMapEditor)TTMapEditor.Instance();
            mSpriteBatch = new SpriteBatch(mGameInstance.GetGraphicsDeviceManager().GraphicsDevice);
            mCurrentScrollPosition = 0;
            int screenWidth = mGameInstance.GetGraphicsDeviceManager().GraphicsDevice.Viewport.Width;
            int screenHeight = mGameInstance.GetGraphicsDeviceManager().GraphicsDevice.Viewport.Height;

            mBackgroundRectangle = new Rectangle(0, 0, screenWidth, screenHeight);
            mTitlePosition = new Vector2(screenWidth / 2, screenHeight / 5);
            mThumbnailWidth = screenWidth * 96 / 100 / 4;
            mThumbnailHeight = screenHeight * 73 / 100 / 4;

            string mapDirectory = Path.Combine(Environment.CurrentDirectory, "Maps");
            if (!Directory.Exists(mapDirectory))
            {
                Directory.CreateDirectory(mapDirectory);
            }
            
            string[] filePaths = Directory.GetFiles(mapDirectory, "*.json", SearchOption.AllDirectories);
            for (int i = 0; i < filePaths.Length; i++)
            {
                filePaths[i] = filePaths[i].Replace(mapDirectory + "\\", "");
            }
            mMapFiles = new List<string>(filePaths);

            foreach (string mapFile in mMapFiles)
            {
                string thumbnailFile = mapFile.Replace(".json", "_thumbnail.png");
                if (!File.Exists(thumbnailFile))
                {
                    MakeThumbnailTextureFromMapFile(mapFile);
                }
                else
                {
                    using (FileStream fileStream = new FileStream(thumbnailFile, FileMode.Open))
                    {
                        mThumbnailTextures.Add(Texture2D.FromStream(mGameInstance.GetGraphicsDeviceManager().GraphicsDevice, fileStream));
                    }
                }
            }

                mPreviousRectangle = new Rectangle((screenWidth / 2) - (mThumbnailWidth / 2) - mThumbnailWidth,
                    (screenHeight / 2) - (mThumbnailHeight / 2),
                    mThumbnailWidth,
                    mThumbnailHeight);

                mCurrentRectangle = new Rectangle((screenWidth / 2) - (mThumbnailWidth),
                    (screenHeight / 2) - (mThumbnailHeight),
                    mThumbnailWidth * 2,
                    mThumbnailHeight * 2);

                mNextRectangle = new Rectangle((screenWidth / 2) - (mThumbnailWidth / 2) + mThumbnailWidth,
                    (screenHeight / 2) - (mThumbnailHeight / 2),
                    mThumbnailWidth,
                    mThumbnailHeight);
            }

        private void selectMap(string pMapName)
        {
            mGameInstance.GetSceneManager().Transition(new MapEditingScene(mStartScene, pMapName, false), true);
        }

        public override void Draw(float pSeconds)
        {
            mGameInstance.GetGraphicsDeviceManager().GraphicsDevice.Clear(Color.Black);
            if (mMapFiles == null || mMapFiles.Count == 0)
            {
                // Safe fallback while map list is not yet populated
                string noMaps = "No maps found";
                mSpriteBatch.Begin();
                Vector2 pos = mTitlePosition - (mSpriteFont.MeasureString(noMaps) / 2);
                mSpriteBatch.DrawString(mSpriteFont, noMaps, pos, Color.White);
                mSpriteBatch.End();
                return;
            }

            mSpriteBatch.Begin();
            mSpriteBatch.Draw(mBackgroundTexture, mBackgroundRectangle, Color.White);
            string mapName = mMapFiles[mCurrentScrollPosition].Substring(0, mMapFiles[mCurrentScrollPosition].Length - 5);
            Vector2 titlePos = mTitlePosition - (mSpriteFont.MeasureString(mapName) / 2);
            //spriteBatch.DrawString(mSpriteFont, mapName, titlePos, Color.White, 0.0f, Vector2.One, 2.0f, SpriteEffects.None, 1.0f);
            mSpriteBatch.DrawString(mSpriteFont, mapName, titlePos, Color.White);
            // Calculate the indices of the previous, current, and next thumbnails
            int prevIndex = (mCurrentScrollPosition - 1 + mMapFiles.Count) % mMapFiles.Count;
            int nextIndex = (mCurrentScrollPosition + 1) % mMapFiles.Count;

            mSpriteBatch.Draw(mThumbnailTextures[prevIndex], mPreviousRectangle, Color.White);
            mSpriteBatch.Draw(mThumbnailTextures[nextIndex], mNextRectangle, Color.White);
            mSpriteBatch.Draw(mThumbnailTextures[mCurrentScrollPosition], mCurrentRectangle, Color.White);
            mSpriteBatch.End();
        }

        public override void Update(float pSeconds)
        {
            InputManager.Update();
            Escape();

            if (InputManager.isKeyPressed(Keys.Left))
            {
                mCurrentScrollPosition = (mCurrentScrollPosition - 1 + mMapFiles.Count) % mMapFiles.Count;
            }
            if (InputManager.isKeyPressed(Keys.Right))
            {
                mCurrentScrollPosition = (mCurrentScrollPosition + 1) % mMapFiles.Count;
            }
            if (InputManager.isKeyPressed(Keys.Enter))
            {
                selectMap(mMapFiles[mCurrentScrollPosition]);
            }
        }

        void MakeThumbnailTextureFromMapFile(string pMapFile)
        {
            pMapFile = "Maps\\" + pMapFile;
            string mapContent = File.ReadAllText(pMapFile);
            MapData mapData = JsonSerializer.Deserialize<MapData>(mapContent);

            int thumbnailWidth = mThumbnailWidth * 2;
            int thumbnailHeight = mThumbnailHeight * 2;
            RenderTarget2D renderTarget = new RenderTarget2D(mGameInstance.GetGraphicsDeviceManager().GraphicsDevice, thumbnailWidth, thumbnailHeight);

            mGameInstance.GetGraphicsDeviceManager().GraphicsDevice.SetRenderTarget(renderTarget);
            mGameInstance.GetGraphicsDeviceManager().GraphicsDevice.Clear(Color.Transparent);

            int screenWidth = TTMapEditor.Instance().GetGraphicsDeviceManager().GraphicsDevice.Viewport.Width;
            int screenHeight = TTMapEditor.Instance().GetGraphicsDeviceManager().GraphicsDevice.Viewport.Height;
            Rectangle playArea = new Rectangle(0, 0, thumbnailWidth, thumbnailHeight);

            mSpriteBatch.Begin();

            Rectangle outlineRect = new Rectangle(0, 0, thumbnailWidth, thumbnailHeight);
            mSpriteBatch.Draw(mGameInstance.GetContentManager().Load<Texture2D>("block"), outlineRect, Color.Black);

            //Todo change to use a colour from the DGS
            Rectangle innerRect = new Rectangle(2, 2, thumbnailWidth - 4, thumbnailHeight - 4);
            mSpriteBatch.Draw(mGameInstance.GetContentManager().Load<Texture2D>("block"), innerRect, Color.Wheat);

            // Draw outlines for walls
            foreach (var wall in mapData.Walls)
            {
                Vector2 pos = new Vector2(float.Parse(wall.Position[0]), float.Parse(wall.Position[1]));
                Vector2 size = new Vector2(float.Parse(wall.Size[0]), float.Parse(wall.Size[1]));
                Rectangle wallRect = GetRect(playArea, pos, size);
                DrawOutline(wallRect, wall.Texture);
            }

            // Draw outlines for tanks
            foreach (var tank in mapData.Tanks)
            {
                Rectangle tankRect = new Rectangle(
                    (int)(playArea.X + (playArea.Width * (float.Parse(tank.Position[0]) / 100))),
                    (int)(playArea.Y + (playArea.Height * (float.Parse(tank.Position[1]) / 100))),
                    10, 10
                );
                tankRect.X -= tankRect.Width / 2;
                tankRect.Y -= tankRect.Height / 2;
                DrawOutline(tankRect, "block");
            }

            // Draw outlines for pickups
            foreach (var pickup in mapData.Pickups)
            {
                Rectangle pickupRect = new Rectangle(
                    (int)(playArea.X + (playArea.Width * (float.Parse(pickup.Position[0]) / 100))),
                    (int)(playArea.Y + (playArea.Height * (float.Parse(pickup.Position[1]) / 100))),
                    10, 10
                );
                pickupRect.X -= pickupRect.Width / 2;
                pickupRect.Y -= pickupRect.Height / 2;
                DrawOutline(pickupRect, "circle");
            }

            mSpriteBatch.End();

            mSpriteBatch.Begin();

            // Draw walls
            foreach (var wall in mapData.Walls)
            {
                Vector2 pos = new Vector2(float.Parse(wall.Position[0]), float.Parse(wall.Position[1]));
                Vector2 size = new Vector2(float.Parse(wall.Size[0]), float.Parse(wall.Size[1]));
                Rectangle wallRect = GetRect(playArea, pos, size);
                //Todo change to use a colour from the DGS
                mSpriteBatch.Draw(mGameInstance.GetContentManager().Load<Texture2D>(wall.Texture), wallRect, Color.DarkGray);
            }

            // Draw tanks
            foreach (var tank in mapData.Tanks)
            {
                Rectangle tankRect = new Rectangle(
                    (int)(playArea.X + (playArea.Width * (float.Parse(tank.Position[0]) / 100))),
                    (int)(playArea.Y + (playArea.Height * (float.Parse(tank.Position[1]) / 100))),
                    9, 9
                );
                tankRect.X -= tankRect.Width / 2;
                tankRect.Y -= tankRect.Height / 2;
                mSpriteBatch.Draw(mGameInstance.GetContentManager().Load<Texture2D>("block"), tankRect, Color.Blue);
            }

            // Draw pickups
            foreach (var pickup in mapData.Pickups)
            {
                Rectangle pickupRect = new Rectangle(
                    (int)(playArea.X + (playArea.Width * (float.Parse(pickup.Position[0]) / 100))),
                    (int)(playArea.Y + (playArea.Height * (float.Parse(pickup.Position[1]) / 100))),
                    9, 9
                );
                pickupRect.X -= pickupRect.Width / 2;
                pickupRect.Y -= pickupRect.Height / 2;
                mSpriteBatch.Draw(mGameInstance.GetContentManager().Load<Texture2D>("circle"), pickupRect, Color.Red);
            }

            mSpriteBatch.End();

            mGameInstance.GetGraphicsDeviceManager().GraphicsDevice.SetRenderTarget(null);

            Texture2D thumbnailTexture = renderTarget;

            using (FileStream stream = new FileStream(pMapFile.Replace(".json", "_thumbnail.png"), FileMode.Create))
            {
                thumbnailTexture.SaveAsPng(stream, thumbnailWidth, thumbnailHeight);
            }

            mThumbnailTextures.Add(thumbnailTexture);

        }

        Rectangle GetRect(Rectangle pPlayArea, Vector2 pPos, Vector2 pSize)
        {
            return new Rectangle((int)(pPlayArea.X + (pPlayArea.Width * (pPos.X / 100.0))),
                                 (int)(pPlayArea.Y + (pPlayArea.Height * (pPos.Y / 100.0))),
                                 (int)(pPlayArea.Width * (pSize.X / 100.0)),
                                 (int)(pPlayArea.Height * (pSize.Y / 100.0)));
        }

        void DrawOutline(Rectangle pRect, string pTextureName)
        {
            int offset = 2;
            Texture2D texture = mGameInstance.GetContentManager().Load<Texture2D>(pTextureName);
            mSpriteBatch.Draw(texture, new Rectangle(pRect.X - offset, pRect.Y - offset, pRect.Width + (2 * offset), pRect.Height + (offset * 2)), Color.Black);
        }

        public override void Escape()
        {
            if (InputManager.isKeyPressed(Keys.Escape))
            {
                mGameInstance.GetSceneManager().Transition(mStartScene);
            }
        }
    }
    
 }
