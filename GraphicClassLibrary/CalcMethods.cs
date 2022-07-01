using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.DataVisualization.Charting;
using System.Drawing;

namespace GraphicClassLibrary
{
    /* Класс методов для вычисления, преимущественно работа с матрицами. */

    public class CalcMethods
    {
        /* Метод CalculateLoactionPoint производит вычисление
          новой точки положения объекта. */

        static public void CalculateLocationPoint(Point3D _point,
                                                  Point3D _startPoint,
                                                  double[,] TMatrix)
        {
            double[] locationMatrix = new double[4];

            double[] currentLocationMatrix = new double[]
            { _startPoint.X, _startPoint.Y, _startPoint.Z, 1 };

            // Проход по столбцам locationMatrix.

            for (int i = 0; i < 4; i++)
            {
                locationMatrix[i] = 0;

                // Ещё один проход.

                for (int j = 0; j < 4; j++)
                {
                    locationMatrix[i] += currentLocationMatrix[j] * TMatrix[j, i];
                }
            }

            _point.X = (float)locationMatrix[0];
            _point.Y = (float)locationMatrix[1];
            _point.Z = (float)locationMatrix[2];
        }

        /* Метод GetTransposedMatrix транспонирует заданную матрицу. */

        static public double[,] GetTransposedMatrix(double[,] _originalMatrix)
        {
            double[,] _transposedMatrix = new double[4, 4];

            for (int i = 0; i <= 3; i++)
            {
                for (int j = 0; j <= 3; j++)
                    _transposedMatrix[i, j] = _originalMatrix[j, i];
            }

            return _transposedMatrix;
        }

        /* Метод getTransitMatrix получает словарь "char-double",
           где char - ось вращения, double - значение перемещения в пикселях.
           Метод выдаёт на выходе результирующую матрицу TRoffsets 
           (матрицу сдвига). */

        static public double[,] getTransitMatrix(List<Dictionary<char, double>> lstAxesOffsets)
        {
            double[,] TRoffsets = new double[4, 4];

            List<double[,]> TRs = new List<double[,]>();

            foreach (Dictionary<char, double> dictAxesOffsets in lstAxesOffsets)
            {
                double[,] TR = new double[4, 4];
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                        TR[i, j] = 0;
                }
                TR[0, 0] = 1;
                TR[1, 1] = 1;
                TR[2, 2] = 1;
                TR[3, 3] = 1;

                foreach (char axis in dictAxesOffsets.Keys)
                {
                    switch (axis)
                    {
                        case 'X':
                            {
                                TR[3, 0] = dictAxesOffsets[axis];
                                break;
                            }
                        case 'Y':
                            {
                                TR[3, 1] = dictAxesOffsets[axis];
                                break;
                            }
                        case 'Z':
                            {
                                TR[3, 2] = dictAxesOffsets[axis];
                                break;
                            }
                    }
                }
                TRs.Add(TR);
            }

            multiplyingMatrices(ref TRoffsets, TRs);

            return TRoffsets;
        }

        /* Метод getRotationMatrix получает словарь "char-double",
           где char - ось вращения, double - угол вращения в градусах.
           Метод выдаёт на выходе результирующую матрицу RTangle
           (матрицу поворота). */

        static public double[,] getRotationMatrix(List<Dictionary<char, double>> lstAxesAngles)
        {
            double[,] RTangles = new double[4, 4];

            List<double[,]> RTs = new List<double[,]>();

            foreach (Dictionary<char, double> dAA in lstAxesAngles)
            {
                /* Приводим словарь "ось-угол" к формату YZX. */

                Dictionary<char, double> dictAxesAngles = new Dictionary<char, double>();
                if (dAA.ContainsKey('Y'))
                    dictAxesAngles.Add('Y', dAA['Y']);
                if (dAA.ContainsKey('Z'))
                    dictAxesAngles.Add('Z', dAA['Z']);
                if (dAA.ContainsKey('X'))
                    dictAxesAngles.Add('X', dAA['X']);

                /* Проходим по каждой оси в словаре "ось-угол". */

                foreach (char axis in dictAxesAngles.Keys)
                {
                    double[,] RT = new double[4, 4];
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                            RT[i, j] = 0;
                    }
                    RT[3, 3] = 1;

                    double angle = dictAxesAngles[axis] * Math.PI / 180;

                    double angleCos = Math.Cos(angle),
                           angleSin = Math.Sin(angle);

                    switch (axis)
                    {
                        case 'X':
                            {
                                RT[0, 0] = 1;
                                RT[1, 1] = angleCos;
                                RT[1, 2] = -angleSin;
                                RT[2, 1] = angleSin;
                                RT[2, 2] = angleCos;
                                break;
                            }
                        case 'Y':
                            {
                                RT[1, 1] = 1;
                                RT[0, 0] = angleCos;
                                RT[0, 2] = angleSin;
                                RT[2, 0] = -angleSin;
                                RT[2, 2] = angleCos;
                                break;
                            }
                        case 'Z':
                            {
                                RT[2, 2] = 1;
                                RT[0, 0] = angleCos;
                                RT[0, 1] = -angleSin;
                                RT[1, 0] = angleSin;
                                RT[1, 1] = angleCos;
                                break;
                            }
                    }
                    RTs.Add(RT);
                }
            }

