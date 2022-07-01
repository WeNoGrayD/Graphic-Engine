using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web.UI.DataVisualization.Charting;

namespace lab2
{
    public partial class fEnlighten : Form
    {
        public SkyBox StarrySky;

        public Observer Stargazer;

        public ViewingWindow Screen;

        public List<LightSource> Stars;

        static public Graphics grSky;

        /* Конструктор формы fEnlighten. */

        public fEnlighten()
        {
            InitializeComponent();

            this.Screen = new ViewingWindow(new RectangleF
                (
                 new Point(0, 0),
                 new SizeF(1.5F, 1.5F)
                ),
                 new RectangleF
                (
                 new Point(0, 0),
                 new SizeF(this.Width, this.Height)
                ));

            Stargazer = new Observer(500, 500, 500, 
                                     new Dictionary<char, double>
                                     {
                                      { 'Y', 0},
                                      { 'Z', 0},
                                      { 'X', 0}
                                     },
                                     1,
                                     this.Screen);

            Stars = new List<LightSource>();

            StarrySky = new SkyBox(Stargazer, Stars);

            Stars_Init(4);

            double[,] skyMatrix = getTransitMatrix(new List<Dictionary<char, double>>
                             {
                              new Dictionary<char, double>
                              {
                                { 'X', -Stargazer.LocationWCS.X },
                                { 'Y', -Stargazer.LocationWCS.Y },
                                { 'Z', -Stargazer.LocationWCS.Z }
                              }
                             });

            List<double[,]> lstStarsMatrices = new List<double[,]>();
            foreach(LightSource star in Stars)
            {
                lstStarsMatrices.Add(skyMatrix);
            }

            /* Для изменения координат всех объектов, которые не являются
               наблюдателем и экраном, формируются общий список 
               этих объектов и общий список их матриц сдвига. */

            List<object> lstObjects = new List<object>();
            lstObjects.Add(StarrySky);
            lstObjects.AddRange(Stars);

            List<double[,]> lstOCSMatrices = new List<double[,]>();
            lstOCSMatrices.Add(skyMatrix);
            lstOCSMatrices.AddRange(lstStarsMatrices);

            // Инициализация координат объектов в СКН.

            updateCS(lstObjects, lstOCSMatrices);

            rotateCamera(ref skyMatrix, ref lstStarsMatrices);

            // Изменение матрицы сдвига объектов.

            lstOCSMatrices = new List<double[,]>();
            lstOCSMatrices.Add(skyMatrix);
            lstOCSMatrices.AddRange(lstStarsMatrices);

            // Изменение координат объектов в СКН после вращения камеры.

            updateCS(lstObjects, lstOCSMatrices);

            this.KeyDown += Observer_KeyDown;

            this.Shown += fEnlighten_Shown;
        }

        /* Метод fEnlighten_Shown инициализирует новый холст,
           связанный с дисплеем устройства, и запускает
           обновление (иницализацию в данном случае) экрана камеры. */

        public void fEnlighten_Shown(object sender, EventArgs e)
        {
            Timer t = new Timer();
            t.Interval = 20000;
            t.Start();

            grSky = this.CreateGraphics();
            grSky.SmoothingMode = SmoothingMode.AntiAlias;

            Stargazer.VW_Update();
        }

