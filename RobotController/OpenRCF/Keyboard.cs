using System;
using System.Collections.Generic;
using System.Windows.Input;
using OpenRCF;

namespace RobotController.OpenRCF
{
    public static class Keyboard
    {
        public static Dictionary<Key, Action> KeyEvent = new Dictionary<Key, Action>()
        {
            { Key.A, () => { } },
            { Key.B, () => { } },
            { Key.C, () => { } },
            { Key.D, () => { } },
            { Key.E, () => { } },
            { Key.F, () => { } },
            { Key.G, () => { } },
            { Key.H, () => { } },
            { Key.I, () => { } },
            { Key.J, () => { } },
            { Key.K, () => { } },
            { Key.N, () => { } },
            { Key.M, () => { } },
            { Key.L, () => { } },
            { Key.O, () => { } },
            { Key.P, () => { } },
            { Key.Q, () => { } },
            { Key.R, () => { } },
            { Key.S, () => { } },
            { Key.T, () => { } },
            { Key.U, () => { } },
            { Key.V, () => { } },
            { Key.W, () => { } },
            { Key.X, () => { } },
            { Key.Y, () => { } },
            { Key.Z, () => { } },
            { Key.Delete, () => { } },
            { Key.Enter, () => { } },
            { Key.Back, () => { } },
        };

        private static Dictionary<Key, bool> ModifierKey = new Dictionary<Key, bool>()
        {
            { Key.LeftShift, false},
            { Key.RightShift, false},
            { Key.LeftCtrl, false},
            { Key.RightCtrl, false},
            { Key.Space, false},
        };

        private static Key digitKeyOld = Key.D0;
        private static Dictionary<Key, bool> DigitKey = new Dictionary<Key, bool>()
        {
            { Key.D0, true},
            { Key.D1, false},
            { Key.D2, false},
            { Key.D3, false},
            { Key.D4, false},
            { Key.D5, false},
            { Key.D6, false},
            { Key.D7, false},
            { Key.D8, false},
            { Key.D9, false},
        };

        private readonly static Dictionary<Key, int[]> ArrowKey = new Dictionary<Key, int[]>()
        {
            { Key.Right, new int[] { 1, 0, 0 } },
            { Key.Left, new int[] { -1, 0, 0 } },
            { Key.Up, new int[] { 0, 1, 0 } },
            { Key.Down, new int[] { 0, -1, 0 } },
            { Key.OemPeriod, new int[] { 0, 0, 1 } },
            { Key.OemComma, new int[] { 0, 0, -1 } },
            { Key.OemBackslash, new int[] { 0, 0, 1 } },
            { Key.OemQuestion, new int[] { 0, 0, -1 } },
            { Key.Delete, new int[] { 0, 0, 0 } },
        };

        public static Vector[] ShiftVector = new Vector[10];
        public static RotationMatrix[] CtrlMatrix = new RotationMatrix[10];
        public static Vector SpaceVector = new Vector(3);

        static Keyboard()
        {
            for (int i = 0; i < ShiftVector.Length; i++)
            {
                ShiftVector[i] = new Vector(3);
            }

            for (int i = 0; i < CtrlMatrix.Length; i++)
            {
                CtrlMatrix[i] = new RotationMatrix();
            }            
        }

        public static float ShiftDistance = 0.05f;

        private static float[] vector = new float[3];
        private static void UpdateShiftVector(int[] arrowVector, int mashNum)
        {
            if (mashNum < 7)
            {
                vector[0] = ShiftDistance * arrowVector[0];
                vector[1] = ShiftDistance * arrowVector[1];
                vector[2] = ShiftDistance * arrowVector[2];
            }
            else
            {
                vector[0] = 3 * ShiftDistance * arrowVector[0];
                vector[1] = 3 * ShiftDistance * arrowVector[1];
                vector[2] = 3 * ShiftDistance * arrowVector[2];
            }

            if (DigitKey[Key.D0]) ShiftVector[0].SetPlus(vector);
            else if (DigitKey[Key.D1]) ShiftVector[1].SetPlus(vector);
            else if (DigitKey[Key.D2]) ShiftVector[2].SetPlus(vector);
            else if (DigitKey[Key.D3]) ShiftVector[3].SetPlus(vector);
            else if (DigitKey[Key.D4]) ShiftVector[4].SetPlus(vector);
            else if (DigitKey[Key.D5]) ShiftVector[5].SetPlus(vector);
            else if (DigitKey[Key.D6]) ShiftVector[6].SetPlus(vector);
            else if (DigitKey[Key.D7]) ShiftVector[7].SetPlus(vector);
            else if (DigitKey[Key.D8]) ShiftVector[8].SetPlus(vector);
            else if (DigitKey[Key.D9]) ShiftVector[9].SetPlus(vector);
        }


