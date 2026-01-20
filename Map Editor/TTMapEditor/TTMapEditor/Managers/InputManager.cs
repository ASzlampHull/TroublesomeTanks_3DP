using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTMapEditor.Managers
{
    internal class InputManager
    {
        static InputManager mInstance = new InputManager();

        public static InputManager Instance
        {
            get { return mInstance; }
        }

    }
}
