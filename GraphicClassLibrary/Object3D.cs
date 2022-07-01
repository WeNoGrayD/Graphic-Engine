using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.DataVisualization.Charting;
using System.Drawing;

namespace GraphicClassLibrary
{
    /* Класс 3D-объекта. */

    public class Object3D : IObjectOnStage
    {
        /* Словарь координатная_система-список_3D_точек. 
           Хранит вершины 3D-объекта. */

        public Dictionary<string, List<Point3D>> Vertexes;

        /* Словарь координатная_система-3D_точка. 
           Хранит центр 3D-объекта в различных координатных системах. */

        public Dictionary<string, Point3D> CenterPoint;

        /* Список рёбер, из которых состоит 3D-объект. */

        public List<Edge> Edges;

        /* Список полигонов, из которых состоит 3D-объект. */

        public List<Polygon> Polygons;

        /* Список вращений 3D-объекта. Каждый элемент списка является
           словарём буква_оси_вращения-угол_вращения_в_градусах. */

        public List<Dictionary<char, double>> Rotations;

        /* Радиус габаритной сферы 3D-объекта. */

        public float R { get; }

        /* Имя объекта. */

        public string Name { get; set; }

        /* Список цветов - палитра объекта. */

        public List<Color> ColorPalette;

        /* Сцена, которой принадлежит данный 3D-объект. */

        public Scene Stage { get; set; }

        /* Светотехническая характеристика объекта:
           коэффициент рассеяного света. */

        public float Ka;

        /* Конструктор класса.*/

        public Object3D(Point3D _wcs,
                        float _r,
                        string _name)
        {
            CenterPoint = new Dictionary<string, Point3D>
            {
                { Scene.CoordinateSystems["WCS"], _wcs },
                { Scene.CoordinateSystems["LCS"], new Point3D(0, 0, 0) },
                { Scene.CoordinateSystems["VCS"], null }
            };

            R = _r;

            Name = _name;

            Rotations = new List<Dictionary<char, double>>();
        }

        /* Метод SetColorPalette загружает палитру цветов. */

        public void SetColorPalette(List<Color> _colorPalette)
        {
            ColorPalette = _colorPalette;
        }

        /* Метод SetPolysAndEdges принимает на вход списки рёбер и полигонов
           и загружает их внутрь 3D-объекта. */

        public void SetPolysAndEdges(
            List<Edge> _edges,
            List<Polygon> _polygons)
        {
            Edges = _edges;
            foreach (Edge edge in Edges)
            {
                edge.Parent = this;
                edge.Col = this.ColorPalette[edge.IndexOfCol];
            }

            Polygons = _polygons;
            foreach (Polygon poly in Polygons)
            {
                poly.Parent = this;
                poly.Col = this.ColorPalette[poly.IndexOfCol];
            }
        }

        /* Метод InitPoints принимает на вход
           название координатной системы и значения точек вершин
           3D-объекта в данной координатной системе.
           Выполняет инициализацию точек вершин в заданной
           координатной системе. 
           Предполагается, что сцена уже подготовлена 
           к добавлению данного 3D-объекта. */

