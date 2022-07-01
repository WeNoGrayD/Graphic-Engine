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

namespace lab1
{
    public partial class DrawCircles : Form
    {
        Graphics canvas;

        int canvasWidth, canvasHeight;

        public DrawCircles()
        {
            InitializeComponent();

            this.Shown += DrawCircles_Shown;

            canvas = Graphics.FromHwnd(this.Handle);
        }

        public void DrawCircles_Shown(object sender, EventArgs e)
        {
            Timer t = new Timer();
            t.Interval = 5000;
            t.Start();

            canvas = this.CreateGraphics();

            canvas.SmoothingMode = SmoothingMode.AntiAlias;
            
            canvasWidth = this.Width;
            canvasHeight = this.Height;

            Point Center = new Point(canvasWidth / 2, canvasHeight / 2 - 20),
                  UpLeftCorner = new Point(0, 0);

            canvas.DrawImage(DrawCircle_Bresenham(Center, 40, Color.Red),
                             UpLeftCorner);
            canvas.DrawImage(DrawCircle_Bresenham(Center, 72, Color.HotPink),
                             UpLeftCorner);
            canvas.DrawImage(DrawCircle_Bresenham(Center, 144, Color.Chocolate),
                             UpLeftCorner);
        }

        public Bitmap DrawCircle_Bresenham(Point Center, int R, Color col)
        {
            Bitmap bitmap = new Bitmap(canvasWidth, canvasHeight);

            int cX = Center.X,
                cY = Center.Y,
                CurrentX = 0,
                CurrentY = 2 * R,
                d = 0,
                Dl = 0,
                Delta = 1 - 2 * R;
            
            void DrawQuart()
            {
                bitmap.SetPixel(cX + CurrentX, cY + CurrentY, col);
                bitmap.SetPixel(cX - CurrentX, cY + CurrentY, col);
                bitmap.SetPixel(cX + CurrentX, cY - CurrentY, col);
                bitmap.SetPixel(cX - CurrentX, cY - CurrentY, col);
            }

            DrawQuart();

            CurrentX = 1;
            CurrentY = 2 * R - 1;
            Dl = CurrentY - 1;
            while (CurrentY >= 0)
            {
                d = Dl * 2;
                if (d < 0)
                {
                    d += CurrentX;
                    CurrentY -= 2;
                    Dl += CurrentY;
                    if (d >= 0)
                    {
                        CurrentX += 2;
                        Dl -= CurrentX;
                    }
                }
                else
                {
                    d -= CurrentY;
                    CurrentX += 2;
                    Dl -= CurrentX;
                    if (d < 0)
                    {
                        CurrentY -= 2;
                        Dl += CurrentY;
                    }
                }

                DrawQuart();
            }

            return bitmap;
        }
    }
}
