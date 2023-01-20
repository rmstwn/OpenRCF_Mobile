﻿using OpenRCF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Navigation;
using static OpenRCF.Mobile;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace OpenRCF
{
    public static class Globals
    {
        public static double Axis1Length = 0.27;
        public static double Axis2Length = 0.25;
        public static double Diam = 0.1;
    }

    public class Mobile
    {
        //static void Main(string[] args)
        //{
        //    Mecanum.Mecanum4WForwardKinematics();
        //}
        public class MobileInfo
        {
            public double[] Odometry;
            public double[] Position;
        }

        public class JointState
        {
            public double[] Velocity;
        }
    }

    public class Mecanum
    {
        /*
              robot:	l1: Axis1Length
                        l2: Axis2Length

                   --|##2##|        |##4##|
                   ^   ##################             y
               l1  ¦   ##################             ^
                   ¦   ##################   front     ¦
                   v   ##################             ¦
                   --|##1##|        |##3##|          -¦-----> x
                        |       l2     |
                        |<------------>|
         */


        static double[] odom = { 0, 0, 0 };
        static double[] cpose = { 0, 0, 0 };

        static double last_time;
        static double[] last_odom;
        static double dt;

        static public MobileInfo Mobile = new MobileInfo();
        static public JointState Joint = new JointState();

        public static MobileInfo Mecanum4WForwardKinematics(double[] velocity)
        {
            /* Mecanum Forward Kinematics

                l = l1 + l2

                             |                       |   | w1 |
                | vx |       |  1    1    1     1    | . | w2 |
                | vy | = R/4 | -1    1    1    -1    |   | w3 |
                | wz |       | -1/l  1/l -1/l  1/l  |   | w4 |
            */

            double current_time = (int)DateTime.Now.Subtract(new DateTime(2023, 1, 1)).TotalMilliseconds;

            //velocities:
            double move_vel_x = (velocity[0] + velocity[1] + velocity[2] + velocity[3]) * Globals.Diam / 8;
            double move_vel_y = (velocity[1] - velocity[0] - velocity[3] + velocity[2]) * Globals.Diam / 8;
            double move_yawrate = (-velocity[0] + velocity[1] - velocity[2] + velocity[3]) * Globals.Diam / 4 / (Globals.Axis1Length + Globals.Axis2Length);
            
            //positions:
            if (Mecanum.last_time > 0 || Mecanum.last_time < 0)
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

            Mobile.Odometry = odom;
            Mobile.Position = cpose;

            return Mobile;

            //Console.WriteLine("posX: " + cpose[0].ToString() + " posY: " + cpose[1].ToString() + " posYaw: " + cpose[2].ToString());
        }

        public static JointState Mecanum4WInverseKinematics(double[] odom)
        {
            /* Mecanum Forward Kinematics
               | w1 |       | 1 -1 -(l1+l2) |   
               | w2 | = 1/R | 1  1  (l1+l2) | . | vx |
               | w3 |       | 1  1 -(l1+l2) |   | vy |
               | w4 |       | 1 -1  (l1+l2) |   | wz |
            */

            double linearX = odom[0];
            double linearY = odom[1];
            double angularZ = odom[2];

            double[] vel = { 0, 0, 0, 0 };

            // w1:
            vel[0] = 2 / Globals.Diam * (linearX - linearY - (Globals.Axis1Length + Globals.Axis2Length) / 2 * angularZ);
            // w2:
            vel[1] = 2 / Globals.Diam * (linearX + linearY + (Globals.Axis1Length + Globals.Axis2Length) / 2 * angularZ);
            // w3:
            vel[2] = 2 / Globals.Diam * (linearX + linearY - (Globals.Axis1Length + Globals.Axis2Length) / 2 * angularZ);
            // w4:
            vel[3] = 2 / Globals.Diam * (linearX - linearY + (Globals.Axis1Length + Globals.Axis2Length) / 2 * angularZ);

            Joint.Velocity = vel;

            //Console.WriteLine("w1: " + velocity[0].ToString() + " w2: " + velocity[1].ToString() + " w3: " + velocity[2].ToString() + " w3: " + velocity[3].ToString());

            return Joint;

        }

        public class ThreadWork
        {
            static double[] vel = { 0, 0, 0, 0 };
            static int[] TargetVel = { 0, 0, 0, 0 };
            static int[] CurrentVel = { 0, 0, 0, 0 };

            static double[] TargetOdom = { 0, 0, 0 };

            static SerialDevice.Dynamixel Dynamixel = new SerialDevice.Dynamixel(1000000);
            static byte[] id = new byte[4] { 11, 12, 13, 14 };

            public static void DoWork()
            {
                Dynamixel.PortOpen("COM3");
                Dynamixel.TorqueEnable(id);

                //Dynamixel.VelocityControlMode(id);

                TargetVel[0] = 30;
                TargetVel[1] = 30;
                TargetVel[2] = 30;
                TargetVel[3] = 30;

                TargetOdom[0] = 1.5;
                TargetOdom[1] = 0;
                TargetOdom[2] = 0;

                Joint = Mecanum4WInverseKinematics(TargetOdom);

                TargetVel[0] = (int)Joint.Velocity[0];
                TargetVel[1] = (int)Joint.Velocity[1];
                TargetVel[2] = (int)Joint.Velocity[2];
                TargetVel[3] = (int)Joint.Velocity[3];

                for (;;)
                {
                    Dynamixel.WriteVelocity(id, TargetVel);
                    Dynamixel.RequestVelocityReply(id);

                    CurrentVel = Dynamixel.Velocity(id);

                    vel[0] = CurrentVel[0];
                    vel[1] = CurrentVel[1];
                    vel[2] = CurrentVel[2];
                    vel[3] = CurrentVel[3];

                    Mobile = Mecanum.Mecanum4WForwardKinematics(vel);

                    Console.WriteLine("Velocity:{0}, Velocity:{1}, Velocity:{2}, Velocity:{3}", CurrentVel[0], CurrentVel[1], CurrentVel[2], CurrentVel[3]);
                    Console.WriteLine("OdomX:{0}, OdomY:{1}, OdomZ:{2}", Mobile.Odometry[0], Mobile.Odometry[1], Mobile.Odometry[2]);
                    Console.WriteLine("PosX:{0}, PosY:{1}, PosZ:{2}", Mobile.Position[0], Mobile.Position[1], Mobile.Position[2]);


                    Thread.Sleep(10);
                }
            }

            public static void DoWork2()
            {
                Dynamixel.PortOpen("COM3");
                Dynamixel.TorqueEnable(id);

                //Dynamixel.VelocityControlMode(id);

                TargetVel[0] = 30;
                TargetVel[1] = 30;
                TargetVel[2] = 30;
                TargetVel[3] = 30;

                TargetOdom[0] = 2.5;
                TargetOdom[1] = 0;
                TargetOdom[2] = 0;

                Joint = Mecanum4WInverseKinematics(TargetOdom);

                TargetVel[0] = (int)Joint.Velocity[0];
                TargetVel[1] = (int)Joint.Velocity[1];
                TargetVel[2] = (int)Joint.Velocity[2];
                TargetVel[3] = (int)Joint.Velocity[3];


                for (; ; )
                {
                    Dynamixel.WriteVelocity(id, TargetVel);
                    Dynamixel.RequestVelocityReply(id);

                    CurrentVel = Dynamixel.Velocity(id);

                    vel[0] = CurrentVel[0];
                    vel[1] = CurrentVel[1];
                    vel[2] = CurrentVel[2];
                    vel[3] = CurrentVel[3];

                    Mobile = Mecanum.Mecanum4WForwardKinematics(vel);

                    Console.WriteLine("Velocity:{0}, Velocity:{1}, Velocity:{2}, Velocity:{3}", CurrentVel[0], CurrentVel[1], CurrentVel[2], CurrentVel[3]);
                    Console.WriteLine("OdomX:{0}, OdomY:{1}, OdomZ:{2}", Mobile.Odometry[0], Mobile.Odometry[1], Mobile.Odometry[2]);
                    Console.WriteLine("PosX:{0}, PosY:{1}, PosZ:{2}", Mobile.Position[0], Mobile.Position[1], Mobile.Position[2]);


                    Thread.Sleep(10);
                }
            }

            public static void StopWork()
            {
                Dynamixel.PortOpen("COM3");
                Dynamixel.TorqueEnable(id);

                TargetVel[0] = 0;
                TargetVel[1] = 0;
                TargetVel[2] = 0;
                TargetVel[3] = 0;

                TargetOdom[0] = 0;
                TargetOdom[1] = 0;
                TargetOdom[2] = 0;

                Joint = Mecanum4WInverseKinematics(TargetOdom);

                TargetVel[0] = (int)Joint.Velocity[0];
                TargetVel[1] = (int)Joint.Velocity[1];
                TargetVel[2] = (int)Joint.Velocity[2];
                TargetVel[3] = (int)Joint.Velocity[3];

                for (; ; )
                {
                    Dynamixel.WriteVelocity(id, TargetVel);
                    Dynamixel.RequestVelocityReply(id);

                    CurrentVel = Dynamixel.Velocity(id);

                    vel[0] = CurrentVel[0];
                    vel[1] = CurrentVel[1];
                    vel[2] = CurrentVel[2];
                    vel[3] = CurrentVel[3];

                    Mobile = Mecanum.Mecanum4WForwardKinematics(vel);

                    Console.WriteLine("Velocity:{0}, Velocity:{1}, Velocity:{2}, Velocity:{3}", CurrentVel[0], CurrentVel[1], CurrentVel[2], CurrentVel[3]);
                    Console.WriteLine("OdomX:{0}, OdomY:{1}, OdomZ:{2}", Mobile.Odometry[0], Mobile.Odometry[1], Mobile.Odometry[2]);
                    Console.WriteLine("PosX:{0}, PosY:{1}, PosZ:{2}", Mobile.Position[0], Mobile.Position[1], Mobile.Position[2]);


                    Thread.Sleep(10);
                }
            }
        }
    }
}