        public void Stars_Init(int starsCount)
        {
            Random rX = new Random(78243823);
            Random rY = new Random(98347934);
            Random rZ = new Random(290765147);
            Random rRGB = new Random(0);

            for (int i = 0; i < starsCount; i++)
            {
                int X = (int)(rX.NextDouble() * StarrySky.Xmax),
                    Y = (int)(rY.NextDouble() * StarrySky.Ymax),
                    Z = (int)(rZ.NextDouble() * StarrySky.Zmax);

                byte[] RGB = new byte[3];
                rRGB.NextBytes(RGB);

                Color Col = Color.FromArgb(RGB[0], RGB[1], RGB[2]);

                Stars.Add(new LightSource(X, Y, Z, Col));
            }
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

            multiplyingMatrices(ref TRoffsets, ref TRs);

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
                {
                    dictAxesAngles.Add( 'Y', dAA['Y'] );

                    if (dAA.ContainsKey('Z'))
                    {
                        dictAxesAngles.Add('Z', dAA['Z']);
                        if (dAA.ContainsKey('X'))
                            dictAxesAngles.Add('X', dAA['X']);
                    }
                    else
                    {
                        if (dAA.ContainsKey('X'))
                            dictAxesAngles.Add('X', dAA['X']);
                    }
                }
                else
                {
                    if (dAA.ContainsKey('Z'))
                    {
                        dictAxesAngles.Add('Z', dAA['Z']);

                        if (dAA.ContainsKey('X'))
                            dictAxesAngles.Add('X', dAA['X']);
                    }
                    else
                    {
                        if (dAA.ContainsKey('X'))
                            dictAxesAngles.Add('X', dAA['X']);
                    }
                }

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

            multiplyingMatrices(ref RTangles, ref RTs);

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

            multiplyingMatrices(ref MSscales, ref MSs);

            return MSscales;
        }

        /* Метод multiplyingMatrices перемножает список матриц (1<n<4)х4 lstMatrices
           и выдаёт на выходе результирующую матрицу resultMatrix. */

        static public void multiplyingMatrices(ref double[,] resultMatrix,
                                        ref List<double[,]> lstMatrices)
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

        /* Приведение координат небесной коробки и звёзд к СКН. */

        public void updateCS(List<object> lstObjects, List<double[,]> lstCSMatrices)
        {
            object o;
            Point3D Location = new Point3D();

            for (int i = 0; i < lstCSMatrices.Count; i++)
            {
                o = lstObjects[i];
                if (o is SkyBox)
                    Location = ((SkyBox)o).LocationOCS;
                if (o is LightSource)
                    Location = ((LightSource)o).LocationOCS;
                if (o is Observer)
                    Location = ((Observer)o).LocationWCS;
                CalculateLocation_Transit(lstCSMatrices[i]);
            }

            return;

            /* Вычисление новой точки положения объекта в СКН. */

            void CalculateLocation_Transit(double[,] TMatrix)
            {
                double[] locationMatrix = new double[4];

                double[] startLocationMatrix = new double[]
                { Location.X, Location.Y, Location.Z, 1 };

                // Проход по столбцам locationMatrix.

                for (int i = 0; i < 4; i++)
                {
                    locationMatrix[i] = 0;

                    // Ещё один проход.

                    for (int j = 0; j < 4; j++)
                    {
                        locationMatrix[i] += startLocationMatrix[j] * TMatrix[j, i];
                    }
                }

                Location.X = (float)locationMatrix[0];
                Location.Y = (float)locationMatrix[1];
                Location.Z = (float)locationMatrix[2];
            }
        }

        /* Управление камерой: изменение угла поворота, 
           расстояния наблюдателя от экрана. */

