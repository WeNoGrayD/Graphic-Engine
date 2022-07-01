using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.DataVisualization.Charting;
using System.Drawing;

namespace GraphicClassLibrary
{
    public class LightSource : IObjectOnStage
    {
        /* Словарь координатная_система-3D-точка 
           Хранит точку расположения источника света. */

        public Dictionary<string, Point3D> LocationPoint;

        /* Максимальная интенсивность источника света. */

        static public float IntensityMax = 1F;

        public Color Col;

        public string Name { get; set; }

        public Scene Stage { get; set; }

        public LightSource(Point3D _wcs,
                           Color _col,
                           string _name)
        {
            LocationPoint = new Dictionary<string, Point3D>
            {
                { "WCS", _wcs },
                { "VCS", null }
            };
            Col = _col;
            Name = _name;
        }

        /* Метод InitPoints инициализирует
           расположение источника света в пространстве сцены. */

        public void InitPoints()
        {
            double[,] _vcs = Stage.Spectator.Screen.VCS_Matrix;

            LocationPoint["VCS"] = new Point3D();

            UpdatePoints(' ',
                new Dictionary<string, double[,]>
                { { "VCS", _vcs } });
        }

        /* 
           Метод UpdatePoints обновляет координаты точек источника света
           в заданных системах координат. На вход принимает:
           1) букву, определяющей режим - какие точки следует обновить.
              Рудимент.
           2) словарь:
           -- ключ: строка-название СК;
           -- значение: матрица преобразования 4х4.
        */

        public void UpdatePoints(char mode,
            Dictionary<string, double[,]> UpdateMatrices)
        {
            List<string> CSs = new List<string>();

            Point3D _updateStartPoint = null;

            /* Сначала обновляются координаты точек источника света
               в мировой СК, затем - в видовой. */

            if (UpdateMatrices.Keys.Contains("WCS"))
                CSs.Add("WCS");
            if (UpdateMatrices.Keys.Contains("VCS"))
                CSs.Add("VCS");

            foreach (string CS in CSs)
            {
                /* Необходимо обновить точки:
                   -- вершин 3D-объекта;
                   -- центра 3D-объекта;
                   -- нормальных векторов полигонов 3D-объекта. */

                Dictionary<char, double> dictAxesOffsets;

                double[,] _matrix = new double[4, 4];

                switch (CS)
                { 
                    case "WCS":
                        {
                            dictAxesOffsets = new Dictionary<char, double>
                                {
                                    { 'X', -LocationPoint[CS].X },
                                    { 'Y', -LocationPoint[CS].Y },
                                    { 'Z', -LocationPoint[CS].Z }
                                };

                            double[,] TR1 = CalcMethods.getTransitMatrix(
                                new List<Dictionary<char, double>>
                                { dictAxesOffsets });

                            double[,] UpdateMatrix = UpdateMatrices[CS];

                            dictAxesOffsets = new Dictionary<char, double>
                                {
                                    { 'X', LocationPoint[CS].X },
                                    { 'Y', LocationPoint[CS].Y },
                                    { 'Z', LocationPoint[CS].Z }
                                };

                            double[,] TR2 = CalcMethods.getTransitMatrix(
                                    new List<Dictionary<char, double>>
                                    { dictAxesOffsets });

                            CalcMethods.multiplyingMatrices(ref _matrix,
                                    new List<double[,]> { TR1, UpdateMatrix, TR2 });

                            _updateStartPoint = LocationPoint[CS];

                            break;
                        }
                    case "VCS":
                        {
                            double[,] _vcs = Stage.Spectator.Screen.VCS_Matrix;

                            dictAxesOffsets = new Dictionary<char, double>
                            {
                                { 'X', Stage.LocationPoint["VCS"].X },
                                { 'Y', Stage.LocationPoint["VCS"].Y },
                                { 'Z', Stage.LocationPoint["VCS"].Z }
                            };

                            double[,] TR = CalcMethods.getTransitMatrix(
                                new List<Dictionary<char, double>>
                                { dictAxesOffsets }
                                );

                            CalcMethods.multiplyingMatrices(ref _matrix,
                                new List<double[,]> { _vcs, TR });

                            /* Для изменения координат 3D-объекта в 
                               видовой системе координат необзодимо
                               начинать с его координат в МСК. */

                            _updateStartPoint = LocationPoint["WCS"];

                            break;
                        }
                }

                CalcMethods.CalculateLocationPoint(LocationPoint[CS],
                                                   _updateStartPoint,
                                                   _matrix);
            }
        }
    }
}
