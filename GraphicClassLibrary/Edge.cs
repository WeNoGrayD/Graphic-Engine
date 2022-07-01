using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.DataVisualization.Charting;
using System.Drawing;

namespace GraphicClassLibrary
{
    /* Класс ребра. */

    public class Edge
    {
        /* 3D-объект-родитель ребра. */

        public Object3D Parent;

        /* Индексы вершин ребра. */

        public int[] VertexIndexes { get; }

        /* Номер цвета ребра в палитре цветов родительского 3D-объекта. */

        public int IndexOfCol { get; }

        /* Цвет ребра. */

        public Color Col;

        /* Конструктор класса. */

        public Edge(int _p1_Ind, int _p2_Ind, int _indOfCol)
        {
            VertexIndexes = new int[] { _p1_Ind, _p2_Ind };

            IndexOfCol = _indOfCol;
        }

        /* Метод GetEdge получает на вход индекс имеющегося в списке рёбер 
           полигона ребра и выдаёт на выход объект класса Edge. */

        public Point3D GetPoint(string CS, short pInd)
        {
            return Parent.Vertexes[CS][VertexIndexes[pInd]];
        }

        /* Метод Clone позволяет клонировать данное ребро. */

        public Edge Clone()
        {
            int[] _VIs = (int[])this.VertexIndexes.Clone();

            int _colInd = this.IndexOfCol;

            Edge edgeClone = new Edge(_VIs[0],
                                      _VIs[1],
                                      _colInd);
            return edgeClone;
        }
    }
}
