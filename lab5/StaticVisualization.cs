using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Web.UI.DataVisualization.Charting;
using GraphicClassLibrary;

namespace lab5
{ 
    /* Перечисление состояний анимации (одного кадра):
       -- анимация не была запущена;
       -- анимация запущена;
       -- анимация остановлена извне;
       -- анимация прекращена. */

    enum AnimationsStates
    {
        None,
        Started,
        Stopped,
        Ended
    }

    /* Класс формы. */

    public partial class StaticVisualization : Form
    {
        /* Сцена. */

        public Scene Stage;

        /* Наблюдатель. */

        public ViewingWindow Screen;

        /* Наблюдатель. */

        public Observer Spectator;

        /* Обозреваемая пирамида. */

        static public Object3D Pyramid;

        /* Холст. */

        static public Graphics grStage;

        /* Состояние анимации на текущий момент. */

        AnimationsStates animState = AnimationsStates.None;

        /* Поток, в котором выполняется анимация. */

        Thread AnimationThread;

        /* Конструктор формы. */

        public StaticVisualization()
        {
            InitializeComponent();

            btnStopAnimation.Enabled = false;

            this.Shown += Pyramid1_Shown;

            this.Screen = new ViewingWindow(
                new RectangleF
                (
                    new Point(0, 0),
                    new SizeF(150F, 150F)
                ),
                new RectangleF
                (
                    new Point(0, 0),
                    new SizeF(pbScreen.Width, pbScreen.Height)
                )
                                            );

            Spectator = new Observer(new Point3D(750, 0, 0),
                                     new Dictionary<char, double>
                                     {
                                         { 'Y', -90 },
                                         { 'Z', 0 },
                                         { 'X', 0 },
                                     },
                                     325,
                                     this.Screen);

            Stage = new Scene(Spectator);

            List<Point3D> _vertexes;

            _vertexes = new List<Point3D>
            {
                new Point3D(-45, -60, -45),
                new Point3D(-45, -60, 45),
                new Point3D(45, -60, 45),
                new Point3D(45, -60, -45),
                new Point3D(-20, 85, -20),
                new Point3D(-20, 85, 20),
                new Point3D(20, 85, 20),
                new Point3D(20, 85, -20),
                new Point3D(-70, 12, -70),
                new Point3D(-70, 12, 70),
                new Point3D(70, 12, 70),
                new Point3D(70, 12, -70)
            };

            Pyramid =
                new Object3D(new Point3D(0, 180, -150),
                             90,
                             "Pyramid_4_sloping_truncated");

            Pyramid.SetColorPalette(
                new List<Color>
                {
                    Color.Red,
                    Color.Orange,
                    Color.Yellow,
                    Color.YellowGreen,
                    Color.Blue,
                    Color.Purple,
                    Color.Black
                });

            List<Edge> _edges = new List<Edge>
            {
                new Edge(4, 5, 6),
                new Edge(5, 6, 6),
                new Edge(6, 7, 6),
                new Edge(7, 4, 6),
                new Edge(0, 1, 6),
                new Edge(1, 2, 6),
                new Edge(2, 3, 6),
                new Edge(3, 0, 6),
                new Edge(4, 0, 6),
                new Edge(5, 1, 6),
                new Edge(6, 2, 6),
                new Edge(7, 3, 6),
                ///////
                new Edge(0, 2, 6),
                new Edge(1, 4, 6),
                new Edge(2, 5, 6),
                new Edge(3, 6, 6),
                new Edge(0, 7, 6),
                new Edge(4, 6, 6),
            };

            List<Polygon> _polygons = new List<Polygon>
            {
                /*
                new Polygon(
                    new List<int> { 4, 5, 12 },
                    0,
                    new Point3D(0, 8100, 0)
                ),//////////
                new Polygon(
                    new List<int> { 6, 7, 12 },
                    1,
                    new Point3D(-13050, 2250, 0)
                ),
                new Polygon(
                    new List<int> { 4, 8, 13 },
                    2,
                    new Point3D(0, -2250, -13050)
                ),
                new Polygon(
                    new List<int> { 0, 9, 13 },
                    3,
                    new Point3D(13050, 2250, 0)
                ),
                new Polygon(
                    new List<int> { 5, 9, 14 },
                    4,
                    new Point3D(0, -2250, 13050)
                ),
                new Polygon(
                    new List<int> { 1, 10, 14 },
                    5,
                    new Point3D(0, 1600, 0)
                ),
                new Polygon(
                    new List<int> { 6, 10, 15 },
                    0,
                    new Point3D(0, 8100, 0)
                ),//////////
                new Polygon(
                    new List<int> { 2, 11, 15 },
                    1,
                    new Point3D(-13050, 2250, 0)
                ),
                new Polygon(
                    new List<int> { 7, 11, 16 },
                    2,
                    new Point3D(0, -2250, -13050)
                ),
                new Polygon(
                    new List<int> { 4, 8, 16 },
                    3,
                    new Point3D(13050, 2250, 0)
                ),
                new Polygon(
                    new List<int> { 0, 1, 17 },
                    4,
                    new Point3D(0, -2250, 13050)
                ),///////////
                new Polygon(
                    new List<int> { 2, 3, 17 },
                    5,
                    new Point3D(0, 1600, 0)
                )*/
                
                new Polygon(
                    new List<int> { 0, 1, 2, 3 },
                    0/*,
                    new Point3D(0, 8100, 0)*/
                ),//////////
                new Polygon(
                    new List<int> { 0, 9, 4, 8 },
                    1/*,
                    new Point3D(-13050, 2250, 0)*/
                ),
                new Polygon(
                    new List<int> { 9, 1, 10, 5 },
                    2/*,
                    new Point3D(0, -2250, -13050)*/
                ),
                new Polygon(
                    new List<int> { 2, 10, 6, 11 },
                    3/*,
                    new Point3D(13050, 2250, 0)*/
                ),
                new Polygon(
                    new List<int> { 8, 3, 11, 7 },
                    4/*,
                    new Point3D(0, -2250, 13050)*/
                ),///////////
                new Polygon(
                    new List<int> { 4, 5, 6, 7 },
                    5/*,
                    new Point3D(0, 1600, 0)*/
                ),  ///////////////////////
                /*
                new Polygon(
                    new List<int[]>
                    {
                        new int[] {0, 1},
                        new int[] {1, 9},
                        new int[] {9, 8},
                        new int[] {8, 0}
                    },
                    5,
                    new Point3D(0, 8100, 0)
                ),
                new Polygon(
                    new List<int[]>
                    {
                        new int[] {1, 2},
                        new int[] {2, 10},
                        new int[] {10, 9},
                        new int[] {9, 1}
                    },
                    5,
                    new Point3D(0, 8100, 0)
                ),
                new Polygon(
                    new List<int[]>
                    {
                        new int[] {2, 3},
                        new int[] {3, 11},
                        new int[] {11, 10},
                        new int[] {10, 2}
                    },
                    5,
                    new Point3D(0, 8100, 0)
                ),
                new Polygon(
                    new List<int[]>
                    {
                        new int[] {3, 0},
                        new int[] {0, 8},
                        new int[] {8, 11},
                        new int[] {11, 3}
                    },
                    5,
                    new Point3D(0, 8100, 0)
                ),
                new Polygon(
                    new List<int[]>
                    {
                        new int[] {4, 5},
                        new int[] {5, 9},
                        new int[] {9, 8},
                        new int[] {8, 4}
                    },
                    5,
                    new Point3D(0, 8100, 0)
                ),
                new Polygon(
                    new List<int[]>
                    {
                        new int[] {5, 6},
                        new int[] {6, 10},
                        new int[] {10, 9},
                        new int[] {9, 5}
                    },
                    5,
                    new Point3D(0, 8100, 0)
                ),
                new Polygon(
                    new List<int[]>
                    {
                        new int[] {6, 7},
                        new int[] {7, 11},
                        new int[] {11, 10},
                        new int[] {10, 6}
                    },
                    5,
                    new Point3D(0, 8100, 0)
                ),
                new Polygon(
                    new List<int[]>
                    {
                        new int[] {7, 4},
                        new int[] {4, 8},
                        new int[] {8, 11},
                        new int[] {11, 7}
                    },
                    5,
                    new Point3D(0, 8100, 0)
                )
                */
            };

            Pyramid.SetPolysAndEdges(_edges, _polygons);
            Stage.AddObject(Pyramid);
            Pyramid.InitPoints(_vertexes);

            Object3D P2 = Pyramid.Clone();
            P2.CenterPoint["WCS"] = new Point3D(0, 180, 150);

            P2.Name = "Pyramid2";

            Stage.AddObject(P2);

            List<Point3D> _vertexesP2 = new List<Point3D>(_vertexes);

            P2.InitPoints(_vertexesP2);

            Object3D P3 = Pyramid.Clone();
            P3.CenterPoint["WCS"] = new Point3D(0, -205, -150);

            P3.Name = "Pyramid3";

            Stage.AddObject(P3);

            List<Point3D> _vertexesP3 = new List<Point3D>(_vertexes);

            P3.InitPoints(_vertexesP3);

            Object3D P4 = Pyramid.Clone();
            P4.CenterPoint["WCS"] = new Point3D(0, -205, 150);

            P4.Name = "Pyramid4";

            Stage.AddObject(P4);

            List<Point3D> _vertexesP4 = new List<Point3D>(_vertexes);

            P4.InitPoints(_vertexesP4);

            Object3D P5 = Pyramid.Clone();
            P5.CenterPoint["WCS"] = new Point3D(-210, 0, -210);

            P5.Name = "Pyramid5";

            Stage.AddObject(P5);

            List<Point3D> _vertexesP5 = new List<Point3D>(_vertexes);

            P5.InitPoints(_vertexesP5);

            Object3D P6 = Pyramid.Clone();
            P6.CenterPoint["WCS"] = new Point3D(-210, 0, 210);

            P6.Name = "Pyramid6";

            Stage.AddObject(P6);

            List<Point3D> _vertexesP6 = new List<Point3D>(_vertexes);

            P6.InitPoints(_vertexesP6);

            Object3D P7 = Pyramid.Clone();
            P7.CenterPoint["WCS"] = new Point3D(210, 0, -210);

            P7.Name = "Pyramid7";

            Stage.AddObject(P7);

            List<Point3D> _vertexesP7 = new List<Point3D>(_vertexes);

            P7.InitPoints(_vertexesP7);

            Object3D P8 = Pyramid.Clone();
            P8.CenterPoint["WCS"] = new Point3D(210, 0, 210);

            P8.Name = "Pyramid8";

            Stage.AddObject(P8);

            List<Point3D> _vertexesP8 = new List<Point3D>(_vertexes);

            P8.InitPoints(_vertexesP8);
        }

