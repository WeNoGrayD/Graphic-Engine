using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.DataVisualization.Charting;
using System.Drawing;

namespace GraphicClassLibrary
{
    /* Класс обзорного окна. */

    public class ViewingWindow
    {
        /* Прямоугольники, задающие размеры маленького (через который
           смотрит наблюдатель на сцене) и большого (через который
           смотрит пользователь, элемент на форме) экранов. */

        public RectangleF VWsmall, VWbig;

        /* Значения масштаба большого прямоугольника по отношению 
           к маленькому по оси X и Y соответственно. */

        public double ScaleX, ScaleY;

        /* Словарь координатная_система-3D_точка, в котором хранится
           точка расположения камеры в различных координатных системах
           (видовая координатная система). */

        public Dictionary<string, Point3D> LocationPoint;

        /* Наблюдатель, к которому привязан экран. */

        public Observer Spectator;

        /* Матрица для перевода координат точек 3D-объектов
           в видовую систему координат. */

        public double[,] VCS_Matrix;

        /* Имя объекта. */

        public string Name { get; set; } = "Camera";

        /* Конструктор класса. */

        public ViewingWindow(RectangleF vw_small, RectangleF vw_big)
        {
            VWsmall = vw_small;
            VWbig = vw_big;
            ScaleX = VWbig.Width / VWsmall.Width;
            ScaleY = VWbig.Height / VWsmall.Height;

            LocationPoint = new Dictionary<string, Point3D>
            {
                { Scene.CoordinateSystems["WCS"], null },
                { Scene.CoordinateSystems["LCS"], new Point3D(0, 0, 0) },
                { Scene.CoordinateSystems["OCS"], null }
            };
        }

        /* Метод InitSpectator присваивает родителя-наблюдателя камере. */

        public void InitSpectator(Observer _spectator)
        {
            Spectator = _spectator;

            VCS_Matrix = CalcMethods.getRotationMatrix(Spectator.Rotations);

            LocationPoint["OCS"] = new Point3D(0, 0, Spectator.d);

            InitLocationPoint();
        }

        /* Метод InitLocationPoint инициализирует точку расположения камеры
           в соответствии с расположением наблюдателя в пространстве МСК. */

        public void InitLocationPoint()
        {
            Dictionary<char, double> dictAxesOffsets;

            double[,] _matrix = new double[4, 4];

            dictAxesOffsets = new Dictionary<char, double>
                                {
                                    { 'X', -Spectator.LocationPoint["WCS"].X },
                                    { 'Y', -Spectator.LocationPoint["WCS"].Y },
                                    { 'Z', -Spectator.LocationPoint["WCS"].Z }
                                };

            double[,] TR1 = CalcMethods.getTransitMatrix(
                new List<Dictionary<char, double>>
                {
                    dictAxesOffsets,
                    new Dictionary<char, double>
                    { {'Z', Spectator.d } }
                }
                );

            dictAxesOffsets = new Dictionary<char, double>
                                {
                                    { 'X', Spectator.LocationPoint["WCS"].X },
                                    { 'Y', Spectator.LocationPoint["WCS"].Y },
                                    { 'Z', Spectator.LocationPoint["WCS"].Z }
                                };

            double[,] TR2 = CalcMethods.getTransitMatrix(
                    new List<Dictionary<char, double>>
                    { dictAxesOffsets }
                    );

            CalcMethods.multiplyingMatrices(ref _matrix,
                new List<double[,]>
                { TR1, CalcMethods.GetTransposedMatrix(VCS_Matrix), TR2 }
                );

            LocationPoint["WCS"] = new Point3D();

            CalcMethods.CalculateLocationPoint(LocationPoint["WCS"],
                                            Spectator.LocationPoint["WCS"],
                                            _matrix);
        }

        /* Метод UpdateLocationPoint обновляет координаты камеры в пространстве. */

        public void UpdateLocationPoint(
            Dictionary<string, double[,]> UpdateMatrices)
        {
            List<string> CSs = new List<string>();

            /* Сначала обновляются координата расположения камеры
               в её локальной СК (хотя это маловероятно),
               затем в мировой, затем - в видовой. */

            if (UpdateMatrices.Keys.Contains("LCS"))
                CSs.Add("LCS");
            if (UpdateMatrices.Keys.Contains("WCS"))
                CSs.Add("WCS");
            if (UpdateMatrices.Keys.Contains("VCS"))
                CSs.Add("VCS");

            foreach (string CS in CSs)
            {
                Dictionary<char, double> dictAxesOffsets;

                double[,] _matrix = new double[4, 4];

                dictAxesOffsets = new Dictionary<char, double>
                                {
                                    { 'X', -Spectator.LocationPoint[CS].X },
                                    { 'Y', -Spectator.LocationPoint[CS].Y },
                                    { 'Z', -Spectator.LocationPoint[CS].Z }
                                };

                double[,] TR1 = CalcMethods.getTransitMatrix(
                    new List<Dictionary<char, double>>
                    { dictAxesOffsets,
                      new Dictionary<char, double>
                      { { 'Z', Spectator.d} } });

                double[,] UpdateMatrix = UpdateMatrices[CS];

                dictAxesOffsets = new Dictionary<char, double>
                                {
                                    { 'X', Spectator.LocationPoint[CS].X },
                                    { 'Y', Spectator.LocationPoint[CS].Y },
                                    { 'Z', Spectator.LocationPoint[CS].Z }
                                };

                double[,] TR2 = CalcMethods.getTransitMatrix(
                        new List<Dictionary<char, double>>
                        { dictAxesOffsets });

                CalcMethods.multiplyingMatrices(ref _matrix,
                        new List<double[,]> { TR1, UpdateMatrix, TR2 });

                CalcMethods.CalculateLocationPoint(LocationPoint[CS],
                                                Spectator.LocationPoint[CS],
                                                _matrix);
            }
        }

        /* Метод UpdateVCS_Matrix обновляет матрицу перевода
           координат в ВКС. */

        public void UpdateVCS_Matrix()
        {
            double[,] RT = CalcMethods.getRotationMatrix(
                new List<Dictionary<char, double>>
                { Spectator.Rotations.Last()});

            CalcMethods.multiplyingMatrices(ref VCS_Matrix,
                new List<double[,]> { VCS_Matrix, RT });
        }
    }
}