        public void InitPoints(List<Point3D> _vertexesLCS)
        {
            Vertexes = new Dictionary<string, List<Point3D>>
            {
                { "WCS", null },
                { "LCS", null },
                { "VCS", null }
            };

            Vertexes["LCS"] = _vertexesLCS;

            Dictionary<char, double> dictAxesOffsets;

            Dictionary<string, List<Point3D>> _InitPoints;

            double[,] _matrix;

            /* Сначала необходимо обновить точки вершин
               и нормальных векторов полигонов в мировой КС.
               Объект добавляется до его поворотов. */

            _InitPoints = new Dictionary<string, List<Point3D>>()
            {
                {
                    Scene.CoordinateSystems["WCS"],
                    new List<Point3D>()
                },
                {
                    Scene.CoordinateSystems["LCS"],
                    new List<Point3D>()
                }
            };

            Vertexes["WCS"] = new List<Point3D>();
            for (int i = 0; i < Vertexes["LCS"].Count; i++)
                Vertexes["WCS"].Add(new Point3D());
            foreach (Polygon poly in Polygons)
            {
                poly.CalcNormalVectorPoint();
                poly.NormalVectorPoint["WCS"] = new Point3D();
            }

            /* 
               Для приведения координат каждой точки к МКС
               нужны их координаты в локальной КС 
               3D-объекта.
            */

            FillInitPointsByCS("LCS");
            FillInitPointsByCS("WCS");

            dictAxesOffsets = new Dictionary<char, double>
            {
                { 'X', CenterPoint["WCS"].X },
                { 'Y', CenterPoint["WCS"].Y },
                { 'Z', CenterPoint["WCS"].Z },
            };

            _matrix = CalcMethods.getTransitMatrix(
                new List<Dictionary<char, double>>
                { dictAxesOffsets }
                );

            for (int i = 0; i < _InitPoints["WCS"].Count; i++)
                CalcMethods.CalculateLocationPoint(_InitPoints["WCS"][i],
                                                _InitPoints["LCS"][i],
                                                _matrix);

            /* Теперь необходимо инициализировать точки
               вершин, нормалей и центра в системе видовых координат. */

            Vertexes["VCS"] = new List<Point3D>();

            for (int i = 0; i < Vertexes["LCS"].Count; i++)
            {
                Vertexes["VCS"].Add(new Point3D());
            }

            foreach (Polygon poly in Polygons)
            {
                poly.NormalVectorPoint["VCS"] = new Point3D();
            }

            CenterPoint["VCS"] = new Point3D();

            _matrix = Stage.Spectator.Screen.VCS_Matrix;

            UpdatePoints('c',
                new Dictionary<string, double[,]>
                { { "VCS", _matrix } });

            /* 
               Метод FillInitPointsByCS заполняет словарь _UpdatePoints
               по ключу CS точками, которые необходимо обновить: 
               -- вершинами 3D-объекта;
               -- центральной точкой;
               -- точками расположения нормальных векторов граней.
            */

            void FillInitPointsByCS(string CS)
            {
                _InitPoints[CS].AddRange(Vertexes[CS]);
                _InitPoints[CS].Add(CenterPoint[CS]);
                foreach (Polygon poly in Polygons)
                    _InitPoints[CS].Add(poly.NormalVectorPoint[CS]);
            }
        }

        /* 
           Метод UpdatePoints обновляет координаты точек объекта
           в заданных системах координат. На вход принимает:
           1) букву, определяющей режим - какие точки следует обновить.
           -- 'a': все (и центральную, и вершины, и нормальные вектора);
           -- 'c": только центральную точку;
           -- 'v': вершины и нормальные вектора.
           2) словарь:
           -- ключ: строка-название СК;
           -- значение: матрица преобразования 4х4.
        */