        public void Observer_KeyDown(object sender, KeyEventArgs e)
        {
            float delta = 0;

            bool isCorrectKey = false,
                 isRotation = false, 
                 isPermissedTransition = false;

            /* D1 : поворот по оси X;
               D2 : поворот по оси Y;
               D3 : поворот по оси Z;
               Up : движение камеры по оси Z вперёд (ближе к экрану); 
               Down : движение камеры по оси Z назад (дальше от экрана). */

            switch (e.KeyCode)
            {
                case Keys.D1:
                    {
                        Stargazer.Rotations.Add(new Dictionary<char, double>
                                                {
                                                 { 'X', 10}
                                                });

                        isCorrectKey = true;
                        isRotation = true;

                        break;
                    }
                case Keys.D2:
                    {
                        Stargazer.Rotations.Add(new Dictionary<char, double>
                                                {
                                                 { 'Y', 10}
                                                });

                        isCorrectKey = true;
                        isRotation = true;

                        break;
                    }
                case Keys.D3:
                    {
                        Stargazer.Rotations.Add(new Dictionary<char, double>
                                                {
                                                 { 'Z', 10}
                                                });

                        isCorrectKey = true;
                        isRotation = true;

                        break;
                    }
                case Keys.Up:
                    {
                        delta = -10;//-0.2F;

                        if (Stargazer.d >= Math.Abs(delta))
                        {
                            isCorrectKey = true;
                            isPermissedTransition = true;
                        }

                        break;
                    }
                case Keys.Down:
                    {
                        delta = 10;// 0.2F;
                        float X = Stargazer.LocationWCS.X,
                              Y = Stargazer.LocationWCS.Y,
                              Z = Stargazer.LocationWCS.Z;

                        if (X >= 0 && X < StarrySky.Xmax &&
                            Y >= 0 && Y < StarrySky.Ymax &&
                            Z >= 0 && Z < StarrySky.Zmax)
                        {
                            isCorrectKey = true;
                            isPermissedTransition = true;
                        }

                        break;
                    }
            }

            if (isCorrectKey)
            {
                double[,] skyMatrix = new double[4, 4],
                          stargazerMatrix = new double[4, 4];
                List<double[,]> lstStarsMatrices = new List<double[,]>();

                List<object> lstObjects = new List<object>();
                List<double[,]> lstCSMatrices = new List<double[,]>();

                if (isRotation)
                {
                    rotateCamera(ref skyMatrix, ref lstStarsMatrices);
                    lstObjects.Add(StarrySky);
                    lstObjects.AddRange(Stars);
                    lstCSMatrices.Add(skyMatrix);
                    lstCSMatrices.AddRange(lstStarsMatrices);
                }
                else
                {
                    if (isPermissedTransition)
                    {
                        Stargazer.d += delta;
                        Stargazer.Screen.LocationOCS.Z += delta;

                        double[,] matrix = getTransitMatrix(new List<Dictionary<char, double>>
                             {
                              new Dictionary<char, double>
                              {
                                { 'Z', delta }
                              }
                             });

                        skyMatrix = stargazerMatrix = matrix;

                        foreach (LightSource star in Stars)
                            lstStarsMatrices.Add(matrix);

                        lstObjects.Add(StarrySky);
                        lstObjects.Add(Stargazer);
                        lstObjects.AddRange(Stars);
                        lstCSMatrices.Add(skyMatrix);
                        lstCSMatrices.Add(stargazerMatrix);
                        lstCSMatrices.AddRange(lstStarsMatrices);
                    }
                }

                updateCS(lstObjects, lstCSMatrices);

                Stargazer.VW_Update();
            }
        }

        /* Поворот объектов вокруг наблюдателя. */

        public void rotateCamera(ref double[,] skyMatrix, ref List<double[,]> lstStarsMatrix)
        {
            skyMatrix = new double[4, 4];
            lstStarsMatrix = new List<double[,]>();

            double[,] TR1 = getTransitMatrix(new List<Dictionary<char, double>>
                             {
                              new Dictionary<char, double>
                              {
                                { 'X', -StarrySky.LocationOCS.X },
                                { 'Y', -StarrySky.LocationOCS.Y },
                                { 'Z', -StarrySky.LocationOCS.Z }
                              }
                             });

            double[,] RT = getRotationMatrix(new List<Dictionary<char, double>>
                                       {
                                        Stargazer.Rotations.Last()
                                       });

            double[,] TR2 = getTransitMatrix(new List<Dictionary<char, double>>
                            {
                             new Dictionary<char, double>
                             {
                               { 'X', StarrySky.LocationOCS.X },
                               { 'Y', StarrySky.LocationOCS.Y },
                               { 'Z', StarrySky.LocationOCS.Z }
                             }
                            });

            List<double[,]> lstSkyP = new List<double[,]> { /*TR1,*/ RT/*, TR2*/ };

            multiplyingMatrices(ref skyMatrix, ref lstSkyP);

            foreach (LightSource star in Stars)
            {
                double[,] matrix = new double[4, 4];

                TR1 = getTransitMatrix(new List<Dictionary<char, double>>
                             {
                              new Dictionary<char, double>
                              {
                                { 'X', -star.LocationOCS.X },
                                { 'Y', -star.LocationOCS.Y },
                                { 'Z', -star.LocationOCS.Z }
                              }
                             });

                RT = getRotationMatrix(new List<Dictionary<char, double>>
                                       {
                                        Stargazer.Rotations.Last()
                                       });

                TR2 = getTransitMatrix(new List<Dictionary<char, double>>
                            {
                             new Dictionary<char, double>
                             {
                               { 'X', star.LocationOCS.X },
                               { 'Y', star.LocationOCS.Y },
                               { 'Z', star.LocationOCS.Z }
                             }
                            });

                List<double[,]> lstStarP = new List<double[,]> { RT };

                multiplyingMatrices(ref matrix, ref lstStarP);

                lstStarsMatrix.Add((double[,])matrix.Clone());
            }
        }

