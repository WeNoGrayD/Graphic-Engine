using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.DataVisualization.Charting;
using System.Drawing;

namespace GraphicClassLibrary
{
    /* Класс полигона. */

    public class Polygon
    {
        /* Родительский 3D-объект. */

        public Object3D Parent;

        /* Словарь координатная_система-3D_точка. 
           Хранит точку расположения нормального вектора грани
           в различных координатных системах. */

        public Dictionary<string, Point3D> NormalVectorPoint { get; }

        /* Список ребёр полигона, которые привязаны к полигону. */

        public List<int> EdgeIndexes;

        /* Номер цвета полигона в палитре цветов родительского 3D-объекта. */

        public int IndexOfCol;

        /* Цвет полигона. */

        public Color Col;

        /* Светотехнические характеристики:
           Kd - коэффициент диффузного отражения грани;
           Ks - коэффициент зеркального отражения грани;
           n - какой-то там эмпирический параметр. */

        public float Kd = 0, Ks = 0, n = 0;

        /* Конструктор класса. */

        public Polygon(List<int> _edgeInds,
                       int _indexOfCol)
        {
            EdgeIndexes = _edgeInds;

            IndexOfCol = _indexOfCol;

            NormalVectorPoint = new Dictionary<string, Point3D>
            {
                { Scene.CoordinateSystems["WCS"], null },
                { Scene.CoordinateSystems["LCS"], null },
                { Scene.CoordinateSystems["VCS"], null }
            };
        }

        /* Метод CalcNormalVectorPoint позволяет рассчитать 
           координаты второго конца нормального вектора плоскости
           полигона. */

        public void CalcNormalVectorPoint()
        {
            List<Point3D> vertexes = new List<Point3D>();

            foreach (int eInd in EdgeIndexes)
            {
                int p1Ind = Parent.Edges[eInd].VertexIndexes[0],
                    p2Ind = Parent.Edges[eInd].VertexIndexes[1];

                Point3D _p1 = Parent.Vertexes["LCS"][p1Ind],
                        _p2 = Parent.Vertexes["LCS"][p2Ind];

                if (!vertexes.Contains(_p1))
                    vertexes.Add(_p1);
                if (!vertexes.Contains(_p2))
                    vertexes.Add(_p2);
                if (vertexes.Count == 3)
                    break;
            }

            Point3D p1 = vertexes[0],
                    p2 = vertexes[1],
                    p3 = vertexes[2];

            float A = 0,
                  B = 0,
                  C = 0,
                  D = 0;

            NormalVectorPoint["LCS"] = new Point3D();

            A = (p2.Y - p1.Y) * (p3.Z - p1.Z) - (p3.Y - p1.Y) * (p2.Z - p1.Z);
            B = -((p2.X - p1.X) * (p3.Z - p1.Z) - (p3.X - p1.X) * (p2.Z - p1.Z));
            C = (p2.Y - p1.Y) * (p3.X - p1.X) - (p3.Y - p1.Y) * (p2.X - p1.X);
            D = A * (-p1.X) + B * (-p1.Y) + C * (-p1.Z);

            float startX = 0, nvX = 0,
                  startY = 0, nvY = 0,
                  startZ = 0, nvZ = 0,
                  t = 0;

            t = -(A * startX + B * startY + C * startZ + D) / (A * A + B * B + C * C);

            nvX = A * t + startX;
            nvY = B * t + startY;
            nvZ = C * t + startZ;

            NormalVectorPoint["LCS"].X = nvX;
            NormalVectorPoint["LCS"].Y = nvY;
            NormalVectorPoint["LCS"].Z = nvZ;
        }

        /* Метод GetEdge получает на вход индекс имеющегося в списке рёбер 
           полигона ребра и выдаёт на выход объект класса Edge. */

        public Edge GetEdge(int eInd)
        {
            return Parent.Edges[eInd];
        }

        /* Метод Clone позволяет клонировать данный полигон. */

        public Polygon Clone()
        {
            List<int> _edgeInds = new List<int>();
            foreach (int edge in this.EdgeIndexes)
            {
                _edgeInds.Add(edge);
            }

            int _colInd = this.IndexOfCol;

            Point3D _nvlLCS = new Point3D
            {
                X = this.NormalVectorPoint["LCS"].X,
                Y = this.NormalVectorPoint["LCS"].Y,
                Z = this.NormalVectorPoint["LCS"].Z
            };

            Polygon polyClone = new Polygon(_edgeInds,
                                            _colInd);
            return polyClone;
        }
    }
}
