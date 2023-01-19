using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using OpenRCF;

namespace RobotController
{
    /// <summary>
    /// Simulator.xaml の相互作用ロジック
    /// </summary>
    public partial class Simulator : Window
    {
        public Simulator()
        {
            InitializeComponent();
            glHost.Child = Camera.glControl;            
            base.Closing += WindowClosingEvent;
        }

        public new void Show()
        {
            if (base.IsActive == false) base.Show();
        }

        private void WindowClosingEvent(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Collapsed;
        }

        private void ButtonRotate_Click(object sender, RoutedEventArgs e)
        {
            sliderRotate.Value = 0;
            Camera.Angle = 0;
        }

        private void SliderRotate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Camera.Angle = (float)sliderRotate.Value;
        }

        private void ButtonZoom_Click(object sender, RoutedEventArgs e)
        {
            if (Camera.Height < 3) Camera.Height += 0.5f;
            else Camera.Height = 0;
        }

        private void SliderZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Camera.Distance = 3 + (float)sliderZoom.Value;
        }


        private void KeyDownHandler(object sender, KeyEventArgs e)
        {
            OpenRCF.Keyboard.KeyDownEvent(sender, e);
            Camera.SetSubjectPosition(OpenRCF.Keyboard.SpaceVector.Get);
        }

        private void KeyUpHandler(object sender, KeyEventArgs e)
        {
            OpenRCF.Keyboard.KeyUpEvent(sender, e);
        }

    }
}
