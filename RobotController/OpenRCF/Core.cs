using System;
using System.Reflection;
using System.Windows.Threading;

namespace OpenRCF
{   
    public static class Core
    {
        private static DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Normal);
        private static EventHandler eventHandler;
        private static uint FPS = 30;
       
        static Core()
        {
            timer.Tick += eventHandler;
            Console.WriteLine("OpenRCF is released under the MIT license.");
        }
                
        public static Rectangle Tile = new Rectangle(6, 6);
        public static ThreeAxis ReferenceFrame = new ThreeAxis(0.1f);
        
        public static Action SetDrawFunction
        {
            set
            {               
                timer.Tick -= eventHandler;
                eventHandler = (sender, e) => 
                {
                    Tile.DrawLineNet(11);
                    ReferenceFrame.Draw();
                    value();
                    Camera.DisplayUpdate();
                };
                timer.Tick += eventHandler;
                timer.Interval = TimeSpan.FromMilliseconds(1000 / FPS);
                timer.Start();
            }
        }

        public static void SetFPS(uint FPS)
        {
            if(0 < FPS && FPS <= 50)
            {
                Core.FPS = FPS;
                timer.Interval = TimeSpan.FromMilliseconds(1000 / FPS);
            }
            else
            {
                Console.WriteLine("Error : " + MethodBase.GetCurrentMethod().Name);
                Console.WriteLine("Specify FPS between 1 and 50.");
            }
        }
    }

}
