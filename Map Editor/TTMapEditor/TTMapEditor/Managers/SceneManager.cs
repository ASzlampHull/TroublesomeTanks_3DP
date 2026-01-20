using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using TTMapEditor.Scenes;


namespace TTMapEditor.Managers
{
    internal class SceneManager
    {
        private List<IScene> mScenes = new List<IScene>();
        static SceneManager mInstance = new SceneManager();

        private SceneManager() { }

        public static SceneManager Instance
        {
            get { return mInstance; }
        }

        public void Push(IScene pScene)
        {
            mScenes.Add(pScene);
        }

        public void Transition(IScene pNextScene, bool pReplaceCurrent = true)
        {
            IScene currentScene = Top;

            if(pNextScene == null)
            {
                pNextScene = Previous;
            }
            if(pReplaceCurrent)
            {
                Pop();
            }
            if(pNextScene != null)
            {
                IScene transitionScene = new TransitionScene(currentScene, pNextScene);
                mScenes.Add(transitionScene);
            }
            else
            {
                //Todo add code to exit the application
            }
        }

        public void Pop()
        {
            if (mScenes.Count > 0)
            {
                mScenes.RemoveAt(mScenes.Count - 1);
            }
        }

        public IScene Top
        {
            get
            {
                if(mScenes.Count > 0)
                {
                    return mScenes.Last();
                }
                return null;
            }
        }

        public IScene Previous
        {
            get
            {
                if(mScenes.Count > 1)
                {
                    return mScenes[mScenes.Count - 2];
                }
                return null;
            }
        }

        public void update(float pSeconds)
        {
            try
            {
                if(mScenes.Count > 0)
                {
                    Top.Update(pSeconds);
                }
            }
            catch(System.Exception e)
            {
                mScenes.Clear();
                mScenes.Add(new ErrorMessageScene(e));
            }
        }

        public void Draw(float pSeconds)
        {
            if(mScenes.Count > 0)
            {
                Top.Draw(pSeconds);
            }
        }



    }
}
