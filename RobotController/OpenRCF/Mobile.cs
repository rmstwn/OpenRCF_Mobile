using OpenRCF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        static double last_time;
        static double[] last_odom;
        static double dt;
        public static void Mecanum4WForwardKinematics(double[] velocity, double[] odom, double[] cpose)
        {
            double current_time = (int)DateTime.Now.Subtract(new DateTime(2023, 1, 1)).TotalMilliseconds;


            /* Mecanum Forward Kinematics

                l = l1 + l2

                             |                       |   | w1 |
                | vx |       |  1    1    1     1    | . | w2 |
                | vy | = R/4 | -1    1    1    -1    |   | w3 |
                | wz |       | 1/l  -1/l  1/l  -1/l  |   | w4 |
            */

            //velocities:
            double move_vel_x = (velocity[0] + velocity[1] + velocity[2] + velocity[3]) * Globals.Diam / 8;
            double move_vel_y = (velocity[1] - velocity[0] - velocity[3] + velocity[2]) * Globals.Diam / 8;
            double move_yawrate = (velocity[0] - velocity[1] + velocity[2] - velocity[3]) * Globals.Diam / 4 / (Globals.Axis1Length + Globals.Axis2Length);
            
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

            Console.WriteLine("posX: " + cpose[0].ToString() + " posY: " + cpose[1].ToString() + " posYaw: " + cpose[2].ToString());
        }

        public static void Mecanum4WInverseKinematics()
        {

        }

        public class ThreadWork
        {
            static double[] vel = { -0.1, 0.1, 0.1, -0.1 };
            static double[] odom = { 0, 0, 0 };
            static double[] cpose = { 0, 0, 0 };

            public static void DoWork()

            {
                for(;;)
                {
                    Mecanum.Mecanum4WForwardKinematics(vel, odom, cpose);
                    Thread.Sleep(100);
                }
            }
        }
    }
}
