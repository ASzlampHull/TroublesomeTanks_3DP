using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using TTMapEditor.Managers;

namespace TTMapEditor.Saving
{
    public class FolderPicker
    {

        readonly SpriteBatch mSpriteBatch;
        readonly Texture2D mPixel;
        readonly SpriteFont mFont;
        string mCurrentPath;
        bool mTypingNewName = true;
        string mNewFolderName = "";
        Rectangle mOverlay;
        Rectangle mButtonNew;
        Rectangle mButtonCancel;
        Rectangle mButtonSelect;
        Rectangle mButtonBack;
        float mTypingTimer = 0f;
        float mTypingDelay = 0.1f;


        public FolderPicker(SpriteBatch pSpriteBatch, Texture2D pPixel, SpriteFont pFont, string pStartingPath)
        {
            mSpriteBatch = pSpriteBatch;
            mPixel = pPixel;
            mFont = pFont;
            mCurrentPath = pStartingPath;
            mOverlay = new Rectangle(0, 0, 800, 600);
            mButtonNew = new Rectangle(10, 550, 120, 30);
            mButtonCancel = new Rectangle(140, 550, 120, 30);
            mButtonSelect = new Rectangle(270, 550, 120, 30);
            mButtonBack = new Rectangle(400, 550, 120, 30);
            mFont = TTMapEditor.Instance().GetContentManager().Load<SpriteFont>("FolderPickerText");
        }

        public bool createNewFolder()
        {
            return false;
        }

        public void Update(float pDeltaTime)
        {
            mTypingTimer += pDeltaTime;
            if (mTypingNewName)
            {
                foreach(Keys k in Enum.GetValues(typeof(Keys)))
                {
                    if (InputManager.isKeyDown(k) && mTypingTimer >= mTypingDelay)
                    {
                        if(k == Keys.Space)
                        {
                            mNewFolderName += " ";
                            mTypingTimer = 0f;
                            continue;
                        }
                        if(k == Keys.Back && mNewFolderName.Length > 0)
                        {
                            mNewFolderName = mNewFolderName.Substring(0, mNewFolderName.Length - 1);
                            mTypingTimer = 0f;
                            continue;
                        }
                        if(k == Keys.Enter)
                        {
                            mTypingNewName = false;
                            createNewFolder();
                            mTypingTimer = 0f;
                            continue;
                        }
                        string s = k.ToString();
                        if (s.Length == 1)
                        {
                            mNewFolderName += s;
                            if(mFont.MeasureString(mNewFolderName).X > 780)
                            {
                                mNewFolderName = mNewFolderName.Substring(0, mNewFolderName.Length - 1);
                                mNewFolderName += "\n";
                                mNewFolderName += s;
                            }
                        }
                        mTypingTimer = 0f;
                    }
                }
            }
            MouseWithinButton(ref mButtonNew);
            MouseWithinButton(ref mButtonCancel);
            MouseWithinButton(ref mButtonSelect);
            MouseWithinButton(ref mButtonBack);
        }

        public void Draw()
        {
            mSpriteBatch.Begin();
            mSpriteBatch.Draw(mPixel, mOverlay, Color.Black);
            mSpriteBatch.Draw(mPixel, mButtonNew, Color.Gray);
            mSpriteBatch.DrawString(mFont, "New Folder", new Vector2(mButtonNew.X + 10, mButtonNew.Y + 5), Color.White);
            mSpriteBatch.Draw(mPixel, mButtonCancel, Color.Gray);
            mSpriteBatch.DrawString(mFont, "Cancel", new Vector2(mButtonCancel.X + 10, mButtonCancel.Y + 5), Color.White);
            mSpriteBatch.Draw(mPixel, mButtonSelect, Color.Gray);
            mSpriteBatch.DrawString(mFont, "Select", new Vector2(mButtonSelect.X + 10, mButtonSelect.Y + 5), Color.White);
            mSpriteBatch.Draw(mPixel, mButtonBack, Color.Gray);
            mSpriteBatch.DrawString(mFont, "Back", new Vector2(mButtonBack.X + 10, mButtonBack.Y + 5), Color.White);
            mSpriteBatch.DrawString(mFont, "Current Path: " + mCurrentPath, new Vector2(10, 10), Color.White);
            mSpriteBatch.DrawString(mFont, mNewFolderName, new Vector2(10, 30), Color.White);
            mSpriteBatch.End();
        }

        public void MouseWithinButton(ref Rectangle pButton)
        {
            Point mousePos = Mouse.GetState().Position;
            if(pButton.Contains(mousePos))
            {
                pButton.Inflate(2, 2);
            }
        }


    }
}
