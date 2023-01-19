using System;
using System.Windows;
using Key = System.Windows.Input.Key;
using OpenRCF;
using static OpenRCF.Mecanum;
using System.Threading;

namespace RobotController
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        Simulator Simulator = new Simulator();

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
            Thread thread1 = new Thread(ThreadWork.DoWork);
            thread1.Start();
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
    
        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button5_Click(object sender, RoutedEventArgs e)
        {

        }


    }
}