        static public void grSky_DrawImage(Bitmap bm, PointF location)
        {
            grSky.DrawImage(bm, location);
        }
    }

    /* Класс наблюдателя. */

    public class Observer
    {
        public SkyBox StarrySky;

        public Point3D LocationWCS;

        public float d;

        public List<Dictionary<char, double>> Rotations;

        public ViewingWindow Screen;

        public List<LightSource> VisibleStars;

        public Observer(int X0, int Y0, int Z0,
                        Dictionary<char, double> startRotation,
                        int _d,
                        ViewingWindow screen)
        {
            LocationWCS = new Point3D(X0, Y0, Z0);

            Rotations = new List<Dictionary<char, double>>();
            Rotations.Add(startRotation);

            d = _d;

            Screen = screen;
            Screen.LocationOCS.Z = d;

            VisibleStars = new List<LightSource>();
        }

        /* Метод VW_Update запускает обновление экрана VW. */

        public void VW_Update()
        {
            fEnlighten.grSky.Clear(Color.Black);

            VisibleStars.Clear();

            foreach (LightSource star in StarrySky.Stars)
            {
                if (star.LocationOCS.Z > 0)
                {

                    double[] starLocationArray = new double[]
                    {
                        star.LocationOCS.X,
                        star.LocationOCS.Y,
                        star.LocationOCS.Z,
                        1
                    };

                    double[,] PR = getProjectiveMatrix(star.LocationOCS.Z);

                    getScreenXY(ref starLocationArray, PR);

                    PointF starLocation = new PointF
                    {
                        X = (float)starLocationArray[0],
                        Y = (float)starLocationArray[1]
                    };

                    /* Если спроецированные на экран координаты звезды
                       не вылезают за его пределы, то записываем её 
                       в обозреваемые наблюдателем и добавляем её на экран. */

                    if (Math.Abs(starLocationArray[0]) <= Screen.VWsmall.Width / 2 &&
                        Math.Abs(starLocationArray[1]) <= Screen.VWsmall.Height / 2)
                    {
                        VisibleStars.Add(star);

                        starLocationArray[0] += Screen.VWsmall.Width / 2;
                        starLocationArray[1] = -starLocation.Y + Screen.VWsmall.Height / 2;

                        double[,] MS = fEnlighten.getScalingMatrix(
                            new List<Dictionary<char, double>>
                            {
                             new Dictionary<char, double>
                             {
                                 { 'X', Screen.ScaleX },
                                 { 'Y', Screen.ScaleY }
                             }
                            });

                        getScreenXY(ref starLocationArray, MS);

                        starLocation = new PointF
                        {
                            X = (float)starLocationArray[0],
                            Y = (float)starLocationArray[1]
                        };

                        RectangleF starRect = 
                            new RectangleF(starLocation, new SizeF(5, 5));

                        /* Изменение интенсивности окраски звезды в зависимости 
                           от её расстояния до наблюдателя. */
                        /*
                     int coefZ = StarrySky.Zmax / 100,
                         RGBint = star.Col.ToArgb();
                     byte[] RGB = new byte[3];

                     RGB[0] = (byte)(RGBint - 25.5 * star.LocationOCS.Z / coefZ);
                     RGB[1] = (byte)((RGBint >> 8) - 25.5 * star.LocationOCS.Z / coefZ);
                     RGB[2] = (byte)((RGBint >> 16) - 25.5 * star.LocationOCS.Z / coefZ);

                     for (int i = 0; i < 3; i++)
                     {
                         if (RGB[i] <0)
                             RGB[i] = 0;
                     }

                     Color starCol = Color.FromArgb(RGB[0], RGB[1], RGB[2]); 
                     */

                        Color starCol = changeSaturateByDistance(star);
                        fEnlighten.grSky.FillEllipse(new SolidBrush(starCol),
                            starRect);
                    }
                }
            }

            void getScreenXY(ref double[] locationArray, double[,] TFMatrix)
            {
                double[] startLocationArray = (double[])locationArray.Clone();

                // Проход по столбцам locationArray.

                for (int i = 0; i < 4; i++)
                {
                    locationArray[i] = 0;

                    // Ещё один проход.

                    for (int j = 0; j < 4; j++)
                    {
                        locationArray[i] += startLocationArray[j] * TFMatrix[j, i];
                    }
                }
            }

            Color changeSaturateByDistance(object o)
            {
                LightSource star = (LightSource)o;

                int R = 0,
                    G = 0,
                    B = 0;

                float H = star.Col.GetHue(),
                      S = star.Col.GetSaturation(),
                      L = star.Col.GetBrightness(),
                      oX = star.LocationOCS.X,
                      oY = star.LocationOCS.Y,
                      oZ = star.LocationOCS.Z,
                      distance = 0,
                      sX = StarrySky.Xmax,
                      sY = StarrySky.Ymax,
                      sZ = StarrySky.Zmax,
                      skyDiagonal = 0;

                distance = (float)Math.Sqrt(oX * oX + oY * oY);
                distance = (float)Math.Sqrt(distance * distance + oZ * oZ);

                skyDiagonal = (float)Math.Sqrt(sX * sX + sY * sY);
                skyDiagonal = (float)Math.Sqrt(skyDiagonal * skyDiagonal + sZ * sZ);

                S -= distance / skyDiagonal;

                if (S < 0)
                    S = 0;

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

                return Color.FromArgb(R, G, B);
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
            PR[0, 0] = d / pointZ;
            PR[1, 1] = d / pointZ;
            PR[2, 2] = 1 / pointZ;
            PR[3, 2] = d / pointZ;
            return PR;
        }
    }