        /* Обработчик события "форма становится видимой". */

        public void Pyramid1_Shown(object sender, EventArgs e)
        {
            Thread.Sleep(500);

            grStage = pbScreen.CreateGraphics();
            grStage.SmoothingMode = SmoothingMode.AntiAlias;

            Spectator.VW_Update(grStage);

            PrintCalculations();
        }

        /* Потокобезопасный вывод информации в lstvObject. */

        public void lstvObject_DoSmth(char mode, string str)
        {
            Action lstvObjectAction = null;

            switch (mode)
            {
                case 'c':
                    {
                        lstvObjectAction = () =>
                        lstvObject.Items.Clear();
                        break;
                    }
                case 'a':
                    {
                        lstvObjectAction = () =>
                        lstvObject.Items.Add(new ListViewItem(str));
                        break;
                    }
            }

            lock (lstvObject)
            {
                if (lstvObject.InvokeRequired)
                    lstvObject.BeginInvoke(lstvObjectAction);
                else
                    lstvObjectAction();
            }
        }

        /* Потокобезопасный вывод информации в lstvObserver. */

        public void lstvObserver_DoSmth(char mode, string str)
        {
            Action lstvObserverAction = null;

            switch (mode)
            {
                case 'c':
                    {
                        lstvObserverAction = () =>
                        lstvObserver.Items.Clear();
                        break;
                    }
                case 'a':
                    {
                        lstvObserverAction = () =>
                        lstvObserver.Items.Add(new ListViewItem(str));
                        break;
                    }
            }

            lock (lstvObserver)
            {
                if (lstvObserver.InvokeRequired)
                    lstvObserver.BeginInvoke(lstvObserverAction);
                else
                    lstvObserverAction();
            }
        }

