using System;
using System.Reflection;

namespace OpenRCF
{        
    public class BlockMatrix
    {
        private Matrix[,] matrix;
        private Matrix _matrix;

        public BlockMatrix(uint[] rows, uint[] columns)
        {
            matrix = new Matrix[rows.Length, columns.Length];

            for (uint i = 0; i < matrix.GetLength(0); i++)
            {
                for (uint j = 0; j < matrix.GetLength(1); j++)
                {
                    matrix[i, j] = new Matrix(rows[i], columns[j]);
                }
            }

            _matrix = new Matrix(_Sum(rows), _Sum(columns));
        }

        public BlockMatrix(uint blockRows, uint rowsConst, uint[] columns)
        {
            matrix = new Matrix[blockRows, columns.Length];

            for (uint i = 0; i < matrix.GetLength(0); i++)
            {
                for (uint j = 0; j < matrix.GetLength(1); j++)
                {
                    matrix[i, j] = new Matrix(rowsConst, columns[j]);
                }
            }

            _matrix = new Matrix(blockRows * rowsConst, _Sum(columns));
        }

        public BlockMatrix(uint[] rows, uint blockColumns, uint columnsConst)
        {
            matrix = new Matrix[rows.Length, blockColumns];

            for (uint i = 0; i < matrix.GetLength(0); i++)
            {
                for (uint j = 0; j < matrix.GetLength(1); j++)
                {
                    matrix[i, j] = new Matrix(rows[i], columnsConst);
                }
            }

            _matrix = new Matrix(_Sum(rows), blockColumns * columnsConst);
        }

        public BlockMatrix(uint blockRows, uint rowsConst, uint blockColumns, uint columnsConst)
        {
            matrix = new Matrix[blockRows, blockColumns];

            for (uint i = 0; i < matrix.GetLength(0); i++)
            {
                for (uint j = 0; j < matrix.GetLength(1); j++)
                {
                    matrix[i, j] = new Matrix(rowsConst, columnsConst);
                }
            }

            _matrix = new Matrix(blockRows * rowsConst, blockColumns * columnsConst);
        }

        private uint _Sum(uint[] array)
        {
            uint result = 0;

            for (int i = 0; i < array.Length; i++)
            {
                result += array[i];
            }

            return result;
        }

        private void _MatrixCopy(Matrix[,] matrix)
        {
            uint row = 0, col = 0;

            for (uint I = 0; I < this.matrix.GetLength(0); I++)
            {
                for (uint J = 0; J < this.matrix.GetLength(1); J++)
                {
                    for (uint i = 0; i < this.matrix[I, J].GetLength(0); i++)
                    {
                        for (uint j = 0; j < this.matrix[I, J].GetLength(1); j++)
                        {
                            _matrix[row + i, col + j] = matrix[I, J][i, j];
                        }
                    }
                    col += this.matrix[0, J].GetLength(1);
                }
                row += this.matrix[I, 0].GetLength(0);
                col = 0;
            }
        }

        private void _MatrixCopy(float[,] matrix)
        {
            uint row = 0, col = 0;

            for (uint I = 0; I < this.matrix.GetLength(0); I++)
            {
                for (uint J = 0; J < this.matrix.GetLength(1); J++)
                {
                    for (uint i = 0; i < this.matrix[I, J].GetLength(0); i++)
                    {
                        for (uint j = 0; j < this.matrix[I, J].GetLength(1); j++)
                        {
                            this.matrix[I, J][i, j] = matrix[row + i, col + j];
                        }
                    }
                    col += this.matrix[0, J].GetLength(1);
                }
                row += this.matrix[I, 0].GetLength(0);
                col = 0;
            }
        }

        public uint GetLength(int dimension)
        {
            return (uint)matrix.GetLength(dimension);
        }

        public Matrix[,] Get
        {
            get{ return matrix; }
        }

