using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTMapEditor.Scenes
{
    internal class ErrorMessageScene : IScene
    {
        
        public ErrorMessageScene(Exception pError)
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
