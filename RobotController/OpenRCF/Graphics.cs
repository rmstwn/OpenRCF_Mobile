using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenRCF
{
    static class Float3
    {
        public static float Distance(float[] v1, float[] v2)
        {
            float[] e = new float[3] { v1[0] - v2[0], v1[1] - v2[1], v1[2] - v2[2] };
            return (float)Math.Sqrt(e[0] * e[0] + e[1] * e[1] + e[2] * e[2]);
        }

        public static float Dot(float[] v1, float[] v2)
        {
            return v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2];
        }

        public static float[] Cross(float[] v1, float[] v2)
        {
            float[] result = new float[3];

            result[0] = v1[1] * v2[2] - v1[2] * v2[1];
            result[1] = v1[2] * v2[0] - v1[0] * v2[2];
            result[2] = v1[0] * v2[1] - v1[1] * v2[0];

            return result;
        }

        public static float Norm(float[] v)
        {
            return (float)Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
        }

        public static float[] Plus(float[] v1, float[] v2)
        {
            return new float[3] { v1[0] + v2[0], v1[1] + v2[1], v1[2] + v2[2] };            
        }

        public static float[] Minus(float[] v1, float[] v2)
        {           
            return new float[3] { v1[0] - v2[0], v1[1] - v2[1], v1[2] - v2[2] };
        }       

        public static float Sum(float[] v)
        {
            return v[0] + v[1] + v[2];
        }

        public static float AbsSum(float[] v)
        {
            return Math.Abs(v[0]) + Math.Abs(v[1]) + Math.Abs(v[2]);
        }

        public static float[] Times(float a, float[] v)
        {
            return new float[3] { a * v[0], a * v[1], a * v[2] };
        }

    }

    public class Color
    {
        private Color4 line = Color4.Black;
        private Color4 body = Color4.Gray;
        
        public void SetRed() { body = Color4.Red; }
        public void SetGreen() { body = Color4.Green; }
        public void SetBlue() { body = Color4.Blue; }
        public void SetYellow() { body = Color4.Yellow; }
        public void SetYellowGreen() { body = Color4.YellowGreen; }
        public void SetSkyBlue() { body = Color4.SkyBlue; }
        public void SetBeige() { body = Color4.Beige; }
        public void SetBrown() { body = Color4.Brown; }
        public void SetIvory() { body = Color4.Ivory; }
        public void SetOrange() { body = Color4.Orange; }
        public void SetPeru() { body = Color4.Peru; }
        public void SetPurple() { body = Color4.Purple; }
        public void SetSilver() { body = Color4.Silver; }
        public void SetGold() { body = Color4.Gold; }
        public void SetBlack() { body = Color4.Black; }
        public void SetWhite() { body = Color4.White; }
        public void SetLightGray() { body = Color4.LightGray; }
        public void SetGray() { body = Color4.Gray; }
        public void SetDarkGray() { body = Color4.DarkGray; }
        public void SetRGB(byte R, byte G, byte B, byte A = 0)
        {
            body = System.Drawing.Color.FromArgb(A, R, G, B);
        }

        internal void EnableBodyColor()
        {
            GL.Material(MaterialFace.Front, MaterialParameter.AmbientAndDiffuse, body);
        }

        internal void EnableLineColor()
        {
            GL.Material(MaterialFace.Front, MaterialParameter.Diffuse, line);
        }
    }

    public interface IContact
    {
        float[] Intersection(float[] p1, float[] p2);
    }

    public interface ICollision
    {
        float[][] AllVertex { get; }
        bool IsCollision(float[] position, float threshold = 0);
        bool IsCollision(params ICollision[] iCollision);
    }
    
    public class RoundObjectBase : ICollision
    {
        public Vector Position = new Vector(3);
        public RotationMatrix Rotate = new RotationMatrix();
        public Color Color = new Color();
        public float LineWidth = 1;      
                
        private static int vertexNum = 16;
        protected float[][] VertexT = new float[vertexNum][];
        protected float[][] VertexC = new float[vertexNum][];
        protected float[][] VertexB = new float[vertexNum][];
        protected float[][] Normal = new float[vertexNum][];

        private int[] checkCode = new int[8];

        private int[] CheckCode()
        {
            int[] result = new int[8];

            result[0] = (int)(10000 * Position[0]);
            result[1] = (int)(10000 * Position[1]);
            result[2] = (int)(10000 * Position[2]);
            result[3] = (int)(10000 * Rotate[0, 0]);
            result[4] = (int)(10000 * Rotate[1, 1]);
            result[5] = (int)(10000 * Rotate[2, 2]);
            result[6] = Size.CheckCode;
            result[7] = Offset.CheckCode;
     
            return result;
        }

        private bool IsChanged()
        {
            int[] checkCodeNow = CheckCode();

            for (int i = 0; i < checkCode.Length; i++)
            {
                if (checkCode[i] != checkCodeNow[i])
                {
                    checkCode = checkCodeNow;
                    return true;
                }
            }

            return false;
        }

        private OffsetHandler Offset = new OffsetHandler();

        public void SetPositionOffset(float x, float y, float z)
        {
            Offset.SetPosition(x, y, z);
        }

        public void SetRotateOffset(float roll, float pitch, float yaw)
        {
            Offset.SetRotate(roll, pitch, yaw);
        }

        private class OffsetHandler
        {
            private float[] position = new float[3];
            private float[,] rotate = new float[3, 3] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };
            private int checkCode = int.MinValue;

            public void SetPosition(float x, float y, float z)
            {
                position[0] = x;
                position[1] = y;
                position[2] = z;
                checkCode++;
            }

            public void SetRotate(float roll, float pitch, float yaw)
            {
                float Sr = (float)Math.Sin(roll);
                float Sp = (float)Math.Sin(pitch);
                float Sy = (float)Math.Sin(yaw);

                float Cr = (float)Math.Cos(roll);
                float Cp = (float)Math.Cos(pitch);
                float Cy = (float)Math.Cos(yaw);

                rotate[0, 0] = Cy * Cp;
                rotate[1, 0] = Sy * Cp;
                rotate[2, 0] = -Sp;

                rotate[0, 1] = Cy * Sp * Sr - Sy * Cr;
                rotate[1, 1] = Sy * Sp * Sr + Cy * Cr;
                rotate[2, 1] = Cp * Sr;

                rotate[0, 2] = Cy * Sp * Cr + Sy * Sr;
                rotate[1, 2] = Sy * Sp * Cr - Cy * Sr;
                rotate[2, 2] = Cp * Cr;

                checkCode++;
            }

            public int CheckCode { get { return checkCode; } }

            public float[] Position { get { return position; } }

            public float[] Plus(float[] p)
            {
                return new float[3] { position[0] + p[0], position[1] + p[1], position[2] + p[2] };
            }

            public float[] Rotate(float[] p)
            {
                float[] result = new float[3];
                result[0] = rotate[0, 0] * p[0] + rotate[0, 1] * p[1] + rotate[0, 2] * p[2];
                result[1] = rotate[1, 0] * p[0] + rotate[1, 1] * p[1] + rotate[1, 2] * p[2];
                result[2] = rotate[2, 0] * p[0] + rotate[2, 1] * p[1] + rotate[2, 2] * p[2];
                return result;
            }

            public float[] TransposeRotate(float[] p)
            {
                float[] result = new float[3];
                result[0] = rotate[0, 0] * p[0] + rotate[1, 0] * p[1] + rotate[2, 0] * p[2];
                result[1] = rotate[0, 1] * p[0] + rotate[1, 1] * p[1] + rotate[2, 1] * p[2];
                result[2] = rotate[0, 2] * p[0] + rotate[1, 2] * p[1] + rotate[2, 2] * p[2];
                return result;
            }

            public float[] PlusRotate(float[] p)
            {
                return Plus(Rotate(p));
            }
        }

        public float Height
        {
            get { return Size.Height; }
            set { Size.Height = value; }
        }

        protected SizeHandler Size = new SizeHandler();

        protected class SizeHandler
        {
            private float radiusBottom = 0.2f;
            private float radiusTop = 0.2f;
            private float height = 0.5f;
            private int checkCode = int.MinValue;
            public int CheckCode { get { return checkCode; } }

            public float RadiusBottom
            {
                get { return radiusBottom; }
                set 
                { 
                    radiusBottom = value;
                    checkCode++;
                }
            }

            public float RadiusTop
            {
                get { return radiusTop; }
                set                 
                { 
                    radiusTop = value;
                    checkCode++;
                }
            }

            public float Height
            {
                get { return height; }
                set
                {
                    height = value;
                    checkCode++;
                }
            }

            public float Radius
            {
                get { return 0.5f * (radiusTop + radiusBottom); }
                set
                {
                    RadiusTop = value;
                    RadiusBottom = value;
                }
            }

            public float RadiusMax
            {
                get 
                {
                    if (radiusTop < radiusBottom) return radiusBottom;
                    else return radiusTop;
                }
            }

            public float RadiusMin
            {
                get
                {
                    if (radiusTop < radiusBottom) return radiusTop;
                    else return radiusBottom;
                }
            }
        }

        public float[] PositionWithOffset
        {
            get { return Position.Plus(Rotate.Times(Offset.Position)); }
        }

        private static class BaseVertex
        {
            public static float[][] VertexT = new float[vertexNum][];
            public static float[][] VertexB = new float[vertexNum][];

            static BaseVertex()
            {
                VertexT[0] = new float[3] { 0, 0, 0.5f };
                VertexB[0] = new float[3] { 0, 0, -0.5f };

                float stepAngle = 2 * (float)Math.PI / (vertexNum - 1);
                float angle = 0;

                for (int i = 1; i < vertexNum; i++)
                {
                    VertexT[i] = new float[3] { (float)Math.Cos(angle), (float)Math.Sin(angle), 0.5f };
                    VertexB[i] = new float[3] { (float)Math.Cos(angle), (float)Math.Sin(angle), -0.5f };
                    angle += stepAngle;
                }
            }
        }

        protected void VertexUpdate()
        {
            if (IsChanged())
            {
                float[] vertex = new float[3];

                for (int i = 0; i < vertexNum; i++)
                {
                    vertex[0] = Size.RadiusTop * BaseVertex.VertexT[i][0];
                    vertex[1] = Size.RadiusTop * BaseVertex.VertexT[i][1];
                    vertex[2] = Size.Height * BaseVertex.VertexT[i][2];
                    VertexT[i] = Position.Plus(Rotate.Times(Offset.PlusRotate(vertex)));

                    vertex[0] = Size.RadiusBottom * BaseVertex.VertexB[i][0];
                    vertex[1] = Size.RadiusBottom * BaseVertex.VertexB[i][1];
                    vertex[2] = Size.Height * BaseVertex.VertexB[i][2];
                    VertexB[i] = Position.Plus(Rotate.Times(Offset.PlusRotate(vertex)));

                    vertex[0] = 0.5f * (VertexT[i][0] + VertexB[i][0]);
                    vertex[1] = 0.5f * (VertexT[i][1] + VertexB[i][1]);
                    vertex[2] = 0.5f * (VertexT[i][2] + VertexB[i][2]);
                    VertexC[i] = new float[3] { vertex[0], vertex[1], vertex[2] };

                    Normal[i] = Rotate.Times(Offset.Rotate(BaseVertex.VertexT[i]));
                }

                Normal[0] = Rotate.Times(Offset.Rotate(new float[3] { 0, 0, 1 }));
            }
        }

        private void DrawVertexT(PrimitiveType primitiveType)
        {
            GL.Normal3(Normal[0]);
            GL.Begin(primitiveType);
            for (int i = 1; i < vertexNum; i++)
            {
                GL.Vertex3(VertexT[i]);
            }
            GL.End();
        }

        private void DrawVertexB(PrimitiveType primitiveType)
        {
            GL.Normal3(-Normal[0][0], -Normal[0][1], -Normal[0][2]);
            GL.Begin(primitiveType);
            for (int i = 1; i < vertexNum; i++)
            {
                GL.Vertex3(VertexB[i]);
            }
            GL.End();
        }

        private void DrawVertexTB(PrimitiveType primitiveType)
        {
            GL.Begin(primitiveType);
            for (int i = 1; i < vertexNum; i++)
            {
                GL.Normal3(Normal[i]);
                GL.Vertex3(VertexT[i]);
                GL.Vertex3(VertexB[i]);
            }
            GL.Normal3(Normal[1]);
            GL.Vertex3(VertexT[1]);
            GL.Vertex3(VertexB[1]);
            GL.End();
        }

        public void DrawTop()
        {
            VertexUpdate();
            Color.EnableBodyColor();
            DrawVertexT(PrimitiveType.TriangleFan);
        }

        public void DrawBottom()
        {
            VertexUpdate();
            Color.EnableBodyColor();
            DrawVertexB(PrimitiveType.TriangleFan);
        }

        public void DrawLine()
        {
            if (0 < LineWidth)
            {
                VertexUpdate();
                Color.EnableLineColor();
                GL.LineWidth(LineWidth);
                DrawVertexT(PrimitiveType.LineLoop);
                DrawVertexB(PrimitiveType.LineLoop);
            }
        }

        public void DrawSide()
        {
            VertexUpdate();
            Color.EnableBodyColor();
            DrawVertexTB(PrimitiveType.TriangleStrip);
        }

        public void Draw()
        {
            VertexUpdate();

            if (0 < LineWidth)
            {
                Color.EnableLineColor();
                GL.LineWidth(LineWidth);
                DrawVertexT(PrimitiveType.LineLoop);
                DrawVertexB(PrimitiveType.LineLoop);
            }

            Color.EnableBodyColor();
            DrawVertexT(PrimitiveType.TriangleFan);
            DrawVertexB(PrimitiveType.TriangleFan);
            DrawVertexTB(PrimitiveType.TriangleStrip);
        }

        public void DrawStripes()
        {
            VertexUpdate();

            if (0 < LineWidth)
            {
                Color.EnableLineColor();
                GL.LineWidth(LineWidth);
                DrawVertexT(PrimitiveType.LineLoop);
                DrawVertexB(PrimitiveType.LineLoop);
                DrawVertexTB(PrimitiveType.Lines);
            }

            Color.EnableBodyColor();
            DrawVertexTB(PrimitiveType.TriangleStrip);
        }

        private float RadiusZ(float z)
        {
            if (0.0001f < Height)
            {
                float t = 0.5f - (z / Height);
                return t * Size.RadiusBottom + (1 - t) * Size.RadiusTop;
            }
            else
            {
                return 0.5f * (Size.RadiusBottom + Size.RadiusTop);
            }
        }

        public float BoundSphereRadius
        {
            get
            {
                float h = 0.5f * Height;
                float r = Size.RadiusMax;
                return (float)Math.Sqrt(h * h + r * r);
            }
        }
        
        public float[] PositionFromThis(float[] position)
        {
            return Offset.TransposeRotate(Rotate.TransposeTimes(Float3.Minus(position, PositionWithOffset)));    
        }

        public float[][] AllVertex
        {
            get
            {
                float[][] result = new float[3 * vertexNum][];

                VertexUpdate();

                for (int i = 0; i < vertexNum; i++)
                {
                    result[3 * i + 0] = VertexT[i];
                    result[3 * i + 1] = VertexC[i];
                    result[3 * i + 2] = VertexB[i];
                }

                return result;
            }
        }

        public bool IsCollision(float[] position, float threshold = 0)
        {
            float[] pf = PositionFromThis(position);
            float r = (float)Math.Sqrt(pf[0] * pf[0] + pf[1] * pf[1]);

            if (0.5f * Height + threshold < Math.Abs(pf[2])) return false;
            if (RadiusZ(pf[2]) + threshold < r) return false;

            return true;
        }

        public bool IsCollision(params ICollision[] obstacle)
        {
            float[][] allVertex;
            float[] p = PositionWithOffset;
            float r = BoundSphereRadius;

            for (int i = 0; i < obstacle.Length; i++)
            {
                if (obstacle[i].IsCollision(p, r))
                {
                    allVertex = AllVertex;

                    for (int j = 0; j < allVertex.GetLength(0); j++)
                    {
                        if (obstacle[i].IsCollision(allVertex[j])) return true;
                    }

                    allVertex = obstacle[i].AllVertex;

                    for (int j = 0; j < allVertex.GetLength(0); j++)
                    {
                        if (IsCollision(allVertex[j])) return true;
                    }
                }
            }

            return false;
        }

        public float DistanceFromLine(float[] p1, float[] p2)
        {
            float[] p01 = Float3.Minus(PositionWithOffset, p1);
            float[] p21 = Float3.Minus(p2, p1);
            float d21 = Float3.Norm(p21);
            float[] cross = Float3.Cross(p21, p01);

            if (0.0001f < d21) return Float3.Norm(cross) / d21;
            else return Float3.Norm(p01);
        }

        protected float[][] Intersections(float[] p1, float[] p2)
        {
            float[][] result = new float[4][] { p2, p2, p2, new float[3] { 1, 1, 1 } };
            float ratio;

            float[] dirL = Float3.Minus(p2, p1);

            if (Float3.AbsSum(dirL) < 0.0001f) return result;

            float[] pt = VertexT[0];
            float[] pb = VertexB[0];
            float[] dirP = Float3.Minus(pb, pt);

            float[] normal = Normal[0];
            float dTop = -Float3.Dot(pt, normal);
            float dBottom = -Float3.Dot(pb, normal);
            float dot = Float3.Dot(dirL, normal);

            if (0.0001f < Math.Abs(dot))
            {
                ratio = -(Float3.Dot(p1, normal) + dTop) / dot;

                if (0 < ratio && ratio < 1)
                {
                    result[0] = Float3.Plus(p1, Float3.Times(ratio, dirL));
                    result[3][0] = ratio;
                }

                ratio = -(Float3.Dot(p1, normal) + dBottom) / dot;

                if (0 < ratio && ratio < 1)
                {
                    result[1] = Float3.Plus(p1, Float3.Times(ratio, dirL));
                    result[3][1] = ratio;
                }
            }

            if (0.0001f < Height)
            {
                float[] pt1 = Float3.Minus(pt, p1);

                float Dvv = Float3.Dot(dirL, dirL);
                float Dsv = Float3.Dot(dirP, dirL);
                float Dpv = Float3.Dot(pt1, dirL);
                float Dss = Float3.Dot(dirP, dirP);
                float Dps = Float3.Dot(pt1, dirP);
                float Dpp = Float3.Dot(pt1, pt1);
                float R2 = Size.RadiusMax * Size.RadiusMax;

                float A = Dvv - Dsv * Dsv / Dss;
                float B = Dpv - Dps * Dsv / Dss;
                float C = Dpp - Dps * Dps / Dss - R2;
                float S = B * B - A * C;

                if (0.0001f < S)
                {
                    S = (float)Math.Sqrt(S);
                    ratio = (B - S) / A;

                    if (0 < ratio && ratio < 1)
                    {
                        result[2] = Float3.Plus(p1, Float3.Times(ratio, dirL));
                        result[3][2] = ratio;
                    }
                }
            }

            return result;
        }

    }

    public class ConeFrustum : RoundObjectBase
    {
        public ConeFrustum(float radiusBottom = 0.3f, float radiusTop = 0.2f, float height = 0.5f)
        {
            RadiusBottom = radiusBottom;
            RadiusTop = radiusTop;            
            Height = height;
            VertexUpdate();
        }

        public float RadiusTop
        {
            get { return Size.RadiusTop; }
            set { Size.RadiusTop = value; }
        }

        public float RadiusBottom
        {
            get { return Size.RadiusBottom; }
            set { Size.RadiusBottom = value; }
        }

        public void SetSize(float radiusTop, float radiusBottom, float height)
        {
            RadiusTop = radiusTop;
            RadiusBottom = radiusBottom;
            Height = height;
        }

    }

    public class Pillar : RoundObjectBase, IContact
    {
        public Pillar(float radius = 0.2f, float height = 0.5f)
        {
            Radius = radius;            
            Height = height;
            VertexUpdate();
        }

        public float Radius
        {
            get { return Size.Radius; }
            set { Size.Radius = value; }
        }

        public float[] Intersection(float[] p1, float[] p2)
        {
            VertexUpdate();

            if (BoundSphereRadius < DistanceFromLine(p1, p2))
            {
                return p2;
            }
            else
            {                
                float[][] pc = Intersections(p1, p2);
                float[] result = p2;
                float ratio = 1;

                for (int i = 0; i < 3; i++)
                {
                    if (IsCollision(pc[i], 0.0001f) && pc[3][i] < ratio)
                    {
                        ratio = pc[3][i];
                        result = pc[i];
                    }
                }

                return result;
            }
        }
    }

    public class ThreeAxis
    {
        private Pillar Axis = new Pillar();
        private ConeFrustum Tip = new ConeFrustum();

        public Vector Position = new Vector(3);
        public RotationMatrix Rotate = new RotationMatrix();

        public ThreeAxis(float axisLength)
        {
            Axis.LineWidth = 0;
            Tip.LineWidth = 0;
            Tip.RadiusTop = 0;
            AxisLength = axisLength;
        }

        private float axisLength;

        public float AxisLength
        {
            get { return axisLength; }
            set
            {
                axisLength = value;
                Axis.Radius = 0.1f * value;
                Axis.Height = 0.6f * value;
                Tip.RadiusBottom = 0.2f * value;
                Tip.Height = 0.4f * value;
            }
        }

        public void Draw()
        {
            GL.PushMatrix();

            GL.Translate(Position[0], Position[1], Position[2]);
            GL.MultMatrix(Rotate.HomArray16);

            Axis.Color.SetBlue();
            Tip.Color.SetBlue();
            DrawArrow(0, Vector3.UnitZ);

            Axis.Color.SetGreen();
            Tip.Color.SetGreen();
            DrawArrow(-90, Vector3.UnitX);

            Axis.Color.SetRed();
            Tip.Color.SetRed();
            DrawArrow(90, Vector3.UnitY);

            GL.PopMatrix();
        }

        private void DrawArrow(int angleDeg, Vector3 axis)
        {
            GL.PushMatrix();
            GL.Rotate(angleDeg, axis);
            GL.Translate(0, 0, 0.3f * AxisLength);
            Axis.DrawSide();
            GL.Translate(0, 0, 0.5f * AxisLength);
            Tip.DrawSide();
            GL.PopMatrix();
        }
    }

    public class PrickleBall
    {
        private Sphere Sphere = new Sphere();
        public Vector Position = new Vector(3);
        public RotationMatrix Rotate = new RotationMatrix();
        private ConeFrustum Cone = new ConeFrustum();
        public int ConeNum = 3;

        public PrickleBall(float diameter = 0.05f)
        {
            Cone.RadiusTop = 0;
            Diameter = diameter;
            Sphere.Color.SetDarkGray();
        }

        public Color Color { get { return Sphere.Color; } }

        public float Diameter
        {
            set
            {
                Sphere.Radius = 0.5f * value;
                Cone.Height = 4 * value;
                Cone.RadiusBottom = 0.3f * value;
            }
        }

        public void Draw()
        {
            GL.PushMatrix();

            GL.Translate(Position[0], Position[1], Position[2]);
            GL.MultMatrix(Rotate.HomArray16);

            Sphere.Draw();

            if (0 < ConeNum)
            {
                Cone.Color.SetBlue();
                DrawCone(0, Vector3.UnitZ);
            }
            if (1 < ConeNum)
            {
                Cone.Color.SetGreen();
                DrawCone(-90, Vector3.UnitX);
            }
            if (2 < ConeNum)
            {
                Cone.Color.SetRed();
                DrawCone(90, Vector3.UnitY);
            }

            GL.PopMatrix();
        }

        private void DrawCone(int angleDeg, Vector3 axis)
        {
            GL.PushMatrix();
            GL.Rotate(angleDeg, axis);
            GL.Translate(0, 0, 0.5f * Cone.Height);
            Cone.DrawSide();
            GL.PopMatrix();
        }

    }
    
    public class SquareObjectBase : ICollision
    {
        public Vector Position = new Vector(3);
        public RotationMatrix Rotate = new RotationMatrix();
        public Color Color = new Color();
        public float LineWidth = 1;        
        
        private static int vertexNum = 9;
        protected float[][] VertexT = new float[vertexNum][];
        protected float[][] VertexC = new float[vertexNum][];
        protected float[][] VertexB = new float[vertexNum][];
        protected float[][] Normal = new float[vertexNum][];

        private int[] checkCode = new int[8];

        private int[] CheckCode()
        {
            int[] result = new int[8];

            result[0] = (int)(10000 * Position[0]);
            result[1] = (int)(10000 * Position[1]);
            result[2] = (int)(10000 * Position[2]);
            result[3] = (int)(10000 * Rotate[0, 0]);
            result[4] = (int)(10000 * Rotate[1, 1]);
            result[5] = (int)(10000 * Rotate[2, 2]);
            result[6] = Offset.CheckCode;
            result[7] = Size.CheckCode;
       
            return result;
        }

        private bool IsChanged()
        {
            int[] checkCodeNow = CheckCode();

            for (int i = 0; i < checkCode.Length; i++)
            {
                if (checkCode[i] != checkCodeNow[i])
                {
                    checkCode = checkCodeNow;
                    return true;
                }
            }

            return false;
        }

        private OffsetHandler Offset = new OffsetHandler();

        public void SetPositionOffset(float x, float y, float z)
        {
            Offset.SetPosition(x, y, z);
        }

        public void SetRotateOffset(float roll, float pitch, float yaw)
        {
            Offset.SetRotate(roll, pitch, yaw);
        }

        private class OffsetHandler
        {
            private float[] position = new float[3];
            private float[,] rotate = new float[3, 3] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };
            private int checkCode = int.MinValue;

            public void SetPosition(float x, float y, float z)
            {
                position[0] = x;
                position[1] = y;
                position[2] = z;
                checkCode++;
            }

            public void SetRotate(float roll, float pitch, float yaw)
            {
                float Sr = (float)Math.Sin(roll);
                float Sp = (float)Math.Sin(pitch);
                float Sy = (float)Math.Sin(yaw);

                float Cr = (float)Math.Cos(roll);
                float Cp = (float)Math.Cos(pitch);
                float Cy = (float)Math.Cos(yaw);

                rotate[0, 0] = Cy * Cp;
                rotate[1, 0] = Sy * Cp;
                rotate[2, 0] = -Sp;

                rotate[0, 1] = Cy * Sp * Sr - Sy * Cr;
                rotate[1, 1] = Sy * Sp * Sr + Cy * Cr;
                rotate[2, 1] = Cp * Sr;

                rotate[0, 2] = Cy * Sp * Cr + Sy * Sr;
                rotate[1, 2] = Sy * Sp * Cr - Cy * Sr;
                rotate[2, 2] = Cp * Cr;

                checkCode++;
            }

            public int CheckCode { get { return checkCode; } }

            public float[] Position { get { return position; } }

            public float[] Plus(float[] p)
            {
                return new float[3] { position[0] + p[0], position[1] + p[1], position[2] + p[2] };
            }

            public float[] Rotate(float[] p)
            {
                float[] result = new float[3];
                result[0] = rotate[0, 0] * p[0] + rotate[0, 1] * p[1] + rotate[0, 2] * p[2];
                result[1] = rotate[1, 0] * p[0] + rotate[1, 1] * p[1] + rotate[1, 2] * p[2];
                result[2] = rotate[2, 0] * p[0] + rotate[2, 1] * p[1] + rotate[2, 2] * p[2];
                return result;
            }

            public float[] TransposeRotate(float[] p)
            {
                float[] result = new float[3];
                result[0] = rotate[0, 0] * p[0] + rotate[1, 0] * p[1] + rotate[2, 0] * p[2];
                result[1] = rotate[0, 1] * p[0] + rotate[1, 1] * p[1] + rotate[2, 1] * p[2];
                result[2] = rotate[0, 2] * p[0] + rotate[1, 2] * p[1] + rotate[2, 2] * p[2];
                return result;
            }

            public float[] PlusRotate(float[] p)
            {
                return Plus(Rotate(p));
            }
        }

        public float SizeZ
        {
            get { return Size.Z; }
            set { Size.Z = value; }
        }

        protected SizeHandler Size = new SizeHandler();

        protected class SizeHandler
        {
            private float sizeTopX = 0.4f;
            private float sizeTopY = 0.4f;
            private float sizeBottomX = 0.4f;
            private float sizeBottomY = 0.4f;
            private float sizeZ = 0.4f;

            private int checkCode = int.MinValue;
            public int CheckCode { get { return checkCode; } }

            public float TopX
            {
                get { return sizeTopX; }
                set
                {
                    sizeTopX = value;
                    checkCode++;
                }
            }

            public float TopY
            {
                get { return sizeTopY; }
                set
                {
                    sizeTopY = value;
                    checkCode++;
                }
            }

            public float BottomX
            {
                get { return sizeBottomX; }
                set
                {
                    sizeBottomX = value;
                    checkCode++;
                }
            }

            public float BottomY
            {
                get { return sizeBottomY; }
                set
                {
                    sizeBottomY = value;
                    checkCode++;
                }
            }

            public float Z
            {
                get { return sizeZ; }
                set
                {
                    sizeZ = value;
                    checkCode++;
                }
            }            

        }

        public float[] PositionWithOffset
        {
            get { return Position.Plus(Rotate.Times(Offset.Position)); }
        }

        private static class BaseVertex
        {
            public static float[][] VertexT = new float[vertexNum][];
            public static float[][] VertexB = new float[vertexNum][];
            public static float[][] Normal = new float[vertexNum][];

            static BaseVertex()
            {
                VertexT[0] = new float[3] { 0, 0, 0.5f };
                VertexT[1] = new float[3] { -0.5f, 0.5f, 0.5f };
                VertexT[2] = new float[3] { -0.5f, 0, 0.5f };
                VertexT[3] = new float[3] { -0.5f, -0.5f, 0.5f };
                VertexT[4] = new float[3] { 0, -0.5f, 0.5f };
                VertexT[5] = new float[3] { 0.5f, -0.5f, 0.5f };
                VertexT[6] = new float[3] { 0.5f, 0, 0.5f };
                VertexT[7] = new float[3] { 0.5f, 0.5f, 0.5f };
                VertexT[8] = new float[3] { 0, 0.5f, 0.5f };

                Vector normalTmp = new Vector(3);

                for (int i = 0; i < vertexNum; i++)
                {
                    VertexB[i] = new float[3];
                    VertexB[i][0] = VertexT[i][0];
                    VertexB[i][1] = VertexT[i][1];
                    VertexB[i][2] = -0.5f;

                    normalTmp[0] = VertexT[i][0];
                    normalTmp[1] = VertexT[i][1];
                    normalTmp[2] = 0;

                    if (0.001f < normalTmp.AbsSum) Normal[i] = normalTmp.Normalize;
                    else Normal[i] = new float[3] { 0, 0, 1 };
                }
            }
        }

        protected void VertexUpdate()
        {
            if (IsChanged())
            {
                float[] vertex = new float[3];

                for (int i = 0; i < vertexNum; i++)
                {
                    vertex[0] = Size.TopX * BaseVertex.VertexT[i][0];
                    vertex[1] = Size.TopY * BaseVertex.VertexT[i][1];
                    vertex[2] = Size.Z * BaseVertex.VertexT[i][2];
                    VertexT[i] = Position.Plus(Rotate.Times(Offset.PlusRotate(vertex)));

                    vertex[0] = Size.BottomX * BaseVertex.VertexB[i][0];
                    vertex[1] = Size.BottomY * BaseVertex.VertexB[i][1];
                    vertex[2] = Size.Z * BaseVertex.VertexB[i][2];
                    VertexB[i] = Position.Plus(Rotate.Times(Offset.PlusRotate(vertex)));

                    vertex[0] = 0.5f * (VertexT[i][0] + VertexB[i][0]);
                    vertex[1] = 0.5f * (VertexT[i][1] + VertexB[i][1]);
                    vertex[2] = 0.5f * (VertexT[i][2] + VertexB[i][2]);
                    VertexC[i] = new float[3] { vertex[0], vertex[1], vertex[2] };

                    Normal[i] = Rotate.Times(Offset.Rotate(BaseVertex.Normal[i]));                    
                }                
            }
        }

        private void DrawVertexT(PrimitiveType primitiveType)
        {
            GL.Normal3(Normal[0]);
            GL.Begin(primitiveType);
            for (int i = 1; i < vertexNum; i++)
            {
                GL.Vertex3(VertexT[i]);
            }
            GL.End();
        }

        private void DrawVertexB(PrimitiveType primitiveType)
        {
            GL.Normal3(-Normal[0][0], -Normal[0][1], -Normal[0][2]);
            GL.Begin(primitiveType);
            for (int i = 1; i < vertexNum; i++)
            {
                GL.Vertex3(VertexB[i]);
            }
            GL.End();
        }

        private void DrawVertexTB(PrimitiveType primitiveType)
        {
            GL.Begin(primitiveType);
            for (int i = 1; i < vertexNum; i += 2)
            {
                GL.Normal3(Normal[i]);
                GL.Vertex3(VertexT[i]);
                GL.Vertex3(VertexB[i]);
            }
            GL.Normal3(Normal[1]);
            GL.Vertex3(VertexT[1]);
            GL.Vertex3(VertexB[1]);
            GL.End();
        }

        public void DrawTop()
        {
            VertexUpdate();
            Color.EnableBodyColor();
            DrawVertexT(PrimitiveType.Polygon);
        }

        public void DrawBottom()
        {
            VertexUpdate();
            Color.EnableBodyColor();
            DrawVertexB(PrimitiveType.Polygon);
        }

        public void DrawLine()
        {
            if (0 < LineWidth)
            {
                VertexUpdate();
                Color.EnableLineColor();
                GL.LineWidth(LineWidth);
                DrawVertexT(PrimitiveType.LineLoop);
                DrawVertexB(PrimitiveType.LineLoop);
                DrawVertexTB(PrimitiveType.Lines);
            }
        }

        public void DrawSide()
        {
            VertexUpdate();
            Color.EnableBodyColor();
            DrawVertexTB(PrimitiveType.QuadStrip);
        }

        public void Draw()
        {
            VertexUpdate();

            if (0 < LineWidth)
            {
                Color.EnableLineColor();
                GL.LineWidth(LineWidth);
                DrawVertexT(PrimitiveType.LineLoop);
                DrawVertexB(PrimitiveType.LineLoop);
                DrawVertexTB(PrimitiveType.Lines);
            }

            Color.EnableBodyColor();
            DrawVertexT(PrimitiveType.Polygon);
            DrawVertexB(PrimitiveType.Polygon);
            DrawVertexTB(PrimitiveType.QuadStrip);
        }

        public float BoundSphereRadius
        {
            get
            {
                float t = (Size.TopX * Size.TopX + Size.TopY * Size.TopY) / 4;
                float b = (Size.BottomX * Size.BottomX + Size.BottomY * Size.BottomY) / 4;
                float z = SizeZ * SizeZ / 4;

                if (b < t) return (float)Math.Sqrt(t + z);
                else return (float)Math.Sqrt(b + z);
            }
        }
        
        public float[] PositionFromThis(float[] position)
        {
            return Offset.TransposeRotate(Rotate.TransposeTimes(Float3.Minus(position, PositionWithOffset)));
        }

        private float[] ShearSize(float z)
        {
            float[] result = new float[2];

            if (0.0001f < SizeZ)
            {
                float t = 0.5f - (z / SizeZ);
                result[0] = t * Size.BottomX + (1 - t) * Size.TopX;
                result[1] = t * Size.BottomY + (1 - t) * Size.TopY;
            }
            else
            {
                result[0] = 0.5f * (Size.BottomX + Size.TopX);
                result[1] = 0.5f * (Size.BottomY + Size.TopY);
            }

            return result;
        }

        public float[][] AllVertex
        {
            get
            {
                float[][] result = new float[3 * vertexNum][];

                VertexUpdate();

                for (int i = 0; i < vertexNum; i++)
                {
                    result[3 * i + 0] = VertexT[i];
                    result[3 * i + 1] = VertexC[i];
                    result[3 * i + 2] = VertexB[i];
                }

                return result;
            }
        }

        public bool IsCollision(float[] position, float threshold = 0)
        {
            float[] pf = PositionFromThis(position);
            float[] shearSize = ShearSize(pf[2]);

            if (0.5f * shearSize[0] + threshold < Math.Abs(pf[0])) return false;
            if (0.5f * shearSize[1] + threshold < Math.Abs(pf[1])) return false;
            if (0.5f * SizeZ + threshold < Math.Abs(pf[2])) return false;

            return true;
        }

        public bool IsCollision(params ICollision[] obstacle)
        {
            float[][] allVertex;
            float[] p = PositionWithOffset;
            float r = BoundSphereRadius;

            for (int i = 0; i < obstacle.Length; i++)
            {
                if (obstacle[i].IsCollision(p, r))
                {
                    allVertex = AllVertex;

                    for (int j = 0; j < allVertex.GetLength(0); j++)
                    {
                        if (obstacle[i].IsCollision(allVertex[j])) return true;
                    }

                    allVertex = obstacle[i].AllVertex;

                    for (int j = 0; j < allVertex.GetLength(0); j++)
                    {
                        if (IsCollision(allVertex[j])) return true;
                    }
                }
            }

            return false;
        }

        public float DistanceFromLine(float[] p1, float[] p2)
        {
            float[] p01 = Float3.Minus(PositionWithOffset, p1);
            float[] p21 = Float3.Minus(p2, p1);

            float d21 = Float3.Norm(p21);
            float[] cross = Float3.Cross(p21, p01);

            if (0.0001f < d21) return Float3.Norm(cross) / d21;
            else return Float3.Norm(p01);
        }
    }

    public class SquareFrustum : SquareObjectBase
    {
        public SquareFrustum(float bottomX = 0.5f, float bottomY = 0.5f, float topX = 0.4f, float topY = 0.4f, float height = 0.4f)
        {
            SizeBottomX = bottomX;
            SizeBottomY = bottomY;
            SizeTopX = topX;
            SizeTopY = topY;
            SizeZ = height;
        }

        public float SizeBottomX
        {
            get { return Size.BottomX; }
            set { Size.BottomX = value; }
        }
        public float SizeBottomY
        {
            get { return Size.BottomY; }
            set { Size.BottomY = value; }
        }
        public float SizeTopX
        {
            get { return Size.TopX; }
            set { Size.TopX = value; }
        }
        public float SizeTopY
        {
            get { return Size.TopY; }
            set { Size.TopY = value; }
        }
    }

    public class Cuboid : SquareObjectBase, IContact
    {
        public Cuboid(float sizeX = 0.4f, float sizeY = 0.4f, float sizeZ = 0.4f)
        {
            SizeX = sizeX;
            SizeY = sizeY;
            SizeZ = sizeZ;
        }

        public float SizeX
        {
            get { return 0.5f * (Size.BottomX + Size.TopX); }
            set
            {
                Size.BottomX = value;
                Size.TopX = value;
            }
        }

        public float SizeY
        {
            get { return 0.5f * (Size.BottomY + Size.TopY); }
            set
            {
                Size.BottomY = value;
                Size.TopY = value;
            }
        }

        public void SetSize(float sizeX, float sizeY, float sizeZ)
        {
            SizeX = sizeX;
            SizeY = sizeY;
            SizeZ = sizeZ;
        }
       
        private float[][] Normal6
        {
            get
            {
                float[][] result = new float[6][];
                result[0] = Normal[0];
                result[1] = Normal[2];
                result[2] = Normal[4];
                result[3] = Normal[6];
                result[4] = Normal[8];
                result[5] = new float[3] { -Normal[0][0], -Normal[0][1], -Normal[0][2] };
                return result;
            }
        }
        private float[][] Point6
        {
            get
            {
                float[][] result = new float[6][];
                result[0] = VertexT[0];
                result[1] = VertexC[2];
                result[2] = VertexC[4];
                result[3] = VertexC[6];
                result[4] = VertexC[8];
                result[5] = VertexB[0];
                return result;
            }
        }
    
        private float[][] Intersections(float[] p1, float[] p2)
        {
            float[][] result = new float[7][] { p2, p2, p2, p2, p2, p2, new float[6] { 1, 1, 1, 1, 1, 1 } };
            float ratio, d, dot;

            float[] dirL = Float3.Minus(p2, p1);
        
            if ((SizeX + SizeY + SizeZ) < 0.0001f || Float3.AbsSum(dirL) < 0.0001f) return result;

            float[][] normal = Normal6;
            float[][] point = Point6;

            for (int i = 0; i < 6; i++)
            {
                d = -Float3.Dot(normal[i], point[i]);
                dot = Float3.Dot(dirL, normal[i]);

                if (0.0001f < Math.Abs(dot))
                {
                    ratio = -(Float3.Dot(normal[i], p1) + d) / dot;

                    if (0 < ratio && ratio < 1)
                    {
                        result[i] = Float3.Plus(p1, Float3.Times(ratio, dirL));
                        result[6][i] = ratio;
                    }
                }
            }

            return result;
        }

        public float[] Intersection(float[] p1, float[] p2)
        {
            VertexUpdate();

            if (BoundSphereRadius < DistanceFromLine(p1, p2))
            {
                return p2;
            }
            else
            {                
                float[][] pc = Intersections(p1, p2);
                float[] result = p2;
                float ratio = 1;

                for (int i = 0; i < 6; i++)
                {
                    if (IsCollision(pc[i], 0.0001f) && pc[6][i] < ratio)
                    {
                        ratio = pc[6][i];
                        result = pc[i];
                    }
                }

                return result;
            }
        }

    }

    public class Circle : IContact
    {
        public Vector Position = new Vector(3);
        public RotationMatrix Rotate = new RotationMatrix();
        public float LineWidth = 1f;
        public Color Color = new Color();
        private float[] Vertex = new float[BaseVertex.Vertex.Length];

        public Circle(float radius = 0.2f)
        {
            Radius = radius;
        }

        private static class BaseVertex
        {
            public static float[] Vertex = new float[19 * 2];
            public static int Count = 0;

            static BaseVertex()
            {
                for (int deg = 0; deg <= 360; deg += 20)
                {
                    Vertex[2 * Count + 0] = (float)Math.Cos((float)Math.PI * deg / 180);
                    Vertex[2 * Count + 1] = (float)Math.Sin((float)Math.PI * deg / 180);
                    Count++;
                }
            }
        }

        private float radius = 0.2f;

        public float Radius
        {
            get { return radius; }
            set
            {
                radius = value;

                for (int i = 0; i < Vertex.Length; i++)
                {
                    Vertex[i] = radius * BaseVertex.Vertex[i];
                }
            }
        }

        public float[] PositionFromThis(float[] position)
        {            
            return Rotate.TransposeTimes(Position.Plus(position, -1, 1));
        }

        public bool IsInside(float[] position, float threshold = 0)
        {
            float[] pf = PositionFromThis(position);
            float r = (float)Math.Sqrt(pf[0] * pf[0] + pf[1] * pf[1]);

            if (radius + threshold < r) return false;
            else return true;
        }

        public bool IsCollision(float[] position, float threshold = 0)
        {
            float[] pf = PositionFromThis(position);
            float r = (float)Math.Sqrt(pf[0] * pf[0] + pf[1] * pf[1]);

            if (radius + threshold < r) return false;
            else if (0.001f + threshold < Math.Abs(pf[2])) return false;
            else return true;
        }

        public float[] Intersection(float[] p1, float[] p2)
        {
            float[] result = p2;
            float[] resultTmp;
            float[] dirL = Float3.Minus(p2, p1);

            if (Radius < 0.0001f || Float3.AbsSum(dirL) < 0.0001f) return result;

            float[] normal = new float[3] { Rotate[0, 2], Rotate[1, 2], Rotate[2, 2] };
            float d = -Float3.Dot(normal, Position.Get);
            float dot = Float3.Dot(dirL, normal);

            if (0.0001f < Math.Abs(dot))
            {
                float ratio = -(Float3.Dot(normal, p1) + d) / dot;

                if (0 < ratio && ratio < 1)
                {
                    resultTmp = Float3.Plus(p1, Float3.Times(ratio, dirL));
                    if (IsCollision(resultTmp)) result = resultTmp;
                }
            }

            return result;
        }

        public void Draw()
        {
            GL.PushMatrix();

            GL.Translate(Position[0], Position[1], Position[2]);
            GL.MultMatrix(Rotate.HomArray16);

            Color.EnableBodyColor();
            DrawVertex(PrimitiveType.TriangleFan);

            if (0 < LineWidth)
            {
                Color.EnableLineColor();
                GL.LineWidth(LineWidth);
                DrawVertex(PrimitiveType.LineLoop);
            }

            GL.PopMatrix();
        }

        public void DrawLine()
        {
            if (0 < LineWidth)
            {
                GL.PushMatrix();

                GL.Translate(Position[0], Position[1], Position[2]);
                GL.MultMatrix(Rotate.HomArray16);

                Color.EnableLineColor();
                GL.LineWidth(LineWidth);
                DrawVertex(PrimitiveType.LineLoop);

                GL.PopMatrix();
            }
        }

        private void DrawVertex(PrimitiveType primitiveType)
        {
            GL.Normal3(Vector3.UnitZ);
            GL.VertexPointer(2, VertexPointerType.Float, 0, Vertex);
            GL.DrawArrays(primitiveType, 0, BaseVertex.Count);
        }

    }

    public class Sphere : ICollision, IContact
    {
        public Vector Position = new Vector(3);
        public float LineWidth = 1;
        public Color Color = new Color();
        private float radius = 0.1f;
        private float[] Vertex = new float[BaseVertex.Vertex.Length];

        public Sphere(float radius = 0.1f)
        {
            Radius = radius;
        }

        public float Radius
        {
            get { return radius; }
            set
            {
                radius = value;

                for (int i = 0; i < Vertex.Length; i++)
                {
                    Vertex[i] = radius * BaseVertex.Vertex[i];
                }
            }
        }

        public float[][] AllVertex 
        { 
            get 
            {
                float[][] result = new float[6][];
                result[0] = Position.Plus(new float[3] { radius, 0, 0 });
                result[1] = Position.Plus(new float[3] { -radius, 0, 0 });
                result[2] = Position.Plus(new float[3] { 0, radius, 0 });
                result[3] = Position.Plus(new float[3] { 0, -radius, 0 });
                result[4] = Position.Plus(new float[3] { 0, 0, radius });
                result[5] = Position.Plus(new float[3] { 0, 0, -radius });
                return result;
            } 
        }

        public bool IsCollision(float[] position, float threshold = 0)
        {
            if (radius + threshold < Position.Distance(position)) return false;
            else return true;
        }

        public bool IsCollision(params ICollision[] iCollision)
        {
            for (int i = 0; i < iCollision.Length; i++)
            {
                if (iCollision[i].IsCollision(Position.Get, radius)) return true;
            }

            return false;
        }

        public void Draw()
        {
            Color.EnableBodyColor();
            DrawVertex(PrimitiveType.TriangleStrip);
        }

        public void DrawLine()
        {
            if (0 < LineWidth)
            {
                Color.EnableLineColor();
                GL.LineWidth(LineWidth);
                DrawVertex(PrimitiveType.LineStrip);
            }
        }

        private void DrawVertex(PrimitiveType primitiveType)
        {
            GL.PushMatrix();
            GL.Translate(Position[0], Position[1], Position[2]);

            GL.Begin(primitiveType);
            for (int i = 0; i < Vertex.Length; i += 3)
            {
                GL.Normal3(BaseVertex.Vertex[i + 0], BaseVertex.Vertex[i + 1], BaseVertex.Vertex[i + 2]);
                GL.Vertex3(Vertex[i + 0], Vertex[i + 1], Vertex[i + 2]);
            }
            GL.End();

            GL.PopMatrix();
        }

        private static class BaseVertex
        {
            public static float[] Vertex = new float[10 * 19 * 2 * 3]; // 13 * 25 -- 15 deg
            public static int Count = 0;

            static BaseVertex()
            {
                float Sphi, Cphi;
                float rx, ry;

                float CphiOld = (float)Math.Cos(-(float)Math.PI / 2);
                float SphiOld = (float)Math.Sin(-(float)Math.PI / 2);

                for (int phi = -90; phi <= 90; phi += 20)
                {
                    Cphi = (float)Math.Cos((float)Math.PI * phi / 180);
                    Sphi = (float)Math.Sin((float)Math.PI * phi / 180);

                    for (int deg = 0; deg <= 360; deg += 20)
                    {
                        rx = (float)Math.Cos((float)Math.PI * deg / 180);
                        ry = (float)Math.Sin((float)Math.PI * deg / 180);

                        WriteVertex(rx * CphiOld, ry * CphiOld, SphiOld);
                        WriteVertex(rx * Cphi, ry * Cphi, Sphi);
                    }

                    CphiOld = Cphi;
                    SphiOld = Sphi;
                }
            }

            private static void WriteVertex(float x, float y, float z)
            {
                Vertex[3 * Count + 0] = x;
                Vertex[3 * Count + 1] = y;
                Vertex[3 * Count + 2] = z;
                Count++;
            }
        }
     
        public float[] Intersection(float[] p1, float[] p2)
        {
            float[] result = p2;
            float ratio, a, b, c, d;
            float[] dirL = Float3.Minus(p2, p1);

            if (Radius < 0.0001f || Float3.AbsSum(dirL) < 0.0001f) return result;

            float[] p10 = Float3.Minus(p1, Position.Get);

            a = Float3.Dot(dirL, dirL);
            b = 2 * Float3.Dot(p10, dirL);
            c = Float3.Dot(p10, p10) - Radius * Radius;
            d = b * b - 4 * a * c;

            if (0 <= d && 0.0001f < Math.Abs(a))
            {
                ratio = (-b + -(float)Math.Sqrt(d)) / (2 * a);

                if (0 < ratio && ratio < 1)
                {
                    result = Float3.Plus(p1, Float3.Times(ratio, dirL));
                }
            }

            return result;
        }
    }

    public class RoughSphere
    {
        public Vector Position = new Vector(3);
        public Color Color = new Color();
        private float radius = 0.1f;
        private float[] Vertex = new float[BaseVertex.Vertex.Length];

        public RoughSphere(float radius = 0.1f)
        {
            Radius = radius;
        }

        public float Radius
        {
            get { return radius; }
            set
            {
                radius = value;

                for (int i = 0; i < Vertex.Length; i++)
                {
                    Vertex[i] = radius * BaseVertex.Vertex[i];
                }
            }
        }

        public void Draw()
        {
            Color.EnableBodyColor();

            GL.PushMatrix();
            GL.Translate(Position[0], Position[1], Position[2]);
            GL.Begin(PrimitiveType.TriangleStrip);
            for (int i = 0; i < Vertex.Length; i += 3)
            {
                GL.Normal3(BaseVertex.Vertex[i + 0], BaseVertex.Vertex[i + 1], BaseVertex.Vertex[i + 2]);
                GL.Vertex3(Vertex[i + 0], Vertex[i + 1], Vertex[i + 2]);
            }
            GL.End();
            GL.PopMatrix();
        }

        private static class BaseVertex
        {
            public static float[] Vertex = new float[5 * 9 * 2 * 3];
            public static int Count = 0;

            static BaseVertex()
            {
                float Sphi, Cphi;
                float rx, ry;

                float CphiOld = (float)Math.Cos(-(float)Math.PI / 2);
                float SphiOld = (float)Math.Sin(-(float)Math.PI / 2);

                for (int phi = -90; phi <= 90; phi += 45)
                {
                    Cphi = (float)Math.Cos((float)Math.PI * phi / 180);
                    Sphi = (float)Math.Sin((float)Math.PI * phi / 180);

                    for (int deg = 0; deg <= 360; deg += 45)
                    {
                        rx = (float)Math.Cos((float)Math.PI * deg / 180);
                        ry = (float)Math.Sin((float)Math.PI * deg / 180);

                        WriteVertex(rx * CphiOld, ry * CphiOld, SphiOld);
                        WriteVertex(rx * Cphi, ry * Cphi, Sphi);
                    }

                    CphiOld = Cphi;
                    SphiOld = Sphi;
                }
            }

            private static void WriteVertex(float x, float y, float z)
            {
                Vertex[3 * Count + 0] = x;
                Vertex[3 * Count + 1] = y;
                Vertex[3 * Count + 2] = z;
                Count++;
            }
        }

    }

    public class Rectangle : IContact
    {
        public Vector Position = new Vector(3);
        public RotationMatrix Rotate = new RotationMatrix();
        public float SizeX = 0.5f, SizeY = 0.5f;
        public float LineWidth = 1;
        public Color Color = new Color();
        
        public Rectangle(float sizeX = 0.5f, float sizeY = 0.5f)
        {
            SizeX = sizeX;
            SizeY = sizeY;
        }

        private int[] checkCode = new int[8];

        private int[] CheckCode()
        {
            int[] result = new int[8];

            result[0] = (int)(10000 * Position[0]);
            result[1] = (int)(10000 * Position[1]);
            result[2] = (int)(10000 * Position[2]);
            result[3] = (int)(10000 * Rotate[0, 0]);
            result[4] = (int)(10000 * Rotate[1, 1]);
            result[5] = (int)(10000 * Rotate[2, 2]);
            result[6] = (int)(10000 * SizeX);
            result[7] = (int)(10000 * SizeY);            

            return result;
        }

        private bool IsChanged()
        {
            int[] checkCodeNow = CheckCode();

            for (int i = 0; i < checkCode.Length; i++)
            {
                if (checkCode[i] != checkCodeNow[i])
                {
                    checkCode = checkCodeNow;
                    return true;
                }
            }

            return false;
        }

        private float[][] Vertex = new float[4][];

        private static class BaseVertex
        {
            public static float[][] Vertex = new float[4][];

            static BaseVertex()
            {
                Vertex[0] = new float[3] { -0.5f, 0.5f, 0 };
                Vertex[1] = new float[3] { -0.5f, -0.5f, 0 };
                Vertex[2] = new float[3] { 0.5f, -0.5f, 0 };
                Vertex[3] = new float[3] { 0.5f, 0.5f, 0 };
            }
        }

        public float[] PositionFromThis(float[] position)
        {
            return Rotate.TransposeTimes(Position.Plus(position, -1, 1));
        }

        public bool IsInside(float[] position, float threshold = 0)
        {
            float[] pf = PositionFromThis(position);
            if (0.5f * SizeX + threshold < Math.Abs(pf[0])) return false;
            if (0.5f * SizeY + threshold < Math.Abs(pf[1])) return false;
            return true;
        }

        public bool IsCollision(float[] position, float threshold = 0)
        {
            float[] pf = PositionFromThis(position);
            if (0.5f * SizeX + threshold < Math.Abs(pf[0])) return false;
            if (0.5f * SizeY + threshold < Math.Abs(pf[1])) return false;
            if (0.001f + threshold < Math.Abs(pf[2])) return false;
            return true;
        }

        public float[] Intersection(float[] p1, float[] p2)
        {
            float[] result = p2;
            float[] resultTmp;
            float[] dirL = Float3.Minus(p2, p1);

            if ((SizeX + SizeY) < 0.0001f || Float3.AbsSum(dirL) < 0.0001f) return result;
                    
            float[] normal = new float[3] { Rotate[0, 2], Rotate[1, 2], Rotate[2, 2] };
            float d = -Float3.Dot(normal, Position.Get);
            float dot = Float3.Dot(dirL, normal);

            if (0.0001f < Math.Abs(dot))
            {
                float ratio = -(Float3.Dot(normal, p1) + d) / dot;

                if (0 < ratio && ratio < 1)
                {
                    resultTmp = Float3.Plus(p1, Float3.Times(ratio, dirL));
                    if(IsCollision(resultTmp))  result = resultTmp;
                }
            }

            return result;
        }

        public void Draw()
        {
            VertexUpdate();

            Color.EnableBodyColor();
            DrawVertex(PrimitiveType.Polygon);

            if (0 < LineWidth)
            {
                Color.EnableLineColor();
                GL.LineWidth(LineWidth);
                DrawVertex(PrimitiveType.LineLoop);
            }
        }

        public void DrawLine()
        {
            if (0 < LineWidth)
            {
                VertexUpdate();

                Color.EnableLineColor();
                GL.LineWidth(LineWidth);
                DrawVertex(PrimitiveType.LineLoop);
            }
        }

        public void DrawLineNet(uint lineNum = 5)
        {
            VertexUpdate();

            Color.EnableLineColor();
            GL.LineWidth(LineWidth);
            GL.Begin(PrimitiveType.Lines);
            float t;
            for (int i = 0; i <= lineNum + 1; i++)
            {
                t = (float)i / (lineNum + 1);
                GL.Vertex3(Plus(Vertex[0], Vertex[1], 1 - t, t));
                GL.Vertex3(Plus(Vertex[3], Vertex[2], 1 - t, t));
                GL.Vertex3(Plus(Vertex[0], Vertex[3], 1 - t, t));
                GL.Vertex3(Plus(Vertex[1], Vertex[2], 1 - t, t));
            }
            GL.End();
        }

        private float[] Plus(float[] v1, float[] v2, float s, float t)
        {
            float[] result = new float[v1.Length];
            for (int i = 0; i < v1.Length; i++)
            {
                result[i] = s * v1[i] + t * v2[i];
            }
            return result;
        }

        private void DrawVertex(PrimitiveType primitiveType)
        {
            GL.Begin(primitiveType);
            GL.Normal3(Rotate[0, 2], Rotate[1, 2], Rotate[2, 2]);
            for (int i = 0; i < Vertex.GetLength(0); i++)
            {
                GL.Vertex3(Vertex[i]);
            }
            GL.End();
        }

        private void VertexUpdate()
        {
            if (IsChanged())
            {
                float[] vector = new float[3];

                for (int i = 0; i < Vertex.GetLength(0); i++)
                {
                    vector[0] = SizeX * BaseVertex.Vertex[i][0];
                    vector[1] = SizeY * BaseVertex.Vertex[i][1];
                    vector[2] = 0;
                    Vertex[i] = Position.Plus(Rotate.Times(vector));
                }
            }
        }
    }

    public class RoughRectangle
    {
        public Vector Position = new Vector(3);
        public RotationMatrix Rotate = new RotationMatrix();
        public float SizeX = 0.5f, SizeY = 0.5f;
        public Color Color = new Color();

        public RoughRectangle(float sizeX = 0.5f, float sizeY = 0.5f)
        {
            SizeX = sizeX;
            SizeY = sizeY;
        }
   
        private float[][] Vertex = new float[4][];

        private static class BaseVertex
        {
            public static float[][] Vertex = new float[4][];

            static BaseVertex()
            {
                Vertex[0] = new float[3] { -0.5f, 0.5f, 0 };
                Vertex[1] = new float[3] { -0.5f, -0.5f, 0 };
                Vertex[2] = new float[3] { 0.5f, -0.5f, 0 };
                Vertex[3] = new float[3] { 0.5f, 0.5f, 0 };
            }
        }
    
        public void Draw()
        {
            VertexUpdate();
            Color.EnableBodyColor();
            DrawVertex(PrimitiveType.Polygon);
        }
    
        private void DrawVertex(PrimitiveType primitiveType)
        {
            GL.Begin(primitiveType);
            GL.Normal3(Rotate[0, 2], Rotate[1, 2], Rotate[2, 2]);
            for (int i = 0; i < Vertex.GetLength(0); i++)
            {
                GL.Vertex3(Vertex[i]);
            }
            GL.End();
        }

        private void VertexUpdate()
        {
            float[] vector = new float[3];

            for (int i = 0; i < Vertex.GetLength(0); i++)
            {
                vector[0] = SizeX * BaseVertex.Vertex[i][0];
                vector[1] = SizeY * BaseVertex.Vertex[i][1];
                vector[2] = 0;
                Vertex[i] = Position.Plus(Rotate.Times(vector));
            }
        }

    }

    public class RobotObject
    {
        public class Joint
        {
            public Vector Position = new Vector(3);
            public RotationMatrix Rotate = new RotationMatrix();
            public Color Color = new Color();
            public float LineWidth = 1;
            public Color LimitedColor = new Color();
            private Object Object = new Object();
            public float Radius;
            public Vector axisInit = new Vector(3);

            public Joint(float radius = 0.1f)
            {
                Color.SetLightGray();
                LimitedColor.SetRed();
                Radius = radius;
                axisInit[2] = 1;
            }

            private bool isLimited = false;
            public void EnableLimitedColor() { isLimited = true; }
            public void DisableLimitedColor() { isLimited = false; }

            public void DrawRevolute()
            {
                if (0.0001f < Radius)
                {
                    GL.PushMatrix();

                    GL.Translate(Position[0], Position[1], Position[2]);
                    GL.MultMatrix(Rotate.HomArray16);

                    Object.Height = 2 * Radius;
                    Object.Radius = Radius;

                    if (isLimited) LimitedColor.EnableBodyColor();
                    else Color.EnableBodyColor();

                    bool isRhombus = 0.999f < Math.Abs(axisInit[2]);

                    if (isRhombus)
                    {
                        Object.DrawVertexTCB(PrimitiveType.TriangleStrip);
                    }
                    else
                    {
                        GL.MultMatrix(axisInit.HomArray16);
                        Object.DrawVertexTB(PrimitiveType.TriangleStrip);
                    }

                    Object.DrawVertexT(PrimitiveType.TriangleFan);
                    Object.DrawVertexB(PrimitiveType.TriangleFan);

                    if (0 < LineWidth)
                    {
                        Object.Radius = 1.01f * Radius;
                        Object.Height = 2.02f * Radius;

                        Color.EnableLineColor();
                        GL.LineWidth(LineWidth);

                        Object.DrawVertexT(PrimitiveType.LineLoop);
                        Object.DrawVertexB(PrimitiveType.LineLoop);
                        if (isRhombus) Object.DrawVertexC(PrimitiveType.LineLoop);

                        Object.Radius = 0.4f * Radius;

                        Object.DrawVertexT(PrimitiveType.LineLoop);
                        Object.DrawVertexB(PrimitiveType.LineLoop);
                    }

                    GL.PopMatrix();
                }
            }

            public void DrawPrismatic(float q)
            {
                if (0.0001f < Radius)
                {
                    GL.PushMatrix();

                    GL.Translate(Position[0], Position[1], Position[2]);
                    GL.MultMatrix(Rotate.HomArray16);
                    GL.MultMatrix(axisInit.HomArray16);

                    if (isLimited) LimitedColor.EnableBodyColor();
                    else Color.EnableBodyColor();

                    Object.Radius = Radius;
                    Object.Height = Radius;

                    GL.Translate(0, 0, -0.5f * Radius);
                    Object.DrawVertexTB(PrimitiveType.TriangleStrip);
                    Object.DrawVertexB(PrimitiveType.TriangleFan);

                    GL.Translate(0, 0, q + Radius);
                    Object.DrawVertexTB(PrimitiveType.TriangleStrip);
                    Object.DrawVertexT(PrimitiveType.TriangleFan);

                    Object.Radius = 0.8f * Radius;
                    Object.Height = q;

                    GL.Translate(0, 0, -0.5f * q - 0.5f * Radius);
                    Object.DrawVertexTB(PrimitiveType.TriangleStrip);

                    if (0 < LineWidth)
                    {
                        Color.EnableLineColor();
                        GL.LineWidth(LineWidth);
                        Object.DrawVertexTB(PrimitiveType.Lines);
                    }

                    GL.PopMatrix();
                }
            }
        }

        public class Link
        {
            public Vector Position = new Vector(3);
            public RotationMatrix Rotate = new RotationMatrix();
            public Color Color = new Color();
            private Object Object = new Object();
            public Vector lInit = new Vector(3);

            public Link()
            {
                Color.SetBlack();
                lInit[2] = 0.5f;
                Radius = 0.1f;
            }

            public float Radius
            {
                get { return Object.Radius; }
                set { Object.Radius = value; }
            }

            public void Draw(float jointRadius)
            {
                if (0.0001f < Radius && 0.001f < lInit.AbsSum)
                {
                    Object.Height = lInit.Norm - 2 * jointRadius;

                    if (0 < Object.Height)
                    {
                        GL.PushMatrix();

                        GL.Translate(Position[0], Position[1], Position[2]);
                        GL.MultMatrix(Rotate.HomArray16);
                        GL.MultMatrix(lInit.HomArray16);

                        Color.EnableBodyColor();
                        Object.DrawVertexTB(PrimitiveType.TriangleStrip);

                        GL.PopMatrix();
                    }
                }
            }

            public void DrawBase(float jointRadius)
            {
                if (0.0001f < Radius && 0.001f < lInit.AbsSum)
                {
                    Object.Height = jointRadius;

                    if (0 < Object.Height)
                    {
                        GL.PushMatrix();

                        GL.Translate(Position[0], Position[1], Position[2]);
                        GL.MultMatrix(Rotate.HomArray16);
                        GL.MultMatrix(lInit.HomArray16);

                        Color.EnableBodyColor();
                        GL.Translate(0, 0, -0.5f * lInit.Norm + 0.5f * jointRadius);
                        Object.DrawVertexTB(PrimitiveType.TriangleStrip);

                        GL.PopMatrix();
                    }
                }
            }

            public void DrawEnd(float jointRadius)
            {
                if (0.0001f < Radius && 0.001f < lInit.AbsSum)
                {
                    Object.Height = lInit.Norm - jointRadius;

                    if (0 < Object.Height)
                    {
                        GL.PushMatrix();

                        GL.Translate(Position[0], Position[1], Position[2]);
                        GL.MultMatrix(Rotate.HomArray16);
                        GL.MultMatrix(lInit.HomArray16);

                        Color.EnableBodyColor();
                        GL.Translate(0, 0, 0.5f * jointRadius);
                        Object.DrawVertexTB_Half(PrimitiveType.TriangleStrip);

                        GL.PopMatrix();
                    }
                }
            }

        }

        private class Object
        {
            private float radius = 0.2f;
            private float height = 0.4f;

            private static int vertexNum = 16;
            private float[][] VertexT = new float[vertexNum][];
            private float[][] VertexB = new float[vertexNum][];
            private float[][] VertexC = new float[vertexNum][];

            public Object()
            {
                for (int i = 0; i < vertexNum; i++)
                {
                    VertexT[i] = new float[3];
                    VertexB[i] = new float[3];
                    VertexC[i] = new float[3];
                }
            }

            public float Radius
            {
                get { return radius; }
                set
                {
                    radius = value;

                    for (int i = 0; i < vertexNum; i++)
                    {
                        VertexT[i][0] = radius * BaseVertex.VertexT[i][0];
                        VertexT[i][1] = radius * BaseVertex.VertexT[i][1];

                        VertexB[i][0] = radius * BaseVertex.VertexB[i][0];
                        VertexB[i][1] = radius * BaseVertex.VertexB[i][1];

                        VertexC[i][0] = radius * BaseVertex.VertexC[i][0];
                        VertexC[i][1] = radius * BaseVertex.VertexC[i][1];
                    }
                }
            }

            public float Height
            {
                get { return height; }
                set
                {
                    height = value;

                    for (int i = 0; i < vertexNum; i++)
                    {
                        VertexT[i][2] = height * BaseVertex.VertexT[i][2];
                        VertexB[i][2] = height * BaseVertex.VertexB[i][2];
                    }
                }
            }

            private static class BaseVertex
            {
                public static float[][] VertexT = new float[vertexNum][];
                public static float[][] VertexB = new float[vertexNum][];
                public static float[][] VertexC = new float[vertexNum][];
                public static float[][] Normal = new float[vertexNum][];

                static BaseVertex()
                {
                    VertexT[0] = new float[3] { 0, 0, 0.5f };
                    VertexB[0] = new float[3] { 0, 0, -0.5f };
                    VertexC[0] = new float[3] { 0, 0, 0 };
                    Normal[0] = new float[3] { 0, 0, 1 };

                    float stepAngle = 2 * (float)Math.PI / (vertexNum - 1);
                    float angle = 0, cos, sin;

                    for (int i = 1; i < vertexNum; i++)
                    {
                        cos = (float)Math.Cos(angle);
                        sin = (float)Math.Sin(angle);
                        VertexT[i] = new float[3] { cos, sin, 0.5f };
                        VertexB[i] = new float[3] { cos, sin, -0.5f };
                        VertexC[i] = new float[3] { 1.5f * cos, 1.5f * sin, 0 };
                        Normal[i] = new float[3] { cos, sin, 0 };
                        angle += stepAngle;
                    }
                }
            }

            public void DrawVertexT(PrimitiveType primitiveType)
            {
                GL.Normal3(Vector3.UnitZ);
                GL.Begin(primitiveType);
                for (int i = 1; i < vertexNum; i++)
                {
                    GL.Vertex3(VertexT[i]);
                }
                GL.End();
            }

            public void DrawVertexB(PrimitiveType primitiveType)
            {
                GL.Normal3(-Vector3.UnitZ);
                GL.Begin(primitiveType);
                for (int i = 1; i < vertexNum; i++)
                {
                    GL.Vertex3(VertexB[i]);
                }
                GL.End();
            }

            public void DrawVertexC(PrimitiveType primitiveType)
            {
                GL.Begin(primitiveType);
                for (int i = 1; i < vertexNum; i++)
                {
                    GL.Normal3(BaseVertex.Normal[i]);
                    GL.Vertex3(VertexC[i]);
                }
                GL.End();
            }

            public void DrawVertexTB(PrimitiveType primitiveType)
            {
                GL.Begin(primitiveType);
                for (int i = 1; i < vertexNum; i++)
                {
                    GL.Normal3(BaseVertex.Normal[i]);
                    GL.Vertex3(VertexT[i]);
                    GL.Vertex3(VertexB[i]);
                }
                GL.Normal3(BaseVertex.Normal[1]);
                GL.Vertex3(VertexT[1]);
                GL.Vertex3(VertexB[1]);
                GL.End();
            }

            public void DrawVertexTCB(PrimitiveType primitiveType)
            {
                GL.Begin(primitiveType);
                for (int i = 1; i < vertexNum; i++)
                {
                    GL.Normal3(BaseVertex.Normal[i]);
                    GL.Vertex3(VertexT[i]);
                    GL.Vertex3(VertexC[i]);
                }
                GL.Normal3(BaseVertex.Normal[1]);
                GL.Vertex3(VertexT[1]);
                GL.Vertex3(VertexC[1]);
                GL.End();

                GL.Begin(primitiveType);
                for (int i = 1; i < vertexNum; i++)
                {
                    GL.Normal3(BaseVertex.Normal[i]);
                    GL.Vertex3(VertexC[i]);
                    GL.Vertex3(VertexB[i]);
                }
                GL.Normal3(BaseVertex.Normal[1]);
                GL.Vertex3(VertexC[1]);
                GL.Vertex3(VertexB[1]);
                GL.End();
            }

            public void DrawVertexTB_Half(PrimitiveType primitiveType)
            {
                GL.Begin(primitiveType);
                for (int i = 1; i < vertexNum; i++)
                {
                    GL.Normal3(BaseVertex.Normal[i]);
                    GL.Vertex3(0.5f * VertexT[i][0], 0.5f * VertexT[i][1], VertexT[i][2]);
                    GL.Vertex3(VertexB[i]);
                }
                GL.Normal3(BaseVertex.Normal[1]);
                GL.Vertex3(0.5f * VertexT[1][0], 0.5f * VertexT[1][1], VertexT[1][2]);
                GL.Vertex3(VertexB[1]);
                GL.End();
            }

            /*

            public void DrawTopBottom()
            {
                Color.EnableBodyColor();
                GL.PushMatrix();
                GL.Translate(Position[0], Position[1], Position[2]);
                GL.MultMatrix(Rotate.HomArray16);
                DrawVertexT(PrimitiveType.TriangleFan);
                DrawVertexB(PrimitiveType.TriangleFan);
                GL.PopMatrix();
            }

            public void DrawTopBottomLine()
            {
                if (0 < LineWidth)
                {
                    Color.EnableLineColor();
                    GL.LineWidth(LineWidth);
                    GL.PushMatrix();
                    GL.Translate(Position[0], Position[1], Position[2]);
                    GL.MultMatrix(Rotate.HomArray16);
                    DrawVertexT(PrimitiveType.LineLoop);
                    DrawVertexB(PrimitiveType.LineLoop);
                    GL.PopMatrix();
                }
            }

            public void DrawSide()
            {
                Color.EnableBodyColor();
                DrawVertexTB(PrimitiveType.TriangleStrip);
            }

            public void Draw()
            {       
                if (0 < LineWidth)
                {
                    Color.EnableLineColor();
                    GL.LineWidth(LineWidth);
                    DrawVertexT(PrimitiveType.LineLoop);
                    DrawVertexB(PrimitiveType.LineLoop);
                }

                Color.EnableBodyColor();
                DrawVertexT(PrimitiveType.TriangleFan);
                DrawVertexB(PrimitiveType.TriangleFan);
                DrawVertexTB(PrimitiveType.TriangleStrip);
            }

            public void DrawStripes()
            {

                if (0 < LineWidth)
                {
                    Color.EnableLineColor();
                    GL.LineWidth(LineWidth);
                    DrawVertexT(PrimitiveType.LineLoop);
                    DrawVertexB(PrimitiveType.LineLoop);
                    DrawVertexTB(PrimitiveType.Lines);
                }

                Color.EnableBodyColor();
                DrawVertexTB(PrimitiveType.TriangleStrip);
            }
            */
        }
    }

    public class Line
    {
        public Vector Position1 = new Vector(3);
        public Vector Position2 = new Vector(3);
        public float Width = 1;
        public Color Color = new Color();

        public Line()
        {
            Position1[0] = -1;
            Position2[0] = 1;
        }

        public float Length
        {
            get { return Position1.Distance(Position2.Get); }
        }

        public void Draw()
        {
            Color.EnableBodyColor();
            GL.LineWidth(Width);

            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(Position1.Get);
            GL.Vertex3(Position2.Get);
            GL.End();
        }

        private RotationMatrix Rotate = new RotationMatrix();
        public void RotatePosition2(float roll, float pitch, float yaw)
        {
            Position2.SetMinus(Position1.Get);
            Rotate.SetRollPitchYaw(roll, pitch, yaw);
            Position2.Set = Position1.Plus(Rotate.Times(Position2.Get));
        }

        public void RotatePosition2(RotationMatrix rotate)
        {
            Position2.SetMinus(Position1.Get);
            Position2.Set = Position1.Plus(rotate.Times(Position2.Get));
        }

        public float[] Intersection(params IContact[] obstacle)
        {
            if (obstacle.Length == 1)
            {
                return obstacle[0].Intersection(Position1.Get, Position2.Get);
            }
            else
            {
                float[] result = Position1.Get;
                float[] resultTmp;
                float dist = float.MaxValue;
                float distTmp;

                for (int i = 0; i < obstacle.Length; i++)
                {
                    resultTmp = obstacle[i].Intersection(Position1.Get, Position2.Get);
                    distTmp = Position1.Distance(resultTmp);

                    if (distTmp < dist)
                    {
                        dist = distTmp;
                        result = resultTmp;
                    }
                }

                return result;
            }
        }
    }

    public class Lidar
    {
        private Pillar Pillar = new Pillar(0.03f, 0.05f);
        private RoughSphere Sphere = new RoughSphere();
        public float LaserLength = 3;
        public float LaserWidth = 1;
        public Color LaserColor = new Color();
        private RotationMatrix StepRotate = new RotationMatrix();
        private float stepAngle;
        private int stepNum;

        public Lidar()
        {
            Pillar.Radius = 0.03f;            
            Pillar.Color.SetBlack();
            LaserColor.SetSkyBlue();
            Sphere.Radius = 0.02f;
            Sphere.Color.SetPurple();            
            StepAngle = 0.15f;
        }

        public Vector Position
        {
            get { return Pillar.Position; }
            set { Pillar.Position = value; }
        }

        public RotationMatrix Rotate
        {
            get { return Pillar.Rotate; }
            set { Pillar.Rotate = value; }
        }

        public float LidarHeight
        {
            get { return Pillar.Height; }
            set { Pillar.Height = value; }
        }

        public float LidarRadius
        {
            get { return Pillar.Radius; }
            set { Pillar.Radius = value; }
        }

        public Color LidarColor { get { return Pillar.Color; } }

        public float PointRadius
        {
            set { Sphere.Radius = value; }
            get { return Sphere.Radius; }
        }

        public Color PointColor { get { return Sphere.Color; } }

        public float StepAngle
        {
            get { return stepAngle; }
            set
            {
                if (0.005f <= value)
                {
                    stepAngle = value;
                }
                else
                {
                    stepAngle = 0.005f;
                    Console.WriteLine("Minimum of StepAngle is 0.005 [rad].");
                }

                StepRotate.SetRz(stepAngle);
                stepNum = (int)(2 * Math.PI / stepAngle) + 1;
                pointCloudAbs = new float[stepNum][];
            }
        }

        private float[][] pointCloudAbs = new float[0][];

        public float[][] PointCloudAbs(params IContact[] obstacle)
        {
            return PointCloudAbs(obstacle, new IContact[0]);
        }

        public float[][] PointCloudAbs(IContact[] obstacle1, params IContact[] obstacle2)
        {
            float[][] result = new float[stepNum][];
            float[] resultTmp;
            float[] laserVector = new float[3] { -LaserLength, 0, 0 };
            float dist, distTmp;

            for (int i = 0; i < result.GetLength(0); i++)
            {
                dist = float.MaxValue;
                result[i] = Position.Get;

                for (int j = 0; j < obstacle1.Length; j++)
                {
                    resultTmp = obstacle1[j].Intersection(Position.Get, Position.Plus(Rotate.Times(laserVector)));
                    distTmp = Position.Distance(resultTmp);

                    if (distTmp < dist)
                    {
                        dist = distTmp;
                        result[i] = resultTmp;
                    }
                }

                for (int j = 0; j < obstacle2.Length; j++)
                {
                    resultTmp = obstacle2[j].Intersection(Position.Get, Position.Plus(Rotate.Times(laserVector)));
                    distTmp = Position.Distance(resultTmp);

                    if (distTmp < dist)
                    {
                        dist = distTmp;
                        result[i] = resultTmp;
                    }
                }

                pointCloudAbs[i] = result[i];
                laserVector = StepRotate.Times(laserVector);
            }

            return result;
        }

        public float[][] PointCloud(params IContact[] obstacle)
        {
            float[][] result = PointCloudAbs(obstacle);

            for (int i = 0; i < result.GetLength(0); i++)
            {
                result[i] = Rotate.TransposeTimes(Position.Plus(result[i], -1, 1));
            }

            return result;
        }

        public float[][] PointCloud(IContact[] obstacle1, params IContact[] obstacle2)
        {
            float[][] result = PointCloudAbs(obstacle1, obstacle2);

            for (int i = 0; i < result.GetLength(0); i++)
            {
                result[i] = Rotate.TransposeTimes(Position.Plus(result[i], -1, 1));
            }

            return result;
        }

        public void Draw()
        {
            Pillar.Draw();

            for (int i = 0; i < pointCloudAbs.GetLength(0); i++)
            {
                if (pointCloudAbs[i] != null)
                {
                    LaserColor.EnableBodyColor();
                    GL.LineWidth(LaserWidth);
                    GL.Begin(PrimitiveType.Lines);
                    GL.Vertex3(Position.Get);
                    GL.Vertex3(pointCloudAbs[i]);
                    GL.End();

                    Sphere.Position.Set = pointCloudAbs[i];
                    Sphere.Draw();
                }
                else
                {
                    break;
                }
            }
        }

    }

}
