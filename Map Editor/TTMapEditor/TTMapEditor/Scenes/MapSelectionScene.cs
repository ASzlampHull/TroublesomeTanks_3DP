using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TTMapEditor.Managers;

namespace TTMapEditor.Scenes
{
    internal class MapSelectionScene : IScene
    {

        GraphicsDevice mGraphicsDevice;
        IGame mGameInstance = TTMapEditor.Instance();
        MainMenuScene mStartScene;

        public MapSelectionScene(MainMenuScene pStartScene)
        {
            mGraphicsDevice = TTMapEditor.Instance().GetGraphicsDeviceManager().GraphicsDevice;
            mStartScene = pStartScene;
        }




        public override void Draw(float pSeconds)
        {
            mGraphicsDevice.Clear(Color.CornflowerBlue);
        }

        public override void Update(float pSeconds)
        {
            InputManager.Update();
            if(InputManager.isKeyPressed(Keys.Escape))
            {
                mGameInstance.GetSceneManager().Transition(mStartScene);
            }
        }
    }
}