        /* Обработчик события нажатия кнопки запуска анимации объектов. */

        private void btnStartAnimation_Click(object sender, EventArgs e)
        {
            AnimationThread = new Thread(Animation);
            AnimationThread.Name = "AnimationThread";
            AnimationThread.Start();
            btnStopAnimation.Enabled = true;
            btnStartAnimation.Enabled = false;
        }

        /* Обработчик события нажатия кнопки остановки анимации объектов. */

        private void btnStopAnimation_Click(object sender, EventArgs e)
        {
            if (animState != AnimationsStates.None)
            {
                AnimationThread.Abort();
                animState = AnimationsStates.None;
                btnStartAnimation.Enabled = true;
                btnStopAnimation.Enabled = false;
            }
        }

        /* Перехват нажатия командных кнопок для управления камерой. */

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            List<Keys> ctrlKeys = new List<Keys>
                        {
                            Keys.D1, Keys.D2, Keys.D3,
                            Keys.D4, Keys.D5, Keys.D6,
                            Keys.Up, Keys.Down,
                            Keys.Left, Keys.Right,
                            Keys.Add, Keys.Subtract
                        };
            if (ctrlKeys.Contains(keyData))
            {
                Observer_KeyDown(keyData);
                return true;
            }
            else
                return base.ProcessCmdKey(ref msg, keyData);
        }

