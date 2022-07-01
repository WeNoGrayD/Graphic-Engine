using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace GraphicClassLibrary
{
    /* Класс графических методов. */

    public class GraphicMethods
    {
        /* Отрисовка линии методом Люка. */

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

        /* Отрисовка линии методом Брезенхэма. */

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
    }
}
