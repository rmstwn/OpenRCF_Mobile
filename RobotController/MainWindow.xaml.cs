using System;
using System.Windows;
using Key = System.Windows.Input.Key;
using OpenRCF;
using System.Threading;
using System.Runtime.InteropServices;
using static OpenRCF.SerialDevice;
using static OpenRCF.Mobile;

namespace RobotController   
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public static uint CRANEX7_8_OpenRCF = 7; //8
        public uint BW = 2;


        Simulator Simulator = new Simulator();

        //Thread thread1 = new Thread(ThreadWork.DoWork);
        //Thread thread2 = new Thread(ThreadWork.DoWork2);
        //Thread thread5 = new Thread(ThreadWork.StopWork);

        Robot CRANEX7 = new Robot(new uint[2] { 3, CRANEX7_8_OpenRCF });

        public MainWindow()
        {
            InitializeComponent();
            Loaded += InitializeOpenRCF;

            CRANEX7.Kinematics.BasePosition[0] = -0.5f;
            CRANEX7.Kinematics.BasePosition[1] = -0.25f;

            CRANEX7.SetPlanarJoint3DOF(0.25f, 0.25f, 0.1f);

            CRANEX7[1, 0].lInit.Set = new float[3] { 0, 0, 0 };
            CRANEX7[1, 0].axisInit.SetUnitVectorZ();

            CRANEX7[1, 1].lInit.Set = new float[3] { 0, 0, 0.05f * BW };
            CRANEX7[1, 1].axisInit.SetUnitVectorX(-1);

            CRANEX7[1, 2].lInit.Set = new float[3] { 0, 0, 0.05f * BW };
            CRANEX7[1, 2].axisInit.SetUnitVectorZ();

            CRANEX7[1, 3].lInit.Set = new float[3] { 0, 0, 0.20f * BW };
            CRANEX7[1, 3].axisInit.SetUnitVectorX(-1);

            CRANEX7[1, 4].lInit.Set = new float[3] { 0, 0, 0.135f * BW };
            CRANEX7[1, 4].axisInit.SetUnitVectorZ();

            CRANEX7[1, 5].lInit.Set = new float[3] { 0, 0, 0.115f * BW };
            CRANEX7[1, 5].axisInit.SetUnitVectorX(-1);

            CRANEX7[1, 6].lInit.Set = new float[3] { 0, 0, 0.02f * BW };
            CRANEX7[1, 6].axisInit.SetUnitVectorZ();

            CRANEX7[1, 7].lInit.Set = new float[3] { 0.002f * BW, 0, 0.02f * BW };
            CRANEX7[1, 7].axisInit.SetUnitVectorX(-1);

            //RealARM[1, 0].JointRange[0] = Dynamixel_kyotsu.DyAngle2rad4servo(300);
            //RealARM[1, 0].JointRange[1] = Dynamixel_kyotsu.DyAngle2rad4servo(3800);

            //RealARM[1, 1].JointRange[0] = Dynamixel_kyotsu.DyAngle2rad4servo(1024);
            //RealARM[1, 1].JointRange[1] = Dynamixel_kyotsu.DyAngle2rad4servo(3072);

            //RealARM[1, 2].JointRange[0] = Dynamixel_kyotsu.DyAngle2rad4servo(300);
            //RealARM[1, 2].JointRange[1] = Dynamixel_kyotsu.DyAngle2rad4servo(3800);

            //RealARM[1, 3].JointRange[0] = Dynamixel_kyotsu.DyAngle2rad4servo(270);
            //RealARM[1, 3].JointRange[1] = Dynamixel_kyotsu.DyAngle2rad4servo(2048);

            //RealARM[1, 4].JointRange[0] = Dynamixel_kyotsu.DyAngle2rad4servo(300);
            //RealARM[1, 4].JointRange[1] = Dynamixel_kyotsu.DyAngle2rad4servo(3800);

            //RealARM[1, 5].JointRange[0] = Dynamixel_kyotsu.DyAngle2rad4servo(1024);
            //RealARM[1, 5].JointRange[1] = Dynamixel_kyotsu.DyAngle2rad4servo(3072);

            //RealARM[1, 6].JointRange[0] = Dynamixel_kyotsu.DyAngle2rad4servo(200);
            //RealARM[1, 6].JointRange[1] = Dynamixel_kyotsu.DyAngle2rad4servo(3900);

            //RealARM[1, 7].JointRange[0] = Dynamixel_kyotsu.DyAngle2rad4servo(2048);
            //RealARM[1, 7].JointRange[1] = Dynamixel_kyotsu.DyAngle2rad4servo(3072);

            CRANEX7.Kinematics.Target[1].Priority = false;

            CRANEX7.Kinematics.ForwardKinematics();
            CRANEX7.Kinematics.SetJointLinkRadiusAuto();     //defo0.2
            CRANEX7.Kinematics.ResetTargets();

        }

        void InitializeOpenRCF(object sender, RoutedEventArgs e)
        {
            Core.SetFPS(30);
            Core.SetDrawFunction = Draw;
            Simulator.Owner = this;
            Simulator.Show();
        }
     
        private void Draw()
        {
            CRANEX7.Draw();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            //thread1.Start();
            CRANEX7.Kinematics.ResetHomePosition();
            CRANEX7.Kinematics.ForwardKinematics();
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            //thread1.Abort();
            //thread2.Start();

            CRANEX7.Kinematics.Target[1].Position[0] = 0.5f;
            CRANEX7.Kinematics.Target[1].Position[1] = 0;
            CRANEX7.Kinematics.Target[1].Position[2] = 0.6f;
            //CRANEX7.Kinematics.Target[1].Rotate.SetRy(0.5*Math.PI);

            CRANEX7.Kinematics.InverseKinematics();

        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            //thread1.Abort();
            //thread2.Abort();

            //thread5.Start();
        }


    }
}
