using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.DataVisualization.Charting;

namespace GraphicClassLibrary
{
    /* Класс сцены. */

    public class Scene
    {
        public int Xmax, Ymax, Zmax;

        /* Перечисление наименований координатных систем:
           -- WCS: мировая система координат (World CS);
           -- LCS: локальная система координат (Local CS);
           -- VCS: видовая система координат (Viewing CS);
           -- OCS: система координат наблюдателя (Observer's CS) */

        public static Dictionary<string, string> CoordinateSystems =
            new Dictionary<string, string>
            {
                { "WCS", "WCS" },
                { "LCS", "LCS" },
                { "VCS", "VCS" },
                { "OCS", "OCS" }
            };

        /* Словарь координатная_система-3D_точка. */

        public Dictionary<string, Point3D> LocationPoint;

        /* Приписанный к сцене наблюдатель. */

        public Observer Spectator;

        /* Количество 3D-объектов на сцене. */

        public static int Objects3DCounter = 0;

        /* Количество источников света на сцене. */

        public static int LightSourcesCounter = 0;

        /* Словарь объектов на сцене и их имён. */

        public Dictionary<string, IObjectOnStage> Objects =
            new Dictionary<string, IObjectOnStage>();

        public Scene(Observer _spectator)
        {
            Xmax = Ymax = Zmax = 1000;

            LocationPoint = new Dictionary<string, Point3D>
            {
                { CoordinateSystems["WCS"], new Point3D(0, 0, 0) },
                { CoordinateSystems["LCS"], new Point3D(0, 0, 0) },
                { CoordinateSystems["VCS"], null }
            };

            Spectator = _spectator;
            Objects.Add(Spectator.Name, Spectator);

            Spectator.Stage = this;

            LocationPoint["VCS"] = new Point3D();

            InitLocationPoint(new Dictionary<string, double[,]>
            { { "VCS", null } });
        }

        /* Метод InitLocationPoint устанавливает точку расположения сцены
           в видовой системе координат. */

        public void InitLocationPoint(Dictionary<string, double[,]>
                                        UpdateMatrices)
        {
            List<string> CSs = UpdateMatrices.Keys.ToList();

            foreach (string CS in CSs)
            {
                switch (CS)
                {
                    case "VCS":
                        {
                            List<Dictionary<char, double>> dictOffsets =
                                new List<Dictionary<char, double>>
                                {
                                    new Dictionary<char, double>
                                    {
                                        {
                                            'X',
                                            -Spectator.Screen.LocationPoint["WCS"].X
                                        },
                                        {
                                            'Y',
                                            -Spectator.Screen.LocationPoint["WCS"].Y
                                        },
                                        {
                                            'Z',
                                            -Spectator.Screen.LocationPoint["WCS"].Z
                                        }
                                    }
                                };

                            double[,] _matrix = new double[4, 4],
                                      TR1 = CalcMethods
                                      .getTransitMatrix(dictOffsets),
                                      _vcs = Spectator.Screen.VCS_Matrix;

                            dictOffsets =
                                new List<Dictionary<char, double>>
                                {
                                    new Dictionary<char, double>
                                    {
                                        {
                                            'X',
                                            Spectator.Screen.LocationPoint["WCS"].X
                                        },
                                        {
                                            'Y',
                                            Spectator.Screen.LocationPoint["WCS"].Y
                                        },
                                        {
                                            'Z',
                                            Spectator.Screen.LocationPoint["WCS"].Z
                                        }
                                    }
                                };

                            double[,] TR2 = CalcMethods
                                      .getTransitMatrix(dictOffsets);

                            CalcMethods.multiplyingMatrices(ref _matrix,
                                new List<double[,]>
                                { TR1, _vcs/*, TR2 */});

                            CalcMethods.CalculateLocationPoint(LocationPoint["VCS"],
                                              Spectator.Screen.LocationPoint["LCS"],
                                                            _matrix);

                            break;
                        }
                }
            }
        }

        /* Метод UpdateLocationPoint обновляет точку расположения сцены
           в видовой системе координат. */