        public void UpdatePoints(char mode,
            Dictionary<string, double[,]> UpdateMatrices)
        {
            List<string> CSs = new List<string>();

            Dictionary<string, List<Point3D>> _UpdatePoints =
                new Dictionary<string, List<Point3D>>();

            List<Point3D> _updateStartVertexes = new List<Point3D>();

            /* Сначала обновляются координаты точек объекта
               в его локальной СК, затем в мировой, затем - в видовой. */

            if (UpdateMatrices.Keys.Contains("LCS"))
                CSs.Add("LCS");
            if (UpdateMatrices.Keys.Contains("WCS"))
                CSs.Add("WCS");
            if (UpdateMatrices.Keys.Contains("VCS"))
                CSs.Add("VCS");

            foreach (string CS in CSs)
                _UpdatePoints.Add(CS, new List<Point3D>());

            foreach (string CS in CSs)
            {
                /* Необходимо обновить точки:
                   -- вершин 3D-объекта;
                   -- центра 3D-объекта;
                   -- нормальных векторов полигонов 3D-объекта. */

                Dictionary<char, double> dictAxesOffsets;

                double[,] _matrix = new double[4, 4];

                FillUpdatePointsByCS(CS);

                switch (CS)
                {
                    case "LCS":
                        {
                            _matrix = UpdateMatrices[CS];

                            _updateStartVertexes = _UpdatePoints[CS];

                            break;
                        }
                    case "WCS":
                        {
                            dictAxesOffsets = new Dictionary<char, double>
                                {
                                    { 'X', -CenterPoint[CS].X },
                                    { 'Y', -CenterPoint[CS].Y },
                                    { 'Z', -CenterPoint[CS].Z }
                                };

                            double[,] TR1 = CalcMethods.getTransitMatrix(
                                new List<Dictionary<char, double>>
                                { dictAxesOffsets });

                            double[,] UpdateMatrix = UpdateMatrices[CS];

                            dictAxesOffsets = new Dictionary<char, double>
                                {
                                    { 'X', CenterPoint[CS].X },
                                    { 'Y', CenterPoint[CS].Y },
                                    { 'Z', CenterPoint[CS].Z }
                                };

                            double[,] TR2 = CalcMethods.getTransitMatrix(
                                    new List<Dictionary<char, double>>
                                    { dictAxesOffsets });

                            CalcMethods.multiplyingMatrices(ref _matrix,
                                    new List<double[,]> { TR1, UpdateMatrix, TR2 });

                            _updateStartVertexes = _UpdatePoints[CS];

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

                            if (!CSs.Contains("WCS"))
                            {
                                _UpdatePoints.Add("WCS", new List<Point3D>());
                                FillUpdatePointsByCS("WCS");
                            }

                            /* Для изменения координат 3D-объекта в 
                               видовой системе координат необзодимо
                               начинать с его координат в МСК. */

                            _updateStartVertexes = _UpdatePoints["WCS"];

                            break;
                        }
                }

                for (int i = 0; i < _UpdatePoints[CS].Count; i++)
                {
                    Point3D _updatePoint = _UpdatePoints[CS][i],
                            _updateStartPoint = _updateStartVertexes[i];

                    CalcMethods.CalculateLocationPoint(_updatePoint,
                                                  _updateStartPoint,
                                                    _matrix);
                }
            }

            /* Метод FillUpdatePointsByCS заполняет словарь _UpdatePoints
               по ключу CS точками, которые необходимо обновить: 
               -- вершинами 3D-объекта;
               -- центральной точкой;
               -- точками расположения нормальных векторов граней. */

            void FillUpdatePointsByCS(string CS)
            {
                if (mode == 'a' || mode == 'v')
                {
                    _UpdatePoints[CS].AddRange(Vertexes[CS]);
                    foreach (Polygon poly in Polygons)
                        _UpdatePoints[CS].Add(poly.NormalVectorPoint[CS]);
                }
                if (mode == 'a' || mode == 'c')
                    _UpdatePoints[CS].Add(CenterPoint[CS]);
            }
        }

        /* Метод Clone позволяет клонировать данный 3D-объект. */

        public Object3D Clone()
        {
            Point3D _wcs = new Point3D
            {
                X = this.CenterPoint["WCS"].X,
                Y = this.CenterPoint["WCS"].Y,
                Z = this.CenterPoint["WCS"].Z
            };

            float _r = this.R;

            string _name = "obj";

            Object3D objClone = new Object3D(_wcs,
                                             _r,
                                             _name);

            List<Color> colorPalette = new List<Color>();
            foreach (Color col in this.ColorPalette)
                colorPalette.Add(col);
            objClone.SetColorPalette(colorPalette);

            List<Edge> edgeClones = new List<Edge>();
            foreach (Edge edge in this.Edges)
                edgeClones.Add(edge.Clone());
            objClone.Edges = edgeClones;
            List<Polygon> polyClones = new List<Polygon>();
            foreach (Polygon poly in this.Polygons)
                polyClones.Add(poly.Clone());
            objClone.Polygons = polyClones;
            objClone.SetPolysAndEdges(edgeClones, polyClones);

            return objClone;
        }
    }
}
