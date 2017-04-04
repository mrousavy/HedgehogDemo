using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Input;
using BrandonPotter.XBox;

namespace HedgehogClient {
    internal static class ControlKeys {
        public static List<XBoxController> Controllers;
        public static Thread XboxControllerThread;


        public enum MovementKey {
            NullByte = -1,  //Disconnect Message
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


        public static Key GetXboxButton(XBoxController controller) {
            if(controller.TriggerRightPressed)
                return Key.W;
            if(controller.ThumbLeftX < 0.0 || controller.ThumbpadLeftPressed)
                return Key.A;
            if(controller.TriggerLeftPressed)
                return Key.S;
            if(controller.ThumbLeftX > 0.0 || controller.ThumbpadRightPressed)
                return Key.A;
            if(controller.ButtonShoulderRightPressed)
                return Key.Add;
            if(controller.ButtonShoulderLeftPressed)
                return Key.Subtract;
            if(controller.ButtonAPressed)
                return Key.Space;

            return Key.Escape;
        }


        public static void RegisterXboxInput(Action<object, KeyEventArgs> callback) {
            IEnumerable<XBoxController> controllers = XBoxController.GetConnectedControllers();
            Controllers = new List<XBoxController>();

            if(controllers == null)
                return;

            foreach(XBoxController controller in controllers) {
                if(controller.IsConnected)
                    Controllers.Add(controller);
            }

            XboxControllerThread = new Thread(() => {
                try {
                    while(true) {
                        foreach(XBoxController controller in Controllers) {
                            if(controller.IsConnected) {
                                Key key = GetXboxButton(controller);
                                if(key != Key.Escape)
                                    callback.Invoke(null, new KeyEventArgs(null, null, 0, key));
                            }
                        }
                        Thread.Sleep(10);
                    }
                } catch {
                    //thread aborted
                }
            });
            XboxControllerThread.Start();
        }
    }
}