            multiplyingMatrices(ref RTangles, RTs);

            return RTangles;
        }

        /* Метод getScalingMatrix получает словарь "char-double",
           где char - ось вращения, double - угол вращения в градусах.
           Метод выдаёт на выходе результирующую матрицу MSscales. */

        static public double[,] getScalingMatrix(List<Dictionary<char, double>> lstAxesScales)
        {
            double[,] MSscales = new double[4, 4];

            List<double[,]> MSs = new List<double[,]>();

            foreach (Dictionary<char, double> dictAxesScales in lstAxesScales)
            {
                double[,] MS = new double[4, 4];
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                        MS[i, j] = 0;
                }
                MS[3, 3] = 1;

                foreach (char axis in dictAxesScales.Keys)
                {
                    switch (axis)
                    {
                        case 'X':
                            {
                                MS[0, 0] = dictAxesScales[axis];
                                break;
                            }
                        case 'Y':
                            {
                                MS[1, 1] = dictAxesScales[axis];
                                break;
                            }
                        case 'Z':
                            {
                                MS[2, 2] = dictAxesScales[axis];
                                break;
                            }
                    }
                }
                MSs.Add(MS);
            }

            multiplyingMatrices(ref MSscales, MSs);

            return MSscales;
        }

        /* Метод multiplyingMatrices перемножает список матриц (1<n<4)х4 lstMatrices
           и выдаёт на выходе результирующую матрицу resultMatrix. */

        /* Принимает на вход матрицу, которую будет изменять (по ссылке),
           и список матриц, участвующих в перемножении. Изменяемая матрица
           считается пустой. */

        static public void multiplyingMatrices(ref double[,] resultMatrix,
                                               List<double[,]> lstMatrices)
        {
            resultMatrix = (double[,])lstMatrices[0].Clone();

            lstMatrices.RemoveAt(0);

            // Проход по матрицам lstMatrices.

            foreach (double[,] matrix in lstMatrices)
            {
                double[,] bufMatrix = new double[4, 4];
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                        bufMatrix[i, j] = 0;
                }

                // Проход по строкам bufMatrix.

                for (int i = 0; i < 4; i++)
                {
                    // Проход по столбцам bufMatrix.

                    for (int j = 0; j < 4; j++)
                    {
                        // Ещё один проход.

                        for (int k = 0; k < 4; k++)
                        {
                            bufMatrix[i, j] += resultMatrix[i, k] * matrix[k, j];
                        }
                    }
                }

                /* Сохренение буферной матрицы bufMatrix в результирующей
                   resultMatrix. */

                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                        resultMatrix[i, j] = bufMatrix[i, j];
                }
            }
        }

        /* Получение цвета в ARGB из HSL. */

        static public Color GetARGBFromHSL(float H, float S, float L)
        {
            int R = 0,
                G = 0,
                B = 0;

            float C = (1 - (float)Math.Abs((double)(2 * L - 1))) * S,
                  h = H / 60,
                  X = C * (1 - (float)Math.Abs((double)(h % 2 - 1))),
                  r = 0, g = 0, b = 0;
            if (h < 0)
                ;
            else
            {
                if (h < 1)
                {
                    r = C;
                    g = X;
                    b = 0;
                }
                else
                {
                    if (h < 2)
                    {
                        r = X;
                        g = C;
                        b = 0;
                    }
                    else
                    {
                        if (h < 3)
                        {
                            r = 0;
                            g = C;
                            b = X;
                        }
                        else
                        {
                            if (h < 4)
                            {
                                r = 0;
                                g = X;
                                b = C;
                            }
                            else
                            {
                                if (h < 5)
                                {
                                    r = X;
                                    g = 0;
                                    b = C;
                                }
                                else
                                {
                                    if (h < 6)
                                    {
                                        r = C;
                                        g = 0;
                                        b = X;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            float m = L - C / 2;

            R = (int)((r + m) * 255);
            G = (int)((g + m) * 255);
            B = (int)((b + m) * 255);

            if (R < 0)
                R = 0;
            else if (R > 255)
                R = 255;
            if (B < 0)
                B = 0;
            else if (B > 255)
                B = 255;
            if (G < 0)
                G = 0;
            else if (G > 255)
                G = 255;

            return Color.FromArgb(R, G, B);
        }
    }
}