        /* Распечатка информации о координатах объекта и наблюдателя. */

        public void PrintCalculations()
        {
            lstvObject_DoSmth
                ('c', null);
            lstvObject_DoSmth
                ('a', "X: " + Pyramid.CenterPoint["VCS"].X.ToString("F3"));
            lstvObject_DoSmth
                ('a', "Y: " + Pyramid.CenterPoint["VCS"].Y.ToString("F3"));
            lstvObject_DoSmth
                ('a', "Z: " + Pyramid.CenterPoint["VCS"].Z.ToString("F3"));

            lstvObserver_DoSmth
                ('c', null);
            lstvObserver_DoSmth
                ('a', "X: " + Spectator.LocationPoint["WCS"].X.ToString("F2"));
            lstvObserver_DoSmth
                ('a', "Y: " + Spectator.LocationPoint["WCS"].Y.ToString("F2"));
            lstvObserver_DoSmth
                ('a', "Z: " + Spectator.LocationPoint["WCS"].Z.ToString("F2"));
        }

        /* Метод Animation в параллельном асинхронном потоке выполняет
           анимацию сцены. */

        async public void Animation(object o)
        {
            Random rLetterIndex = new Random(793481);
            Random rRotationAngle = new Random(84717612);

            while (true)
            {
                Dictionary<char, double> Pyramid_Rotation,
                                         Spectator_Rotation;

                int letterIndex = rLetterIndex.Next(1, 3);
                char rotationAxis = ' ';
                switch (letterIndex)
                {
                    case 1: rotationAxis = 'X'; break;
                    case 2: rotationAxis = 'Y'; break;
                    case 3: rotationAxis = 'Z'; break;
                }

                double rotationAngle = rRotationAngle.Next(-20, 20);

                Pyramid_Rotation = new Dictionary<char, double>
                {
                    { rotationAxis, rotationAngle }
                };

                int counterOfSecondsOfMoveToOneSide = 50,
                FramesPerSecond = 24,
                interval = counterOfSecondsOfMoveToOneSide * 1000 /
                           FramesPerSecond;

                for ( ;
                     counterOfSecondsOfMoveToOneSide > 0;
                     counterOfSecondsOfMoveToOneSide--)
                {

                    List<Dictionary<char, double>> _pyramidMatrix =
                        new List<Dictionary<char, double>>();

                    _pyramidMatrix.Add(Pyramid_Rotation);

                    /* Что мы делаем:
                       создаём словарь координатных систем, для которых проводятся
                       изменения координат точек объектов
                       {
                         ключ = название объекта, для которого проводятся изменения;
                         значение = список координатных систем, для которых
                         проводятся изменения координат точек объектов.
                       } */

                    Dictionary<string, List<string>> CSs =
                        new Dictionary<string, List<string>>();

                    List<string> _css1 = new List<string>
                    {
                        Scene.CoordinateSystems["VCS"]
                    };

                    List<string> _css2 = new List<string>
                    {
                        Scene.CoordinateSystems["WCS"],
                        Scene.CoordinateSystems["VCS"]
                    };

                    CSs.Add("Stage", _css1);
                    //CSs.Add(Spectator.Name, _css1);
                    foreach (IObjectOnStage obj in Stage.Objects.Values.
                                       Where(c => c.Name != "Spectator"))
                        CSs.Add(obj.Name, _css2);

                    /* Что мы делаем:
                       создаём словарь матриц изменения координат точек объекта 
                       {
                         ключ = название объекта, для которого проводятся изменения;
                         значение = словарь 
                         {
                           ключ = координатная система, для которой меняем
                           координаты точек объекта;
                           значение = матрица изменения координат
                           в данной координатной системе.
                         }
                       } */

                    Dictionary<string, Dictionary<string, double[,]>>
                        UpdateMatrices = new
                        Dictionary<string, Dictionary<string, double[,]>>();

                    UpdateMatrices.Add("Stage",
                                       new Dictionary<string, double[,]>
                                       {
                                           /*
                                           {
                                             Scene.CoordinateSystems["VCS"],
                                             getRotationMatrix(_pyramidMatrix)
                                           }
                                           */
                                       });
                    /*UpdateMatrices.Add(Spectator.Name,
                                       new Dictionary<string, double[,]>
                                       {/*
                                           {
                                             Scene.CoordinateSystems["VCS"],
                                             getRotationMatrix(_pyramidMatrix)
                                           }
                                       */
                    //});

                    foreach (IObjectOnStage obj in Stage.Objects.Values.
                                       Where(c => c.Name != "Spectator"))
                    {
                        UpdateMatrices.Add(obj.Name,
                                         new Dictionary<string, double[,]>
                                         {
                                           {
                                             Scene.CoordinateSystems["WCS"],
                                             CalcMethods
                                             .getRotationMatrix(_pyramidMatrix)
                                           },
                                           {
                                             Scene.CoordinateSystems["VCS"],
                                             CalcMethods
                                             .getRotationMatrix(_pyramidMatrix)
                                           }
                                         });
                    }

                    while (animState == AnimationsStates.Stopped)
                        ;

                    animState = AnimationsStates.Started;
                    FrameSynthesis(CSs, UpdateMatrices);
                    animState = AnimationsStates.Ended;

                    PrintCalculations();

                    Thread.Sleep(interval / counterOfSecondsOfMoveToOneSide);
                }
            }
        }

