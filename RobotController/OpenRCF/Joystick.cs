using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.XInput;
using static System.Windows.Forms.AxHost;
using System.Threading;
using OpenRCF;
using static OpenRCF.RobotObject;
using static OpenRCF.Mobile;

namespace RobotController.OpenRCF
{

    internal class Joystick
    {
        static void Main()
        {
            double[] TargetOdom = { 0, 0, 0 };
            double[] vel = { 0, 0, 0, 0 };
            int[] TargetVel = { 0, 0, 0, 0 };
            int[] CurrentVel = { 0, 0, 0, 0 };

            SerialDevice.Dynamixel Dynamixel = new SerialDevice.Dynamixel(1000000);
            byte[] id = new byte[4] { 13, 12, 14, 11 };
            //byte[] id = new byte[4] { 11, 12, 13, 14 };

            Dynamixel.PortOpen("COM3");
            Dynamixel.TorqueEnable(id);

            MobileInfo Mobile = new MobileInfo();
            JointState Joint = new JointState();


            Console.WriteLine("Start XGamepadApp");
            // Initialize XInput
            var controllers = new[] { new Controller(UserIndex.One), new Controller(UserIndex.Two), new Controller(UserIndex.Three), new Controller(UserIndex.Four) };
            
            // Get 1st controller available
            Controller controller = null;
            foreach (var selectControler in controllers)
            {
                if (selectControler.IsConnected)
                {
                    controller = selectControler;
                    break;
                }
            }

            if (controller == null)
            {
                Console.WriteLine("No XInput controller installed");
            }
            else
            {
                Console.WriteLine("Found a XInput controller available");
                Console.WriteLine("Press buttons on the controller to display events or escape key to exit... ");

                // Poll events from joystick
                var previousState = controller.GetState();
                while (controller.IsConnected)
                {
                    //if (IsKeyPressed(ConsoleKey.Escape))
                    //{
                    //    break;
                    //}
                    var state = controller.GetState();
                    //if (previousState.PacketNumber != state.PacketNumber)

                    TargetOdom[0] = map(state.Gamepad.LeftThumbY, -32768, 32767, -0.5, 0.5);
                    TargetOdom[1] = map(state.Gamepad.LeftThumbX, -32768, 32767, -0.5, 0.5);
                    TargetOdom[2] = map(state.Gamepad.RightThumbX, -32768, 32767, -0.6, 0.6);

                    if (TargetOdom[0] < 0.1 && TargetOdom[0] > -0.1) TargetOdom[0] = 0;
                    if (TargetOdom[1] < 0.1 && TargetOdom[1] > -0.1) TargetOdom[1] = 0;
                    if (TargetOdom[2] < 0.1 && TargetOdom[2] > -0.1) TargetOdom[2] = 0;

                    Joint = Mecanum4WInverseKinematics(TargetOdom);

                    //Console.WriteLine("TargetOdom:{0}, TargetOdom:{1}, TargetOdom:{2}", TargetOdom[0], TargetOdom[1], TargetOdom[2]);
                    //Console.WriteLine("GVel:{0}, GVel:{1}, GVel:{2}, GVel:{3}", Joint.RPM[0], Joint.RPM[1], Joint.RPM[2], Joint.RPM[3]);

                    TargetVel[0] = (int)(Math.Ceiling(Joint.RPM[0] / 0.229));
                    TargetVel[1] = (int)(Math.Ceiling(Joint.RPM[1] / 0.229));
                    TargetVel[2] = (int)(Math.Ceiling(Joint.RPM[2] / 0.229));
                    TargetVel[3] = (int)(Math.Ceiling(Joint.RPM[3] / 0.229)); 

                    Dynamixel.WriteVelocity(id, TargetVel);
                    Dynamixel.RequestVelocityReply(id);

                    CurrentVel = Dynamixel.Velocity(id);

                    vel[0] = CurrentVel[0];
                    vel[1] = CurrentVel[1];
                    vel[2] = CurrentVel[2];
                    vel[3] = CurrentVel[3];

                    Mobile = Mecanum4WForwardKinematics(vel);

                    CurrentVel = Dynamixel.Velocity(id);

                    //Console.WriteLine("RawX:{0}, VelX:{1}", state.Gamepad.LeftThumbX, vel[0]);
                    Console.WriteLine("Velocity:{0}, Velocity:{1}, Velocity:{2}, Velocity:{3}", CurrentVel[0], CurrentVel[1], CurrentVel[2], CurrentVel[3]);
                    //Console.WriteLine(state.Gamepad);
                    Thread.Sleep(30);
                    previousState = state;
                }
            }
            Console.WriteLine("End XGamepadApp");
        }

        public static bool IsKeyPressed(ConsoleKey key)
        {
            return Console.KeyAvailable && Console.ReadKey(true).Key == key;
        }

        public static double map(double x, double in_min, double in_max, double out_min, double out_max)
        {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }
    }
}
