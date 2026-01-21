using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTMapEditor.Managers;

namespace TTMapEditor.Scenes
{
    internal class MapEditingScene : IScene
    {

        GraphicsDevice mGraphicsDevice;
        IGame mGameInstance = TTMapEditor.Instance();
        MainMenuScene mStartScene;

        public MapEditingScene(MainMenuScene pStartScene)
        {
            mGraphicsDevice = TTMapEditor.Instance().GetGraphicsDeviceManager().GraphicsDevice;
            mStartScene = pStartScene;
        }

        public override void Draw(float pSeconds)
        {
            mGraphicsDevice.Clear(Color.Yellow);
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
