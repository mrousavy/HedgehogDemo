using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HedgehogClient {
    internal static class ControlKeys {
        public enum MovementKey {
            W = 0,          //Forward
            A = 1,          //Left
            S = 2,          //Back
            D = 3,          //Right
            Plus = 4,       //Speed Up
            Minus = 5,      //Speed Down
            Space = 6,      //Hand Brake
            Stop = 7        //Stop any Movement
        }

        public static MovementKey GetKey(Key? key) {
            if(key == null)
                return MovementKey.Stop;

            switch(key) {
                case Key.W:
                    return MovementKey.W;
                case Key.A:
                    return MovementKey.A;
                case Key.S:
                    return MovementKey.S;
                case Key.D:
                    return MovementKey.D;
                case Key.OemPlus:
                case Key.Add:
                    return MovementKey.Plus;
                case Key.OemMinus:
                case Key.Subtract:
                    return MovementKey.Minus;
                case Key.Space:
                    return MovementKey.Space;
                default:
                    throw new Exception("Wrong Input");
            }
        }


        public static string FriendlyStatus(MovementKey key) {
            switch(key) {
                case MovementKey.W:
                    return "Forwards";
                case MovementKey.A:
                    return "Left";
                case MovementKey.S:
                    return "Backwards";
                case MovementKey.D:
                    return "Right";
                case MovementKey.Space:
                    return "Braking";
                default:
                    return "/";
            }
        }
    }
}
