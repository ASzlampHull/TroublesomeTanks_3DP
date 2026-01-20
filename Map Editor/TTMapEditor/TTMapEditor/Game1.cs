using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TTMapEditor.Managers;

namespace TTMapEditor
{
    /// <summary>
    /// This interface specifics anything that we want to get global access to.
    /// </summary>
    internal interface IGame
    { 
        SceneManager GetSceneManager();

        ContentManager GetContentManager();

        InputManager GetInputManager();

        GraphicsDeviceManager GetGraphicsDeviceManager();
    }



    internal class TTMapEditor : Game, IGame
    {
        
        private SceneManager mSceneManager = SceneManager.Instance;
        private InputManager mInputManager = InputManager.Instance;

        private GraphicsDeviceManager mGraphics;
        private SpriteBatch mSpriteBatch;

        private static IGame mGameInterface = null;

        public static IGame Instance()
        {
            if(mGameInterface == null)
            {
                mGameInterface = new TTMapEditor();
            }
            return mGameInterface;
        }

        public SceneManager GetSceneManager()
        {
            return mSceneManager;
        }

        public ContentManager GetContentManager()
        {
            return Content;
        }

        public InputManager GetInputManager()
        {
            return mInputManager;
        }

        public GraphicsDeviceManager GetGraphicsDeviceManager()
        {
            return mGraphics;
        }

        public TTMapEditor()
        {
            mGraphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            
            mGraphics.PreferredBackBufferHeight = DGS.Instance.GetInt("SCREENHEIGHT");
            mGraphics.PreferredBackBufferWidth = DGS.Instance.GetInt("SCREENWIDTH");
            mGraphics.IsFullScreen = DGS.Instance.GetBool("IS_FULL_SCREEN");
            Window.Title = "TT Map Editor";

        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            mSpriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