    public class ViewingWindow
    {
        public RectangleF VWsmall, VWbig;

        public double ScaleX, ScaleY;

        public Point3D LocationOCS = new Point3D(0, 0, 0);

        public ViewingWindow(RectangleF vw_small, RectangleF vw_big)
        {
            VWsmall = vw_small;
            VWbig = vw_big;
            ScaleX = VWbig.Width / VWsmall.Width;
            ScaleY = VWbig.Height / VWsmall.Height;
        }
    }

    /* Класс источника света. */

    public class LightSource : ICloneable
    {
        public Point3D LocationWCS, LocationOCS;

        public Color Col;

        public LightSource(int Xl, int Yl, int Zl, Color ColL)
        {
            LocationWCS = new Point3D(Xl, Yl, Zl);
            LocationOCS = new Point3D(Xl, Yl, Zl);
            Col = ColL;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    /* Класс небесной коробки. */

    public class SkyBox
    {
        public int Xmax, Ymax, Zmax;

        public Point3D LocationOCS = new Point3D();

        public Observer Stargazer;

        public List<LightSource> Stars;

        public SkyBox(Observer stargazer, List<LightSource> stars)
        {
            Xmax = Ymax = Zmax = 1000;

            Stargazer = stargazer;
            Stargazer.StarrySky = this;

            Stars = stars;
        }
    }
}
