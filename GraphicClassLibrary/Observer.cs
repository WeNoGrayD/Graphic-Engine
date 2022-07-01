using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.DataVisualization.Charting;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GraphicClassLibrary
{
    /* Класс наблюдателя. */

    public class Observer : IObjectOnStage
    {
        /* Класс 2D-ребра. Необходим для закрашивания граней. */

        struct Edge2D
        {
            public int Ind;
            public Point[] ps;
            public Color Col;
        };

        /* Сцена, которой принадлежит наблюдатель. */

        public Scene Stage { get; set; }

        /* Словарь координатная_система-3D_точка, в котором хранится
           точка расположения наблюдателя в различных координатных системах. */

        public Dictionary<string, Point3D> LocationPoint;

        /* Расстояние от наблюдателя до экрана. */

        public float d;

        /* Список вращений наблюдателя относительно мира (сцены).
           Вращение - это словарь буква_оси_вращения-угол_вращения. */

        public List<Dictionary<char, double>> Rotations;

        /* Экран, через который видит наблюдатель. */

        public ViewingWindow Screen;

        /* Имя наблюдателя. */

        public string Name { get; set; }

        /* Словарь имя_3D_объекта-3D_объект_на_сцене_видимый_наблюдателем.*/

        public List<Object3D> VisibleObjects;

        /* Конструктор класса. */

        public Observer(Point3D _startLocationPoint,
                        Dictionary<char, double> startRotation,
                        int _d,
                        ViewingWindow screen)
        {
            d = _d;

            LocationPoint = new Dictionary<string, Point3D>
            {
                { Scene.CoordinateSystems["WCS"], _startLocationPoint },
                { Scene.CoordinateSystems["LCS"], new Point3D(0, 0, 0) },
                { Scene.CoordinateSystems["VCS"], new Point3D(0, 0, -d) }
            };

            Rotations = new List<Dictionary<char, double>>();
            Rotations.Add(startRotation);

            Screen = screen;
            Screen.InitSpectator(this);

            Name = "Spectator";
        }

        /* 
           Метод UpdatePoints обновляет координаты наблюдателя
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

            double[,] _matrix;

            Point3D startLocationPoint = null;

            /* Сначала обновляются координату точки расположения наблюдателя
               в мировой системе координат, затем - в видовой. */

            if (UpdateMatrices.Keys.Contains("WCS"))
                CSs.Add("WCS");
            if (UpdateMatrices.Keys.Contains("VCS"))
                CSs.Add("VCS");

            foreach (string CS in CSs)
            {
                _matrix = new double[4, 4];

                switch (CS)
                {
                    case "WCS":
                        {
                            /* Переносим наблюдателя в начало СК сцены,
                               производим необходимые изменения,
                               возвращаем обратно. */

                            double[,] TR1 = CalcMethods.getTransitMatrix(
                                new List<Dictionary<char, double>>
                                {
                                    new Dictionary<char, double>
                                    {
                                        { 'X', -LocationPoint["WCS"].X },
                                        { 'Y', -LocationPoint["WCS"].Y },
                                        { 'Z', -LocationPoint["WCS"].Z }
                                    }
                                }),
                                      UpdateMatrix = UpdateMatrices[CS],
                                      TR2 = CalcMethods.getTransitMatrix(
                                new List<Dictionary<char, double>>
                                {
                                    new Dictionary<char, double>
                                    {
                                        { 'X', LocationPoint["WCS"].X },
                                        { 'Y', LocationPoint["WCS"].Y },
                                        { 'Z', LocationPoint["WCS"].Z }
                                    }
                                });

                            CalcMethods.multiplyingMatrices(
                                ref _matrix,
                                new List<double[,]>
                                { TR1, UpdateMatrix, TR2 });

                            startLocationPoint = LocationPoint[CS];

                            break;
                        }
                    case "VCS":
                        {
                            _matrix = UpdateMatrices[CS];

                            /* Переносим наблюдателя в начало видовой
                               системы координат. */

                            startLocationPoint = LocationPoint[CS];

                            break;
                        }
                }

                CalcMethods.CalculateLocationPoint(LocationPoint[CS],
                                                startLocationPoint,
                                                _matrix);
            }

            /* После обновления местоположения наблюдателя 
               необходимо проделать аналогичные действия
               в отношении связанной с ним камеры. */

            Screen.InitLocationPoint();
        }

        /* Метод VW_Update запускает обновление экрана VW. */

        public void VW_Update(Graphics grStage)
        {
            VisibleObjects = new List<Object3D>();

            grStage.Clear(Color.White);

            float bWidth = Screen.VWbig.Width,
                  bHeight = Screen.VWbig.Height,
                  scrWidth = Screen.VWsmall.Width,
                  scrHeight = Screen.VWsmall.Height;

            Bitmap bitmap = new Bitmap((int)Math.Round(bWidth),
                           (int)Math.Round(bHeight));

            double[,] MS = CalcMethods.getScalingMatrix(
                new List<Dictionary<char, double>>
                {
                             new Dictionary<char, double>
                             {
                                 { 'X', Screen.ScaleX },
                                 { 'Y', Screen.ScaleY }
                             }
                });

            foreach (Object3D o in Stage.Objects.Values.Where(so => so is Object3D))
            {
                Object3D obj = ((Object3D)o);

                Point3D CenterPoint = obj.CenterPoint["VCS"];
                float R = obj.R,
                      perspWidth = (CenterPoint.Z + d) * scrWidth / d,
                      perspHeight = (CenterPoint.Z + d) * scrHeight / d;

                if ((CenterPoint.Z > -R &&
                    (
                      Math.Abs(CenterPoint.X) < R + perspWidth / 2 ||
                      -Math.Abs(CenterPoint.X) > -R - perspWidth / 2
                    ) &&
                    (
                      Math.Abs(CenterPoint.Y) < R + perspHeight / 2 ||
                      -Math.Abs(CenterPoint.Y) > -R - perspHeight / 2
                    )))
                {
                    VisibleObjects.Add(obj);

                    obj.UpdatePoints('v', new Dictionary<string, double[,]>
                    { {"VCS", Screen.VCS_Matrix }});

                }
            }

            for (int i = 0; i < VisibleObjects.Count; i++)
            {
                for (int j = 0; j < VisibleObjects.Count - i - 1; i++)
                {
                    Object3D obj1 = VisibleObjects[j],
                             obj2 = VisibleObjects[j + 1];
                    float obj1Depth = obj1.CenterPoint["VCS"].Z;
                    float obj2Depth = obj2.CenterPoint["VCS"].Z;

                    if (obj1Depth < obj2Depth)
                    {
                        Object objBuf = obj1;
                        VisibleObjects[j] = obj2;
                        VisibleObjects[j + 1] = obj1;
                    }
                }
            }

            foreach (Object3D obj in VisibleObjects)
            {
                List<int> ViewedEdges = new List<int>();

                foreach (Polygon poly in obj.Polygons)
                {
                    Point3D polyNVP = new Point3D
                    {
                        X = poly.NormalVectorPoint["VCS"].X -
                        obj.CenterPoint["VCS"].X,
                        Y = poly.NormalVectorPoint["VCS"].Y -
                        obj.CenterPoint["VCS"].Y,
                        Z = poly.NormalVectorPoint["VCS"].Z -
                        obj.CenterPoint["VCS"].Z,
                    };

                    float S = 0;

                    bool polyIsVisible = false;

                    List<Point3D> vertexes = new List<Point3D>();

                    foreach (int eInd in poly.EdgeIndexes)
                    {
                        int p1Ind = obj.Edges[eInd].VertexIndexes[0],
                            p2Ind = obj.Edges[eInd].VertexIndexes[1];

                        Point3D p1 = obj.Vertexes["VCS"][p1Ind],
                                p2 = obj.Vertexes["VCS"][p2Ind];

                        if (!vertexes.Contains(p1))
                            vertexes.Add(p1);
                        if (!vertexes.Contains(p2))
                            vertexes.Add(p2);
                    }

                    foreach (Point3D polyPoint in vertexes)
                    {
                        S = GetScalarMult(polyNVP, polyPoint);

                        polyIsVisible = polyIsVisible || S < 0;
                        if (polyIsVisible)
                            break;
                    }

                    if (!polyIsVisible)
                        continue;

                    double[,] PR;

                    double[] locationArray1,
                             locationArray2;

                    /* 
                       Словарь. Его содержимое:
                       - ключ: индекс в списке вершин 3D-объекта
                       точки в трёхмерном пространстве,
                       которая ненаблюдаема, но связана ребром
                       с видимой (после всех преобразований) на экране
                       точкой;
                       - значение: словарь. ЕГО содержимое:
                         - ключ: индекс видимой точки, связанной
                         с невидимой.
                         - значение: проекция невидимой точки
                         на плоскость экрана для ребра
                         с индеком "ключ".
                    */

                    Dictionary<int, Dictionary<int, Point>>
                        dictUnseenVertexes =
                        new Dictionary<int, Dictionary<int, Point>>();

                    List<Edge2D> polyEdges2D = new List<Edge2D>();

                    foreach (int edgeInd in poly.EdgeIndexes)
                    {

                        if (ViewedEdges.Contains(edgeInd))
                            ;// continue;
                        else
                            ViewedEdges.Add(edgeInd);

                        Edge edge = obj.Edges[edgeInd];

                        int p1Ind = edge.VertexIndexes[0],
                            p2Ind = edge.VertexIndexes[1],
                            pV_Ind = -1,
                            pNV_Ind = -1,
                            /* Для того, чтобы знать, какую 2D-точку
                               проекции загружать в словарь
                               невидимых точук. */
                            unseenVertexIndByEdge = -1;

                        Point3D p1_3D = new Point3D
                                {
                                    X = obj.Vertexes["VCS"][p1Ind].X,
                                    Y = obj.Vertexes["VCS"][p1Ind].Y,
                                    Z = obj.Vertexes["VCS"][p1Ind].Z
                                },
                                p2_3D = new Point3D
                                {
                                    X = obj.Vertexes["VCS"][p2Ind].X,
                                    Y = obj.Vertexes["VCS"][p2Ind].Y,
                                    Z = obj.Vertexes["VCS"][p2Ind].Z
                                };

                        Dictionary<string, bool>
                                   p1_3D_Conditions = null,
                                   p2_3D_Conditions = null;

                        Point p1_2D, p2_2D;

                        Dictionary<string, bool>
                                   p1_2D_Conditions = null,
                                   p2_2D_Conditions = null;

                        /* Если объект полностью наблюдаем, то
                           для неё ни одна грань объекта не пересекает
                           плоскость экрана и для неё не нужно
                           высчитывать координаты точек её
                           пересечения с экраном. Аналогично
                           в том случае, если данное ребро
                           в не полностью наблюдаемом объекте
                           не пересекает экран. */

                        /* Если обе точки ребра попадают в поле зрения
                           наблюдателя, то дополнительных действий 
                           производить не требуется.
                           Если видна лишь одна точка, то она заменяется
                           точкой пересечения ребра с плоскостью экрана.
                           Если не видны обе, то переходим к следующему
                           ребру. */

                        FillPoint3D_Conditions(p1_3D,
                            ref p1_3D_Conditions);
                        FillPoint3D_Conditions(p2_3D,
                            ref p2_3D_Conditions);

                        if (!p1_3D_Conditions["PerspVisibility"] ||
                            !p2_3D_Conditions["PerspVisibility"])
                        {
                            /* Если только одна из точек не входит
                              в пирамиду видимости по оси Z
                              (направлению камеры), то вычисляем
                              точку пересечения ребра с экраном,
                              в противном случае пропускаем это ребро. */

                            if (p1_3D_Conditions["Z>=0"] ^
                                p2_3D_Conditions["Z>=0"])
                            {
                                Point3D pVisible = null,
                                        pNonVisible = null;

                                Dictionary<string, bool>
                                    pVisible_3D_Conditions = null,
                                    pNonVisible_3D_Conditions = null;

                                if (p1_3D_Conditions["Z>=0"])
                                {
                                    pV_Ind = p1Ind;
                                    pNV_Ind = p2Ind;
                                    unseenVertexIndByEdge = 2;

                                    pVisible = p1_3D;
                                    pNonVisible = p2_3D;

                                    pVisible_3D_Conditions =
                                        p1_3D_Conditions;
                                    pNonVisible_3D_Conditions =
                                        p2_3D_Conditions;
                                }
                                else
                                {
                                    pV_Ind = p2Ind;
                                    pNV_Ind = p1Ind;
                                    unseenVertexIndByEdge = 1;

                                    pVisible = p2_3D;
                                    pNonVisible = p1_3D;

                                    pVisible_3D_Conditions =
                                        p2_3D_Conditions;
                                    pNonVisible_3D_Conditions =
                                        p1_3D_Conditions;
                                }

                                float X = (pVisible.X - pNonVisible.X) *
                                          (-pNonVisible.Z) /
                                          (pVisible.Z + -pNonVisible.Z) +
                                          pNonVisible.X,
                                          Y = (pVisible.Y - pNonVisible.Y) *
                                          (-pNonVisible.Z) /
                                          (pVisible.Z + -pNonVisible.Z) +
                                          pNonVisible.Y;

                                pNonVisible.X = X;
                                pNonVisible.Y = Y;
                                pNonVisible.Z = 0;

                                FillPoint3D_Conditions(pNonVisible,
                                    ref pNonVisible_3D_Conditions);

                                /*
                                  Если точка сзади экрана после проекции
                                  на его плоскость находится в его пределах,
                                  а точка спереди экрана находится в пирамиде
                                  видимости, то это стандартный случай.
                                  Если одна или обе точки остаются вне
                                  пирамиды видимости, то необходимо проверить,
                                  будет ли видно ребро на экране.
                                 */

                                if (!pVisible_3D_Conditions["PerspVisibility"])
                                {
                                    if (!pVisible_3D_Conditions["X>=-PW"] &&
                                        !pNonVisible_3D_Conditions["X>=-PW"])
                                        continue;
                                    else if (!pVisible_3D_Conditions["X<=PW"] &&
                                             !pNonVisible_3D_Conditions["X<=PW"])
                                        continue;

                                    if (!pVisible_3D_Conditions["Y>=-PH"] &&
                                        !pNonVisible_3D_Conditions["Y>=-PH"])
                                        continue;
                                    else if (!pVisible_3D_Conditions["Y<=PH"] &&
                                             !pNonVisible_3D_Conditions["Y<=PH"])
                                        continue;
                                }

                                /* В словарь невидимых точек необходимо
                                   занести новую (если необходимо),
                                   а после создать запись
                                   во вложенном словаре о связи с новой
                                   видимой точкой (если её опять же нет). */

                                if (!dictUnseenVertexes.Keys.Contains(pNV_Ind))
                                    dictUnseenVertexes.Add(pNV_Ind,
                                        new Dictionary<int, Point>());
                            }
                            else
                            {
                                bool F = false;

                                if (!p1_3D_Conditions["Z>=0"] &&
                                    !p2_3D_Conditions["Z>=0"])
                                    F = true;

                                if (!p1_3D_Conditions["X>=-PW"] &&
                                    !p2_3D_Conditions["X>=-PW"])
                                    F = true;
                                else if (!p1_3D_Conditions["X<=PW"] &&
                                         !p2_3D_Conditions["X<=PW"])
                                    F = true;
                                if (!p1_3D_Conditions["Y>=-PH"] &&
                                    !p2_3D_Conditions["Y>=-PH"])
                                    F = true;
                                else if (!p1_3D_Conditions["Y<=PH"] &&
                                         !p2_3D_Conditions["Y<=PH"])
                                    F = true;

                                if (F)
                                {
                                    continue;
                                }
                            }
                        }

                        locationArray1 = new double[]
                        {
                                p1_3D.X,
                                p1_3D.Y,
                                p1_3D.Z,
                                1
                        };

                        locationArray2 = new double[]
                        {
                                p2_3D.X,
                                p2_3D.Y,
                                p2_3D.Z,
                                1
                        };

                        PR = getProjectiveMatrix(p1_3D.Z);

                        getScreenXY(locationArray1, PR);

                        PR = getProjectiveMatrix(p2_3D.Z);

                        getScreenXY(locationArray2, PR);
                        /*
                        p1_2D = new Point
                        {
                            X = (int)Math.Truncate(locationArray1[0]),
                            Y = (int)Math.Truncate(locationArray1[1])
                        };

                        p2_2D = new Point
                        {
                            X = (int)Math.Truncate(locationArray2[0]),
                            Y = (int)Math.Truncate(locationArray2[1])
                        };
                        */
                        locationArray1[0] = /*Math.Truncate(*/locationArray1[0] + scrWidth / 2;//);
                        locationArray1[1] = /*Math.Truncate(*/-locationArray1[1] + scrHeight / 2;//);

                        locationArray2[0] = /*Math.Truncate(*/locationArray2[0] + scrWidth / 2;//);
                        locationArray2[1] = /*Math.Truncate(*/-locationArray2[1] + scrHeight / 2;//);

                        getScreenXY(locationArray1, MS);
                        getScreenXY(locationArray2, MS);

                        /* Если обе точки ребра видны, то дополнительных
                           действий совершать не требуется.
                           Если только одна из точек ребра находится
                           за пределами видимости, то находим точку
                           пересечения грани с экраном.
                           Если не видны обе точки ребра, то пропускаем её.*/

                        p1_2D = new Point
                        {
                            X = (int)Math.Truncate(locationArray1[0]),
                            Y = (int)Math.Truncate(locationArray1[1])
                        };

                        p2_2D = new Point
                        {
                            X = (int)Math.Truncate(locationArray2[0]),
                            Y = (int)Math.Truncate(locationArray2[1])
                        };

                        FillPoint2D_Conditions(p1_2D, ref p1_2D_Conditions);
                        FillPoint2D_Conditions(p2_2D, ref p2_2D_Conditions);

                        if (!p1_2D_Conditions["ScrVisibility"] ||
                            !p2_2D_Conditions["ScrVisibility"])
                        {
                            bool F = false;

                            if (!p1_2D_Conditions["X>=0"] &&
                                !p2_2D_Conditions["X>=0"])
                                F = true;
                            else if (!p1_2D_Conditions["X<SW"] &&
                                     !p2_2D_Conditions["X<SW"])
                                F = true;
                            if (!p1_2D_Conditions["Y>=0"] &&
                                !p2_2D_Conditions["Y>=0"])
                                F = true;
                            else if (!p1_2D_Conditions["Y<SH"] &&
                                     !p2_2D_Conditions["Y<SH"])
                                F = true;

                            if (F)
                            {
                                if (pNV_Ind >= 0 &&
                                    dictUnseenVertexes[pNV_Ind].Count == 1)
                                    dictUnseenVertexes.Remove(pNV_Ind);
                                continue;
                            }

                            /* pVisible - это условно видимая точка,
                               т.е. точка, вокруг которой будем
                               строить все измерения. */

                            Point pVisible_2D = new Point(),
                                  pNonVisible_2D = new Point();

                            Dictionary<string, bool>
                                pVisible_2D_Conditions = null,
                                pNonVisible_2D_Conditions = null;

                            double[] locationArrayVisible = null,
                                     locationArrayNonVisible = null;

                            /* 
                               dist - расстояние от координат
                               видимой точки до координаты невидимой.
                               Положительное. 
                               toBorder - расстояние от видимой точки
                               до соответствующей стороны экрана.
                               Положительное.
                               smallDist - расстояние от координаты
                               видимой точки до масштабированной координаты
                               невидимой точки. Положительное.
                            */

                            float distX = 0, distY = 0,
                                  toBorderX = 0, toBorderY = 0,
                                  smallDistX = 0, smallDistY = 0;

                            if (!p1_2D_Conditions["ScrVisibility"] &&
                                !p2_2D_Conditions["ScrVisibility"])
                                continue;

                            if (!p1_2D_Conditions["ScrVisibility"])
                            {
                                pVisible_2D = p2_2D;
                                pNonVisible_2D = p1_2D;

                                pVisible_2D_Conditions =
                                    p2_2D_Conditions;
                                pNonVisible_2D_Conditions =
                                    p1_2D_Conditions;

                                locationArrayVisible =
                                      locationArray2;
                                locationArrayNonVisible =
                                      locationArray1;

                                RelocatePointToBorder();
                            }

                            if (!p2_2D_Conditions["ScrVisibility"])
                            {
                                pVisible_2D = p1_2D;
                                pNonVisible_2D = p2_2D;

                                pVisible_2D_Conditions =
                                    p1_2D_Conditions;
                                pNonVisible_2D_Conditions =
                                    p2_2D_Conditions;

                                locationArrayVisible =
                                      locationArray1;
                                locationArrayNonVisible =
                                      locationArray2;

                                RelocatePointToBorder();
                            }

                            void RelocatePointToBorder()
                            {
                                /* Начальные координаты
                                   невидимой точки. */

                                float oldX = (float)locationArrayNonVisible[0],
                                      oldY = (float)locationArrayNonVisible[1],
                                      newX = 0, newY = 0;

                                if (oldX <= pVisible_2D.X)
                                {
                                    distX = -(-oldX + pVisible_2D.X);
                                    toBorderX = -pVisible_2D.X;
                                }
                                else
                                {
                                    distX = oldX + -pVisible_2D.X;
                                    toBorderX = bWidth - 1 - pVisible_2D.X;
                                }

                                if (oldY <= pVisible_2D.Y)
                                {
                                    distY = -(-oldY + pVisible_2D.Y);
                                    toBorderY = -pVisible_2D.Y;
                                }
                                else
                                {
                                    distY = oldY - pVisible_2D.Y;
                                    toBorderY = bHeight - 1 - pVisible_2D.Y;
                                }

                                smallDistX =
                                    distX * Math.Abs(toBorderY) / Math.Abs(distY);

                                smallDistY =
                                    distY * Math.Abs(toBorderX) / Math.Abs(distX);

                                bool pNV_VisibleX =
                                     !(!pNonVisible_2D_Conditions["X>=0"] ||
                                       !pNonVisible_2D_Conditions["X<SW"]),
                                     pNV_VisibleY =
                                     !(!pNonVisible_2D_Conditions["Y>=0"] ||
                                       !pNonVisible_2D_Conditions["Y<SH"]);

                                if (distY == 0 ||
                                   (!pNV_VisibleX && pNV_VisibleY) ||
                                   (!pNV_VisibleX && !pNV_VisibleY &&
                                    Math.Abs(toBorderX) <
                                    Math.Abs(smallDistX)))
                                    newX/*pNonVisible_2D.X*/ =
                                        /*(int)Math.Truncate(*/pVisible_2D.X + toBorderX;//);
                                else
                                {
                                    newX/*pNonVisible_2D.X*/ = pVisible_2D.X +
                                        /*(int)Math.Truncate(*/smallDistX;//);
                                }
                                if (distX == 0 ||
                                    (!pNV_VisibleY && pNV_VisibleX) ||
                                    (!pNV_VisibleY && !pNV_VisibleX &&
                                    Math.Abs(toBorderY) <
                                    Math.Abs(smallDistY)))
                                    newY/*pNonVisible_2D.Y*/ =
                                        /*(int)Math.Truncate(*/pVisible_2D.Y + toBorderY;//);
                                else
                                {
                                    newY/*pNonVisible_2D.Y*/ = pVisible_2D.Y +
                                        /*(int)Math.Truncate(*/smallDistY;//);
                                }

                                locationArrayNonVisible[0] = newX;// pNonVisible_2D.X;
                                locationArrayNonVisible[1] = newY;// pNonVisible_2D.Y;
                            }
                        }

                        p1_2D = new Point
                        {
                            X = (int)Math.Truncate(locationArray1[0]),
                            Y = (int)Math.Truncate(locationArray1[1])
                        };

                        p2_2D = new Point
                        {
                            X = (int)Math.Truncate(locationArray2[0]),
                            Y = (int)Math.Truncate(locationArray2[1])
                        };

                        polyEdges2D.Add(new Edge2D
                        { Ind = edgeInd, ps = new Point[] { p1_2D, p2_2D },
                          Col = Color.Black });

                        /* Если одна из точек находилась в пирамиде видимости,
                           но сзади экрана, то необходимо добавить её проекцию
                           в словарь невидимых точек. */

                        if (pNV_Ind >= 0)
                        {
                            Point pProection = new Point();
                            if (unseenVertexIndByEdge == 1)
                                pProection = new Point
                                {
                                    X = p1_2D.X,
                                    Y = p1_2D.Y
                                };
                            else
                                pProection = new Point
                                {
                                    X = p2_2D.X,
                                    Y = p2_2D.Y
                                };

                            if (!dictUnseenVertexes[pNV_Ind].Keys.Contains(pV_Ind))
                                dictUnseenVertexes[pNV_Ind].Add(pV_Ind, pProection);
                        }

                        GraphicMethods.DrawLine_Bresenham(bitmap,
                                                     p1_2D,
                                                     p2_2D,
                                                     edge.Col);
                    }

                    /* Составляем массив рёбер. */

                    List<Point[]> edges = new List<Point[]>();

                    /* Пересечение полигоном плоскости экрана возможно
                       в двух случаях: 
                       (1) когда две невидимые точки связаны ребром
                       между собой и рёбрами между одной и той же
                       видимой точкой;
                       (2) когда две видимые точки связаны ребром
                       между собой и рёбрами между одной и той же
                       невидимой точкой. */

                    foreach (int pNV1 in dictUnseenVertexes.Keys)
                    {
                        /* Просмотр пар видимых точек. */

                        foreach (int pV1 in dictUnseenVertexes[pNV1].Keys)
                        {
                            foreach (int pV2 in dictUnseenVertexes[pNV1].Keys)
                            {
                                if (pV1 != pV2)
                                {
                                    edges.Add(new Point[]
                                        {
                                        dictUnseenVertexes[pNV1][pV1],
                                        dictUnseenVertexes[pNV1][pV2]
                                        });
                                }
                            }
                        }

                        foreach (int pNV2 in dictUnseenVertexes.Keys)
                        {
                            if (pNV1 != pNV2)
                            {
                                foreach (int pV_same in dictUnseenVertexes[pNV1].Keys)
                                {
                                    if (dictUnseenVertexes[pNV2].Keys.Contains(pV_same))
                                    {
                                        edges.Add(new Point[]
                                            {
                                                dictUnseenVertexes[pNV1][pV_same],
                                                dictUnseenVertexes[pNV2][pV_same]
                                            });
                                    }
                                }

                                /* Поиск вершин по рёбрам, которые помогут
                                   построить отрезок пересечения полигона
                                   с экранной плоскостью. */

                                List<List<Edge>> viewedEdges =
                                    new List<List<Edge>>
                                    { new List<Edge>(), new List<Edge>() };

                                bool[] edgesAreFound =
                                    EdgeSearching(
                                        new bool[] { false, false },
                                        0,
                                        pNV1,
                                        viewedEdges,
                                        pNV2);

                                if (edgesAreFound.Contains(true))
                                {
                                    foreach (List<Edge> vE in viewedEdges
                                        .Where(viEd => viEd.Count > 0))
                                    {
                                        int pV1 = -1, pV2 = -1;

                                        foreach (int _pV1 in
                                            dictUnseenVertexes[pNV1].Keys)
                                        {
                                            if (vE[0].VertexIndexes.Contains(_pV1))
                                                pV1 = _pV1;
                                        }

                                        if (pV1 == -1)
                                            continue;

                                        foreach (int _pV2 in
                                            dictUnseenVertexes[pNV2].Keys)
                                        {
                                            if (vE.Last().VertexIndexes.Contains(_pV2))
                                                pV2 = _pV2;
                                        }

                                        if (pV2 == -1)
                                            continue;

                                        GraphicMethods.DrawLine_Bresenham(bitmap,
                                                     dictUnseenVertexes[pNV1][pV1],
                                                     dictUnseenVertexes[pNV2][pV2],
                                                     Color.Red);

                                        break;
                                    }
                                }

                                bool[] EdgeSearching(
                                    bool[] innerEdgesAreFound,
                                    int innerStepSide,
                                    int pStart,
                                    List<List<Edge>> innerViewedEdges,
                                    int pSearch)
                                {
                                    if (!innerEdgesAreFound[innerStepSide])
                                    {
                                        foreach (int edgeId in poly.EdgeIndexes)
                                        {
                                            Edge edge = obj.Edges[edgeId];
                                            if (edge.VertexIndexes.Contains(pStart) &&
                                                edge.VertexIndexes.Contains(pSearch))
                                            {
                                                int iSS = innerStepSide;

                                                innerEdgesAreFound[iSS] = true;

                                                innerViewedEdges[iSS].Add(edge);

                                                if (iSS == 1)
                                                    return innerEdgesAreFound;
                                            }
                                        }
                                    }

                                    bool oneSideIsFound = innerEdgesAreFound[0];
                                    int pInter = -1;

                                    foreach (int edgeId in poly.EdgeIndexes)
                                    {
                                        Edge edge = obj.Edges[edgeId];
                                        if (edge.VertexIndexes.Contains(pStart) &&
                                            !viewedEdges[1].Contains(edge))
                                        {
                                            if (edge.VertexIndexes[0] == pStart)
                                                pInter = edge.VertexIndexes[1];
                                            else
                                                pInter = edge.VertexIndexes[0];

                                            List<List<Edge>> innerViewedEdgesBuf =
                                                new List<List<Edge>>();
                                            for (int i = 0; i < 2; i++)
                                            {
                                                List<Edge> iVE = innerViewedEdges[i];
                                                Edge[] iVE_buf = new Edge[iVE.Count];
                                                iVE.CopyTo(iVE_buf);
                                                innerViewedEdgesBuf
                                                    .Add(new List<Edge>(iVE_buf));
                                            }

                                            innerViewedEdgesBuf[1]
                                                .Add(edge);

                                            bool[] innerEdgesAreFoundBuf =
                                                EdgeSearching(
                                                innerEdgesAreFound,
                                                1,
                                                pInter,
                                                innerViewedEdgesBuf,
                                                pSearch);

                                            if (innerEdgesAreFoundBuf.Contains(true))
                                            {
                                                for (int i = 0; i < 2; i++)
                                                    if (innerEdgesAreFoundBuf[i])
                                                    {
                                                        innerViewedEdges[i] =
                                                            innerViewedEdgesBuf[i];
                                                        innerEdgesAreFound[1] = true;
                                                    }

                                                if (innerEdgesAreFoundBuf[1])
                                                    return innerEdgesAreFound;
                                            }
                                        }
                                        /*
                                        if (oneSideIsFound)
                                            break;
                                            */
                                    }

                                    return innerEdgesAreFound;
                                }
                            }
                        }
                    }

                    /* Отрисовка полученных рёбер пересечения полигонов 
                       с плоскостью экрана. */

                    foreach (Point[] ps in edges)
                    {
                        GraphicMethods.DrawLine_Bresenham(bitmap,
                                                          ps[0],
                                                          ps[1],
                                                          Color.Red);
                    }

                    if (polyEdges2D.Count > 0)
                        PaintPolygon(poly, polyEdges2D);
                }
            }

            grStage.DrawImage(bitmap, new Point(0, 0));

            void getScreenXY(double[] locationArray, double[,] TFMatrix)
            {
                double[] startLocationArray = (double[])locationArray.Clone();

                // Проход по столбцам locationArray.

                for (int i = 0; i < 4; i++)
                {
                    locationArray[i] = 0;

                    // Ещё один проход.

                    for (int j = 0; j < 4; j++)
                        locationArray[i] += startLocationArray[j] * TFMatrix[j, i];
                }
            }

            /*
               Метод FillPoint3D_Conditions заполняет
               словарь условий нахождения точки в пределах
               пирамиды видимости.
             */

            void FillPoint3D_Conditions(Point3D P_3D,
                ref Dictionary<string, bool> dictP_3D_Conditions)
            {
                string X_3D_cond1 = "X>=-PW",
                       X_3D_cond2 = "X<=PW",
                       Y_3D_cond1 = "Y>=-PH",
                       Y_3D_cond2 = "Y<=PH",
                       Z_3D_cond = "Z>=0",
                       PV_cond = "PerspVisibility",
                       SV_cond = "ScrVisibility";

                float P_3D_PerspWidth = (P_3D.Z + d) * scrWidth / d,
                      P_3D_PerspHeight = (P_3D.Z + d) * scrHeight / d;

                dictP_3D_Conditions = new Dictionary<string, bool>
                    {
                        { X_3D_cond1, P_3D.X >= -P_3D_PerspWidth / 2 },
                        { X_3D_cond2, P_3D.X <= P_3D_PerspWidth / 2 },
                        { Y_3D_cond1, P_3D.Y >= -P_3D_PerspHeight / 2 },
                        { Y_3D_cond2, P_3D.Y <= P_3D_PerspHeight / 2 },
                        { Z_3D_cond, P_3D.Z >= 0 },
                        { PV_cond, false },
                        { SV_cond, false }
                    };

                dictP_3D_Conditions[PV_cond] =
                    dictP_3D_Conditions[X_3D_cond1] &&
                    dictP_3D_Conditions[X_3D_cond2] &&
                    dictP_3D_Conditions[Y_3D_cond1] &&
                    dictP_3D_Conditions[Y_3D_cond2] &&
                    dictP_3D_Conditions[Z_3D_cond];

                dictP_3D_Conditions[SV_cond] =
                    dictP_3D_Conditions[X_3D_cond1] &&
                    dictP_3D_Conditions[X_3D_cond2] &&
                    dictP_3D_Conditions[Y_3D_cond1] &&
                    dictP_3D_Conditions[Y_3D_cond2];
            }

            /*
               Метод DefineVertexesVisibility определяет
               видимость ребра, связывающего две точки.
               Метод возвращает false, если положение
               точек указывает на невидимость ребра,
               и true, если положение точек допускает
               видимость ребра.
             */

            bool DefineVertexesVisibility(ref Point3D p1_3D, ref Point3D p2_3D,
                Dictionary<string, bool> p1_3D_Conditions,
                Dictionary<string, bool> p2_3D_Conditions,
                ref Point3D pNonVisible1,
                ref Point3D pNonVisible2,
                ref Point3D pVisible)
            {
                /* 
                   Если не видна первая точка:
                */

                if (!p1_3D_Conditions["ScreenVisibility"])
                {
                    pNonVisible1 = p1_3D;

                    if (!p1_3D_Conditions["X>=-PW"])
                    {
                        if (!p2_3D_Conditions["X>=-PW"])
                            return false;
                        if (p2_3D_Conditions["X>=-PW"] &&
                            p2_3D_Conditions["X<=PW"])
                            pVisible = p2_3D;
                        if (!p2_3D_Conditions["X<=PW"])
                            pNonVisible2 = p2_3D;
                    }
                    else if (!p1_3D_Conditions["X<=PW"])
                    {
                        if (p2_3D_Conditions["X>=-PW"])
                            pNonVisible2 = p2_3D;
                        if (p2_3D_Conditions["X>=-PW"] &&
                            p2_3D_Conditions["X<=PW"])
                            pVisible = p2_3D;
                        if (!p2_3D_Conditions["X<=PW"])
                            return false;
                    }

                    if (!p1_3D_Conditions["Y>=-PH"])
                    {
                        if (!p2_3D_Conditions["Y>=-PH"])
                            return false;
                        if (p2_3D_Conditions["Y>=-PH"] &&
                            p2_3D_Conditions["Y<=PH"] &&
                            pNonVisible2 == null)
                            pVisible = p2_3D;
                        if (!p2_3D_Conditions["Y<=PH"] &&
                            pNonVisible2 == null)
                            pNonVisible2 = p2_3D;
                    }
                    else if (!p1_3D_Conditions["Y<=PH"])
                    {
                        if (p2_3D_Conditions["Y>=-PH"] &&
                            pNonVisible2 == null)
                            pNonVisible2 = p2_3D;
                        if (p2_3D_Conditions["Y>=-PH"] &&
                            p2_3D_Conditions["Y<=PH"] &&
                            pNonVisible2 == null)
                            pVisible = p2_3D;
                        if (!p2_3D_Conditions["Y<=PH"])
                            return false;
                    }
                }

                /*
                   Если не видна вторая точка:
                 */

                if (!p2_3D_Conditions["ScreenVisibility"])
                {
                    pNonVisible1 = p2_3D;

                    if (!p2_3D_Conditions["X>=-PW"])
                    {
                        if (!p1_3D_Conditions["X>=-PW"])
                            return false;
                        if (p1_3D_Conditions["X>=-PW"] &&
                            p1_3D_Conditions["X<=PW"])
                            pVisible = p1_3D;
                        if (!p1_3D_Conditions["X<=PW"])
                            pNonVisible2 = p1_3D;
                    }
                    else if (!p2_3D_Conditions["X<=PW"])
                    {
                        if (p1_3D_Conditions["X>=-PW"])
                            pNonVisible2 = p1_3D;
                        if (p1_3D_Conditions["X>=-PW"] &&
                            p1_3D_Conditions["X<=PW"])
                            pVisible = p1_3D;
                        if (!p1_3D_Conditions["X<=PW"])
                            return false;
                    }

                    if (!p2_3D_Conditions["Y>=-PH"])
                    {
                        if (!p1_3D_Conditions["Y>=-PH"])
                            return false;
                        if (p1_3D_Conditions["Y>=-PH"] &&
                            p1_3D_Conditions["Y<=PH"] &&
                            pNonVisible2 == null)
                            pVisible = p1_3D;
                        if (!p1_3D_Conditions["Y<=PH"] &&
                            pNonVisible2 == null)
                            pNonVisible2 = p1_3D;
                    }
                    else if (!p2_3D_Conditions["Y<=PH"])
                    {
                        if (p1_3D_Conditions["Y>=-PH"] &&
                            pNonVisible2 == null)
                            pNonVisible2 = p1_3D;
                        if (p1_3D_Conditions["Y>=-PH"] &&
                            p1_3D_Conditions["Y<=PH"] &&
                            pNonVisible2 == null)
                            pVisible = p1_3D;
                        if (!p1_3D_Conditions["Y<=PH"])
                            return false;
                    }
                }

                return true;
            }

            /*
               Метод FillPoint2D_Conditions заполняет
               словари условий нахождения точек в пределах
               экрана обзора.
             */

            void FillPoint2D_Conditions(Point P_2D,
                ref Dictionary<string, bool> dictP_2D_Conditions)
            {
                string X_2D_cond1 = "X>=0",
                       X_2D_cond2 = "X<SW",
                       Y_2D_cond1 = "Y>=0",
                       Y_2D_cond2 = "Y<SH",
                       SV_cond = "ScrVisibility";

                dictP_2D_Conditions = new Dictionary<string, bool>
                    {
                        { X_2D_cond1, P_2D.X >= 0 },
                        { X_2D_cond2, P_2D.X < bWidth },
                        { Y_2D_cond1, P_2D.Y >= 0 },
                        { Y_2D_cond2, P_2D.Y < bHeight },
                        { SV_cond, false }
                    };

                dictP_2D_Conditions[SV_cond] =
                    dictP_2D_Conditions[X_2D_cond1] &&
                    dictP_2D_Conditions[X_2D_cond2] &&
                    dictP_2D_Conditions[Y_2D_cond1] &&
                    dictP_2D_Conditions[Y_2D_cond2];
            }

            /* Метод GetSaturatePolygonColor вычисляет
               насыщенность (интенсивность) цвета
               получаемой на вход грани по модели Фонга. */

            float GetSaturationOfPolygonColor(Polygon poly)
            {
                Point3D N = new Point3D
                {
                    X = poly.NormalVectorPoint["WCS"].X -
                    poly.Parent.CenterPoint["WCS"].X,
                    Y = poly.NormalVectorPoint["WCS"].Y -
                    poly.Parent.CenterPoint["WCS"].Y,
                    Z = poly.NormalVectorPoint["WCS"].Z -
                    poly.Parent.CenterPoint["WCS"].Z
                }, L, R, S;

                float lenN = GetVectorLength(N), 
                      lenL = 0,
                      lenR = 0,
                      lenS = 0,
                      scalarNL = 0, scalarRS = 0;

                NormalizeVector(N, lenN);

                float polySaturationNew = LightSource.IntensityMax *
                    poly.Parent.Ka;

                /* Проход по источникам света. */

                foreach (LightSource ls in poly.Parent.Stage.Objects.Values
                    .Where(so => so is LightSource))
                {
                    L = new Point3D
                    {
                        X = ls.LocationPoint["WCS"].X - 
                        poly.NormalVectorPoint["WCS"].X,
                        Y = ls.LocationPoint["WCS"].Y -
                        poly.NormalVectorPoint["WCS"].Y,
                        Z = ls.LocationPoint["WCS"].Z -
                        poly.NormalVectorPoint["WCS"].Z
                    };
                    lenL = GetVectorLength(L);
                    NormalizeVector(L, lenL);
                    
                    float nx = (float)((-Math.Acos(N.X) + Math.Acos(L.X)) * 180 / Math.PI),
                        ny = (float)((-Math.Acos(N.Y) + Math.Acos(L.Y)) * 180 / Math.PI),
                        nz = (float)((-Math.Acos(N.Z) + Math.Acos(L.Z)) * 180 / Math.PI);
                    Point3D neN = new Point3D(nx, ny, nz);
                    double[,] matr = CalcMethods.getRotationMatrix
                        (new List<Dictionary<char, double>>
                        {
                            new Dictionary<char, double>
                            {
                                { 'X', nx },
                                { 'Y', ny },
                                { 'Z', nz }
                            }
                        });
                    //CalcMethods.CalculateLocationPoint(N, N, matr);
                    //lenN = GetVectorLength(N);
                    //NormalizeVector(N, lenN);

                    /*
                    float cosX_L = L.X / lenL,
                          cosY_L = L.Y / lenL,
                          cosZ_L = L.Z / lenL,
                          cosX_L = Math.Acos(cosX_L) + (90 * ;
                          */
                    float ox = (float)Math.Cos(nx * Math.PI / 180) * N.X,
                          oy = (float)Math.Cos(ny * Math.PI / 180) * N.Y,
                          oz = (float)Math.Cos(nz * Math.PI / 180) * N.Z;
                    Point3D o = new Point3D(ox, oy, oz);
                    float leno = GetVectorLength(o);
                    NormalizeVector(o, leno);

                    float ux = (float)Math.Cos(nx * Math.PI / 180) * o.X,
                          uy = (float)Math.Cos(ny * Math.PI / 180) * o.Y,
                          uz = (float)Math.Cos(nz * Math.PI / 180) * o.Z;
                    Point3D u = new Point3D(ux, uy, uz);
                    float lenu = GetVectorLength(u);
                    NormalizeVector(u, lenu);

                    R = new Point3D
                    {
                        X = L.X + (L.X - o.X),
                        Y = L.Y + (L.Y - o.Y),
                        Z = L.Z + (L.Z - o.Z)
                    };
                    lenR = GetVectorLength(R);
                    NormalizeVector(R, lenR);

                    S = new Point3D
                    {
                        X = poly.NormalVectorPoint["VCS"].X,
                        Y = poly.NormalVectorPoint["VCS"].Y,
                        Z = poly.NormalVectorPoint["VCS"].Z
                    };

                    double[,] TR = CalcMethods.getTransitMatrix
                        (new List<Dictionary<char, double>>
                        {
                            new Dictionary<char, double>
                            {
                                { 'X', -LocationPoint["WCS"].X },
                                { 'Y', -LocationPoint["WCS"].Y },
                                { 'Z', -LocationPoint["WCS"].Z }
                            }
                        }),
                        _vcs = CalcMethods.GetTransposedMatrix(Screen.VCS_Matrix),
                        matrix = null;
                    CalcMethods.multiplyingMatrices(ref matrix, 
                        new List<double[,]> { _vcs, TR});
                    CalcMethods.CalculateLocationPoint(S, S, matrix);
                    lenS = GetVectorLength(S);
                    NormalizeVector(S, lenS);

                    scalarNL = GetScalarMult(N, L);
                    scalarRS = GetScalarMult(R, S);

                    polySaturationNew += ls.Col.GetSaturation() * 
                        (poly.Kd * scalarNL + 
                        poly.Ks * (float)Math.Pow(scalarRS, poly.n));
                }

                return polySaturationNew;

                /* Получение длины вектора.*/

                float GetVectorLength(Point3D V)
                {
                    return (float)Math.Sqrt(V.X * V.X + V.Y * V.Y + V.Z * V.Z);
                }

                /* Нормализация вектора. */

                void NormalizeVector(Point3D V, float lenV)
                {
                    V.X = V.X / lenV;
                    V.Y = V.Y / lenV;
                    V.Z = V.Z / lenV;
                }
            }

            /* Получение скалярного умножения векторов. */

            float GetScalarMult(Point3D V1, Point3D V2)
            {
                return V1.X * V2.X +
                       V1.Y * V2.Y +
                       V1.Z * V2.Z;
            }

            /* Закраска полигона. */

            void PaintPolygon(Polygon poly, List<Edge2D> polyEdges2D)
            {
                float polySaturationNew = GetSaturationOfPolygonColor(poly);
                if (polySaturationNew > 1)
                    polySaturationNew = 1;
                else if (polySaturationNew < 0)
                    polySaturationNew = 0;
                Color polyColNew = CalcMethods.GetARGBFromHSL(
                    poly.Col.GetHue(), polySaturationNew, poly.Col.GetBrightness());

                SolidBrush brush = new SolidBrush(polyColNew);
                /*
                if (polyEdges2D.Count == 0)
                {
                    grStage.FillRectangle(brush, 0, 0, bitmap.Width, bitmap.Height);
                    return;
                }
                */
                /* Создание двунаправленного списка двухмерных рёбер полигона. */

                List<LinkedList<Edge2D>> links = new List<LinkedList<Edge2D>>();

                List<Edge2D> edges = new List<Edge2D>();
                foreach (Edge2D e in polyEdges2D)
                    edges.Add(e);

                while (edges.Count != 0)
                {
                    foreach (Edge2D e1 in polyEdges2D)
                    {
                        if (!edges.Contains(e1))
                            continue;

                        if (links.Exists(ls => ls.Contains(e1)))
                        {
                            LinkedListNode<Edge2D> eNode =
                                links.Last().Find(e1);
                            if (eNode.Previous != null &&
                                eNode.Next != null)
                                continue;
                        }

                        links.Add(MakeLinkBetweenEdges(e1,
                                new LinkedList<Edge2D>()));
                    }
                }

                LinkedList<Edge2D> MakeLinkBetweenEdges(Edge2D e1,
                        LinkedList<Edge2D> incompleteLink)
                {
                    LinkedList<Edge2D> link = new LinkedList<Edge2D>();
                    link.AddLast(e1);
                    edges.Remove(e1);

                    foreach (Edge2D e2 in polyEdges2D)
                    {
                        if (!edges.Contains(e2))
                            continue;

                        LinkedListNode<Edge2D> e1Node =
                            link.Find(e1);
                        if (e1Node.Previous != null &&
                            e1Node.Next != null)
                        {
                            if (incompleteLink.Last.Value
                                .Equals(link.First.Value))
                                incompleteLink.Concat(link);
                            else
                            {
                                link.Concat(incompleteLink);
                                incompleteLink = link;
                            }
                            break;
                        }

                        if (e1.Equals(e2))
                            continue;

                        bool edgeIsFit = false;

                        if (e1.ps[0].Equals(e2.ps[0]))
                            e2.ps.Reverse();
                        if (e1.ps[0].Equals(e2.ps[1]))
                        {
                            link.AddBefore(link.Find(e1), e2);
                            edgeIsFit = true;
                        }

                        if (e1.ps[1].Equals(e2.ps[1]))
                            e2.ps.Reverse();
                        if (e1.ps[1].Equals(e2.ps[0]))
                        {
                            link.AddAfter(link.Find(e1), e2);
                            edgeIsFit = true;
                        }

                        if (edgeIsFit)
                        {
                            LinkedList<Edge2D> linkBuf = MakeLinkBetweenEdges(e2, link);
                            foreach (Edge2D e in linkBuf)
                                incompleteLink.AddLast(e);
                        }
                    }

                    return link;
                }

                Dictionary<string, bool> p1Flags, p2Flags;

                /* Связывание цепей рёбер, углы крайних
                   из которых срезаны одной границей экрана. */

                List<LinkedList<Edge2D>> links1Restrict =
                    new List<LinkedList<Edge2D>>();

                /* Связывание цепей рёбер, углы крайних
                       из которых срезаны двумя границами экрана. */

                List<LinkedList<Edge2D>> links2Restrict =
                    new List<LinkedList<Edge2D>>();

                /* Связывание цепей рёбер, углы крайних
                       из которых срезаны тремя границами экрана. */

                List<LinkedList<Edge2D>> links3Restrict =
                    new List<LinkedList<Edge2D>>();

                foreach (LinkedList<Edge2D> link1 in links)
                {
                    List<Edge2D> sideEdges1 = new List<Edge2D>
                    { link1.First.Value, link1.Last.Value };

                    foreach (Edge2D e1 in sideEdges1)
                    {
                        foreach (Point p1 in e1.ps)
                        {
                            SetPointFlags(p1, out p1Flags);
                            if (p1Flags["isWithin"])
                                continue;

                            foreach (LinkedList<Edge2D> link2 in links)
                            {
                                List<Edge2D> sideEdges2 = new List<Edge2D>
                                { link2.First.Value, link2.Last.Value };

                                foreach (Edge2D e2 in sideEdges2)
                                {
                                    foreach (Point p2 in e2.ps)
                                    {
                                        SetPointFlags(p2, out p2Flags);
                                        if (p2Flags["isWithin"] ||
                                            p1.Equals(p2))
                                            continue;

                                        /* Одно ребро не может ограничиваться
                                           одной границей экрана. */

                                        if (!e1.Equals(e2))
                                            CheckPointsFlags(1, p1, p2);

                                        CheckPointsFlags(2, p1, p2);

                                        CheckPointsFlags(3, p1, p2);
                                    }
                                }
                            }
                        }
                    }
                }

                int l1 = links1Restrict.Count,
                    l2 = links2Restrict.Count,
                    l3 = links3Restrict.Count;

                links.AddRange(links1Restrict);
                links.AddRange(links2Restrict);
                links.AddRange(links3Restrict);

                List<LinkedList<Edge2D>> linksCopy = null,
                                         linksBuf;

                while (!(links.Count <= 1))
                {
                    linksCopy = new List<LinkedList<Edge2D>>();
                    foreach (LinkedList<Edge2D> link in links)
                        linksCopy.Add(link);

                    linksBuf = new List<LinkedList<Edge2D>>();

                    foreach (LinkedList<Edge2D> link in links)
                    {
                        if (!linksCopy.Contains(link))
                            continue;

                        LinkedList<Edge2D> chain = new LinkedList<Edge2D>();
                        foreach (Edge2D e in link)
                            chain.AddLast(e);

                        linksBuf.Add(MakeLinkBetweenLinks(link, chain));
                    }

                    links.Clear();
                    foreach (LinkedList<Edge2D> link in linksBuf)
                        links.Add(link);

                    linksCopy.Clear();
                }

                int edgesCount = 0;
                foreach (LinkedList<Edge2D> link in links)
                    edgesCount += link.Count;
                if (edgesCount < 3)
                    return;

                /* Флаги расположения двухмерной точки на экране. */

                void SetPointFlags(Point p, out Dictionary<string, bool> pFlags)
                {
                    pFlags = new Dictionary<string, bool>
                    {
                        { "X==0", false },
                        { "X==bW", false },
                        { "Y==0", false },
                        { "Y==bH", false},
                        { "isWithin", true }
                    };

                    if (p.X == 0)
                        pFlags["X==0"] = true;
                    if (p.X == bitmap.Width - 1)
                        pFlags["X==bW"] = true;
                    if (p.Y == 0)
                        pFlags["Y==0"] = true;
                    if (p.Y == bitmap.Height - 1)
                        pFlags["Y==bH"] = true;
                    pFlags["isWithin"] = !(pFlags["X==0"] ||
                        pFlags["X==bW"] || pFlags["Y==0"] ||
                        pFlags["Y==bH"]);
                }

                /* Связывание рёбер, углы между которыми обрезаны экраном. */

                void CheckPointsFlags(int mode, Point p1, Point p2)
                {
                    switch (mode)
                    {
                        case 1:
                            {
                                for (int i = 0; i <= 3; i++)
                                {
                                    if (p1Flags.Values.ToArray()[i] &&
                                        p2Flags.Values.ToArray()[i])
                                    {
                                        Edge2D e = new Edge2D
                                        {
                                            Ind = -1,
                                            ps = new Point[]
                                            { p1, p2 },
                                            Col = polyColNew
                                        };

                                        LinkedList<Edge2D> link = new LinkedList<Edge2D>();
                                        link.AddLast(e);

                                        if (!SearchRepeating(1, link))
                                            links1Restrict.Add(link);

                                        break;
                                    }
                                }

                                break;
                            }
                        case 2:
                            {
                                List<Edge2D> es = new List<Edge2D>();

                                bool isOk = false;

                                if ((p1Flags["X==0"] &&
                                     p2Flags["Y==0"]) ||
                                    (p2Flags["X==0"] &&
                                     p1Flags["Y==0"]))
                                {
                                    es.Add(new Edge2D
                                    {
                                        Ind = -1,
                                        ps = new Point[]
                                        { p1, new Point(0, 0) },
                                        Col = polyColNew
                                    });
                                    es.Add(new Edge2D
                                    {
                                        Ind = -1,
                                        ps = new Point[]
                                        { new Point(0, 0), p2 },
                                        Col = polyColNew
                                    });

                                    isOk = true;
                                }

                                if ((p1Flags["X==bW"] &&
                                     p2Flags["Y==0"]) ||
                                    (p2Flags["X==bW"] &&
                                     p1Flags["Y==0"]))
                                {
                                    es.Add(new Edge2D
                                    {
                                        Ind = -1,
                                        ps = new Point[]
                                        { p1, new Point(((int)bWidth - 1), 0) },
                                        Col = polyColNew
                                    });
                                    es.Add(new Edge2D
                                    {
                                        Ind = -1,
                                        ps = new Point[]
                                        { new Point(((int)bWidth - 1), 0), p2 },
                                        Col = polyColNew
                                    });

                                    isOk = true;
                                }

                                if ((p1Flags["X==bW"] &&
                                     p2Flags["Y==bH"]) ||
                                    (p2Flags["X==bW"] &&
                                     p1Flags["Y==bH"]))
                                {
                                    es.Add(new Edge2D
                                    {
                                        Ind = -1,
                                        ps = new Point[]
                                        { p1, new Point(((int)bWidth - 1), ((int)bHeight - 1)) },
                                        Col = polyColNew
                                    });
                                    es.Add(new Edge2D
                                    {
                                        Ind = -1,
                                        ps = new Point[]
                                        { new Point(((int)bWidth - 1), ((int)bHeight-1)), p2 },
                                        Col = polyColNew
                                    });

                                    isOk = true;
                                }

                                if ((p1Flags["X==0"] &&
                                    p2Flags["Y==bH"]) ||
                                   (p2Flags["X==0"] &&
                                    p1Flags["Y==bH"]))
                                {
                                    es.Add(new Edge2D
                                    {
                                        Ind = -1,
                                        ps = new Point[]
                                        { p1, new Point(0, ((int)bHeight - 1)) },
                                        Col = polyColNew
                                    });
                                    es.Add(new Edge2D
                                    {
                                        Ind = -1,
                                        ps = new Point[]
                                        { new Point(0, ((int)bHeight - 1)), p2 },
                                        Col = polyColNew
                                    });

                                    isOk = true;
                                }

                                if (isOk)
                                {
                                    LinkedList<Edge2D> link = new LinkedList<Edge2D>();
                                    foreach (Edge2D e in es)
                                        link.AddLast(e);

                                    if (!SearchRepeating(2, link))
                                        links2Restrict.Add(link);
                                }

                                break;
                            }
                        case 3:
                            {
                                /* Проекция точки, лежащей на пересечении нормали 
                                   от центра объекта до полигона, необходима
                                   для получения информации о том, 
                                   с какой стороны закрашивать этот самый полигон,
                                   ограниченный 2D-рёбрами. */

                                Point3D polyNVP_3D = poly.NormalVectorPoint["VCS"];
                                double[,] PR = getProjectiveMatrix(polyNVP_3D.Z);
                                double[] locationArrayNVP = new double[]
                                { polyNVP_3D.X, polyNVP_3D.Y, polyNVP_3D.Z, 1};
                                getScreenXY(locationArrayNVP, PR);
                                Point polyNVP_2D = new Point
                                {
                                    X = (int)Math.Truncate(locationArrayNVP[0]),
                                    Y = (int)Math.Truncate(locationArrayNVP[1])
                                };
                                locationArrayNVP[0] = Math.Truncate(polyNVP_2D.X + scrWidth / 2);
                                locationArrayNVP[1] = Math.Truncate(-polyNVP_2D.Y + scrHeight / 2);
                                getScreenXY(locationArrayNVP, MS);
                                polyNVP_2D = new Point
                                {
                                    X = (int)Math.Truncate(locationArrayNVP[0]),
                                    Y = (int)Math.Truncate(locationArrayNVP[1])
                                };

                                List<Edge2D> es = new List<Edge2D>();

                                Point pBuf1, pBuf2;

                                bool isOk = false;

                                if ((p1Flags["X==0"] &&
                                     p2Flags["X==bW"]) ||
                                    (p2Flags["X==0"] &&
                                     p1Flags["X==bW"]))
                                {
                                    if (p1Flags["X==0"])
                                    {
                                        pBuf1 = p1;
                                        pBuf2 = p2;
                                    }
                                    else
                                    {
                                        pBuf1 = p2;
                                        pBuf2 = p1;
                                    }

                                    bool psFlag = pBuf1.Y >= pBuf2.Y;

                                    if ((psFlag && polyNVP_2D.Y >= pBuf1.Y) ||
                                        (!psFlag && polyNVP_2D.Y >= pBuf2.Y))
                                    {
                                        es.Add(new Edge2D
                                        {
                                            Ind = -1,
                                            ps = new Point[]
                                            { pBuf1, new Point(0, ((int)bHeight - 1)) },
                                            Col = polyColNew
                                        });
                                        es.Add(new Edge2D
                                        {
                                            Ind = -1,
                                            ps = new Point[]
                                            { new Point(0, ((int)bHeight - 1)), 
                                              new Point(((int)bWidth - 1), ((int)bHeight - 1)) },
                                            Col = polyColNew
                                        });
                                        es.Add(new Edge2D
                                        {
                                            Ind = -1,
                                            ps = new Point[]
                                            { new Point(((int)bWidth - 1), ((int)bHeight - 1)),
                                              pBuf2 },
                                            Col = polyColNew
                                        });
                                    }
                                    else
                                    {
                                        es.Add(new Edge2D
                                        {
                                            Ind = -1,
                                            ps = new Point[]
                                            { pBuf1, new Point(0, 0) },
                                            Col = polyColNew
                                        });
                                        es.Add(new Edge2D
                                        {
                                            Ind = -1,
                                            ps = new Point[]
                                            { new Point(0, 0),
                                              new Point(((int)bWidth - 1), 0) },
                                            Col = polyColNew
                                        });
                                        es.Add(new Edge2D
                                        {
                                            Ind = -1,
                                            ps = new Point[]
                                            { new Point(((int)bWidth - 1), 0), pBuf2 },
                                            Col = polyColNew
                                        });
                                    }

                                    isOk = true;
                                }

                                if ((p1Flags["Y==0"] &&
                                     p2Flags["Y==bH"]) ||
                                    (p2Flags["Y==0"] &&
                                     p1Flags["Y==bH"]))
                                {
                                    if (p1Flags["Y==0"])
                                    {
                                        pBuf1 = p1;
                                        pBuf2 = p2;
                                    }
                                    else
                                    {
                                        pBuf1 = p2;
                                        pBuf2 = p1;
                                    }

                                    bool psFlag = pBuf1.X >= pBuf2.X;

                                    if ((psFlag && polyNVP_2D.X >= pBuf1.X) ||
                                        (!psFlag && polyNVP_2D.X >= pBuf2.X))
                                    {
                                        es.Add(new Edge2D
                                        {
                                            Ind = -1,
                                            ps = new Point[]
                                            { pBuf1, new Point(((int)bWidth - 1), 0) },
                                            Col = polyColNew
                                        });
                                        es.Add(new Edge2D
                                        {
                                            Ind = -1,
                                            ps = new Point[]
                                            { new Point(((int)bWidth - 1), 0),
                                              new Point(((int)bWidth - 1), ((int)bHeight - 1)) },
                                            Col = polyColNew
                                        });
                                        es.Add(new Edge2D
                                        {
                                            Ind = -1,
                                            ps = new Point[]
                                            { new Point(((int)bWidth - 1), ((int)bHeight - 1)), pBuf2 },
                                            Col = polyColNew
                                        });
                                    }
                                    else
                                    {
                                        es.Add(new Edge2D
                                        {
                                            Ind = -1,
                                            ps = new Point[]
                                            { pBuf1, new Point(0, 0) },
                                            Col = polyColNew
                                        });
                                        es.Add(new Edge2D
                                        {
                                            Ind = -1,
                                            ps = new Point[]
                                            { new Point(0, 0),
                                              new Point(0, ((int)bHeight - 1)) },
                                            Col = polyColNew
                                        });
                                        es.Add(new Edge2D
                                        {
                                            Ind = -1,
                                            ps = new Point[]
                                            { new Point(0, ((int)bHeight - 1) ), pBuf2 },
                                            Col = polyColNew
                                        });
                                    }

                                    isOk = true;
                                }

                                if (isOk)
                                {
                                    LinkedList<Edge2D> link = new LinkedList<Edge2D>();
                                    foreach (Edge2D e in es)
                                        link.AddLast(e);

                                    if (!SearchRepeating(3, link))
                                        links3Restrict.Add(link);
                                }

                                break;
                            }
                    }

                    /* Поиск повторяющейся цепи рёбер и удаление её из списка цепей
                       рёбер, проходящих по границам экрана. */

                    bool SearchRepeating(int linksRestrictNum, LinkedList<Edge2D> link)
                    {
                        LinkedList<Edge2D> link1 = new LinkedList<Edge2D>();
                        foreach (Edge2D e in link)
                            link1.AddLast(e);

                        List<LinkedList<Edge2D>> linksRestrict = null;
                        if (linksRestrictNum == 1)
                            linksRestrict = links1Restrict;
                        if (linksRestrictNum == 2)
                            linksRestrict = links2Restrict;
                        if (linksRestrictNum == 3)
                            linksRestrict = links3Restrict;

                        foreach(LinkedList<Edge2D> link2 in linksRestrict)
                        {
                            bool linksAreEqual = true;
                            //if (link2.Equals(link1))
                            for (int i = 0; i < link1.Count; i++)
                            {
                                Edge2D e1 = link1.ElementAt(i),
                                       e2 = link2.ElementAt(i);

                                for (int j = 0; j <= 1; j++)
                                {
                                    Point pnt1 = e1.ps[j],
                                          pnt2 = e1.ps[j];
                                    linksAreEqual &= pnt1.Equals(pnt2);
                                    if (!linksAreEqual)
                                        break;
                                }
                            }

                            if (linksAreEqual)
                                return true;

                            linksAreEqual = true;
                            ReverseLink(link1);

                            for (int i = 0; i < link1.Count; i++)
                            {
                                Edge2D e1 = link1.ElementAt(i),
                                       e2 = link2.ElementAt(i);

                                for (int j = 0; j <= 1; j++)
                                {
                                    Point pnt1 = e1.ps[j],
                                          pnt2 = e1.ps[j];
                                    linksAreEqual &= pnt1.Equals(pnt2);
                                    if (!linksAreEqual)
                                        break;
                                }
                            }

                            if (linksAreEqual)
                                return true;
                        }

                        return false;
                    }
                }

                /* Реверсирование списка рёбер и их точек внутри. */

                LinkedList<Edge2D> ReverseLink(LinkedList<Edge2D> link)
                {
                    LinkedList<Edge2D> revLink = new LinkedList<Edge2D>();

                    foreach (Edge2D e in link)
                        revLink.AddFirst(e);
                    //revLink.Reverse();

                    foreach (Edge2D e in revLink)
                    //e.ps.Reverse();
                    {
                        Point pBuf = new Point(e.ps[0].X, e.ps[0].Y);
                        e.ps[0] = new Point(e.ps[1].X, e.ps[1].Y);
                        e.ps[1] = pBuf;
                    }

                    return revLink;
                }

                /* Связывание цепей рёбер. */

                LinkedList<Edge2D> MakeLinkBetweenLinks(LinkedList<Edge2D> link1,
                    LinkedList<Edge2D> incompleteLink)
                {
                    linksCopy.Remove(link1);

                    LinkedList<Edge2D> link = new LinkedList<Edge2D>();

                    /* Связывание существующих цепей рёбер. */

                    foreach (LinkedList<Edge2D> link2 in links)
                    {
                        if (!linksCopy.Contains(link2))
                            continue;

                        bool linkIsFit = false;

                        while (true)
                        {
                            Edge2D e1, e2;

                            e1 = incompleteLink.First.Value;
                            e2 = link2.First.Value;

                            if (e1.ps[0].Equals(e2.ps[0]))
                            {
                                foreach (Edge2D ed1 in ReverseLink(link2))
                                    link.AddLast(ed1);
                                foreach (Edge2D ed2 in incompleteLink)
                                    link.AddLast(ed2);
                                linkIsFit = true;

                                break;
                            }

                            e1 = incompleteLink.First.Value;
                            e2 = link2.Last.Value;

                            if (e1.ps[0].Equals(e2.ps[1]))
                            {
                                foreach (Edge2D ed1 in link2)
                                    link.AddLast(ed1);
                                foreach (Edge2D ed2 in incompleteLink)
                                    link.AddLast(ed2);
                                linkIsFit = true;

                                break;
                            }

                            e1 = incompleteLink.Last.Value;
                            e2 = link2.First.Value;

                            if (e1.ps[1].Equals(e2.ps[0]))
                            {
                                foreach (Edge2D ed1 in incompleteLink)
                                    link.AddLast(ed1);
                                foreach (Edge2D ed2 in link2)
                                    link.AddLast(ed2);
                                linkIsFit = true;

                                break;
                            }

                            e1 = link1.Last.Value;
                            e2 = link2.Last.Value;

                            if (e1.ps[1].Equals(e2.ps[1]))
                            {
                                foreach (Edge2D ed1 in incompleteLink)
                                    link.AddLast(ed1);
                                foreach (Edge2D ed2 in ReverseLink(link2))
                                    link.AddLast(ed2);
                                linkIsFit = true;
                            }

                            break;
                        }
                        if (linkIsFit)
                            incompleteLink = MakeLinkBetweenLinks(link2, link);
                    }

                    return incompleteLink;
                }

                if (links.Count == 0)
                    return;

                LinkedList<Edge2D> edgesChain = links[0],
                                   bufChain = new LinkedList<Edge2D>();

                Edge2D eHigh = new Edge2D();
                int minX = (int)Math.Truncate(bWidth) - 1, 
                    maxX = 0,
                    minY = (int)Math.Truncate(bHeight) - 1,
                    maxY = 0;
                foreach(Edge2D e in edgesChain)
                {
                    foreach(Point p in e.ps)
                    {
                        int X = p.X, Y = p.Y;

                        if (X < minX)
                            minX = X;
                        else if (X > maxX)
                            maxX = X;
                        if (Y <= minY)
                        {
                            minY = Y;
                            eHigh = e;
                        }
                        else if (Y > maxY)
                            maxY = Y;
                    }
                }

                /* Создание двунаправленного списка рёбер, в котором
                   первое ребро содержит точку с минимальной высотой. */

                LinkedListNode<Edge2D> eHighNode = edgesChain.Find(eHigh),
                                       edgeNode = eHighNode;
                
                if (eHigh.ps[1].Y == minY)
                {
                    while (edgeNode != null)
                    {
                        bufChain.AddLast(ReverseEdge(edgeNode.Value));
                        edgeNode = edgeNode.Previous;
                    }
                    if (!edgesChain.Last.Equals(eHighNode))
                    {
                        edgeNode = edgesChain.Last;
                        while (edgeNode != eHighNode)
                        {
                            bufChain.AddLast(ReverseEdge(edgeNode.Value));
                            edgeNode = edgeNode.Previous;
                        }
                    }

                    Edge2D ReverseEdge(Edge2D ed)
                    {
                        Edge2D revEd = ed;

                        Point pBuf = new Point(revEd.ps[0].X, revEd.ps[0].Y);
                        revEd.ps[0] = new Point(revEd.ps[1].X, revEd.ps[1].Y);
                        revEd.ps[1] = pBuf;

                        return revEd;
                    }
                }
                else
                {
                    while (edgeNode != null)
                    {
                        bufChain.AddLast(edgeNode.Value);
                        edgeNode = edgeNode.Next;
                    }
                    if (!edgesChain.First.Equals(eHighNode))
                    {
                        edgeNode = edgesChain.First;
                        while (edgeNode != eHighNode)
                        {
                            bufChain.AddLast(edgeNode.Value);
                            edgeNode = edgeNode.Next;
                        }
                    }
                }
                
                edgesChain = bufChain;

                Edge2D edge1 = edgesChain.First.Value;
                edgeNode = edgesChain.First.Next;
                Edge2D edge2;
                int deltaX = maxX - minX,
                    deltaY = maxY - minY;

                if (edgesChain.Count <= 2)
                    return;

                for (;
                      edgeNode.Next != null;
                      edgeNode = edgeNode.Next)
                {
                    edge2 = edgeNode.Value;
                    Point p1 = edge1.ps[0],
                          p2 = edge2.ps[1],
                          p3 = edge2.ps[0];

                    grStage.FillPolygon(new SolidBrush(polyColNew), new Point[] { p1, p2, p3 });

                    /*
                    float j1, j2,
                          dX2X1 = p2.X - p1.X,
                          dX2X3 = p2.X - p3.X,
                          dX3X1 = p3.X - p1.X,
                          dY3Y1 = p3.Y - p1.Y,
                          dY2Y1 = p2.Y - p1.Y,
                          dY2Y3 = p2.Y - p3.Y;

                    bool F1 = ((p1.X < p2.X) && (p1.X < p3.X) ||
                               (p1.X <= p2.X) && (p1.X < p3.X) ||
                               (p1.X < p2.X) && (p1.X <= p3.X) ||
                               (p1.X <= p2.X) && (p1.X <= p3.X)),
                         F2 = ((p1.X > p2.X) && (p1.X > p3.X) ||
                               (p1.X > p2.X) && (p1.X >= p3.X) ||
                               (p1.X >= p2.X) && (p1.X > p3.X)),
                         F3 = ((p1.X > p2.X) && (p1.X < p3.X) ||
                               (p1.X > p2.X) && (p1.X <= p3.X) ||
                               (p1.X >= p2.X) && (p1.X < p3.X) ||
                               (p1.X >= p2.X) && (p1.X <= p3.X)),
                         F4 = ((p1.X > p3.X) && (p1.X < p2.X) ||
                               (p1.X > p3.X) && (p1.X <= p2.X) ||
                               (p1.X >= p3.X) && (p1.X < p2.X));
                               */
                    /*
                    for (int i = 1; i + minY < maxY; i++)
                    {
                        int jLeft = 0, jRight = 0;
                        
                        if (p3.Y >= p2.Y)
                        {
                            if (dY3Y1 == 0)
                                j1 = 0;
                            else
                                j1 = dX3X1 * i / dY3Y1;

                            if (i + minY <= p2.Y)
                            {
                                if (dY2Y1 == 0)
                                    j2 = 0;
                                else
                                    j2 = dX2X1 * i / dY2Y1;

                                if (F1 || F3)
                                    jLeft = (int)Math.Truncate(j2 + p1.X);
                                else if (F2 || F4)
                                    jRight = (int)Math.Truncate(j2 + p1.X);
                            }
                            else
                            {
                                if (dY2Y3 == 0)
                                    j2 = 0;
                                else
                                    j2 = -dX2X3 * (i - dY2Y1) / -dY2Y3;

                                if (F1 || F3)
                                    jLeft = (int)Math.Truncate(j2 + p2.X);
                                else if (F2 || F4)
                                    jRight = (int)Math.Truncate(j2 + p2.X);
                            }

                            if (F1 || F3)
                                jRight = (int)Math.Truncate(j1 + p1.X);
                            else if (F2 || F4)
                                jLeft = (int)Math.Truncate(j1 + p1.X);
                        }
                        else
                        {
                            if (dY2Y1 == 0)
                            {
                                j1 = 0;
                            }
                            else
                                j1 = p1.X + dX2X1 * i / dY2Y1;

                            if (i + minY <= p2.Y)
                            {                       
                                if (dY3Y1 == 0)
                                    j2 = 0;
                                else
                                    j2 = dX3X1 * i / dY3Y1;

                                if (F1 || F4)
                                    jLeft = (int)Math.Truncate(j2 + p1.X);
                                else if (F2 || F3)
                                    jRight = (int)Math.Truncate(j2 + p1.X);
                            }
                            else
                            {
                                if (dY2Y3 == 0)
                                    j2 = 0;
                                else
                                    j2 = dX2X3 * (i - dY3Y1) / dY2Y3;

                                if (F1 || F4)
                                    jLeft = (int)Math.Truncate(j2 + p3.X);
                                else if (F2 || F3)
                                    jRight = (int)Math.Truncate(j2 + p3.X);
                            }

                            if (F1 || F4)
                                jRight = (int)Math.Truncate(j1 + p1.X);
                            else if (F2 || F3)
                                jLeft = (int)Math.Truncate(j1 + p1.X);
                        }
                        */

                        /*
                        if (p3.Y <= p2.Y)
                        {
                            if (p3.X >= p2.X)
                            {
                                jLeft = (int)Math.Truncate(p1.X + dX2X1 * i / dY2Y1);
                                if (i + minY <= p3.Y)
                                    jRight = (int)Math.Truncate(j2);
                                else
                                    jRight = (int)Math.Truncate(j2);
                            }
                            else
                            {
                                jRight = (int)Math.Truncate(p1.X + dX2X1 * i / dY2Y1);
                                if (i + minY <= p3.Y)
                                    jLeft = (int)Math.Truncate(j2);
                                else
                                    jLeft = (int)Math.Truncate(j2);
                            }

                            /*if (p2.Y - p1.Y == 0)
                                j1 = p1.X;
                            else
                            j1 = p1.X + (p2.X - p1.X) * i / (p2.Y - p1.Y);*/
                        /*
                        j1 = p1.X + dX2X1 * i / dY2Y1;

                        if (i + minY <= p3.Y)
                        {
                            /*if (p3.Y - p1.Y == 0)
                                j2 = p3.X;
                            else
                            j2 = p1.X + (p3.X - p1.X) * i / (p3.Y - p1.Y);*/
                        /*  j2 = p1.X + dX3X1 * i / dY3Y1;
                      }
                      else
                      {
                          /*if (p3.Y - p1.Y == 0)
                              j2 = p3.X;
                          else
                          j2 = p1.X + (p3.X - p2.X) * (i - (p3.Y - p1.Y))
                              / (p2.Y - p3.Y);*/
                        /*j2 = p3.X + dX2X3 * (i - dY3Y1)
                            / dY2Y3;
                      }

                    if (p3.X >= p2.X)
                    {
                        jLeft = (int)Math.Truncate(j1);
                        jRight = (int)Math.Truncate(j2);
                    }
                    else
                    {
                        jLeft = (int)Math.Truncate(j2);
                        jRight = (int)Math.Truncate(j1);
                    }
                    */
                        //}
                        //else
                        //{
                        /*if (p3.Y - p1.Y == 0)
                            j2 = p3.X;
                        else*/
                        /*j2 = p1.X + dX3X1 * i / dY3Y1;

                        if (i + minY <= p2.Y)
                        {*/
                        /*if (p2.Y - p1.Y == 0)
                            j1 = p1.X;
                        else
                        j1 = p1.X + (p2.X - p1.X) * i / (p2.Y - p1.Y);*/
                        /* j1 = p1.X + dX2X1 * i / dY2Y1;
                     }
                     else
                     {*/
                        /*if (p3.Y - p2.Y == 0)
                            j1 = p2.X;
                        else
                        j1 = p1.X + (p2.X - p3.X) * (i - (p2.X - p1.X))
                            / (p3.Y - p2.Y);*/
                        /*      j1 = p2.X + dX2X3 * (i - dY2Y1)
                                  / -dY2Y3;
                          }

                          if (p3.X >= p2.X)
                          {
                              jLeft = (int)Math.Truncate(j2);
                              jRight = (int)Math.Truncate(j1);
                          }
                          else
                          {
                              jLeft = (int)Math.Truncate(j1);
                              jRight = (int)Math.Truncate(j2);
                          }

                      }
                      *//*
                        if (edgesChain.First.Next.Equals(edgeNode))
                            jLeft++;

                        for (int j = jLeft; j < jRight && j < bWidth; j++)
                        {
                            if(j >= 0)
                                bitmap.SetPixel(j, i + minY, polyColNew);
                        }
                    }*/
                }
            }
        }

        /* Метод getProjectiveMatrix выдаёт на выходе 
           результирующую матрицу PR (матрицу проекции). */

        private double[,] getProjectiveMatrix(float pointZ)
        {
            double[,] PR = new double[4, 4];

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                    PR[i, j] = 0;
            }

            PR[0, 0] = d / (pointZ + d);
            PR[1, 1] = d / (pointZ + d);
            PR[3, 2] = 1 / (pointZ + d);
            PR[3, 3] = d / (pointZ + d);

            return PR;
        }
    }
}
