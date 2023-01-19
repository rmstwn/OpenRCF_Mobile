using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenRCF
{
    static class Camera
    {
        private static int GraphicsMode_AntialiasLevel = 6; // Low quality 0  ~  12 High quality
        private static GraphicsMode mode = new GraphicsMode(
            GraphicsMode.Default.ColorFormat,
            GraphicsMode.Default.Depth,
            GraphicsMode.Default.Stencil,
            GraphicsMode_AntialiasLevel,
            GraphicsMode.Default.AccumulatorFormat,
            GraphicsMode.Default.Buffers,
            GraphicsMode.Default.Stereo
        );
      
        internal static GLControl glControl = new GLControl(mode);
      
        static Camera()
        {            
            glControl.Load += glControl_Load;
            glControl.Resize += glControl_Resize;        
        }

        private static Vector3 viewedPosition;
        private static Vector3 viewPosition;
        private static Matrix4 CameraMatrix;
        private static float distance = 3;
        private static float angle = 0;
        private static float height = 1.5f;

        private static Light[] light = new Light[8];

        private class Light
        {            
            private float[] ambient = new float[4] { 0.03f, 0.03f, 0.03f, 0 };
            private float[] position = new float[4] { 0, 0, 3, 0 };
            private LightName LightName;
            private EnableCap EnableCap;

            public void SetPosition(float x, float y, float z)
            {
                position[0] = x;
                position[1] = y;
                position[2] = z;
                GL.Light(LightName, LightParameter.Position, position);
            }

            public void SetAmbient(float ambient)
            {
                this.ambient[0] = ambient;
                this.ambient[1] = ambient;
                this.ambient[2] = ambient;
                GL.Light(LightName, LightParameter.Ambient, this.ambient);
            }

            public Light(int i)
            {                
                if (i == 0) LightName = LightName.Light0;
                else if (i == 1) LightName = LightName.Light1;
                else if (i == 2) LightName = LightName.Light2;
                else if (i == 3) LightName = LightName.Light3;
                else if (i == 4) LightName = LightName.Light4;
                else if (i == 5) LightName = LightName.Light5;
                else if (i == 6) LightName = LightName.Light6;
                else if (i == 7) LightName = LightName.Light7;

                GL.Light(LightName, LightParameter.Ambient, ambient);
                GL.Light(LightName, LightParameter.Diffuse, System.Drawing.Color.Gray);
                GL.Light(LightName, LightParameter.Position, position);

                if (i == 0) EnableCap = EnableCap.Light0;
                else if (i == 1) EnableCap = EnableCap.Light1;
                else if (i == 2) EnableCap = EnableCap.Light2;
                else if (i == 3) EnableCap = EnableCap.Light3;
                else if (i == 4) EnableCap = EnableCap.Light4;
                else if (i == 5) EnableCap = EnableCap.Light5;
                else if (i == 6) EnableCap = EnableCap.Light6;
                else if (i == 7) EnableCap = EnableCap.Light7;
            }
        
            public void Enable() { GL.Enable(EnableCap); }

            public void Disable() { GL.Disable(EnableCap); }

            public void UpdatePosition()
            {
                GL.Light(LightName, LightParameter.Position, position);
            }
        }

        public static float Distance
        {
            get { return distance; }
            set
            {
                if (0 < value) distance = value;
                else distance = 0.001f;

                SettingUpdate();
            }
        }

        public static float Angle
        {
            get { return angle; }
            set
            {
                angle = value;
                SettingUpdate();
            }
        }

        public static float Height
        {
            get { return height; }
            set
            {
                height = value;
                SettingUpdate();
            }
        }

        public static void SetSubjectPosition(float[] position)
        {
            viewedPosition[0] = position[0];
            viewedPosition[1] = position[1];
            viewedPosition[2] = position[2];
            SettingUpdate();
        }

        public static void SetAmbient(float ambient)
        {
            for (int i = 0; i < light.Length; i++)
            {
                if (1 < ambient) light[i].SetAmbient(1);
                else if (ambient < 0) light[i].SetAmbient(0);
                else light[i].SetAmbient(ambient);
            }
        }

        private static void glControl_Load(object sender, EventArgs e)
        {
            GL.ClearColor(Color4.White);

            GL.Viewport(0, 0, glControl.Width, glControl.Height);
           
            viewPosition[0] = viewedPosition[0] + distance * (float)Math.Cos(angle - 0.5 * Math.PI);
            viewPosition[1] = viewedPosition[1] + distance * (float)Math.Sin(angle - 0.5 * Math.PI);
            viewPosition[2] = viewedPosition[2] + height;

            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver3, glControl.AspectRatio, 0.2f, 15);
            GL.LoadMatrix(ref proj);

            GL.MatrixMode(MatrixMode.Modelview);
            CameraMatrix = Matrix4.LookAt(viewPosition, viewedPosition, Vector3.UnitZ);
            GL.LoadMatrix(ref CameraMatrix);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Lighting);        
            GL.ShadeModel(ShadingModel.Smooth);
         
            for (int i = 0; i < light.Length; i++)
            {
                light[i] = new Light(i);
            }
              
            light[0].SetPosition(12, 9, 3);
            light[1].SetPosition(9, -12, 12);
            light[2].SetPosition(-12, 9, 12);
            light[3].SetPosition(-9, -12, -3);
            light[4].SetPosition(0, 0, -12);

            light[0].Enable();
            light[1].Enable();
            light[2].Enable();
            light[3].Enable();
            light[4].Enable();

            GL.EnableClientState(ArrayCap.VertexArray);
        }

        private static void glControl_Resize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, glControl.Width, glControl.Height);
        }

        private static void SettingUpdate()
        {
            viewPosition[0] = viewedPosition[0] + distance * (float)Math.Cos(angle - 0.5 * Math.PI);
            viewPosition[1] = viewedPosition[1] + distance * (float)Math.Sin(angle - 0.5 * Math.PI);
            viewPosition[2] = viewedPosition[2] + height;
           
            CameraMatrix = Matrix4.LookAt(viewPosition, viewedPosition, Vector3.UnitZ);
            GL.LoadMatrix(ref CameraMatrix);

            for (int i = 0; i < light.Length; i++)
            {
                light[i].UpdatePosition();
            }
        }

        internal static void DisplayUpdate()
        {
            glControl.SwapBuffers();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

    }

}
