using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTMapEditor.Scenes
{
    internal class TransitionScene : IScene
    {
        public TransitionScene(IScene pPreviousScene, IScene pNextScene)
        {

        }
        
        public override void Draw(float pSeconds)
        {
            throw new NotImplementedException();
        }

        public override void Update(float pSeconds)
        {
            throw new NotImplementedException();
        }
    }
}
