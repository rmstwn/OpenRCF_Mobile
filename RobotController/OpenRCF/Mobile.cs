using OpenRCF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using static OpenRCF.Mobile;
using static OpenRCF.RobotObject;
using static OpenRCF.SerialDevice;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace OpenRCF
{
    public class MobileInfo
    {
        public double[] Odometry;
        public double[] Position;
    }

    public class JointState
    {
        public double[] RPM;
    }

    public class Mobile
    {

        public static class GlobalsMecanum
        {
            // Mecanum
            public static double Axis1Length = 0.27;
            public static double Axis2Length = 0.25;
            public static double WheelDiameter = 0.1;
            public static double Circumference = Math.PI * GlobalsMecanum.WheelDiameter;
        }

        public static class GlobalsOmnidirectional
        {
            // Omni
            public static double AxisLength = 0.16;
            public static double WheelDiameter = 0.1;
            public static double Delta = 45; //degree

            public static double sin = Math.Sin(GlobalsOmnidirectional.Delta * (Math.PI / 180));
            public static double cos = Math.Cos(GlobalsOmnidirectional.Delta * (Math.PI / 180));
        }

        public static class Mecanum
        {
            /* Mecanum
                  robot:	l1: Axis1Length
                            l2: Axis2Length
                          ID=12          ID=11
                       --|##2##|        |##4##|
                       ^   ##################             y
                   l1  ¦   ##################             ^
                       ¦   ##################   front     ¦
                       v   ##################             ¦
                       --|##1##|        |##3##|          -¦-----> x
                          ID=13          ID=14
                            |       l2     |
                            |<------------>|
            */

            static double[] odom = { 0, 0, 0 };
            static double[] cpose = { 0, 0, 0 };

            static double last_time;
            static double[] last_odom;
            static double dt;

            static public MobileInfo MobileMecanum = new MobileInfo();
            static public JointState JointMecanum = new JointState();

            public static MobileInfo ForwardKinematics(double[] rpm)
            {
                /* Mecanum Forward Kinematics

                    l = l1 + l2

                                 |                      |   | w1 |
                    | vx |       |  1    1    1     1   | . | w2 |
                    | vy | = R/4 | -1    1    1    -1   |   | w3 |
                    | wz |       | -1/l  1/l -1/l  1/l  |   | w4 |
                */

                double current_time = DateTime.Now.Subtract(new DateTime(2023, 1, 1)).TotalMilliseconds;

                //velocities:
                //double move_vel_x = (velocity[0] + velocity[1] + velocity[2] + velocity[3]) * GlobalsMecanum.WheelDiameter / 8;
                //double move_vel_y = (velocity[1] - velocity[0] - velocity[3] + velocity[2]) * GlobalsMecanum.WheelDiameter / 8;
                //double move_yawrate = (-velocity[0] + velocity[1] - velocity[2] + velocity[3]) * GlobalsMecanum.WheelDiameter / 4 / (GlobalsMecanum.Axis1Length + GlobalsMecanum.Axis2Length);

                double move_vel_x = ((rpm[0] + rpm[1] + rpm[2] + rpm[3]) / 4) / 60 * GlobalsMecanum.Circumference;
                double move_vel_y = ((rpm[1] - rpm[0] - rpm[3] + rpm[2]) / 4) / 60 * GlobalsMecanum.Circumference;
                double move_yawrate = ((-rpm[0] + rpm[1] - rpm[2] + rpm[3]) / 4) / 60 * GlobalsMecanum.WheelDiameter / 4 / (GlobalsMecanum.Axis1Length + GlobalsMecanum.Axis2Length);

                //positions:
                if (last_time > 0 || last_time < 0)
                {
                    dt = (current_time - last_time) / 1000;

                    if (dt > 0 && dt < 1)
                    {
                        // compute second order midpoint velocities
                        double vel_x_mid = 0.5 * (move_vel_x + last_odom[0]);
                        double vel_y_mid = 0.5 * (move_vel_y + last_odom[1]);
                        double yawrate_mid = 0.5 * (move_yawrate + last_odom[2]);

                        // compute midpoint yaw angle
                        double yaw_mid = cpose[2] + 0.5 * yawrate_mid * dt;

                        // integrate position using midpoint velocities and yaw angle
                        cpose[0] += vel_x_mid * dt * Math.Cos(yaw_mid) + vel_y_mid * dt * -Math.Sin(yaw_mid);
                        cpose[1] += vel_x_mid * dt * Math.Sin(yaw_mid) + vel_y_mid * dt * Math.Cos(yaw_mid);

                        // integrate yaw angle using midpoint yawrate
                        cpose[2] += yawrate_mid * dt;
                    }
                    else
                    {
                        Console.WriteLine("invalid joint state delta time: " + dt + " sec");
                    }
                }

                odom[0] = move_vel_x;
                odom[1] = move_vel_y;
                odom[2] = move_yawrate;

                last_time = current_time;
                last_odom = odom;

                MobileMecanum.Odometry = odom;
                MobileMecanum.Position = cpose;

                return MobileMecanum;

                //Console.WriteLine("posX: " + cpose[0].ToString() + " posY: " + cpose[1].ToString() + " posYaw: " + cpose[2].ToString());
            }

            public static JointState InverseKinematics(double[] odom)
            {
                /* Mecanum Forward Kinematics
                   | w1 |       | 1 -1 -(l1+l2) |   
                   | w2 | = 1/C | 1  1  (l1+l2) | . | vx |
                   | w3 |       | 1  1 -(l1+l2) |   | vy |
                   | w4 |       | 1 -1  (l1+l2) |   | wz |
                */

                double linearX = odom[0];
                double linearY = odom[1];
                double angularZ = odom[2];

                double[] rpm = { 0, 0, 0, 0 };

                //convert m/s to m/min
                double linear_vel_x_mins = linearX * 60;
                double linear_vel_y_mins = linearY * 60;

                //convert rad/s to rad/min
                double angular_vel_z_mins = angularZ * 60;

                //Vt = ω * radius
                double tangential_vel = angular_vel_z_mins * GlobalsMecanum.Axis1Length;

                double x_rpm = linear_vel_x_mins / GlobalsMecanum.Circumference; //1 / C
                double y_rpm = linear_vel_y_mins / GlobalsMecanum.Circumference;
                double tan_rpm = tangential_vel / GlobalsMecanum.Circumference;

                //calculate for the target motor RPM and direction
                //front-left motor
                rpm[0] = x_rpm - y_rpm - tan_rpm;
                //rear-left motor
                rpm[2] = x_rpm + y_rpm - tan_rpm;

                //front-right motor
                rpm[1] = x_rpm + y_rpm + tan_rpm;
                //rear-right motor
                rpm[3] = x_rpm - y_rpm + tan_rpm;

                //// w1:
                //vel[0] = 2 / GlobalsMecanum.WheelDiameter * (linearX - linearY - (GlobalsMecanum.Axis1Length + GlobalsMecanum.Axis2Length) / 2 * angularZ);
                //// w2:
                //vel[1] = 2 / GlobalsMecanum.WheelDiameter * (linearX + linearY + (GlobalsMecanum.Axis1Length + GlobalsMecanum.Axis2Length) / 2 * angularZ);
                //// w3:
                //vel[2] = 2 / GlobalsMecanum.WheelDiameter * (linearX + linearY - (GlobalsMecanum.Axis1Length + GlobalsMecanum.Axis2Length) / 2 * angularZ);
                //// w4:
                //vel[3] = 2 / GlobalsMecanum.WheelDiameter * (linearX - linearY + (GlobalsMecanum.Axis1Length + GlobalsMecanum.Axis2Length) / 2 * angularZ);

                JointMecanum.RPM = rpm;

                //Console.WriteLine("w1: " + velocity[0].ToString() + " w2: " + velocity[1].ToString() + " w3: " + velocity[2].ToString() + " w3: " + velocity[3].ToString());

                return JointMecanum;
            }

            public static void Move(Dynamixel Dynamixel, byte[] DxlId, int BaudRate, double[] TargetOdom)
            {
                MobileInfo MobileInfo = new MobileInfo();
                JointState JointState = new JointState();

                int[] TargetVel = { 0, 0, 0, 0 };
                int[] CurrentVel = { 0, 0, 0, 0 };
                double[] vel = { 0, 0, 0, 0 };

                if (TargetOdom[0] < 0.01 && TargetOdom[0] > -0.01) TargetOdom[0] = 0;
                if (TargetOdom[1] < 0.01 && TargetOdom[1] > -0.01) TargetOdom[1] = 0;
                if (TargetOdom[2] < 0.01 && TargetOdom[2] > -0.01) TargetOdom[2] = 0;

                JointState = InverseKinematics(TargetOdom);

                //Console.WriteLine("TargetOdom:{0}, TargetOdom:{1}, TargetOdom:{2}", TargetOdom[0], TargetOdom[1], TargetOdom[2]);
                //Console.WriteLine("GVel:{0}, GVel:{1}, GVel:{2}, GVel:{3}", Joint.RPM[0], Joint.RPM[1], Joint.RPM[2], Joint.RPM[3]);

                TargetVel[0] = (int)(Math.Ceiling(JointState.RPM[0] / 0.229));
                TargetVel[1] = (int)(Math.Ceiling(JointState.RPM[1] / 0.229));
                TargetVel[2] = (int)(Math.Ceiling(JointState.RPM[2] / 0.229));
                TargetVel[3] = (int)(Math.Ceiling(JointState.RPM[3] / 0.229));

                Dynamixel.WriteVelocity(DxlId, TargetVel);
                Dynamixel.RequestVelocityReply(DxlId);

                CurrentVel = Dynamixel.Velocity(DxlId);

                vel[0] = CurrentVel[0];
                vel[1] = CurrentVel[1];
                vel[2] = CurrentVel[2];
                vel[3] = CurrentVel[3];

                MobileInfo = ForwardKinematics(vel);

                CurrentVel = Dynamixel.Velocity(DxlId);

                //Console.WriteLine("RawX:{0}, VelX:{1}", state.Gamepad.LeftThumbX, vel[0]);
                Console.WriteLine("Velocity:{0}, Velocity:{1}, Velocity:{2}, Velocity:{3}", CurrentVel[0], CurrentVel[1], CurrentVel[2], CurrentVel[3]);
                //Console.WriteLine(state.Gamepad);
            }

            public static void MovePos(Dynamixel Dynamixel, byte[] DxlId, int BaudRate, double Speed, double[] TargetPos)
            {
                //SerialDevice.Dynamixel Dynamixel = new SerialDevice.Dynamixel(BaudRate);
                MobileInfo Mobile = new MobileInfo();
                JointState Joint = new JointState();

                int[] TargetVel = { 0, 0, 0, 0 };
                int[] CurrentVel = { 0, 0, 0, 0 };
                double[] vel = { 0, 0, 0, 0 };
                double[] TargetOdom = { 0, 0, 0, 0 };

                //Dynamixel.PortOpen(COMport);
                //Dynamixel.TorqueEnable(DxlId);

                if (TargetPos[0] != 0)
                {
                    if (TargetPos[0] > 0)
                    {
                        TargetOdom[0] = Speed;
                    }
                    else TargetOdom[0] = -Speed;
                }
                if (TargetPos[1] != 0)
                {
                    if (TargetPos[1] > 0)
                    {
                        TargetOdom[1] = Speed;
                    }
                    else TargetOdom[1] = -Speed;
                }
                if (TargetPos[2] != 0)
                {
                    if (TargetPos[2] > 0)
                    {
                        TargetOdom[2] = Speed;
                    }
                    else TargetOdom[2] = -Speed;
                }

                Joint = InverseKinematics(TargetOdom);

                TargetVel[0] = (int)(Math.Ceiling(Joint.RPM[0] / 0.229));
                TargetVel[1] = (int)(Math.Ceiling(Joint.RPM[1] / 0.229));
                TargetVel[2] = (int)(Math.Ceiling(Joint.RPM[2] / 0.229));
                TargetVel[3] = (int)(Math.Ceiling(Joint.RPM[3] / 0.229));

                for (; ; )
                {
                    Dynamixel.WriteVelocity(DxlId, TargetVel);
                    Dynamixel.RequestVelocityReply(DxlId);

                    CurrentVel = Dynamixel.Velocity(DxlId);

                    vel[0] = CurrentVel[0] * 0.229;
                    vel[1] = CurrentVel[1] * 0.229;
                    vel[2] = CurrentVel[2] * 0.229;
                    vel[3] = CurrentVel[3] * 0.229;

                    Mobile = ForwardKinematics(vel);

                    if (TargetOdom[0] < 0.01 && TargetOdom[0] > -0.01) TargetOdom[0] = 0;
                    if (TargetOdom[1] < 0.01 && TargetOdom[1] > -0.01) TargetOdom[1] = 0;
                    if (TargetOdom[2] < 0.01 && TargetOdom[2] > -0.01) TargetOdom[2] = 0;

                    Console.WriteLine("TVelocity:{0}, TVelocity:{1}, TVelocity:{2}, TVelocity:{3}", TargetVel[0], TargetVel[1], TargetVel[2], TargetVel[3]);
                    Console.WriteLine("TOdomX:{0}, TOdomY:{1}, TOdomZ:{2}", TargetOdom[0], TargetOdom[1], TargetOdom[2]);
                    Console.WriteLine("TPosX:{0}, TPosY:{1}, TPosZ:{2}", TargetPos[0], TargetPos[1], TargetPos[2]);
                    Console.WriteLine("Velocity:{0}, Velocity:{1}, Velocity:{2}, Velocity:{3}", CurrentVel[0], CurrentVel[1], CurrentVel[2], CurrentVel[3]);
                    Console.WriteLine("OdomX:{0}, OdomY:{1}, OdomZ:{2}", Mobile.Odometry[0], Mobile.Odometry[1], Mobile.Odometry[2]);
                    Console.WriteLine("PosX:{0}, PosY:{1}, PosZ:{2}", Mobile.Position[0], Mobile.Position[1], Mobile.Position[2]);

                    if (Math.Abs(Mobile.Position[0]) >= Math.Abs(TargetPos[0]) && Math.Abs(Mobile.Position[1]) >= Math.Abs(TargetPos[1]) && Math.Abs(Mobile.Position[2]) >= Math.Abs(TargetPos[2]))
                    {
                        Mobile.Position[0] = (int)0;
                        Mobile.Position[1] = (int)0;
                        Mobile.Position[2] = (int)0;

                        TargetPos[0] = (int)0;
                        TargetPos[1] = (int)0;
                        TargetPos[2] = (int)0;

                        TargetVel[0] = (int)0;
                        TargetVel[1] = (int)0;
                        TargetVel[2] = (int)0;
                        TargetVel[3] = (int)0;

                        Dynamixel.WriteVelocity(DxlId, TargetVel);

                        break;
                    }

                    Thread.Sleep(35);
                }
            }
        }

        public class Omnidirectional
        {

            /* Omnidirectional
               robot:	L: AxisLength

               ID=14  |###|       |###|  ID=11
                    |###|           |###|          y
                  |###| ############# |###|        ^
                        #############              ¦
                        #############     front    ¦  
                        #############              ¦
                  |###| ############# |###|       -¦-----> x
                    |###|           |###|        
               ID=13  |###|       |###|  ID=12          
                              |   L   |
                              |<----->|
            */

            static double[] odom = { 0, 0, 0 };
            static double[] cpose = { 0, 0, 0 };

            static double last_time;
            static double[] last_odom;
            static double dt;

            static public MobileInfo MobileOmni = new MobileInfo();
            static public JointState JointOmni = new JointState();

            public static MobileInfo ForwardKinematics(double[] rpm)
            {
                double current_time = DateTime.Now.Subtract(new DateTime(2023, 1, 1)).TotalMilliseconds;

                double[] velocity = { 0, 0, 0, 0 };

                velocity[0] = (2 * Math.PI * (GlobalsOmnidirectional.WheelDiameter / 2)) * (rpm[0] / 60);
                velocity[1] = (2 * Math.PI * (GlobalsOmnidirectional.WheelDiameter / 2)) * (rpm[1] / 60);
                velocity[2] = (2 * Math.PI * (GlobalsOmnidirectional.WheelDiameter / 2)) * (rpm[2] / 60);
                velocity[3] = (2 * Math.PI * (GlobalsOmnidirectional.WheelDiameter / 2)) * (rpm[3] / 60);

                double move_vel_x = velocity[0] * GlobalsOmnidirectional.sin - velocity[2] * GlobalsOmnidirectional.sin - velocity[1] * GlobalsOmnidirectional.cos + velocity[3] * GlobalsOmnidirectional.cos;
                double move_vel_y = -velocity[0] * GlobalsOmnidirectional.cos + velocity[2] * GlobalsOmnidirectional.cos - velocity[1] * GlobalsOmnidirectional.sin + velocity[3] * GlobalsOmnidirectional.sin;
                double move_yawrate = -(velocity[0] + velocity[1] + velocity[2] + velocity[3]) / GlobalsOmnidirectional.AxisLength;

                //positions:
                if (last_time > 0 || last_time < 0)
                {
                    dt = (current_time - last_time) / 1000;

                    if (dt > 0 && dt < 1)
                    {
                        // compute second order midpoint velocities
                        double vel_x_mid = 0.5 * (move_vel_x + last_odom[0]);
                        double vel_y_mid = 0.5 * (move_vel_y + last_odom[1]);
                        double yawrate_mid = 0.5 * (move_yawrate + last_odom[2]);

                        // compute midpoint yaw angle
                        double yaw_mid = cpose[2] + 0.5 * yawrate_mid * dt;

                        // integrate position using midpoint velocities and yaw angle
                        cpose[0] += vel_x_mid * dt * Math.Cos(yaw_mid) + vel_y_mid * dt * -Math.Sin(yaw_mid);
                        cpose[1] += vel_x_mid * dt * Math.Sin(yaw_mid) + vel_y_mid * dt * Math.Cos(yaw_mid);

                        // integrate yaw angle using midpoint yawrate
                        cpose[2] += yawrate_mid * dt;
                    }
                    else
                    {
                        Console.WriteLine("invalid joint state delta time: " + dt + " sec");
                    }
                }

                odom[0] = move_vel_x;
                odom[1] = move_vel_y;
                odom[2] = move_yawrate;

                last_time = current_time;
                last_odom = odom;

                MobileOmni.Odometry = odom;
                MobileOmni.Position = cpose;

                return MobileOmni;
            }

            public static JointState InverseKinematics(double[] odom)
            {
                Matrix J = new Matrix(3, 4);
                Matrix JJ = new Matrix(3, 3);

                Matrix JTranspose = new Matrix(4, 3);
                Matrix JInverse = new Matrix(3, 3);
                Matrix JPseudo = new Matrix(4, 3);

                Vector Odom = new Vector(3);
                Vector Result = new Vector(4);

                double[] rpm = { 0, 0, 0, 0 };

                J[0, 0] = (float)GlobalsOmnidirectional.sin;
                J[0, 1] = -(float)GlobalsOmnidirectional.cos;
                J[0, 2] = -(float)GlobalsOmnidirectional.sin;
                J[0, 3] = (float)GlobalsOmnidirectional.cos;

                J[1, 0] = -(float)GlobalsOmnidirectional.cos;
                J[1, 1] = -(float)GlobalsOmnidirectional.sin;
                J[1, 2] = (float)GlobalsOmnidirectional.cos;
                J[1, 3] = (float)GlobalsOmnidirectional.sin;

                J[2, 0] = 1;
                J[2, 1] = 1;
                J[2, 2] = 1;
                J[2, 3] = 1;

                //J.ConsoleWrite();

                JTranspose.Set = J.Transpose;
                //JTranspose.ConsoleWrite();

                JJ.Set = J.Times(JTranspose.Get);
                //JJ.ConsoleWrite();

                JInverse.Set = JJ.Inverse;
                //JInverse.ConsoleWrite();

                JPseudo.Set = JTranspose.Times(JInverse.Get);
                //JPseudo.ConsoleWrite();

                Odom[0] = (float)odom[0];
                Odom[1] = (float)odom[1];
                Odom[2] = (float)odom[2];

                //Odom.ConsoleWrite();

                Result.Set = JPseudo.Times(Odom.Get);
                //Result.ConsoleWrite();

                rpm[0] = (double)(Result[0] / (2 * Math.PI * (GlobalsOmnidirectional.WheelDiameter / 2)) * 60);
                rpm[1] = (double)(Result[1] / (2 * Math.PI * (GlobalsOmnidirectional.WheelDiameter / 2)) * 60);
                rpm[2] = (double)(Result[2] / (2 * Math.PI * (GlobalsOmnidirectional.WheelDiameter / 2)) * 60);
                rpm[3] = (double)(Result[3] / (2 * Math.PI * (GlobalsOmnidirectional.WheelDiameter / 2)) * 60);

                JointOmni.RPM = rpm;

                //Console.WriteLine("RPM:{0}, RPM:{1}, RPM:{2}, RPM:{3}", rpm[0], rpm[1], rpm[2], rpm[3]);

                return JointOmni;
            }

            public static void Move(Dynamixel Dynamixel, byte[] DxlId, int BaudRate, double[] TargetOdom)
            {
                MobileInfo MobileInfo = new MobileInfo();
                JointState JointState = new JointState();

                int[] TargetVel = { 0, 0, 0, 0 };
                int[] CurrentVel = { 0, 0, 0, 0 };
                double[] vel = { 0, 0, 0, 0 };

                if (TargetOdom[0] < 0.01 && TargetOdom[0] > -0.01) TargetOdom[0] = 0;
                if (TargetOdom[1] < 0.01 && TargetOdom[1] > -0.01) TargetOdom[1] = 0;
                if (TargetOdom[2] < 0.01 && TargetOdom[2] > -0.01) TargetOdom[2] = 0;

                JointState = InverseKinematics(TargetOdom);

                //Console.WriteLine("TargetOdom:{0}, TargetOdom:{1}, TargetOdom:{2}", TargetOdom[0], TargetOdom[1], TargetOdom[2]);
                //Console.WriteLine("GVel:{0}, GVel:{1}, GVel:{2}, GVel:{3}", Joint.RPM[0], Joint.RPM[1], Joint.RPM[2], Joint.RPM[3]);

                TargetVel[0] = (int)(Math.Ceiling(JointState.RPM[0] / 0.229));
                TargetVel[1] = (int)(Math.Ceiling(JointState.RPM[1] / 0.229));
                TargetVel[2] = (int)(Math.Ceiling(JointState.RPM[2] / 0.229));
                TargetVel[3] = (int)(Math.Ceiling(JointState.RPM[3] / 0.229));

                Dynamixel.WriteVelocity(DxlId, TargetVel);
                Dynamixel.RequestVelocityReply(DxlId);

                CurrentVel = Dynamixel.Velocity(DxlId);

                vel[0] = CurrentVel[0];
                vel[1] = CurrentVel[1];
                vel[2] = CurrentVel[2];
                vel[3] = CurrentVel[3];

                MobileInfo = ForwardKinematics(vel);

                CurrentVel = Dynamixel.Velocity(DxlId);

                //Console.WriteLine("RawX:{0}, VelX:{1}", state.Gamepad.LeftThumbX, vel[0]);
                Console.WriteLine("Velocity:{0}, Velocity:{1}, Velocity:{2}, Velocity:{3}", CurrentVel[0], CurrentVel[1], CurrentVel[2], CurrentVel[3]);
                //Console.WriteLine(state.Gamepad);
            }

            public static void MovePos(Dynamixel Dynamixel, byte[] DxlId, int BaudRate, double Speed, double[] TargetPos)
            {
                //SerialDevice.Dynamixel Dynamixel = new SerialDevice.Dynamixel(BaudRate);
                MobileInfo Mobile = new MobileInfo();
                JointState Joint = new JointState();

                int[] TargetVel = { 0, 0, 0, 0 };
                int[] CurrentVel = { 0, 0, 0, 0 };
                double[] vel = { 0, 0, 0, 0 };
                double[] TargetOdom = { 0, 0, 0, 0 };

                //Dynamixel.PortOpen(COMport);
                //Dynamixel.TorqueEnable(DxlId);

                if (TargetPos[0] != 0)
                {
                    if (TargetPos[0] > 0)
                    {
                        TargetOdom[0] = Speed;
                    }
                    else TargetOdom[0] = -Speed;
                }
                if (TargetPos[1] != 0)
                {
                    if (TargetPos[1] > 0)
                    {
                        TargetOdom[1] = Speed;
                    }
                    else TargetOdom[1] = -Speed;
                }
                if (TargetPos[2] != 0)
                {
                    if (TargetPos[2] > 0)
                    {
                        TargetOdom[2] = Speed;
                    }
                    else TargetOdom[2] = -Speed;
                }

                Joint = InverseKinematics(TargetOdom);

                TargetVel[0] = (int)(Math.Ceiling(Joint.RPM[0] / 0.229));
                TargetVel[1] = (int)(Math.Ceiling(Joint.RPM[1] / 0.229));
                TargetVel[2] = (int)(Math.Ceiling(Joint.RPM[2] / 0.229));
                TargetVel[3] = (int)(Math.Ceiling(Joint.RPM[3] / 0.229));

                for (; ; )
                {
                    Dynamixel.WriteVelocity(DxlId, TargetVel);
                    Dynamixel.RequestVelocityReply(DxlId);

                    CurrentVel = Dynamixel.Velocity(DxlId);

                    vel[0] = CurrentVel[0] * 0.229;
                    vel[1] = CurrentVel[1] * 0.229;
                    vel[2] = CurrentVel[2] * 0.229;
                    vel[3] = CurrentVel[3] * 0.229;


                    Mobile = ForwardKinematics(vel);

                    if (TargetOdom[0] < 0.01 && TargetOdom[0] > -0.01) TargetOdom[0] = 0;
                    if (TargetOdom[1] < 0.01 && TargetOdom[1] > -0.01) TargetOdom[1] = 0;
                    if (TargetOdom[2] < 0.01 && TargetOdom[2] > -0.01) TargetOdom[2] = 0;

                    Console.WriteLine("TVelocity:{0}, TVelocity:{1}, TVelocity:{2}, TVelocity:{3}", TargetVel[0], TargetVel[1], TargetVel[2], TargetVel[3]);
                    Console.WriteLine("TOdomX:{0}, TOdomY:{1}, TOdomZ:{2}", TargetOdom[0], TargetOdom[1], TargetOdom[2]);
                    Console.WriteLine("TPosX:{0}, TPosY:{1}, TPosZ:{2}", TargetPos[0], TargetPos[1], TargetPos[2]);
                    Console.WriteLine("Velocity:{0}, Velocity:{1}, Velocity:{2}, Velocity:{3}", CurrentVel[0], CurrentVel[1], CurrentVel[2], CurrentVel[3]);
                    Console.WriteLine("OdomX:{0}, OdomY:{1}, OdomZ:{2}", Mobile.Odometry[0], Mobile.Odometry[1], Mobile.Odometry[2]);
                    Console.WriteLine("PosX:{0}, PosY:{1}, PosZ:{2}", Mobile.Position[0], Mobile.Position[1], Mobile.Position[2]);

                    if (Math.Abs(Mobile.Position[0]) >= Math.Abs(TargetPos[0]) && Math.Abs(Mobile.Position[1]) >= Math.Abs(TargetPos[1]) && Math.Abs(Mobile.Position[2]) >= Math.Abs(TargetPos[2]))
                    {
                        Mobile.Position[0] = (int)0;
                        Mobile.Position[1] = (int)0;
                        Mobile.Position[2] = (int)0;

                        TargetPos[0] = (int)0;
                        TargetPos[1] = (int)0;
                        TargetPos[2] = (int)0;

                        TargetVel[0] = (int)0;
                        TargetVel[1] = (int)0;
                        TargetVel[2] = (int)0;
                        TargetVel[3] = (int)0;

                        Dynamixel.WriteVelocity(DxlId, TargetVel);

                        break;
                    }

                    Thread.Sleep(35);
                }
            }
        }
    }

    //public class Mobile
    //{
    //    //static void Main(string[] args)
    //    //{
    //    //    Mecanum.Mecanum4WForwardKinematics();
    //    //}
    //    public class MobileInfo
    //    {
    //        public double[] Odometry;
    //        public double[] Position;
    //    }

    //    public class JointState
    //    {
    //        public double[] RPM;
    //    }

    //    /* Mecanum
    //          robot:	l1: Axis1Length
    //                    l2: Axis2Length
    //                  ID=12          ID=11
    //               --|##2##|        |##4##|
    //               ^   ##################             y
    //           l1  ¦   ##################             ^
    //               ¦   ##################   front     ¦
    //               v   ##################             ¦
    //               --|##1##|        |##3##|          -¦-----> x
    //                  ID=13          ID=14
    //                    |       l2     |
    //                    |<------------>|
    //    */

    //    static double[] odom = { 0, 0, 0 };
    //    static double[] cpose = { 0, 0, 0 };

    //    static double last_time;
    //    static double[] last_odom;
    //    static double dt;

    //    static public MobileInfo MobileMecanum = new MobileInfo();
    //    static public JointState JointMecanum = new JointState();

    //    public static MobileInfo Mecanum4WForwardKinematics(double[] rpm)
    //    {
    //        /* Mecanum Forward Kinematics

    //            l = l1 + l2

    //                         |                      |   | w1 |
    //            | vx |       |  1    1    1     1   | . | w2 |
    //            | vy | = R/4 | -1    1    1    -1   |   | w3 |
    //            | wz |       | -1/l  1/l -1/l  1/l  |   | w4 |
    //        */

    //        double current_time = DateTime.Now.Subtract(new DateTime(2023, 1, 1)).TotalMilliseconds;

    //        //velocities:
    //        //double move_vel_x = (velocity[0] + velocity[1] + velocity[2] + velocity[3]) * GlobalsMecanum.WheelDiameter / 8;
    //        //double move_vel_y = (velocity[1] - velocity[0] - velocity[3] + velocity[2]) * GlobalsMecanum.WheelDiameter / 8;
    //        //double move_yawrate = (-velocity[0] + velocity[1] - velocity[2] + velocity[3]) * GlobalsMecanum.WheelDiameter / 4 / (GlobalsMecanum.Axis1Length + GlobalsMecanum.Axis2Length);

    //        double move_vel_x = ((rpm[0] + rpm[1] + rpm[2] + rpm[3]) / 4) / 60 * GlobalsMecanum.Circumference;
    //        double move_vel_y = ((rpm[1] - rpm[0] - rpm[3] + rpm[2]) / 4) / 60 * GlobalsMecanum.Circumference;
    //        double move_yawrate = ((-rpm[0] + rpm[1] - rpm[2] + rpm[3]) / 4) / 60 * GlobalsMecanum.WheelDiameter / 4 / (GlobalsMecanum.Axis1Length + GlobalsMecanum.Axis2Length);

    //        //positions:
    //        if (last_time > 0 || last_time < 0)
    //        {
    //            dt = (current_time - last_time) / 1000;

    //            if (dt > 0 && dt < 1)
    //            {
    //                // compute second order midpoint velocities
    //                double vel_x_mid = 0.5 * (move_vel_x + last_odom[0]);
    //                double vel_y_mid = 0.5 * (move_vel_y + last_odom[1]);
    //                double yawrate_mid = 0.5 * (move_yawrate + last_odom[2]);

    //                // compute midpoint yaw angle
    //                double yaw_mid = cpose[2] + 0.5 * yawrate_mid * dt;

    //                // integrate position using midpoint velocities and yaw angle
    //                cpose[0] += vel_x_mid * dt * Math.Cos(yaw_mid) + vel_y_mid * dt * -Math.Sin(yaw_mid);
    //                cpose[1] += vel_x_mid * dt * Math.Sin(yaw_mid) + vel_y_mid * dt * Math.Cos(yaw_mid);

    //                // integrate yaw angle using midpoint yawrate
    //                cpose[2] += yawrate_mid * dt;
    //            }
    //            else
    //            {
    //                Console.WriteLine("invalid joint state delta time: " + dt + " sec");
    //            }
    //        }

    //        odom[0] = move_vel_x;
    //        odom[1] = move_vel_y;
    //        odom[2] = move_yawrate;

    //        last_time = current_time;
    //        last_odom = odom;

    //        MobileMecanum.Odometry = odom;
    //        MobileMecanum.Position = cpose;

    //        return MobileMecanum;

    //        //Console.WriteLine("posX: " + cpose[0].ToString() + " posY: " + cpose[1].ToString() + " posYaw: " + cpose[2].ToString());
    //    }

    //    public static JointState Mecanum4WInverseKinematics(double[] odom)
    //    {
    //        /* Mecanum Forward Kinematics
    //           | w1 |       | 1 -1 -(l1+l2) |   
    //           | w2 | = 1/C | 1  1  (l1+l2) | . | vx |
    //           | w3 |       | 1  1 -(l1+l2) |   | vy |
    //           | w4 |       | 1 -1  (l1+l2) |   | wz |
    //        */

    //        double linearX = odom[0];
    //        double linearY = odom[1];
    //        double angularZ = odom[2];

    //        double[] rpm = { 0, 0, 0, 0 };

    //        //convert m/s to m/min
    //        double linear_vel_x_mins = linearX * 60;
    //        double linear_vel_y_mins = linearY * 60;

    //        //convert rad/s to rad/min
    //        double angular_vel_z_mins = angularZ * 60;

    //        //Vt = ω * radius
    //        double tangential_vel = angular_vel_z_mins * GlobalsMecanum.Axis1Length;

    //        double x_rpm = linear_vel_x_mins / GlobalsMecanum.Circumference; //1 / C
    //        double y_rpm = linear_vel_y_mins / GlobalsMecanum.Circumference;
    //        double tan_rpm = tangential_vel / GlobalsMecanum.Circumference;

    //        //calculate for the target motor RPM and direction
    //        //front-left motor
    //        rpm[0] = x_rpm - y_rpm - tan_rpm;
    //        //rear-left motor
    //        rpm[2] = x_rpm + y_rpm - tan_rpm;

    //        //front-right motor
    //        rpm[1] = x_rpm + y_rpm + tan_rpm;
    //        //rear-right motor
    //        rpm[3] = x_rpm - y_rpm + tan_rpm;

    //        //// w1:
    //        //vel[0] = 2 / GlobalsMecanum.WheelDiameter * (linearX - linearY - (GlobalsMecanum.Axis1Length + GlobalsMecanum.Axis2Length) / 2 * angularZ);
    //        //// w2:
    //        //vel[1] = 2 / GlobalsMecanum.WheelDiameter * (linearX + linearY + (GlobalsMecanum.Axis1Length + GlobalsMecanum.Axis2Length) / 2 * angularZ);
    //        //// w3:
    //        //vel[2] = 2 / GlobalsMecanum.WheelDiameter * (linearX + linearY - (GlobalsMecanum.Axis1Length + GlobalsMecanum.Axis2Length) / 2 * angularZ);
    //        //// w4:
    //        //vel[3] = 2 / GlobalsMecanum.WheelDiameter * (linearX - linearY + (GlobalsMecanum.Axis1Length + GlobalsMecanum.Axis2Length) / 2 * angularZ);

    //        JointMecanum.RPM = rpm;

    //        //Console.WriteLine("w1: " + velocity[0].ToString() + " w2: " + velocity[1].ToString() + " w3: " + velocity[2].ToString() + " w3: " + velocity[3].ToString());

    //        return JointMecanum;
    //    }

    //    /* Omnidirectional
    //       robot:	L: AxisLength

    //       ID=14  |###|       |###|  ID=11
    //            |###|           |###|          y
    //          |###| ############# |###|        ^
    //                #############              ¦
    //                #############     front    ¦  
    //                #############              ¦
    //          |###| ############# |###|       -¦-----> x
    //            |###|           |###|        
    //       ID=13  |###|       |###|  ID=12          
    //                      |   L   |
    //                      |<----->|
    //    */

    //    //static double[] odom = { 0, 0, 0 };
    //    //static double[] cpose = { 0, 0, 0 };

    //    //static double last_time;
    //    //static double[] last_odom;
    //    //static double dt;

    //    static public MobileInfo MobileOmni = new MobileInfo();
    //    static public JointState JointOmni = new JointState();

    //    public static MobileInfo Omnidirectional4WForwardKinematics(double[] rpm)
    //    {
    //        double current_time = DateTime.Now.Subtract(new DateTime(2023, 1, 1)).TotalMilliseconds;

    //        double[] velocity = { 0, 0, 0, 0 };

    //        velocity[0] = (2 * Math.PI * (GlobalsOmnidirectional.WheelDiameter / 2)) * (rpm[0] / 60);
    //        velocity[1] = (2 * Math.PI * (GlobalsOmnidirectional.WheelDiameter / 2)) * (rpm[1] / 60);
    //        velocity[2] = (2 * Math.PI * (GlobalsOmnidirectional.WheelDiameter / 2)) * (rpm[2] / 60);
    //        velocity[3] = (2 * Math.PI * (GlobalsOmnidirectional.WheelDiameter / 2)) * (rpm[3] / 60);

    //        double move_vel_x = velocity[0] * GlobalsOmnidirectional.sin - velocity[2] * GlobalsOmnidirectional.sin - velocity[1] * GlobalsOmnidirectional.cos + velocity[3] * GlobalsOmnidirectional.cos;
    //        double move_vel_y = -velocity[0] * GlobalsOmnidirectional.cos + velocity[2] * GlobalsOmnidirectional.cos - velocity[1] * GlobalsOmnidirectional.sin + velocity[3] * GlobalsOmnidirectional.sin;
    //        double move_yawrate = -(velocity[0] + velocity[1] + velocity[2] + velocity[3]) / GlobalsOmnidirectional.AxisLength;

    //        //positions:
    //        if (last_time > 0 || last_time < 0)
    //        {
    //            dt = (current_time - last_time) / 1000;

    //            if (dt > 0 && dt < 1)
    //            {
    //                // compute second order midpoint velocities
    //                double vel_x_mid = 0.5 * (move_vel_x + last_odom[0]);
    //                double vel_y_mid = 0.5 * (move_vel_y + last_odom[1]);
    //                double yawrate_mid = 0.5 * (move_yawrate + last_odom[2]);

    //                // compute midpoint yaw angle
    //                double yaw_mid = cpose[2] + 0.5 * yawrate_mid * dt;

    //                // integrate position using midpoint velocities and yaw angle
    //                cpose[0] += vel_x_mid * dt * Math.Cos(yaw_mid) + vel_y_mid * dt * -Math.Sin(yaw_mid);
    //                cpose[1] += vel_x_mid * dt * Math.Sin(yaw_mid) + vel_y_mid * dt * Math.Cos(yaw_mid);

    //                // integrate yaw angle using midpoint yawrate
    //                cpose[2] += yawrate_mid * dt;
    //            }
    //            else
    //            {
    //                Console.WriteLine("invalid joint state delta time: " + dt + " sec");
    //            }
    //        }

    //        odom[0] = move_vel_x;
    //        odom[1] = move_vel_y;
    //        odom[2] = move_yawrate;

    //        last_time = current_time;
    //        last_odom = odom;

    //        MobileOmni.Odometry = odom;
    //        MobileOmni.Position = cpose;

    //        return MobileOmni;
    //    }

    //    public static JointState Omnidirectional4WInverseKinematics(double[] odom)
    //    {
    //        Matrix J = new Matrix(3, 4);
    //        Matrix JJ = new Matrix(3, 3);

    //        Matrix JTranspose = new Matrix(4, 3);
    //        Matrix JInverse = new Matrix(3, 3);
    //        Matrix JPseudo = new Matrix(4, 3);

    //        Vector Odom = new Vector(3);
    //        Vector Result = new Vector(4);

    //        double[] rpm = { 0, 0, 0, 0 };

    //        J[0, 0] = (float)GlobalsOmnidirectional.sin;
    //        J[0, 1] = -(float)GlobalsOmnidirectional.cos;
    //        J[0, 2] = -(float)GlobalsOmnidirectional.sin;
    //        J[0, 3] = (float)GlobalsOmnidirectional.cos;

    //        J[1, 0] = -(float)GlobalsOmnidirectional.cos;
    //        J[1, 1] = -(float)GlobalsOmnidirectional.sin;
    //        J[1, 2] = (float)GlobalsOmnidirectional.cos;
    //        J[1, 3] = (float)GlobalsOmnidirectional.sin;

    //        J[2, 0] = 1;
    //        J[2, 1] = 1;
    //        J[2, 2] = 1;
    //        J[2, 3] = 1;

    //        //J.ConsoleWrite();

    //        JTranspose.Set = J.Transpose;
    //        //JTranspose.ConsoleWrite();

    //        JJ.Set = J.Times(JTranspose.Get);
    //        //JJ.ConsoleWrite();

    //        JInverse.Set = JJ.Inverse;
    //        //JInverse.ConsoleWrite();

    //        JPseudo.Set = JTranspose.Times(JInverse.Get);
    //        //JPseudo.ConsoleWrite();

    //        Odom[0] = (float)odom[0];
    //        Odom[1] = (float)odom[1];
    //        Odom[2] = (float)odom[2];

    //        //Odom.ConsoleWrite();

    //        Result.Set = JPseudo.Times(Odom.Get);
    //        //Result.ConsoleWrite();

    //        rpm[0] = (double)(Result[0] / (2 * Math.PI * (GlobalsOmnidirectional.WheelDiameter / 2)) * 60);
    //        rpm[1] = (double)(Result[1] / (2 * Math.PI * (GlobalsOmnidirectional.WheelDiameter / 2)) * 60);
    //        rpm[2] = (double)(Result[2] / (2 * Math.PI * (GlobalsOmnidirectional.WheelDiameter / 2)) * 60);
    //        rpm[3] = (double)(Result[3] / (2 * Math.PI * (GlobalsOmnidirectional.WheelDiameter / 2)) * 60);

    //        JointOmni.RPM = rpm;

    //        //Console.WriteLine("RPM:{0}, RPM:{1}, RPM:{2}, RPM:{3}", rpm[0], rpm[1], rpm[2], rpm[3]);

    //        return JointOmni;
    //    }
    //}
}
