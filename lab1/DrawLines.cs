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
    public partial class DrawLines : Form
    {
        Graphics canvas;

        List<Bitmap> bitmaps = new List<Bitmap>();

        int matrixWidth, matrixHeight,
            canvasWidth, canvasHeight;

        public DrawLines()
        {
            InitializeComponent();

            this.Shown += DrawLines_Shown;
            
            canvas = Graphics.FromHwnd(this.Handle);

            canvasWidth = this.Width;
            canvasHeight = this.Height;
        }

        public void DrawLines_Shown(object sender, EventArgs e)
        {
            Timer t = new Timer();
            t.Interval = 20000;
            t.Start();

            canvas = this.CreateGraphics();

            canvas.SmoothingMode = SmoothingMode.AntiAlias;

            canvas.Clear(Color.White);

            matrixWidth = 50;
            matrixHeight = 80;

            canvas.DrawImage(DrawLetter('и'), 165, 40);

            canvas.DrawImage(DrawLetter('н'), 265, 40);
        }

        public Bitmap DrawLetter(char Letter)
        {
            Bitmap bitmap = new Bitmap(matrixWidth, matrixHeight, canvas);

            List<Point[]> Lines = new List<Point[]>();

            switch (Letter)
            {
                case 'и':
                    {
                        Lines.Add(new Point[]{new Point(1, 1),
                                              new Point(1, 78)});
                        Lines.Add(new Point[]{new Point(1, 78),
                                              new Point(48, 1)});
                        Lines.Add(new Point[]{new Point(48, 1),
                                              new Point(48, 78)});

                        DrawLine_Bresenham(bitmap,
                                           Lines[0][0], Lines[0][1],
                                           Color.HotPink);
                        DrawLine_Bresenham(bitmap,
                                           Lines[1][0], Lines[1][1],
                                           Color.HotPink);
                        DrawLine_Bresenham(bitmap,
                                           Lines[2][0], Lines[2][1],
                                           Color.HotPink);
                        break;
                    }
                case 'н':
                    {
                        Lines.Add(new Point[]{new Point(1, 1),
                                              new Point(1, 78)});
                        Lines.Add(new Point[]{new Point(1, 38),
                                              new Point(48, 38)});
                        Lines.Add(new Point[]{new Point(48, 1),
                                              new Point(48, 78)});

                        DrawLine_Luke(bitmap,
                                           Lines[0][0], Lines[0][1]);
                        DrawLine_Luke(bitmap,
                                           Lines[1][0], Lines[1][1]);
                        DrawLine_Luke(bitmap,
                                           Lines[2][0], Lines[2][1]);
                        break;
                    }
            }

            bitmap.Tag = Lines;
            bitmaps.Add(bitmap);

            return bitmap;
        }

        static public void DrawLine_Luke(Bitmap bitmap, Point start, Point finish)
        {
            Color col = Color.Chocolate;

            int currentX = start.X,
                currentY = start.Y,
                finishX = finish.X,
                finishY = finish.Y,
                dX = Math.Abs(currentX - finishX),
                dY = Math.Abs(currentY - finishY),
                incX = 0,
                incY = 0,
                Cumul = 0;

            if (currentX < finishX)
                incX = 1;
            else
                incX = -1;

            if (currentY < finishY)
                incY = 1;
            else
                incY = -1;

            bitmap.SetPixel(currentX, currentY, col);

            if (dX > dY)
            {
                Cumul = dX / 2;

                for (int i = 0; i < dX; i++)
                {
                    currentX += incX;
                    Cumul += dY;

                    if (Cumul >= dX)
                    {
                        currentY += incY;
                        Cumul -= dX;
                    }

                    bitmap.SetPixel(currentX, currentY, col);
                }
            }

            else
            {
                Cumul = dY / 2;

                for (int i = 0; i < dY; i++)
                {
                    currentY += incY;
                    Cumul += dX;

                    if (Cumul >= dY)
                    {
                        currentX += incX;
                        Cumul -= dY;
                    }

                    bitmap.SetPixel(currentX, currentY, col);
                }
            }
        }

        static public void DrawLine_Bresenham(Bitmap bitmap, 
                                              Point start,
                                              Point finish,
                                              Color Col)
        {
            int currentX = start.X,
                currentY = start.Y,
                finishX = finish.X,
                finishY = finish.Y,
                dX = Math.Abs(currentX - finishX),
                dY = Math.Abs(currentY - finishY),
                dX2 = dX * 2,
                dY2 = dY * 2,
                dXY,
                incX = 0, 
                incY = 0,
                Err = 0;

            if (currentX < finishX)
                incX = 1;
            else
                incX = -1;

            if (currentY < finishY)
                incY = 1;
            else
                incY = -1;

            SetPixel();

            if (dX > dY)
            {
                Err = dY2 - dX;
                dXY = dY2 - dX2;

                for (int i = 0; i < dX; i++)
                {
                    if (Err >= 0)
                    {
                        currentY += incY;
                        Err += dXY;
                    }

                    else
                        Err += dY2;

                    currentX += incX;

                    SetPixel();                
                }
            }

            else
            {
                Err = dX2 - dY;
                dXY = dX2 - dY2;

                for (int i = 0; i < dY; i++)
                {
                    if (Err >= 0)
                    {
                        currentX += incX;
                        Err += dXY;
                    }

                    else
                        Err += dX2;

                    currentY += incY;

                    SetPixel();
                }
            }

            void SetPixel()
            {
                /*
                if (currentX >= 0 &&
                    currentX < bitmap.Width &&
                    currentY > 0 &&
                    currentY < bitmap.Height) */
                finishY = finishY;
                finishX = finishX;
                int a = start.X;
                    bitmap.SetPixel(currentX, currentY, Col);
            }
        }

        public Bitmap LetterRotation(char Letter)
        {
            Bitmap oldBitmap, newBitmap = new Bitmap(1, 1);

            List<Point[]> Lines = new List<Point[]>();

            Point[] line;

            Point upLeftCorner = new Point(1, 1), startPoint, attachmentPoint;

            switch (Letter)
            {
                case 'и':
                    {
                        oldBitmap = bitmaps[0];

                        Lines = (List<Point[]>)oldBitmap.Tag;

                        /* Скрепление линий в одну букву.*/

                        line = Lines[0];
                        RotateLine(ref line, 1, Math.PI / 3);
                        TransferLine(ref line, upLeftCorner, line[0]);
                        attachmentPoint = line[1];

                        line = Lines[1];
                        RotateLine(ref line, 0, Math.PI / 3);
                        TransferLine(ref line, attachmentPoint, line[0]);
                        attachmentPoint = line[1];

                        line = Lines[2];
                        RotateLine(ref line, 1, Math.PI / 3);
                        TransferLine(ref line, attachmentPoint, line[0]);

                        /* Перемещение буквы по частям. */

                        int minX = Lines.Min(ps => ps.Min(p => p.X)),
                            minY = Lines.Min(ps => ps.Min(p => p.Y)),
                            maxX = Lines.Max(ps => ps.Max(p => p.X)),
                            maxY = Lines.Max(ps => ps.Max(p => p.Y)),
                            sumX = Math.Abs(minX - maxX),
                            sumY = Math.Abs(minY - maxY),
                            startMinX = Lines[0].Min(p => p.X),
                            startMinY = Lines[0].Min(p => p.Y);

                        line = Lines[0];
                        startPoint = new Point(1 + Math.Abs(minX - line[0].X),
                                               1 + Math.Abs(minY - line[0].Y));

                        TransferLine(ref line, startPoint, line[0]);
                        attachmentPoint = line[1];

                        line = Lines[1];
                        TransferLine(ref line, attachmentPoint, line[0]);
                        attachmentPoint = line[1];

                        line = Lines[2];
                        TransferLine(ref line, attachmentPoint, line[0]);

                        newBitmap = new Bitmap(sumX + 2, sumY + 2);

                        DrawLine_Bresenham(newBitmap,
                                           Lines[0][0], Lines[0][1], Color.HotPink);
                        DrawLine_Bresenham(newBitmap,
                                           Lines[1][0], Lines[1][1], Color.HotPink);
                        DrawLine_Bresenham(newBitmap,
                                           Lines[2][0], Lines[2][1], Color.HotPink);
                        break;
                    }
                case 'н':
                    {
                        oldBitmap = bitmaps[1];

                        Lines = (List<Point[]>)oldBitmap.Tag;

                        /* Скрепление линий в одну букву.*/

                        line = Lines[0];
                        RotateLine(ref line, 1, -Math.PI / 4);
                        TransferLine(ref line, upLeftCorner, line[0]);
                        attachmentPoint = new Point(
                            Math.Abs(line[1].X - line[0].X) / 2,
                            Math.Abs(line[1].Y - line[0].Y) / 2);
                    
                        line = Lines[1];
                        RotateLine(ref line, 0, -Math.PI / 4);
                        TransferLine(ref line, attachmentPoint, line[0]);

                        line = Lines[2];
                        RotateLine(ref line, 1, -Math.PI / 4);

                        line = Lines[1];
                        attachmentPoint.X += Math.Abs(line[1].X - line[0].X);
                        attachmentPoint.X -= Math.Abs(Lines[2][1].X - Lines[2][0].X) / 2;
                        attachmentPoint.Y -= Math.Abs(line[1].Y - line[0].Y);
                        attachmentPoint.Y -= Math.Abs(Lines[2][1].Y - Lines[2][0].Y) / 2;

                        line = Lines[2];
                        TransferLine(ref line, attachmentPoint, line[0]);

                        /* Перемещение буквы по частям. */

                        int minX = Lines.Min(ps => ps.Min(p => p.X)),
                            minY = Lines.Min(ps => ps.Min(p => p.Y)),
                            maxX = Lines.Max(ps => ps.Max(p => p.X)),
                            maxY = Lines.Max(ps => ps.Max(p => p.Y)),
                            sumX = Math.Abs(minX - maxX),
                            sumY = Math.Abs(minY - maxY),
                            startMinX = Lines[0].Min(p => p.X),
                            startMinY = Lines[0].Min(p => p.Y);

                        line = Lines[0];
                        startPoint = new Point(1 + Math.Abs(minX - line[0].X),
                                               1 + Math.Abs(minY - line[0].Y));

                        TransferLine(ref line, startPoint, line[0]);
                        attachmentPoint = new Point(
                            startPoint.X + Math.Abs(Math.Abs(line[1].X) - Math.Abs(line[0].X)) / 2,
                            startPoint.Y + Math.Abs(Math.Abs(line[1].Y) - Math.Abs(line[0].Y)) / 2);

                        line = Lines[1];
                        TransferLine(ref line, attachmentPoint, line[0]);
                        attachmentPoint.X += Math.Abs(line[1].X - line[0].X);
                        attachmentPoint.X -= Math.Abs(Lines[2][1].X - Lines[2][0].X) / 2;
                        attachmentPoint.Y -= Math.Abs(line[1].Y - line[0].Y);
                        attachmentPoint.Y -= Math.Abs(Lines[2][1].Y - Lines[2][0].Y) / 2;

                        line = Lines[2];
                        TransferLine(ref line, attachmentPoint, line[0]);

                        newBitmap = new Bitmap(sumX * 2 + 2, sumY * 2 + 2);

                        DrawLine_Luke(newBitmap,
                                           Lines[0][0], Lines[0][1]);
                        DrawLine_Luke(newBitmap,
                                           Lines[1][0], Lines[1][1]);
                        DrawLine_Luke(newBitmap,
                                           Lines[2][0], Lines[2][1]);
                        break;
                    }
            }

            return newBitmap;
        }

        public void RotateLine (ref Point[] line, int pivotPoint, double phi)
        {
            int dX, dY, notAttachedPoint = 0;

            switch (pivotPoint)
            {
                case 0:
                    { notAttachedPoint = 1; break; }
                case 1:
                    { notAttachedPoint = 0; break; }
            }

            double phiCos = Math.Cos(phi),
                   phiSin = Math.Sin(phi);

            /* Т.к. координаты на холсте отзеркалены (значения по оси ординат
               идут вниз; если перевернуть картинку, то положительные значения
               по оси абсцисс будут идти влево, а не вправо), то разницам
               в координатах следует поставить противоположные знаки. */

            dX = -(line[pivotPoint].X - line[notAttachedPoint].X);
            dY = -(line[pivotPoint].Y - line[notAttachedPoint].Y);

            double hipo = Math.Sqrt(dX * dX + dY * dY), 
                   gammaCos = dX / hipo,
                   gamma = Math.Acos(gammaCos),
                   psi = gamma + phi,
                   psiCos = Math.Cos(psi),
                   psiSin = Math.Sin(psi);
            
            line[notAttachedPoint].X = line[pivotPoint].X;
            line[notAttachedPoint].X += (int)((dX * phiCos - dY * phiSin));
            line[notAttachedPoint].Y = line[pivotPoint].Y;
            line[notAttachedPoint].Y += (int)((dX * phiSin + dY * phiCos));
        }

        public void TransferLine(ref Point[] line, Point attachmentPoint, Point anchoredPoint)
        {
            int dX = attachmentPoint.X - anchoredPoint.X,
                dY = attachmentPoint.Y - anchoredPoint.Y;

            line[0].X += dX;
            line[0].Y += dY;
            line[1].X += dX;
            line[1].Y += dY;
        }

        private void btnAffines_Click(object sender, EventArgs e)
        {
            canvas.Clear(Color.White);

            canvas.DrawImage(LetterRotation('и'), 115, 40);
            canvas.DrawImage(LetterRotation('н'), 275, 40);
        }
    }
}