        public Matrix[,] Set
        {
            set
            {
                if (matrix.GetLength(0) == value.GetLength(0) && matrix.GetLength(1) == value.GetLength(1))
                {
                    for (int I = 0; I < matrix.GetLength(0); I++)
                    {
                        for (int J = 0; J < matrix.GetLength(1); J++)
                        {
                            matrix[I, J].Set = value[I, J].Get;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                    ConsoleWriteErrorMessage();
                }
            }
        }

        public float[,] GetMatrix
        {
            get
            {
                _MatrixCopy(matrix);
                return _matrix.Get;
            }
        }

        public float[,] SetMatrix
        {
            set
            {
                if (_matrix.GetLength(0) == value.GetLength(0) && _matrix.GetLength(1) == value.GetLength(1))
                {
                    _MatrixCopy(value);
                }
                else
                {
                    Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                    ConsoleWriteErrorMessage();
                }
            }
        }

        public void FixBlockDiagonal()
        {
            for (int I = 0; I < matrix.GetLength(0); I++)
            {
                for (int J = 0; J < matrix.GetLength(1); J++)
                {
                    if (I != J) matrix[I, J].FixZeroMatrix();
                }
            }
        }

        public void FixBlockLowerTriangle()
        {
            for (int I = 0; I < matrix.GetLength(0); I++)
            {
                for (int J = 0; J < matrix.GetLength(1); J++)
                {
                    if (I < J) matrix[I, J].FixZeroMatrix();
                }
            }
        }

        public void FixBlockUpperTriangle()
        {
            for (int I = 0; I < matrix.GetLength(0); I++)
            {
                for (int J = 0; J < matrix.GetLength(1); J++)
                {
                    if (I > J) matrix[I, J].FixZeroMatrix();
                }
            }
        }

        public Matrix this[uint row, uint column]
        {
            get { return matrix[row, column]; }
            set { matrix[row, column] = value; }
        }

        public Matrix[] this[uint row]
        {
            get
            {
                Matrix[] result = new Matrix[matrix.GetLength(1)];

                if (row < matrix.GetLength(0))
                {
                    for (int I = 0; I < matrix.GetLength(1); I++)
                    {
                        result[I] = new Matrix(matrix[row, I].GetLength(0), matrix[row, I].GetLength(1));
                        result[I].Set = matrix[row, I].Get;
                    }
                }
                else
                {
                    Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                    ConsoleWriteErrorMessage();
                }

                return result;
            }
            set
            {
                if (matrix.GetLength(1) == value.GetLength(0) && row < matrix.GetLength(0))
                {
                    for (int I = 0; I < matrix.GetLength(1); I++)
                    {
                        matrix[row, I].Set = value[I].Get;
                    }
                }
                else
                {
                    Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                    ConsoleWriteErrorMessage();
                }
            }
        }

        public Matrix[] GetColumn(uint column)
        {
            Matrix[] result = new Matrix[matrix.GetLength(0)];

            if (column < matrix.GetLength(1))
            {
                for (int I = 0; I < matrix.GetLength(0); I++)
                {
                    result[I].Set = matrix[I, column].Get;
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public void SetColumn(uint column, Matrix[] matrix)
        {
            if (this.matrix.GetLength(0) == matrix.GetLength(0) && column < this.matrix.GetLength(1))
            {
                for (int I = 0; I < this.matrix.GetLength(0); I++)
                {
                    this.matrix[I, column].Set = matrix[I].Get;
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }
        }

        public void SetIdentity()
        {
            for (int I = 0; I < matrix.GetLength(0); I++)
            {
                for (int J = 0; J < matrix.GetLength(1); J++)
                {
                    if (I == J) matrix[I, J].SetIdentity();
                    else matrix[I, J].SetZeroMatrix();
                }
            }
        }

        public void SetDiagonal(float diag)
        {
            for (int I = 0; I < matrix.GetLength(0); I++)
            {
                for (int J = 0; J < matrix.GetLength(1); J++)
                {
                    if (I == J) matrix[I, J].SetDiagonal(diag);
                    else matrix[I, J].SetZeroMatrix(); ;
                }
            }
        }

        public void SetDiagonal(Vector[] vector)
        {
            int k = 0;

            for (int I = 0; I < matrix.GetLength(0); I++)
            {
                for (int J = 0; J < matrix.GetLength(1); J++)
                {
                    if (I == J)
                    {
                        matrix[I, J].SetDiagonal(vector[k].Get);
                        k++;
                    }
                    else matrix[I, J].SetZeroMatrix();

                    if (k == vector.GetLength(0)) return;
                }
            }
        }

        public Matrix[,] GetDiagonal
        {
            get
            {
                Matrix[,] result = new Matrix[matrix.GetLength(0), matrix.GetLength(1)];

                for (int I = 0; I < matrix.GetLength(0); I++)
                {
                    for (int J = 0; J < matrix.GetLength(1); J++)
                    {
                        result[I, J] = new Matrix(matrix[I, J].GetLength(0), matrix[I, J].GetLength(1));
                        if (I == J) result[I, J].Set = matrix[I, J].Get;
                    }
                }

                return result;
            }
        }

        public void ConsoleWrite(ushort digit = 2)
        {
            _MatrixCopy(matrix);
            _matrix.ConsoleWrite(digit);
        }

        public void SetRandom(int min = -10, int max = 10)
        {
            for (uint I = 0; I < matrix.GetLength(0); I++)
            {
                for (uint J = 0; J < matrix.GetLength(1); J++)
                {
                    matrix[I, J].SetRandom(min, max);
                }
            }
        }

        public Matrix[,] Transpose
        {
            get
            {
                Matrix[,] result = new Matrix[matrix.GetLength(1), matrix.GetLength(0)];

                for (int I = 0; I < matrix.GetLength(1); I++)
                {
                    for (int J = 0; J < matrix.GetLength(0); J++)
                    {
                        result[I, J] = new Matrix(matrix[J, I].GetLength(1), matrix[J, I].GetLength(0));
                        result[I, J].Set = matrix[J, I].Transpose;
                    }
                }

                return result;
            }
        }
     
        public Matrix[,] TransposeTimes(Matrix[,] matrix)
        {
            Matrix[,] result = new Matrix[this.matrix.GetLength(1), matrix.GetLength(1)];

            if (this.matrix.GetLength(0) == matrix.GetLength(0))
            {
                for (int I = 0; I < this.matrix.GetLength(1); I++)
                {
                    for (int J = 0; J < matrix.GetLength(1); J++)
                    {
                        result[I, J] = new Matrix(this.matrix[0, I].GetLength(1), matrix[0, J].GetLength(1));

                        for (int K = 0; K < this.matrix.GetLength(0); K++)
                        {
                            result[I, J].SetPlus(this.matrix[K, I].TransposeTimes(matrix[K, J].Get));
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public Vector[] TransposeTimes(Vector[] vector)
        {
            Vector[] result = new Vector[matrix.GetLength(1)];

            if (matrix.GetLength(0) == vector.GetLength(0))
            {
                for (int I = 0; I < matrix.GetLength(1); I++)
                {
                    result[I] = new Vector(matrix[0, I].GetLength(1));

                    for (int J = 0; J < vector.GetLength(0); J++)
                    {
                        result[I].SetPlus(matrix[J, I].TransposeTimes(vector[J].Get));
                    }
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public Matrix[,] Times(Matrix[,] matrix)
        {
            Matrix[,] result = new Matrix[this.matrix.GetLength(0), matrix.GetLength(1)];

            if (this.matrix.GetLength(1) == matrix.GetLength(0))
            {
                for (int I = 0; I < this.matrix.GetLength(0); I++)
                {
                    for (int J = 0; J < matrix.GetLength(1); J++)
                    {
                        result[I, J] = new Matrix(this.matrix[I, 0].GetLength(0), matrix[0, J].GetLength(1));

                        for (int K = 0; K < this.matrix.GetLength(1); K++)
                        {
                            result[I, J].SetPlus(this.matrix[I, K].Times(matrix[K, J].Get));
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public Vector[] Times(Vector[] vector)
        {
            Vector[] result = new Vector[matrix.GetLength(0)];

            if (matrix.GetLength(1) == vector.GetLength(0))
            {
                for (int I = 0; I < matrix.GetLength(0); I++)
                {
                    result[I] = new Vector(matrix[I, 0].GetLength(0));

                    for (int J = 0; J < vector.GetLength(0); J++)
                    {
                        result[I].SetPlus(matrix[I, J].Times(vector[J].Get));
                    }
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public Matrix[,] Times(float scalar)
        {
            Matrix[,] result = new Matrix[matrix.GetLength(0), matrix.GetLength(1)];

            for (int I = 0; I < matrix.GetLength(0); I++)
            {
                for (int J = 0; J < matrix.GetLength(1); J++)
                {
                    result[I, J] = new Matrix(matrix[I, J].GetLength(0), matrix[I, J].GetLength(1));
                    result[I, J].Set = matrix[I, J].Times(scalar);
                }
            }

            return result;
        }

        public Matrix[,] TimesHadamard(Matrix[,] matrix)
        {
            Matrix[,] result = new Matrix[this.matrix.GetLength(0), this.matrix.GetLength(1)];

            if (this.matrix.GetLength(0) == matrix.GetLength(0) && this.matrix.GetLength(1) == matrix.GetLength(1))
            {
                for (int I = 0; I < this.matrix.GetLength(0); I++)
                {
                    for (int J = 0; J < this.matrix.GetLength(1); J++)
                    {
                        result[I, J] = new Matrix(this.matrix[I, J].GetLength(0), this.matrix[I, J].GetLength(1));
                        result[I, J].Set = this.matrix[I, J].TimesHadamard(matrix[I, J].Get);
                    }
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public Matrix[,] TimesLeft(Matrix[,] matrix)
        {
            Matrix[,] result = new Matrix[matrix.GetLength(0), this.matrix.GetLength(1)];

            if (matrix.GetLength(1) == this.matrix.GetLength(0))
            {
                for (int I = 0; I < matrix.GetLength(0); I++)
                {
                    for (int J = 0; J < this.matrix.GetLength(1); J++)
                    {
                        result[I, J] = new Matrix(matrix[I, 0].GetLength(0), this.matrix[0, J].GetLength(1));

                        for (int K = 0; K < matrix.GetLength(1); K++)
                        {
                            result[I, J].SetPlus(matrix[I, K].Times(this.matrix[K, J].Get));
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public Matrix[,] Plus(Matrix[,] matrix)
        {
            Matrix[,] result = new Matrix[this.matrix.GetLength(0), this.matrix.GetLength(1)];

            if (this.matrix.GetLength(0) == matrix.GetLength(0) && this.matrix.GetLength(1) == matrix.GetLength(1))
            {
                for (int I = 0; I < this.matrix.GetLength(0); I++)
                {
                    for (int J = 0; J < this.matrix.GetLength(1); J++)
                    {
                        result[I, J] = new Matrix(this.matrix[I, J].GetLength(0), this.matrix[I, J].GetLength(1));
                        result[I, J].Set = this.matrix[I, J].Plus(matrix[I, J].Get);
                    }
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public void SetPlus(Matrix[,] matrix)
        {
            if (this.matrix.GetLength(0) == matrix.GetLength(0) && this.matrix.GetLength(1) == matrix.GetLength(1))
            {
                for (int I = 0; I < this.matrix.GetLength(0); I++)
                {
                    for (int J = 0; J < this.matrix.GetLength(1); J++)
                    {
                        this.matrix[I, J].SetPlus(matrix[I, J].Get);
                    }
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }
        }

        public Matrix[,] Minus(Matrix[,] matrix)
        {
            Matrix[,] result = new Matrix[this.matrix.GetLength(0), this.matrix.GetLength(1)];

            if (this.matrix.GetLength(0) == matrix.GetLength(0) && this.matrix.GetLength(1) == matrix.GetLength(1))
            {
                for (int I = 0; I < this.matrix.GetLength(0); I++)
                {
                    for (int J = 0; J < this.matrix.GetLength(1); J++)
                    {
                        result[I, J] = new Matrix(this.matrix[I, J].GetLength(0), this.matrix[I, J].GetLength(1));
                        result[I, J].Set = this.matrix[I, J].Minus(matrix[I, J].Get);
                    }
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public void SetMinus(Matrix[,] matrix)
        {
            if (this.matrix.GetLength(0) == matrix.GetLength(0) && this.matrix.GetLength(1) == matrix.GetLength(1))
            {
                for (int I = 0; I < this.matrix.GetLength(0); I++)
                {
                    for (int J = 0; J < this.matrix.GetLength(1); J++)
                    {
                        this.matrix[I, J].SetMinus(matrix[I, J].Get);
                    }
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }
        }

        public float QuadraticForm(Vector[] vector)
        {
            float result = 0;
            Vector[] vectorTmp;

            if (matrix.GetLength(1) == vector.GetLength(0) && matrix.GetLength(0) == vector.GetLength(0))
            {
                vectorTmp = this.Times(vector);

                for (int I = 0; I < matrix.GetLength(0); I++)
                {
                    result += vector[I].TimesDot(vectorTmp[I].Get);
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public Matrix[,] Inverse
        {
            get
            {
                Matrix[,] result = new Matrix[matrix.GetLength(0), matrix.GetLength(1)];

                if (_matrix.GetLength(0) == _matrix.GetLength(1))
                {
                    _MatrixCopy(matrix);
                    _matrix.SetInverse();

                    uint row = 0, col = 0;

                    for (uint I = 0; I < matrix.GetLength(0); I++)
                    {
                        for (uint J = 0; J < matrix.GetLength(1); J++)
                        {
                            result[I, J] = new Matrix(matrix[I, J].GetLength(0), matrix[I, J].GetLength(1));

                            for (uint i = 0; i < matrix[I, J].GetLength(0); i++)
                            {
                                for (uint j = 0; j < matrix[I, J].GetLength(1); j++)
                                {
                                    result[I, J][i, j] = _matrix[row + i, col + j];
                                }
                            }
                            col += matrix[0, J].GetLength(1);
                        }
                        row += matrix[I, 0].GetLength(0);
                        col = 0;
                    }
                }
                else
                {
                    Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                    ConsoleWriteErrorMessage();
                }

                return result;
            }
        }

        public float Trace
        {
            get
            {
                float result = 0;

                for (int I = 0; I < matrix.GetLength(0); I++)
                {
                    if (I < matrix.GetLength(1)) result += matrix[I, I].Trace;
                }

                return result;
            }
        }

        public void SetInverse()
        {
            _MatrixCopy(matrix);
            _MatrixCopy(_matrix.Inverse);
        }

        private void ConsoleWriteErrorMessage(string text = "")
        {
            if (text == "") Console.WriteLine("Please check the number of rows or columns in block matrix.");
            else Console.WriteLine(text);
        }
    }

    public class Matrix
    {
        private float[,] matrix;
        private bool isZeroMatrix;

        public Matrix(uint rows, uint columns)
        {
            matrix = new float[rows, columns];
            isZeroMatrix = false;
        }

        public uint GetLength(int k)
        {
            return (uint)matrix.GetLength(k);
        }

        public float[,] Get
        {
            get
            {
                float[,] result = new float[matrix.GetLength(0), matrix.GetLength(1)];

                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        result[i, j] = matrix[i, j];
                    }
                }

                return result;
            }
        }

        public float[,] Set
        {
            set
            {
                if (matrix.GetLength(0) == value.GetLength(0) && matrix.GetLength(1) == value.GetLength(1))
                {
                    if (isZeroMatrix) return;

                    for (int i = 0; i < matrix.GetLength(0); i++)
                    {
                        for (int j = 0; j < matrix.GetLength(1); j++)
                        {
                            matrix[i, j] = value[i, j];
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                    ConsoleWriteErrorMessage();
                }
            }
        }

        public float this[uint row, uint column]
        {
            get { return matrix[row, column]; }
            set
            {
                if (isZeroMatrix) return;
                matrix[row, column] = value;
            }
        }

        public float[] this[uint row]
        {
            get
            {
                float[] result = new float[matrix.GetLength(1)];

                if (row < matrix.GetLength(0))
                {
                    for (int i = 0; i < matrix.GetLength(1); i++)
                    {
                        result[i] = matrix[row, i];
                    }
                }
                else
                {
                    Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                    ConsoleWriteErrorMessage();
                }

                return result;
            }
            set
            {
                if (matrix.GetLength(1) == value.GetLength(0) && row < matrix.GetLength(0))
                {
                    if (isZeroMatrix) return;

                    for (int i = 0; i < matrix.GetLength(1); i++)
                    {
                        matrix[row, i] = value[i];
                    }
                }
                else
                {
                    Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                    ConsoleWriteErrorMessage();
                }
            }
        }

        public float[] GetColumn(uint column)
        {
            float[] result = new float[matrix.GetLength(0)];

            if (column < matrix.GetLength(1))
            {
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    result[i] = matrix[i, column];
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public void SetColumn(uint column, float[] vector)
        {
            if (matrix.GetLength(0) == vector.Length && column < matrix.GetLength(1))
            {
                if (isZeroMatrix) return;

                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    matrix[i, column] = vector[i];
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }
        }

        public void SetIdentity()
        {
            if (isZeroMatrix) return;

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if (i == j) matrix[i, j] = 1;
                    else matrix[i, j] = 0;
                }
            }
        }

        public void SetDiagonal(float diag)
        {
            if (isZeroMatrix) return;

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if (i == j) matrix[i, j] = diag;
                    else matrix[i, j] = 0;
                }
            }
        }

        public void SetDiagonal(params float[] vector)
        {
            if (isZeroMatrix) return;

            int k = 0;

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if (i == j)
                    {
                        matrix[i, j] = vector[k];
                        k++;
                    }
                    else matrix[i, j] = 0;

                    if (k == vector.GetLength(0)) return;
                }
            }
        }

        public float[,] GetDiagonal
        {
            get
            {
                float[,] result = new float[matrix.GetLength(0), matrix.GetLength(1)];

                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        if (i == j) result[i, j] = matrix[i, j];
                    }
                }

                return result;
            }
        }

        public void SetZeroMatrix()
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    matrix[i, j] = 0;
                }
            }
        }

        public void FixZeroMatrix()
        {
            SetZeroMatrix();
            isZeroMatrix = true;
        }

        public void SetRandom(int min = -10, int max = 10)
        {
            if (isZeroMatrix) return;

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    matrix[i, j] = Random.random.Next(min, max);
                }
            }
        }

        public void ConsoleWrite(ushort digit = 2)
        {
            string text;
            string Fn = "F" + digit.ToString();
            int cursorPos;

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    text = matrix[i, j].ToString(Fn);
                    cursorPos = (12 + digit) * (j + 1) - text.Length;
                    if (0 < cursorPos) Console.CursorLeft = cursorPos;
                    Console.Write(text);
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        public float[,] Transpose
        {
            get
            {
                float[,] result = new float[matrix.GetLength(1), matrix.GetLength(0)];

                for (int i = 0; i < matrix.GetLength(1); i++)
                {
                    for (int j = 0; j < matrix.GetLength(0); j++)
                    {
                        result[i, j] = matrix[j, i];
                    }
                }

                return result;
            }
        }

        public float[,] TransposeTimes(float[,] matrix)
        {
            float[,] result = new float[this.matrix.GetLength(1), matrix.GetLength(1)];

            if (isZeroMatrix) return result;

            if (this.matrix.GetLength(0) == matrix.GetLength(0))
            {
                for (int i = 0; i < this.matrix.GetLength(1); i++)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        for (int k = 0; k < this.matrix.GetLength(0); k++)
                        {
                            result[i, j] += this.matrix[k, i] * matrix[k, j];
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public float[] TransposeTimes(float[] vector)
        {
            float[] result = new float[matrix.GetLength(1)];

            if (matrix.GetLength(0) == vector.GetLength(0))
            {
                if (isZeroMatrix) return result;

                for (int i = 0; i < matrix.GetLength(1); i++)
                {
                    for (int j = 0; j < vector.GetLength(0); j++)
                    {
                        result[i] += matrix[j, i] * vector[j];
                    }
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public float[,] Times(float[,] matrix)
        {
            float[,] result = new float[this.matrix.GetLength(0), matrix.GetLength(1)];

            if (this.matrix.GetLength(1) == matrix.GetLength(0))
            {
                if (isZeroMatrix) return result;

                for (int i = 0; i < this.matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        for (int k = 0; k < this.matrix.GetLength(1); k++)
                        {
                            result[i, j] += this.matrix[i, k] * matrix[k, j];
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public float[] Times(float[] vector)
        {
            float[] result = new float[matrix.GetLength(0)];

            if (matrix.GetLength(1) == vector.GetLength(0))
            {
                if (isZeroMatrix) return result;

                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < vector.GetLength(0); j++)
                    {
                        result[i] += matrix[i, j] * vector[j];
                    }
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public float[,] Times(float scalar)
        {
            float[,] result = new float[matrix.GetLength(0), matrix.GetLength(1)];

            if (isZeroMatrix) return result;

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    result[i, j] = scalar * matrix[i, j];
                }
            }

            return result;
        }

        public void SetTimes(float scalar)
        {      
            if (isZeroMatrix) return;

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    matrix[i, j] = scalar * matrix[i, j];                    
                }
            }            
        }

        public float[,] TimesHadamard(float[,] matrix)
        {
            float[,] result = new float[this.matrix.GetLength(0), this.matrix.GetLength(1)];

            if (this.matrix.GetLength(0) == matrix.GetLength(0) && this.matrix.GetLength(1) == matrix.GetLength(1))
            {
                if (isZeroMatrix) return result;

                for (int i = 0; i < this.matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < this.matrix.GetLength(1); j++)
                    {
                        result[i, j] = this.matrix[i, j] * matrix[i, j];
                    }
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public float[,] TimesLeft(float[,] matrix)
        {
            float[,] result = new float[matrix.GetLength(0), this.matrix.GetLength(1)];

            if (matrix.GetLength(1) == this.matrix.GetLength(0))
            {
                if (isZeroMatrix) return result;

                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < this.matrix.GetLength(1); j++)
                    {
                        for (int k = 0; k < matrix.GetLength(1); k++)
                        {
                            result[i, j] += matrix[i, k] * this.matrix[k, j];
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public float[] TimesLeft(float[] vector)
        {
            float[] result = new float[matrix.GetLength(1)];

            if (vector.Length == matrix.GetLength(0))
            {
                if (isZeroMatrix) return result;

                for (int i = 0; i < matrix.GetLength(1); i++)
                {
                    for (int j = 0; j < vector.Length; j++)
                    {
                        result[i] += vector[j] * matrix[j, i];
                    }
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }
            return result;
        }

        public float QuadraticForm(float[] vector)
        {
            float result = 0;
            float[] vectorTmp;

            if (matrix.GetLength(1) == vector.GetLength(0) && matrix.GetLength(0) == vector.GetLength(0))
            {
                if (isZeroMatrix) return result;

                vectorTmp = this.Times(vector);

                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    result += vector[i] * vectorTmp[i];
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public float[,] Plus(float[,] matrix)
        {
            float[,] result = new float[this.matrix.GetLength(0), this.matrix.GetLength(1)];

            if (this.matrix.GetLength(0) == matrix.GetLength(0) && this.matrix.GetLength(1) == matrix.GetLength(1))
            {
                for (int i = 0; i < this.matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < this.matrix.GetLength(1); j++)
                    {
                        result[i, j] = this.matrix[i, j] + matrix[i, j];
                    }
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public void SetPlus(float[,] matrix)
        {
            if (this.matrix.GetLength(0) == matrix.GetLength(0) && this.matrix.GetLength(1) == matrix.GetLength(1))
            {
                if (isZeroMatrix) return;

                for (int i = 0; i < this.matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < this.matrix.GetLength(1); j++)
                    {
                        this.matrix[i, j] += matrix[i, j];
                    }
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }
        }

        public float[,] Minus(float[,] matrix)
        {
            float[,] result = new float[this.matrix.GetLength(0), this.matrix.GetLength(1)];

            if (this.matrix.GetLength(0) == matrix.GetLength(0) && this.matrix.GetLength(1) == matrix.GetLength(1))
            {
                for (int i = 0; i < this.matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < this.matrix.GetLength(1); j++)
                    {
                        result[i, j] = this.matrix[i, j] - matrix[i, j];
                    }
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public void SetMinus(float[,] matrix)
        {
            if (this.matrix.GetLength(0) == matrix.GetLength(0) && this.matrix.GetLength(1) == matrix.GetLength(1))
            {
                if (isZeroMatrix) return;

                for (int i = 0; i < this.matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < this.matrix.GetLength(1); j++)
                    {
                        this.matrix[i, j] -= matrix[i, j];
                    }
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }
        }

        public float[,] Inverse
        {
            get
            {
                float[,] result = new float[matrix.GetLength(0), matrix.GetLength(1)];

                if (matrix.GetLength(0) == matrix.GetLength(1))
                {
                    int n = matrix.GetLength(0);

                    for (int i = 0; i < n; i++)
                    {
                        for (int j = 0; j < n; j++)
                        {
                            if (i == j) result[i, j] = 1;
                            else result[i, j] = 0;

                        }
                    }

                    float[,] origin = new float[matrix.GetLength(0), matrix.GetLength(1)];

                    for (int i = 0; i < matrix.GetLength(0); i++)
                    {
                        for (int j = 0; j < matrix.GetLength(1); j++)
                        {
                            origin[i, j] = matrix[i, j];
                        }
                    }

                    int max;
                    float tmp;

                    for (int i = 0; i < n; i++)
                    {
                        max = i;
                        for (int j = i + 1; j < n; j++)
                        {
                            if (Math.Abs(origin[j, i]) > Math.Abs(origin[max, i]))
                            {
                                max = j;
                            }
                        }

                        if (max != i)
                        {
                            for (int k = 0; k < n; k++)
                            {
                                tmp = origin[max, k];
                                origin[max, k] = origin[i, k];
                                origin[i, k] = tmp;

                                tmp = result[max, k];
                                result[max, k] = result[i, k];
                                result[i, k] = tmp;
                            }
                        }

                        tmp = origin[i, i];

                        if (0.00001f < Math.Abs(tmp))
                        {
                            for (int k = 0; k < n; k++)
                            {
                                origin[i, k] /= tmp;
                                result[i, k] /= tmp;
                            }

                            for (int j = 0; j < n; j++)
                            {
                                if (i != j)
                                {
                                    tmp = origin[j, i] / origin[i, i];
                                    for (int k = 0; k < n; k++)
                                    {
                                        origin[j, k] = origin[j, k] - origin[i, k] * tmp;
                                        result[j, k] = result[j, k] - result[i, k] * tmp;
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                            ConsoleWriteErrorMessage("Unable to compute inverse matrix");
                            break;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                    ConsoleWriteErrorMessage();
                }

                return result;
            }
        }

        public void SetInverse()
        {
            this.Set = this.Inverse;
        }

        public float Trace
        {
            get
            {
                float result = 0;

                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    if (i < matrix.GetLength(1)) result += matrix[i, i];
                }

                return result;
            }
        }

        /*
        public float[,] CholeskyDecomposition
        {
            get
            {
                float[,] result = new float[matrix.GetLength(0), matrix.GetLength(1)];

                if (matrix.GetLength(0) == matrix.GetLength(1))
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        result[j, j] = (float)Math.Sqrt(matrix[j, j] - CalcllT(j, j, result));

                        if (result[j, j] < 0.00001f || float.IsNaN(result[j, j]))
                        {
                            Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                            ConsoleWriteErrorMessage("Matrix must be positive definite and symmetric");
                            result[j, j] = 0;
                            break;
                        }
                        else
                        {
                            for (int i = j + 1; i < matrix.GetLength(0); i++)
                            {
                                result[i, j] = (matrix[i, j] - CalcllT(i, j, result)) / result[j, j];
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                    ConsoleWriteErrorMessage();
                }

                return result;
            }
        }

        private float CalcllT(int i, int j, float[,] L)
        {
            float llT = 0;

            for (int k = 0; k < i && k < j; k++)
            {
                llT = llT + L[i, k] * L[j, k];
            }

            return llT;
        }

        public float[,] Inverse_LowerTriangle
        {
            get
            {
                float[,] result = new float[matrix.GetLength(0), matrix.GetLength(1)];
                float[,] origin = new float[matrix.GetLength(0), matrix.GetLength(1)];

                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        origin[i, j] = matrix[i, j];
                    }
                }

                if (matrix.GetLength(0) == matrix.GetLength(1))
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        for (int i = j + 1; i < matrix.GetLength(0); i++)
                        {
                            if (0.00001f < Math.Abs(origin[i, i]))
                            {
                                origin[i, j] = origin[i, j] / origin[i, i];
                            }
                            else
                            {
                                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                                ConsoleWriteErrorMessage("Unable to compute inverse matrix");
                                return result;
                            }
                        }
                    }

                    for (int i = 0; i < matrix.GetLength(0); i++)
                    {
                        result[i, i] = 1 / origin[i, i];
                        origin[i, i] = 1;
                    }

                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        for (int k = 0; k < j + 1; k++)
                        {
                            for (int i = j + 1; i < matrix.GetLength(0); i++)
                            {
                                result[i, k] = result[i, k] - origin[i, j] * result[j, k];
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                    ConsoleWriteErrorMessage();
                }

                return result;
            }
        }
        */

        private void ConsoleWriteErrorMessage(string text = "")
        {
            if (text == "") Console.WriteLine("Please check the number of rows or columns.");
            else Console.WriteLine(text);
        }
    }

    public class RotationMatrix
    {
        private float[,] matrix = new float[3, 3];

        public RotationMatrix(double roll = 0, double pitch = 0, double yaw = 0)
        {            
            SetRollPitchYaw(roll, pitch, yaw);
        }

        public uint GetLength(int k)
        {
            return (uint)matrix.GetLength(k);
        }

        public float[,] Get
        {
            get
            {
                float[,] result = new float[matrix.GetLength(0), matrix.GetLength(1)];

                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        result[i, j] = matrix[i, j];
                    }
                }

                return result;
            }
        }

        public float[,] Set
        {
            set
            {
                if (matrix.GetLength(0) == value.GetLength(0) && matrix.GetLength(1) == value.GetLength(1))
                {
                    for (int i = 0; i < matrix.GetLength(0); i++)
                    {
                        for (int j = 0; j < matrix.GetLength(1); j++)
                        {
                            matrix[i, j] = value[i, j];
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                    ConsoleWriteErrorMessage();
                }
            }
        }

        public float this[uint row, uint column]
        {
            get { return matrix[row, column]; }
            set { matrix[row, column] = value; }
        }

        public float[] this[uint row]
        {
            get
            {
                float[] result = new float[matrix.GetLength(1)];

                if (row < matrix.GetLength(0))
                {
                    for (int i = 0; i < matrix.GetLength(1); i++)
                    {
                        result[i] = matrix[row, i];
                    }
                }
                else
                {
                    Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                    ConsoleWriteErrorMessage();
                }

                return result;
            }
            set
            {
                if (matrix.GetLength(1) == value.GetLength(0) && row < matrix.GetLength(0))
                {
                    for (int i = 0; i < matrix.GetLength(1); i++)
                    {
                        matrix[row, i] = value[i];
                    }
                }
                else
                {
                    Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                    ConsoleWriteErrorMessage();
                }
            }
        }

        public float[] GetColumn(uint column)
        {
            float[] result = new float[matrix.GetLength(0)];

            if (column < matrix.GetLength(1))
            {
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    result[i] = matrix[i, column];
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public void SetColumn(uint column, float[] vector)
        {
            if (matrix.GetLength(0) == vector.Length && column < matrix.GetLength(1))
            {
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    matrix[i, column] = vector[i];
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }
        }

        public void SetIdentity()
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if (i == j) matrix[i, j] = 1;
                    else matrix[i, j] = 0;
                }
            }
        }

        public void SetRollPitchYaw(double roll, double pitch, double yaw)
        {
            SetRollPitchYaw((float)roll, (float)pitch, (float)yaw);
        }

        public void SetRollPitchYaw(float roll, float pitch, float yaw)
        {
            float Sr = (float)Math.Sin(roll);
            float Sp = (float)Math.Sin(pitch);
            float Sy = (float)Math.Sin(yaw);

            float Cr = (float)Math.Cos(roll);
            float Cp = (float)Math.Cos(pitch);
            float Cy = (float)Math.Cos(yaw);

            matrix[0, 0] = Cy * Cp;
            matrix[1, 0] = Sy * Cp;
            matrix[2, 0] = -Sp;

            matrix[0, 1] = Cy * Sp * Sr - Sy * Cr;
            matrix[1, 1] = Sy * Sp * Sr + Cy * Cr;
            matrix[2, 1] = Cp * Sr;

            matrix[0, 2] = Cy * Sp * Cr + Sy * Sr;
            matrix[1, 2] = Sy * Sp * Cr - Cy * Sr;
            matrix[2, 2] = Cp * Cr;
        }

        public float[] RollPitchYaw
        {
            get
            {
                float[] result = new float[3];

                result[0] = (float)Math.Atan2(matrix[2, 1], matrix[2, 2]);
                result[1] = (float)Math.Atan2(-matrix[2, 0], Math.Sqrt(matrix[2, 1] * matrix[2, 1] + matrix[2, 2] * matrix[2, 2]));
                result[2] = (float)Math.Atan2(matrix[1, 0], matrix[0, 0]);

                return result;
            }
        }

        private float[,] Rx(float theta)
        {
            float[,] result = new float[matrix.GetLength(0), matrix.GetLength(1)];

            result[0, 0] = 1;
            result[0, 1] = 0;
            result[0, 2] = 0;

            result[1, 0] = 0;
            result[1, 1] = (float)Math.Cos(theta);
            result[1, 2] = -(float)Math.Sin(theta);

            result[2, 0] = 0;
            result[2, 1] = (float)Math.Sin(theta);
            result[2, 2] = (float)Math.Cos(theta);

            return result;
        }

        public void SetRx(float theta)
        {
            this.Set = Rx(theta);
        }

        public void SetRx(double theta)
        {            
            this.Set = Rx((float)theta);
        }

        private float[,] Ry(float theta)
        {
            float[,] result = new float[matrix.GetLength(0), matrix.GetLength(1)];

            result[0, 0] = (float)Math.Cos(theta);
            result[0, 1] = 0;
            result[0, 2] = (float)Math.Sin(theta);

            result[1, 0] = 0;
            result[1, 1] = 1;
            result[1, 2] = 0;

            result[2, 0] = -(float)Math.Sin(theta);
            result[2, 1] = 0;
            result[2, 2] = (float)Math.Cos(theta);

            return result;
        }

        public void SetRy(float theta)
        {      
            this.Set = Ry(theta);
        }

        public void SetRy(double theta)
        {
            this.Set = Ry((float)theta);            
        }

        private float[,] Rz(float theta)
        {
            float[,] result = new float[matrix.GetLength(0), matrix.GetLength(1)];

            result[0, 0] = (float)Math.Cos(theta);
            result[0, 1] = -(float)Math.Sin(theta);
            result[0, 2] = 0;

            result[1, 0] = (float)Math.Sin(theta);
            result[1, 1] = (float)Math.Cos(theta);
            result[1, 2] = 0;

            result[2, 0] = 0;
            result[2, 1] = 0;
            result[2, 2] = 1;

            return result;
        }

        public void SetRz(float theta)
        {
            this.Set = Rz((float)theta);
        }

        public void SetRz(double theta)
        {
            this.Set = Rz((float)theta);
        }
   
        private float[,] Rn(float theta, float[] axis)
        {
            float[,] result = new float[matrix.GetLength(0), matrix.GetLength(1)];

            float Cth = (float)Math.Cos(theta);
            float Sth = (float)Math.Sin(theta);

            result[0, 0] = Cth + axis[0] * axis[0] * (1 - Cth);
            result[0, 1] = axis[0] * axis[1] * (1 - Cth) - axis[2] * Sth;
            result[0, 2] = axis[0] * axis[2] * (1 - Cth) + axis[1] * Sth;

            result[1, 0] = axis[0] * axis[1] * (1 - Cth) + axis[2] * Sth;
            result[1, 1] = Cth + axis[1] * axis[1] * (1 - Cth);
            result[1, 2] = axis[1] * axis[2] * (1 - Cth) - axis[0] * Sth;

            result[2, 0] = axis[0] * axis[2] * (1 - Cth) - axis[1] * Sth;
            result[2, 1] = axis[1] * axis[2] * (1 - Cth) + axis[0] * Sth;
            result[2, 2] = Cth + axis[2] * axis[2] * (1 - Cth);

            return result;
        }

        public void SetRn(float theta, float[] axis)
        {       
            if(axis.Length != 3)
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);                
                ConsoleWriteErrorMessage("Length of axis vector ( number of elements ) must be 3");
                return;
            }

            float axisNorm = (float)Math.Sqrt(axis[0] * axis[0] + axis[1] * axis[1] + axis[2] * axis[2]);
            
            if(0.001f < Math.Abs(1 - axisNorm))
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage("Norm of axis vector must be 1");
                return;
            }

            this.Set = Rn(theta, axis);
        }
  
        public void ConsoleWrite(ushort digit = 2)
        {
            string text;
            string Fn = "F" + digit.ToString();
            int cursorPos;

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    text = matrix[i, j].ToString(Fn);
                    cursorPos = (8 + digit) * (j + 1) - text.Length;
                    if (0 < cursorPos) Console.CursorLeft = cursorPos;
                    Console.Write(text);
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        public float[,] Transpose
        {
            get
            {
                float[,] result = new float[matrix.GetLength(1), matrix.GetLength(0)];

                for (int i = 0; i < matrix.GetLength(1); i++)
                {
                    for (int j = 0; j < matrix.GetLength(0); j++)
                    {
                        result[i, j] = matrix[j, i];
                    }
                }

                return result;
            }
        }

        public void SetTranspose()
        {
            for (int i = 0; i < matrix.GetLength(1); i++)
            {
                for (int j = 0; j < matrix.GetLength(0); j++)
                {
                    matrix[i, j] = matrix[j, i];
                }
            }
        }

        public float[,] TransposeTimes(float[,] matrix)
        {
            float[,] trans = this.Transpose;       
            float[,] result = new float[trans.GetLength(0), matrix.GetLength(1)];

            if (trans.GetLength(1) == matrix.GetLength(0))
            {
                for (int i = 0; i < trans.GetLength(0); i++)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        for (int k = 0; k < trans.GetLength(1); k++)
                        {
                            result[i, j] += trans[i, k] * matrix[k, j];
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public float[] TransposeTimes(float[] vector)
        {
            float[,] trans = this.Transpose;
            float[] result = new float[trans.GetLength(0)];

            if (trans.GetLength(1) == vector.GetLength(0))
            {
                for (int i = 0; i < trans.GetLength(0); i++)
                {
                    for (int j = 0; j < vector.GetLength(0); j++)
                    {
                        result[i] += trans[i, j] * vector[j];
                    }
                }
            }           
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public float[,] Times(float[,] matrix)
        {
            float[,] result = new float[this.matrix.GetLength(0), matrix.GetLength(1)];

            if (this.matrix.GetLength(1) == matrix.GetLength(0))
            {
                for (int i = 0; i < this.matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        for (int k = 0; k < this.matrix.GetLength(1); k++)
                        {
                            result[i, j] += this.matrix[i, k] * matrix[k, j];
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public void SetTimes(float[,] matrix)
        {
            this.Set = this.Times(matrix);
        }

        public float[] Times(float[] vector)
        {
            float[] result = new float[matrix.GetLength(0)];

            if (matrix.GetLength(1) == vector.GetLength(0))
            {
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < vector.GetLength(0); j++)
                    {
                        result[i] += matrix[i, j] * vector[j];
                    }
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }
            return result;
        }
   
        public float[,] TimesRx(float theta)
        {
            return Times(Rx(theta));
        }

        public float[,] TimesRy(float theta)
        {
            return Times(Ry(theta));
        }

        public float[,] TimesRz(float theta)
        {
            return Times(Rz(theta));
        }

        public float[,] TimesRn(float theta, float[] axis)
        {
            return Times(Rn(theta, axis));
        }

        public float[,] TimesLeft(float[,] matrix)
        {
            float[,] result = new float[matrix.GetLength(0), this.matrix.GetLength(1)];

            if (matrix.GetLength(1) == this.matrix.GetLength(0))
            {
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < this.matrix.GetLength(1); j++)
                    {
                        for (int k = 0; k < matrix.GetLength(1); k++)
                        {
                            result[i, j] += matrix[i, k] * this.matrix[k, j];
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public float[] TimesLeft(float[] vector)
        {
            float[] result = new float[matrix.GetLength(1)];

            if (vector.Length == matrix.GetLength(0))
            {
                for (int i = 0; i < matrix.GetLength(1); i++)
                {
                    for (int j = 0; j < vector.Length; j++)
                    {
                        result[i] += vector[j] * matrix[j, i];
                    }
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }
            return result;
        }

        public float[,] Plus(float[,] matrix)
        {
            float[,] result = new float[this.matrix.GetLength(0), this.matrix.GetLength(1)];

            if (this.matrix.GetLength(0) == matrix.GetLength(0) && this.matrix.GetLength(1) == matrix.GetLength(1))
            {
                for (int i = 0; i < this.matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < this.matrix.GetLength(1); j++)
                    {
                        result[i, j] = this.matrix[i, j] + matrix[i, j];
                    }
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public void SetPlus(float[,] matrix)
        {
            if (this.matrix.GetLength(0) == matrix.GetLength(0) && this.matrix.GetLength(1) == matrix.GetLength(1))
            {
                for (int i = 0; i < this.matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < this.matrix.GetLength(1); j++)
                    {
                        this.matrix[i, j] += matrix[i, j];
                    }
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }
        }

        public float[,] Minus(float[,] matrix)
        {
            float[,] result = new float[this.matrix.GetLength(0), this.matrix.GetLength(1)];

            if (this.matrix.GetLength(0) == matrix.GetLength(0) && this.matrix.GetLength(1) == matrix.GetLength(1))
            {
                for (int i = 0; i < this.matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < this.matrix.GetLength(1); j++)
                    {
                        result[i, j] = this.matrix[i, j] - matrix[i, j];
                    }
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public void SetMinus(float[,] matrix)
        {
            if (this.matrix.GetLength(0) == matrix.GetLength(0) && this.matrix.GetLength(1) == matrix.GetLength(1))
            {
                for (int i = 0; i < this.matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < this.matrix.GetLength(1); j++)
                    {
                        this.matrix[i, j] -= matrix[i, j];
                    }
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }
        }

        public float[,] Inverse
        {
            get { return this.Transpose; }
        }

        public void SetInverse()
        {
            this.SetTranspose();
        }

        public float Trace
        {
            get
            {
                float result = 0;

                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    if (i < matrix.GetLength(1)) result += matrix[i, i];
                }

                return result;
            }
        }

        private readonly float epsilon = 0.0001f;
        public float[] AngleAxisVector
        {
            get
            {
                float[] s = new float[3];
                float[] n = new float[3];
                float sNorm, theta;
                float[] result = new float[3];

                s[0] = matrix[2, 1] - matrix[1, 2];
                s[1] = matrix[0, 2] - matrix[2, 0];
                s[2] = matrix[1, 0] - matrix[0, 1];

                sNorm = (float)Math.Sqrt(s[0] * s[0] + s[1] * s[1] + s[2] * s[2]);

                if (epsilon <= sNorm)
                {
                    n[0] = s[0] / sNorm;
                    n[1] = s[1] / sNorm;
                    n[2] = s[2] / sNorm;
                }
                else if (1 - epsilon <= matrix[0, 0] + matrix[1, 1] + matrix[2, 2])
                {
                    n[0] = 0;
                    n[1] = 0;
                    n[2] = 0;
                }
                else if (matrix[0, 1] >= 0 - epsilon && matrix[1, 2] >= 0 - epsilon && matrix[2, 0] >= 0 - epsilon)
                {
                    n[0] = (float)Math.Sqrt(0.5f * (matrix[0, 0] + 1));
                    n[1] = (float)Math.Sqrt(0.5f * (matrix[1, 1] + 1));
                    n[2] = (float)Math.Sqrt(0.5f * (matrix[2, 2] + 1));
                    sNorm = 0;
                }
                else if (matrix[0, 1] >= 0 - epsilon && matrix[1, 2] <= 0 + epsilon && matrix[2, 0] <= 0 + epsilon)
                {
                    n[0] = (float)Math.Sqrt(0.5f * (matrix[0, 0] + 1));
                    n[1] = (float)Math.Sqrt(0.5f * (matrix[1, 1] + 1));
                    n[2] = -(float)Math.Sqrt(0.5f * (matrix[2, 2] + 1));
                    sNorm = 0;
                }
                else if (matrix[0, 1] <= 0 + epsilon && matrix[1, 2] <= 0 + epsilon && matrix[2, 0] >= 0 - epsilon)
                {
                    n[0] = (float)Math.Sqrt(0.5f * (matrix[0, 0] + 1));
                    n[1] = -(float)Math.Sqrt(0.5f * (matrix[1, 1] + 1));
                    n[2] = (float)Math.Sqrt(0.5f * (matrix[2, 2] + 1));
                    sNorm = 0;
                }
                else if (matrix[0, 1] <= 0 + epsilon && matrix[1, 2] >= 0 - epsilon && matrix[2, 0] <= 0 + epsilon)
                {
                    n[0] = -(float)Math.Sqrt(0.5f * (matrix[0, 0] + 1));
                    n[1] = (float)Math.Sqrt(0.5f * (matrix[1, 1] + 1));
                    n[2] = (float)Math.Sqrt(0.5f * (matrix[2, 2] + 1));
                    sNorm = 0;
                }
                else
                {
                    Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                    ConsoleWriteErrorMessage("This is not a rotation matrix");
                }

                theta = (float)Math.Atan2(sNorm, matrix[0, 0] + matrix[1, 1] + matrix[2, 2] - 1);

                result[0] = theta * n[0];
                result[1] = theta * n[1];
                result[2] = theta * n[2];

                return result;
            }
        }

        public float[] HomArray16
        {
            get
            {
                float[] result = new float[16];

                result[0] = matrix[0, 0];
                result[1] = matrix[1, 0];
                result[2] = matrix[2, 0];

                result[4] = matrix[0, 1];
                result[5] = matrix[1, 1];
                result[6] = matrix[2, 1];

                result[8] = matrix[0, 2];
                result[9] = matrix[1, 2];
                result[10] = matrix[2, 2];

                result[15] = 1;

                return result;
            }
        }

        private void ConsoleWriteErrorMessage(string text = "")
        {
            if (text == "") Console.WriteLine("Please check the number of rows or columns.");
            else Console.WriteLine(text);
        }
    }

    public class BlockVector
    {
        private Vector[] vector;
        private Vector _vector;

        public BlockVector(uint[] rows)
        {
            vector = new Vector[rows.Length];

            for (int I = 0; I < vector.Length; I++)
            {
                vector[I] = new Vector(rows[I]);
            }

            _vector = new Vector(_Sum(rows));
        }

        public BlockVector(uint blockRows, uint rowsConst)
        {
            vector = new Vector[blockRows];

            for (int I = 0; I < vector.Length; I++)
            {
                vector[I] = new Vector(rowsConst);
            }

            _vector = new Vector(blockRows * rowsConst);
        }

        private uint _Sum(uint[] array)
        {
            uint result = 0;

            for (int i = 0; i < array.Length; i++)
            {
                result += array[i];
            }

            return result;
        }

        public uint Length
        {
            get{ return (uint)vector.Length; }
        }

        private void _VectorCopy(Vector[] vector)
        {
            uint row = 0;

            for (uint I = 0; I < this.vector.Length; I++)
            {
                for (uint i = 0; i < this.vector[I].Length; i++)
                {
                    _vector[row + i] = vector[I][i];                    
                }
                row += this.vector[I].Length;
            }            
        }

        private void _VectorCopy(float[] vector)
        {
            uint row = 0;

            for (uint I = 0; I < this.vector.Length; I++)
            {
                for (uint i = 0; i < this.vector[I].Length; i++)
                {
                    this.vector[I][i] = vector[row + i];
                }
                row += this.vector[I].Length;
            }
        }

        public Vector[] Get
        {
            get
            {
                Vector[] result = new Vector[vector.Length];

                for (int I = 0; I < vector.Length; I++)
                {
                    result[I] = new Vector(vector[I].Length);
                    result[I] = vector[I];
                }

                return result;
            }
        }

        public Vector[] Set
        {
            set
            {
                if (vector.Length == value.Length)
                {
                    for (int I = 0; I < vector.Length; I++)
                    {
                        vector[I].Set = value[I].Get;
                    }
                }
                else
                {
                    Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                    ConsoleWriteErrorMessage();
                }
            }
        }

        public float[] GetVector
        {
            get
            {
                _VectorCopy(vector);
                return _vector.Get;
            }
        }

        public float[] SetVector
        {
            set
            {
                if (_vector.Length == value.Length)
                {
                    _VectorCopy(value);
                }
                else
                {
                    Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                    ConsoleWriteErrorMessage();
                }
            }
        }

        public Vector this[uint row]
        {
            get { return vector[row]; }
            set { vector[row].Set = value.Get; }
        }

        public float Norm
        {
            get
            {
                float result = 0;

                for (int I = 0; I < vector.Length; I++)
                {
                    result += vector[I].SquareSum;
                }

                return (float)Math.Sqrt(result);
            }
        }

        public float SquareSum
        {
            get
            {
                float result = 0;

                for (int I = 0; I < vector.Length; I++)
                {
                    result += vector[I].SquareSum;                    
                }

                return result;
            }
        }

        public Vector[] Abs
        {
            get
            {
                Vector[] result = new Vector[vector.Length];

                for (int I = 0; I < vector.Length; I++)
                {
                    result[I] = new Vector(vector[I].Length);
                    result[I].Set = vector[I].Abs;                    
                }

                return result;
            }
        }

        public void SetAbs()
        {
            for (int I = 0; I < vector.Length; I++)
            {
                vector[I].SetAbs();                
            }
        }

        public float Sum
        {
            get
            {
                float result = 0;

                for (int I = 0; I < vector.Length; I++)
                {
                    result += vector[I].Sum;
                }

                return result;
            }
        }

        public float AbsSum
        {
            get
            {
                float result = 0;

                for (int I = 0; I < vector.Length; I++)
                {
                    result += vector[I].AbsSum;
                }

                return result;
            }
        }

        public void SetZeroVector()
        {
            for (int I = 0; I < vector.Length; I++)
            {
                vector[I].SetZeroVector();
            }
        }

        public void SetRandom(int min = -10, int max = 10)
        {
            for (int I = 0; I < vector.Length; I++)
            {
                vector[I].SetRandom(min, max);
            }
        }

        public void ConsoleWrite(ushort digit = 2)
        {
            _VectorCopy(vector);
            _vector.ConsoleWrite(digit);
        }

        public Vector[] Plus(Vector[] vector)
        {
            Vector[] result = new Vector[this.vector.Length];

            if (this.vector.Length == vector.Length)
            {
                for (int I = 0; I < this.vector.Length; I++)
                {
                    result[I] = new Vector(this.vector[I].Length);
                    result[I].Set = this.vector[I].Plus(vector[I].Get);
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public void SetPlus(Vector[] vector)
        {
            if (this.vector.Length == vector.Length)
            {
                for (int I = 0; I < this.vector.Length; I++)
                {
                    this.vector[I].SetPlus(vector[I].Get);
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }
        }

        public Vector[] Minus(Vector[] vector)
        {
            Vector[] result = new Vector[this.vector.Length];

            if (this.vector.Length == vector.Length)
            {
                for (int I = 0; I < this.vector.Length; I++)
                {
                    result[I] = new Vector(this.vector[I].Length);
                    result[I].Set = this.vector[I].Minus(vector[I].Get);
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public void SetMinus(Vector[] vector)
        {
            if (this.vector.Length == vector.Length)
            {
                for (int I = 0; I < this.vector.Length; I++)
                {
                    this.vector[I].SetMinus(vector[I].Get);
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }
        }
     
        public Vector[] Times(float scalar)
        {
            Vector[] result = new Vector[vector.Length];

            for (int I = 0; I < vector.Length; I++)
            {
                result[I] = new Vector(vector[I].Length);
                result[I].Set = vector[I].Times(scalar);
            }

            return result;
        }

        public float TimesDot(Vector[] vector)
        {
            float result = 0;

            if (this.vector.Length == vector.Length)
            {
                for (int I = 0; I < this.vector.Length; I++)
                {
                    result += this.vector[I].TimesDot(vector[I].Get);
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public Vector[] TimesHadamard(Vector[] vector)
        {
            Vector[] result = new Vector[this.vector.Length];

            if (this.vector.Length == vector.Length)
            {
                for (int I = 0; I < this.vector.Length; I++)
                {
                    result[I] = new Vector(this.vector[I].Length);
                    result[I].Set = this.vector[I].TimesHadamard(vector[I].Get);
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public float Distance(Vector[] vector)
        {
            float result = 0;

            if (this.vector.Length == vector.Length)
            {
                Vector[] error = Minus(vector);

                for (int I = 0; I < this.vector.Length; I++)
                {
                    result += error[I].SquareSum;
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return (float)Math.Sqrt(result);
        }

        public Vector[] Normalize
        {
            get
            {
                Vector[] result = new Vector[vector.Length];
                float norm = _GetNorm(vector);

                if (0.0001f < norm)
                {
                    for (int I = 0; I < vector.Length; I++)
                    {
                        result[I] = new Vector(vector[I].Length);
                        result[I].Set = vector[I].Times(1 / norm);
                    }
                }
                else
                {
                    Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                    ConsoleWriteErrorMessage("Zero vector cannot be normalized");
                }
                return result;
            }
        }

        public void SetNormalize()
        {
            this.Set = this.Normalize;
        }

        public float FormedAngle(Vector[] vector)
        {
            float dotProduct = TimesDot(vector);
            float normProduct = _GetNorm(this.vector) * _GetNorm(vector);

            if (normProduct < 0.0001f)
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage("Formed angle by a zero vector cannot be calculated");
                return 0;
            }
            else
            {
                float d = dotProduct / normProduct;

                if (1 <= d) return 0;
                else if (d <= -1) return (float)Math.PI;
                else return (float)Math.Acos(d);
            }
        }

        private float _GetNorm(Vector[] vector)
        {
            float result = 0;

            for (int I = 0; I < vector.Length; I++)
            {
                result += vector[I].SquareSum;
            }

            return (float)Math.Sqrt(result);
        }

        private void ConsoleWriteErrorMessage(string text = "")
        {
            if (text == "") Console.WriteLine("Please check the length of block vector.");
            else Console.WriteLine(text);
        }
    }

    public class Vector
    {
        private float[] vector;

        public Vector(uint rows)
        {
            vector = new float[rows];
        }

        public uint Length
        {
            get
            {
                return (uint)vector.Length;
            }
        }

        public float[] Get
        {
            get
            {
                float[] result = new float[vector.Length];

                for (int i = 0; i < vector.Length; i++)
                {
                    result[i] = vector[i];
                }
           
                return result;
            }
        }

        public float[] Set
        {
            set
            {
                if (vector.Length == value.Length)
                {
                    for (int i = 0; i < vector.Length; i++)
                    {
                        vector[i] = value[i];
                    }
                }
                else
                {
                    Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                    ConsoleWriteErrorMessage();
                }
            }
        }

        public void SetStack(float[] vector1, float[] vector2)
        {
            if (vector.Length == vector1.Length + vector2.Length)
            {
                for (int i = 0; i < vector1.Length; i++)
                {
                    vector[i] = vector1[i];
                }

                for (int i = 0; i < vector2.Length; i++)
                {
                    vector[i + vector1.Length] = vector2[i];
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }
        }
 
        public float Max
        {
            get
            {
                float result = vector[0];

                for(int i = 1; i < vector.Length; i++)
                {
                    if (result < vector[i]) result = vector[i];
                }

                return result;
            }
        }

        public float Min
        {
            get
            {
                float result = vector[0];

                for (int i = 1; i < vector.Length; i++)
                {
                    if (vector[i] < result) result = vector[i];
                }

                return result;
            }
        }

        public float this[uint row]
        {
            get { return vector[row]; }
            set { vector[row] = value; }
        }

        public float Norm
        {
            get
            {
                float result = 0;

                for (int i = 0; i < vector.Length; i++)
                {
                    result += vector[i] * vector[i];
                }

                return (float)Math.Sqrt(result);
            }
        }

        public float NormXY
        {
            get
            {
                if (1 < vector.Length) return (float)Math.Sqrt(vector[0] * vector[0] + vector[1] * vector[1]);
                else return Math.Abs(vector[0]);
            }
        }

        public float SquareSum
        {
            get
            {
                float result = 0;

                for (int i = 0; i < vector.Length; i++)
                {
                    result += vector[i] * vector[i];
                }

                return result;
            }
        }

        public float[] Abs
        {
            get
            {
                float[] result = new float[vector.Length];

                for (int i = 0; i < vector.Length; i++)
                {
                    result[i] = Math.Abs(vector[i]);
                }

                return result;
            }
        }

        public void SetAbs()
        {
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] = Math.Abs(vector[i]);
            }
        }

        public float Sum
        {
            get
            {
                float result = 0;

                for (int i = 0; i < vector.Length; i++)
                {
                    result += vector[i];
                }

                return result;
            }
        }

        public float AbsSum
        {
            get
            {
                float result = 0;

                for (int i = 0; i < vector.Length; i++)
                {
                    result += Math.Abs(vector[i]);
                }

                return result;
            }
        }

        public void SetZeroVector()
        {
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] = 0;                
            }
        }

        public void SetUnitVectorX(float alpha = 1)
        {
            for (int i = 0; i < vector.Length; i++)
            {
                if(i == 0) vector[i] = alpha;
                else vector[i] = 0;
            }
        }

        public void SetUnitVectorY(float alpha = 1)
        {
            for (int i = 0; i < vector.Length; i++)
            {
                if (i == 1) vector[i] = alpha;
                else vector[i] = 0;
            }
        }

        public void SetUnitVectorZ(float alpha = 1)
        {
            for (int i = 0; i < vector.Length; i++)
            {
                if (i == 2) vector[i] = alpha;
                else vector[i] = 0;
            }
        }

        public void SetUnitVector(uint n, float alpha = 1)
        {
            for (int i = 0; i < vector.Length; i++)
            {
                if (i == n) vector[i] = alpha;
                else vector[i] = 0;
            }
        }
        
        public void SetRandom(int min = -10, int max = 10)
        {
            if (min <= max)
            {
                for (int i = 0; i < vector.Length; i++)
                {
                    vector[i] = Random.random.Next(min, max);
                }
            }
        }

        public void ConsoleWrite(ushort digit = 2)
        {
            Console.Write("  [");
            for (int i = 0; i < vector.Length; i++)
            {
                if (0 <= (int)vector[i]) Console.Write("   " + vector[i].ToString("F" + digit.ToString()) + "   ");
                else Console.Write("  " + vector[i].ToString("F" + digit.ToString()) + "   ");
            }
            Console.WriteLine("]^T");
            Console.WriteLine();
        }

        public float[] MiddlePoint(float[] vector)
        {
            float[] result = new float[this.vector.Length];

            if (this.vector.Length == vector.Length)
            {
                for (int i = 0; i < this.vector.Length; i++)
                {
                    result[i] = 0.5f * (this.vector[i] + vector[i]);
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public float[] Plus(float[] vector, float s = 1, float t = 1)
        {
            float[] result = new float[this.vector.Length];

            if (this.vector.Length == vector.Length)
            {
                for (int i = 0; i < this.vector.Length; i++)
                {
                    result[i] = s * this.vector[i] + t * vector[i];
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public void SetPlus(float[] vector)
        {    
            if (this.vector.Length == vector.Length)
            {
                for (int i = 0; i < this.vector.Length; i++)
                {
                    this.vector[i] += vector[i];
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }
        }

        public float[] Minus(float[] vector)
        {
            float[] result = new float[this.vector.Length];

            if (this.vector.Length == vector.Length)
            {
                for (int i = 0; i < this.vector.Length; i++)
                {
                    result[i] = this.vector[i] - vector[i];
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public void SetMinus(float[] vector)
        {
            if (this.vector.Length == vector.Length)
            {
                for (int i = 0; i < this.vector.Length; i++)
                {
                    this.vector[i] -= vector[i];
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }
        }

        public float Distance(float[] vector)
        {       
            float result = 0;

            if (this.vector.Length == vector.Length)
            {       
                float[] error = Minus(vector);

                for (int i = 0; i < this.vector.Length; i++)
                {
                    result += error[i] * error[i];
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }
            
            return (float)Math.Sqrt(result);     
            
        }

        public float[] Times(float scalar)
        {
            float[] result = new float[vector.Length];

            for (int i = 0; i < vector.Length; i++)
            {
                result[i] = scalar * vector[i];
            }

            return result;
        }

        public void SetTimes(float scalar)
        {            
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] = scalar * vector[i];
            }           
        }

        public float TimesDot(float[] vector)
        {
            float result = 0;

            if (this.vector.Length == vector.Length)
            {
                for (int i = 0; i < this.vector.Length; i++)
                {
                    result += this.vector[i] * vector[i];
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public float[] TimesCross(float[] vector)
        {
            float[] result = new float[this.vector.Length];

            if (this.vector.Length == 3 && vector.Length == 3)
            {
                result[0] = this.vector[1] * vector[2] - this.vector[2] * vector[1];
                result[1] = this.vector[2] * vector[0] - this.vector[0] * vector[2];
                result[2] = this.vector[0] * vector[1] - this.vector[1] * vector[0];
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage("Length of vector must be 3");
            }

            return result;
        }

        public float[] TimesHadamard(float[] vector)
        {
            float[] result = new float[this.vector.Length];

            if (this.vector.Length == vector.Length)
            {
                for (int i = 0; i < this.vector.Length; i++)
                {
                    result[i] = this.vector[i] * vector[i];
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage();
            }

            return result;
        }

        public float[] Normalize
        {
            get
            {
                float[] result = new float[vector.Length];
                float norm = _GetNorm(vector);

                if (0.0001f < norm)
                {
                    for (int i = 0; i < vector.Length; i++)
                    {
                        result[i] = vector[i] / norm;
                    }
                }
                else
                {
                    Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);                    
                    ConsoleWriteErrorMessage("Zero vector cannot be normalized");
                }
                return result;
            }
        }

        public void SetNormalize()
        {
            this.Set = this.Normalize;
        }

        public float[] NormalizedCrossProduct(float[] vector)
        {
            float[] result = new float[this.vector.Length];

            if (this.vector.Length == 3 && vector.Length == 3)
            {
                result[0] = this.vector[1] * vector[2] - this.vector[2] * vector[1];
                result[1] = this.vector[2] * vector[0] - this.vector[0] * vector[2];
                result[2] = this.vector[0] * vector[1] - this.vector[1] * vector[0];
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage("Length of vector must be 3");                
                return result;
            }

            float norm = _GetNorm(result);

            if(norm < 0.0001f)
            {
                if (0.001f < Math.Abs(this.vector[0])) result = new float[3] { this.vector[1], -this.vector[0], 0 };
                else if (0.001f < Math.Abs(this.vector[1])) result = new float[3] { 0, this.vector[2], -this.vector[1] };
                else if (0.001f < Math.Abs(this.vector[2])) result = new float[3] { -this.vector[2], 0, this.vector[0] };

                norm = _GetNorm(result);
            }
            
            if (0.0001f < norm)
            {
                for (int i = 0; i < this.vector.Length; i++)
                {
                    result[i] = result[i] / norm;
                }
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage("Zero vector cannot be normalized");
            }

            return result;
        }

        public float FormedAngle(float[] vector)
        {
            float dotProduct = TimesDot(vector);
            float normProduct = _GetNorm(this.vector) * _GetNorm(vector);

            if (normProduct < 0.0001f)
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                ConsoleWriteErrorMessage("Formed angle by a zero vector cannot be calculated");
                return 0;
            }
            else
            {
                float d = dotProduct / normProduct;

                if (1 <= d) return 0;
                else if (d <= -1) return (float)Math.PI;
                else return (float)Math.Acos(d);
            }
        }

        private float _GetNorm(float[] vector)
        {
            float result = 0;

            for (int i = 0; i < vector.Length; i++)
            {
                result += vector[i] * vector[i];
            }

            return (float)Math.Sqrt(result);            
        }

        public float[,] ConvertRotationMatrix
        {
            get
            {
                float[,] result = new float[3, 3] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };
             
                if(vector.Length != 3)
                {
                    Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                    ConsoleWriteErrorMessage();
                    return result;
                }

                float norm = _GetNorm(vector);

                if (0.0001f < norm)
                {                    
                    float normXY = (float)Math.Sqrt(vector[0] * vector[0] + vector[1] * vector[1]); // norm of cross product with unitZ           
                    float[] n = new float[3];

                    if (0.0001f < normXY)
                    {
                        n[0] = -vector[1] / normXY;
                        n[1] = vector[0] / normXY;
                        n[2] = 0;
                    }
                    else
                    {                        
                        if (0 < vector[2]) return result;
                        else n = new float[3] { 0, 1, 0 };
                    }

                    float Sth = normXY / norm;
                    float Cth = vector[2] / norm;

                    result[0, 0] = Cth + n[0] * n[0] * (1 - Cth);
                    result[0, 1] = n[0] * n[1] * (1 - Cth) - n[2] * Sth;
                    result[0, 2] = n[0] * n[2] * (1 - Cth) + n[1] * Sth;

                    result[1, 0] = n[0] * n[1] * (1 - Cth) + n[2] * Sth;
                    result[1, 1] = Cth + n[1] * n[1] * (1 - Cth);
                    result[1, 2] = n[1] * n[2] * (1 - Cth) - n[0] * Sth;

                    result[2, 0] = n[0] * n[2] * (1 - Cth) - n[1] * Sth;
                    result[2, 1] = n[1] * n[2] * (1 - Cth) + n[0] * Sth;
                    result[2, 2] = Cth + n[2] * n[2] * (1 - Cth);
                }
         
                return result;
            }
        }

        public float[] HomArray16
        {
            get
            {
                float[] result = new float[16];
                float[,] matrix = ConvertRotationMatrix;

                result[0] = matrix[0, 0];
                result[1] = matrix[1, 0];
                result[2] = matrix[2, 0];

                result[4] = matrix[0, 1];
                result[5] = matrix[1, 1];
                result[6] = matrix[2, 1];

                result[8] = matrix[0, 2];
                result[9] = matrix[1, 2];
                result[10] = matrix[2, 2];

                result[15] = 1;

                return result;                
            }
        }

        private void ConsoleWriteErrorMessage(string text = "")
        {
            if (text == "") Console.WriteLine("Please check the length of vector.");
            else Console.WriteLine(text);
        }

    }

    struct Random
    {
        public static System.Random random = new System.Random();
    }

}