        public static float SpaceDistance = 0.2f;

        private static void UpdateSpaceVector(int[] arrowVector)
        {
            vector[0] = SpaceDistance * arrowVector[0];
            vector[1] = SpaceDistance * arrowVector[1];
            vector[2] = SpaceDistance * arrowVector[2];

            SpaceVector.SetPlus(vector);
        }

        public static float CtrlAngle = (float)Math.PI / 12;

        private static RotationMatrix R = new RotationMatrix();
        private static void UpdateCtrlMatrix(int[] arrowVector, int mashNum)
        {
            if (arrowVector[0] + arrowVector[1] + arrowVector[2] == 0)
            {
                if (DigitKey[Key.D0]) CtrlMatrix[0].SetIdentity();
                else if (DigitKey[Key.D1]) CtrlMatrix[1].SetIdentity();
                else if (DigitKey[Key.D2]) CtrlMatrix[2].SetIdentity();
                else if (DigitKey[Key.D3]) CtrlMatrix[3].SetIdentity();
                else if (DigitKey[Key.D4]) CtrlMatrix[4].SetIdentity();
                else if (DigitKey[Key.D5]) CtrlMatrix[5].SetIdentity();
                else if (DigitKey[Key.D6]) CtrlMatrix[6].SetIdentity();
                else if (DigitKey[Key.D7]) CtrlMatrix[7].SetIdentity();
                else if (DigitKey[Key.D8]) CtrlMatrix[8].SetIdentity();
                else if (DigitKey[Key.D9]) CtrlMatrix[9].SetIdentity();
            }
            else
            {
                float[] axis = new float[3] { -arrowVector[1], arrowVector[0], arrowVector[2] };
                R.SetRn(CtrlAngle, axis);

                if (DigitKey[Key.D0]) CtrlMatrix[0].SetTimes(R.Get);
                else if (DigitKey[Key.D1]) CtrlMatrix[1].SetTimes(R.Get);
                else if (DigitKey[Key.D2]) CtrlMatrix[2].SetTimes(R.Get);
                else if (DigitKey[Key.D3]) CtrlMatrix[3].SetTimes(R.Get);
                else if (DigitKey[Key.D4]) CtrlMatrix[4].SetTimes(R.Get);
                else if (DigitKey[Key.D5]) CtrlMatrix[5].SetTimes(R.Get);
                else if (DigitKey[Key.D6]) CtrlMatrix[6].SetTimes(R.Get);
                else if (DigitKey[Key.D7]) CtrlMatrix[7].SetTimes(R.Get);
                else if (DigitKey[Key.D8]) CtrlMatrix[8].SetTimes(R.Get);
                else if (DigitKey[Key.D9]) CtrlMatrix[9].SetTimes(R.Get);
            }
        }

        private static int mashCounter = 0;
        private static Key KeyOld;
        internal static void KeyDownEvent(object sender, KeyEventArgs e)
        {            
            if (e.Key == KeyOld) mashCounter++;
            else mashCounter = 1;

            KeyOld = e.Key;

            if (ModifierKey[Key.LeftShift] || ModifierKey[Key.RightShift])
            {
                if (ArrowKey.ContainsKey(e.Key))
                {
                    UpdateShiftVector(ArrowKey[e.Key], mashCounter);
                }
            }
            else if (ModifierKey[Key.LeftCtrl] || ModifierKey[Key.RightCtrl])
            {
                if (ArrowKey.ContainsKey(e.Key))
                {
                    UpdateCtrlMatrix(ArrowKey[e.Key], mashCounter);
                }
            }
            else if (ModifierKey[Key.Space])
            {
                if (ArrowKey.ContainsKey(e.Key))
                {
                    UpdateSpaceVector(ArrowKey[e.Key]);
                }
            }
            else if (ModifierKey.ContainsKey(e.Key))
            {
                ModifierKey[e.Key] = true;
            }
            else if (DigitKey.ContainsKey(e.Key))
            {
                DigitKey[digitKeyOld] = false;
                DigitKey[e.Key] = true;
                digitKeyOld = e.Key;
            }
            else if (KeyEvent.ContainsKey(e.Key))
            {
                KeyEvent[e.Key]();
            }
        }

        internal static void KeyUpEvent(object sender, KeyEventArgs e)
        {
            if (ModifierKey.ContainsKey(e.Key))
            {
                ModifierKey[e.Key] = false;
            }
        }

    }

}