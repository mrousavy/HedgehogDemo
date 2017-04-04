using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HedgehogClient {
    internal class ControlKeys {
        public enum MovementKeys {
            W,          //Forward
            A,          //Left
            S,          //Back
            D,          //Right
            Plus,       //Speed Up
            Minus,      //Speed Down
            Space       //Hand Brake
        }
    }
}
