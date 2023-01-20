using System;
using System.Windows;
using Key = System.Windows.Input.Key;
using OpenRCF;
using static OpenRCF.Mecanum;
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
        Simulator Simulator = new Simulator();

        Thread thread1 = new Thread(ThreadWork.DoWork);
        Thread thread2 = new Thread(ThreadWork.DoWork2);
        Thread thread5 = new Thread(ThreadWork.StopWork);

        public MainWindow()
        {
            InitializeComponent();
            Loaded += InitializeOpenRCF;
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

        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            thread1.Start();
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            thread1.Abort();
            thread2.Start();
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            thread1.Abort();
            thread2.Abort();

            thread5.Start();
        }


    }
}