        public void UpdateLocationPoint(Dictionary<string, double[,]>
                                        UpdateMatrices)
        {
            List<string> CSs = UpdateMatrices.Keys.ToList();

            foreach (string CS in CSs)
            {
                switch (CS)
                {
                    case "VCS":
                        {
                            List<Dictionary<char, double>> dictOffsets =
                                new List<Dictionary<char, double>>
                                {
                                    new Dictionary<char, double>
                                    {
                                        {
                                            'X',
                                            -LocationPoint["VCS"].X
                                        },
                                        {
                                            'Y',
                                            -LocationPoint["VCS"].Y
                                        },
                                        {
                                            'Z',
                                            -LocationPoint["VCS"].Z
                                        }
                                    }
                                };

                            double[,] _matrix = new double[4, 4],
                                      TR1 = CalcMethods
                                      .getTransitMatrix(dictOffsets),
                                      _vcs = Spectator.Screen.VCS_Matrix;

                            dictOffsets =
                                new List<Dictionary<char, double>>
                                {
                                    new Dictionary<char, double>
                                    {
                                        {
                                            'X',
                                            LocationPoint["VCS"].X
                                        },
                                        {
                                            'Y',
                                            LocationPoint["VCS"].Y
                                        },
                                        {
                                            'Z',
                                            LocationPoint["VCS"].Z
                                        }
                                    }
                                };

                            double[,] TR2 = CalcMethods
                                      .getTransitMatrix(dictOffsets);

                            CalcMethods.multiplyingMatrices(ref _matrix,
                                new List<double[,]>
                                { TR1, _vcs, TR2 });

                            CalcMethods.CalculateLocationPoint(LocationPoint["VCS"],
                                                               LocationPoint["VCS"],
                                                            _matrix);

                            break;
                        }
                }
            }
        }

        /* Метод AddObject выполняет следующие действия:
           -- добавляет 3D-объект в список объектов на сцене;
           -- добавляет */

        public void AddObject(IObjectOnStage obj)
        {
            obj.Stage = this;

            Objects.Add(obj.Name, obj);

            string objKey = "";

            if (obj is Object3D)
            {
                objKey = "Obj" + (Objects3DCounter + 1).ToString() + "CS";

                LocationPoint.Add(objKey,
                                  new Point3D
                                  (
                                      -((Object3D)obj)
                                      .CenterPoint[CoordinateSystems["WCS"]].X,
                                      -((Object3D)obj)
                                      .CenterPoint[CoordinateSystems["WCS"]].Y,
                                      -((Object3D)obj)
                                      .CenterPoint[CoordinateSystems["WCS"]].Z
                                  ));

                Objects3DCounter++;
            }
            else if (obj is LightSource)
            {
                objKey = "LS" + (LightSourcesCounter + 1).ToString() + "CS";

                LocationPoint.Add(objKey,
                                  new Point3D
                                  (
                                      -((LightSource)obj)
                                      .LocationPoint[CoordinateSystems["WCS"]].X,
                                      -((LightSource)obj)
                                      .LocationPoint[CoordinateSystems["WCS"]].Y,
                                      -((LightSource)obj)
                                      .LocationPoint[CoordinateSystems["WCS"]].Z
                                  ));

                LightSourcesCounter++;
            }

            CoordinateSystems.Add(obj.Name, objKey);
        }

        /* Метод UpdateObjectsLocationsByCameraView обновляет
           видовые координаты всех 3D-объектов на сцене. */

        public void UpdateObjectsLocationsByCameraView()
        {
            InitLocationPoint(new Dictionary<string, double[,]>
            { { "VCS", null } });

            foreach (string objName in Objects.Keys.Where(k => k != Spectator.Name))
            {
                IObjectOnStage obj = Objects[objName];

                obj.UpdatePoints('c',
                    new Dictionary<string, double[,]>
                    {
                        {
                            "VCS",
                            Spectator.Screen.VCS_Matrix
                        }
                    });
            }
        }
    }
}