        /* Метод FrameSynthesis выполняет синтез кадра.
           На вход принимает:
           -- словарь имя_объекта-список_координатных_систем_в_которых_
              _будут_проводиться_изменения;
           -- словарь имя_объекта_список_матриц_изменения_координат_
              _в_соответствии_с_координатной_системой. */

        async public void FrameSynthesis(Dictionary<string, List<string>> CSs,
            Dictionary<string, Dictionary<string, double[,]>> UpdateMatrices)
        {
            /* Блок, в котором изменяются координаты точек:
               -- сцены;
               -- объектов на сцене, включая наблюдателя. */

            Stage.UpdateLocationPoint(UpdateMatrices["Stage"]);

            foreach (IObjectOnStage obj in Stage.Objects.Values)
            {
                //////////////////////////!!!!!!!!!!!!!!
                if (!obj.Name.Equals("Spectator"))
                    obj.UpdatePoints('a', UpdateMatrices[obj.Name]);
            }

            Spectator.VW_Update(grStage);
        }

        /* Управление камерой: изменение угла поворота, 
           расстояния наблюдателя от экрана. */

        public void Observer_KeyDown(Keys keyData)
        {
            float deltaX = 0, deltaY = 0, deltaZ = 0;

            bool keyIsCorrect = false,
                 moveIsRotation = false,
                 moveIsTransition = false;

            /* D1 : поворот по оси X;
               D2 : поворот по оси Y;
               D3 : поворот по оси Z;
               Up : движение камеры по оси Z вперёд (ближе к экрану); 
               Down : движение камеры по оси Z назад (дальше от экрана). */

            switch (keyData)
            {
                case Keys.D1:
                    {
                        Spectator.Rotations.Add(new Dictionary<char, double>
                                                {
                                                 { 'X', 10}
                                                });

                        keyIsCorrect = true;
                        moveIsRotation = true;

                        break;
                    }
                case Keys.D2:
                    {
                        Spectator.Rotations.Add(new Dictionary<char, double>
                                                {
                                                 { 'X', -10}
                                                });

                        keyIsCorrect = true;
                        moveIsRotation = true;

                        break;
                    }
                case Keys.D3:
                    {
                        Spectator.Rotations.Add(new Dictionary<char, double>
                                                {
                                                 { 'Y', 10}
                                                });

                        keyIsCorrect = true;
                        moveIsRotation = true;

                        break;
                    }
                case Keys.D4:
                    {
                        Spectator.Rotations.Add(new Dictionary<char, double>
                                                {
                                                 { 'Y', -10}
                                                });

                        keyIsCorrect = true;
                        moveIsRotation = true;

                        break;
                    }
                case Keys.D5:
                    {
                        Spectator.Rotations.Add(new Dictionary<char, double>
                                                {
                                                 { 'Z', 10}
                                                });

                        keyIsCorrect = true;
                        moveIsRotation = true;

                        break;
                    }
                case Keys.D6:
                    {
                        Spectator.Rotations.Add(new Dictionary<char, double>
                                                {
                                                 { 'Z', -10}
                                                });

                        keyIsCorrect = true;
                        moveIsRotation = true;

                        break;
                    }
                case Keys.Left:
                    {
                        deltaX = 10;

                        SaveTransitObserver('X', deltaX);

                        break;
                    }
                case Keys.Right:
                    {
                        deltaX = -10;

                        SaveTransitObserver('X', deltaX);

                        break;
                    }
                case Keys.Up:
                    {
                        deltaY = -10;

                        SaveTransitObserver('Y', deltaY);

                        break;
                    }
                case Keys.Down:
                    {
                        deltaY = 10;

                        SaveTransitObserver('Y', deltaY);

                        break;
                    }
                case Keys.Add:
                    {
                        /* Дельта указывается относительно камеры. */

                        deltaZ = -10;

                        if (Spectator.d > Math.Abs(deltaZ))
                        {
                            Spectator.d += deltaZ;
                            TransitObserver('Z', deltaZ,
                                new List<string> { "WCS", "VCS" });

                            keyIsCorrect = true;
                            moveIsTransition = true;
                        }

                        break;
                    }
                case Keys.Subtract:
                    {
                        /* Дельта указывается относительно камеры. */

                        deltaZ = 10;

                        Spectator.d += deltaZ;

                        SaveTransitObserver('Z', deltaZ);

                        break;
                    }
            }

            if (keyIsCorrect)
            {
                if (moveIsRotation)
                {
                    /* Обновление матрицы приведения к видовым координатам
                       в соответствии с новым поворотом наблюдателя и камеры. */

                    Screen.UpdateVCS_Matrix();

                    /* Инициализация точки расположения камеры в пространстве. */
                    
                    Spectator.UpdatePoints(' ',
                        new Dictionary<string, double[,]>
                        {
                            {
                                "WCS",
                                CalcMethods.GetTransposedMatrix(Screen.VCS_Matrix)
                            }
                        });
                        
                }
                else
                {
                    if (moveIsTransition)
                    {
                        // Авось ещё что-нибудь добавить надо будет.
                        ;
                    }
                }

                while (true)
                {
                    if (animState == AnimationsStates.None ||
                       animState == AnimationsStates.Ended ||
                       animState == AnimationsStates.Stopped)
                        break;
                }

                animState = AnimationsStates.Stopped;
                Stage.UpdateObjectsLocationsByCameraView();
         
                Spectator.VW_Update(grStage);
                animState = AnimationsStates.None;

                PrintCalculations();
            }

            /* Локальный метод TransitObserver выполняет перемещение
               камеры для необходимых координатных систем. */

            void TransitObserver(char axe, float delta, List<string> CSs)
            {
                Dictionary<string, double[,]> UpdateMatrices =
                    new Dictionary<string, double[,]>();

                foreach (string CS in CSs)
                {
                    double[,] UpdateMatrix = new double[4, 4],
                              TR;

                    switch (CS)
                    {
                        case "WCS":
                            {
                                TR = CalcMethods.getTransitMatrix(
                                  new List<Dictionary<char, double>>
                                  {
                                      new Dictionary<char, double>
                                      {
                                          { axe, -delta }
                                      }
                                  });

                                double[,] RT = CalcMethods
                                    .GetTransposedMatrix(Screen.VCS_Matrix);

                                CalcMethods.multiplyingMatrices(
                                    ref UpdateMatrix,
                                    new List<double[,]>
                                    { TR, RT }
                                    );

                                break;
                            }
                        case "VCS":
                            {
                                /* Для VCS менять можно лишь расстояние по оси Z. */

                                TR = CalcMethods.getTransitMatrix(
                                  new List<Dictionary<char, double>>
                                  {
                                        new Dictionary<char, double>
                                        {
                                            { 'Z', -delta }
                                        }
                                  });

                                UpdateMatrix = TR;

                                break;
                            }
                    }

                    UpdateMatrices.Add(CS, UpdateMatrix);
                }

                Spectator.UpdatePoints(' ', UpdateMatrices);
            }

            void SaveTransitObserver(char axe, float delta)
            {
                /* Сохраняем старые координаты наблюдателя в МСК. */

                Point3D oldLocationPoint = new Point3D
                {
                    X = Spectator.LocationPoint["WCS"].X,
                    Y = Spectator.LocationPoint["WCS"].Y,
                    Z = Spectator.LocationPoint["WCS"].Z,
                };

                TransitObserver(axe, delta, new List<string> { "WCS" });

                if (Spectator.LocationPoint["WCS"].X > Stage.Xmax ||
                    Spectator.LocationPoint["WCS"].Y > Stage.Ymax ||
                    Spectator.LocationPoint["WCS"].Z > Stage.Zmax)
                {
                    Spectator.d -= deltaZ;
                    Spectator.LocationPoint["WCS"] = oldLocationPoint;
                }
                else
                {
                    if (axe == 'Z')
                        TransitObserver(axe, delta, new List<string> { "VCS" });
                    keyIsCorrect = true;
                    moveIsTransition = true;
                }
            }
        }
    }
}