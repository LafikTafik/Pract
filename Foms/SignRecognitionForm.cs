using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NAMI.Foms
{
    public partial class SignRecognitionForm : Form
    {
        private Mat originalImage = new Mat();

        public SignRecognitionForm()
        {
            InitializeComponent();
        }

        private void roundedButton3_Click(object sender, EventArgs e)
        {

            using (OpenFileDialog ofd = new OpenFileDialog())
            {

                ofd.Filter = "Image Files (*.jpg;*.png;*.bmp)|*.jpg;*.png;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    // Загружаем изображение
                    originalImage = CvInvoke.Imread(ofd.FileName, ImreadModes.Color);

                    // Проверяем, успешно ли загружено
                    if (originalImage.IsEmpty)
                    {
                        MessageBox.Show("Ошибка загрузки изображения.");
                        return;
                    }

                    // Отображаем изображение
                    picboxsign.Image = originalImage.ToBitmap();

                    // Распознаём дорожный знак
                    string signName = DetectTrafficSign(originalImage);
                    labelSign.Text = $"Дорожный знак: {signName}";
                }
            }
        }
        private string DetectTrafficSign(Mat image)
        {
            if (image.IsEmpty) return "Неизвестный";

            // Предварительная обработка
            Mat resized = new Mat();
            CvInvoke.Resize(image, resized, new Size(640, 640));

            Mat gray = new Mat();
            CvInvoke.CvtColor(resized, gray, ColorConversion.Bgr2Gray);
            CvInvoke.EqualizeHist(gray, gray);
            CvInvoke.GaussianBlur(gray, gray, new Size(5, 5), 0);

            Mat binary = new Mat();
            CvInvoke.Threshold(gray, binary, 150, 255, ThresholdType.BinaryInv);



            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(binary, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

            for (int i = 0; i < contours.Size; i++)
            {
                VectorOfPoint contour = contours[i];
                double perimeter = CvInvoke.ArcLength(contour, true);
                VectorOfPoint approx = new VectorOfPoint();
                CvInvoke.ApproxPolyDP(contour, approx, 0.04 * perimeter, true);
                Rectangle roi = CvInvoke.BoundingRectangle(approx);
                int corners = approx.Size;
                Rectangle rect = CvInvoke.BoundingRectangle(approx);

                Console.WriteLine($"Контур {i}: Углы={corners}, Ширина={rect.Width}, Высота={rect.Height}");

                if (rect.Width < 80 || rect.Height < 80)
                {
                    Console.WriteLine("Пропускаем: слишком маленький объект");
                    continue;
                }

                string shape = GetShape(corners, rect);
                string color = GetDominantColor(resized, rect);
                bool hasWhiteCenter = HasWhiteCenter(resized, rect, roi);
                bool hasRedBorder = HasRedBorder(resized, rect, roi);
                bool isSpeedLimitNumber = HasSpeedLimitDigit(resized, rect, roi);
                bool isInvertedTriangle = IsInvertedTriangle(approx);
                bool hasPedestrianPattern = HasPedestrianPattern(resized, rect, roi);

                Console.WriteLine($"Форма: {shape}, Цвет: {color}");
                Console.WriteLine($"Белый центр: {hasWhiteCenter}, Красная окантовка: {hasRedBorder}, Цифра: {isSpeedLimitNumber}, Пешеход: {hasPedestrianPattern}");

                // Стоп
                if (shape == "octagon" && color == "red")
                {
                    return "Стоп";
                }

                // Ограничение скорости
                if (shape == "circle" && hasRedBorder && hasWhiteCenter && isSpeedLimitNumber)
                {
                    return "Ограничение скорости";
                }

                // Уступите дорогу
                if (shape == "triangle" && color == "red" && hasWhiteCenter && isInvertedTriangle)
                {
                    return "Уступите дорогу";
                }

                // Пешеходный переход
                if (shape == "circle" && color == "blue" && hasPedestrianPattern)
                {
                    return "Пешеходный переход";
                }

                // Движение прямо
                if (shape == "square" && color == "blue" && HasStraightArrow(resized, rect))
                {
                    return "Движение прямо";
                }

                // Направление
                if (shape == "square" && color == "green" && HasDirectionArrow(resized, rect))
                {
                    return "Направление";
                }
            }

            return "Неизвестный знак";
        }

        private string GetShape(int corners, Rectangle rect)
        {
            double aspectRatio = (double)rect.Width / rect.Height;

            if (corners >= 8 && Math.Abs(aspectRatio - 1) < 0.2)
                return "octagon"; // Восьмиугольник

            if (corners == 3)
                return "triangle"; // Треугольник

            if (corners == 4 && Math.Abs(aspectRatio - 1) < 0.2)
                return "square"; // Квадрат

            if (corners == 4 && aspectRatio > 1.5)
                return "horizontal_rectangle"; // Горизонтальный прямоугольник

            if (corners == 4 && aspectRatio < 0.6)
                return "vertical_rectangle"; // Вертикальный прямоугольник

            if (corners > 6 && Math.Abs(aspectRatio - 1) < 0.1)
                return "circle"; // Круг

            return "unknown";
        }


        private void roundedButton1_Click(object sender, EventArgs e)
        {
            this.Close(); // или Hide(), если хотите сохранить состояние
        }

        private int CountNonZero(Mat mask)
        {
            VectorOfPoint points = new VectorOfPoint();
            CvInvoke.FindNonZero(mask, points);
            return points.Size;
        }

        private string GetDominantColor(Mat image, Rectangle roi)
        {
            // Проверка, чтобы ROI был внутри изображения
            if (roi.X < 0 || roi.Y < 0 ||
                roi.Width <= 0 || roi.Height <= 0 ||
                roi.X + roi.Width > image.Cols || roi.Y + roi.Height > image.Rows)
            {
                return "unknown";
            }

            using (Mat cropped = new Mat())
            {
                int top = roi.Y;
                int bottom = image.Rows - roi.Y - roi.Height;
                int left = roi.X;
                int right = image.Cols - roi.X - roi.Width;

                // Убедимся, что все значения положительные
                top = Math.Max(0, top);
                bottom = Math.Max(0, bottom);
                left = Math.Max(0, left);
                right = Math.Max(0, right);

                CvInvoke.CopyMakeBorder(image, cropped, top, bottom, left, right, BorderType.Constant, new MCvScalar(0));

                // Переводим в HSV для анализа цвета
                Mat hsv = new Mat();
                CvInvoke.CvtColor(cropped, hsv, ColorConversion.Bgr2Hsv);

                ScalarArray lowerRed = new ScalarArray(new MCvScalar(0, 100, 100));
                ScalarArray upperRed = new ScalarArray(new MCvScalar(10, 255, 255));
                ScalarArray lowerBlue = new ScalarArray(new MCvScalar(100, 150, 50));
                ScalarArray upperBlue = new ScalarArray(new MCvScalar(140, 255, 255));

                Mat maskRed = new Mat();
                CvInvoke.InRange(hsv, lowerRed, upperRed, maskRed);
                int redCount = CountNonZero(maskRed);

                Mat maskBlue = new Mat();
                CvInvoke.InRange(hsv, lowerBlue, upperBlue, maskBlue);
                int blueCount = CountNonZero(maskBlue);

                Mat grayMask = new Mat();
                CvInvoke.CvtColor(cropped, grayMask, ColorConversion.Bgr2Gray);
                CvInvoke.Threshold(grayMask, grayMask, 200, 255, ThresholdType.Binary);
                int whiteCount = CountNonZero(grayMask);

                if (redCount > blueCount && redCount > whiteCount)
                    return "red";

                if (blueCount > whiteCount)
                    return "blue";

                return "white";
            }
        }


        private bool IsInvertedTriangle(VectorOfPoint approx)
        {
            if (approx.Size != 3) return false;

            Point p1 = approx[0];
            Point p2 = approx[1];
            Point p3 = approx[2];

            int lowestY = Math.Max(p1.Y, Math.Max(p2.Y, p3.Y));

            return (p1.Y == lowestY || p2.Y == lowestY || p3.Y == lowestY);
        }

        private bool HasDirectionArrow(Mat image, Rectangle rect)
        {
            using (Mat cropped = new Mat())
            {

                CvInvoke.CopyMakeBorder(image, cropped, rect.Y, image.Rows - rect.Bottom, rect.X, image.Cols - rect.Width, BorderType.Constant, new MCvScalar(0));

                Mat gray = new Mat();
                CvInvoke.CvtColor(cropped, gray, ColorConversion.Bgr2Gray);
                CvInvoke.Threshold(gray, gray, 200, 255, ThresholdType.Binary);

                VectorOfVectorOfPoint arrowContours = new VectorOfVectorOfPoint();
                CvInvoke.FindContours(gray, arrowContours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

                for (int i = 0; i < arrowContours.Size; i++)
                {
                    VectorOfPoint approx = new VectorOfPoint();
                    double peri = CvInvoke.ArcLength(arrowContours[i], true);
                    CvInvoke.ApproxPolyDP(arrowContours[i], approx, 0.04 * peri, true);

                    if (approx.Size >= 5 && approx.Size <= 10)
                        return true;
                }

                return false;
            }
        }

        private bool HasStraightArrow(Mat image, Rectangle rect)
        {
            using (Mat cropped = new Mat())
            {
                CvInvoke.CopyMakeBorder(image, cropped, rect.Y, image.Rows - rect.Bottom, rect.X, image.Cols - rect.Width, BorderType.Constant, new MCvScalar(0));

                Mat gray = new Mat();
                CvInvoke.CvtColor(cropped, gray, ColorConversion.Bgr2Gray);
                CvInvoke.Threshold(gray, gray, 200, 255, ThresholdType.Binary);

                VectorOfVectorOfPoint arrowContours = new VectorOfVectorOfPoint();
                CvInvoke.FindContours(gray, arrowContours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

                for (int i = 0; i < arrowContours.Size; i++)
                {
                    VectorOfPoint approx = new VectorOfPoint();
                    double peri = CvInvoke.ArcLength(arrowContours[i], true);
                    CvInvoke.ApproxPolyDP(arrowContours[i], approx, 0.04 * peri, true);

                    // Стрелка вверх → около 6-8 углов
                    if (approx.Size >= 6 && approx.Size <= 9)
                        return true;
                }

                return false;
            }
        }

        private bool HasWhiteCenter(Mat image, Rectangle rect, Rectangle roi)
        {
            using (Mat cropped = new Mat())
            {

                int top = roi.Y;
                int bottom = image.Rows - roi.Y - roi.Height;
                int left = roi.X;
                int right = image.Cols - roi.X - roi.Width;

                if (top < 0 || bottom < 0 || left < 0 || right < 0)
                    return false;

                CvInvoke.CopyMakeBorder(image, cropped, top, bottom, left, right, BorderType.Constant, new MCvScalar(0));
                Mat hsv = new Mat();
                CvInvoke.CvtColor(cropped, hsv, ColorConversion.Bgr2Hsv);

                ScalarArray lowerWhite = new ScalarArray(new MCvScalar(0, 0, 200));
                ScalarArray upperWhite = new ScalarArray(new MCvScalar(180, 30, 255));
                Mat maskWhite = new Mat();
                CvInvoke.InRange(hsv, lowerWhite, upperWhite, maskWhite);

                int whiteCount = CountNonZero(maskWhite);
                double totalArea = roi.Width * roi.Height;

                return whiteCount / totalArea > 0.6;
            }
        }

        private bool HasRedBorder(Mat image, Rectangle rect, Rectangle roi)
        {
            using (Mat cropped = new Mat())
            {
                int top = roi.Y;
                int bottom = image.Rows - roi.Y - roi.Height;
                int left = roi.X;
                int right = image.Cols - roi.X - roi.Width;

                if (top < 0 || bottom < 0 || left < 0 || right < 0)
                    return false;

                CvInvoke.CopyMakeBorder(image, cropped, top, bottom, left, right, BorderType.Constant, new MCvScalar(0));

                Mat hsv = new Mat();
                CvInvoke.CvtColor(cropped, hsv, ColorConversion.Bgr2Hsv);

                ScalarArray lowerRed = new ScalarArray(new MCvScalar(0, 100, 100));
                ScalarArray upperRed = new ScalarArray(new MCvScalar(10, 255, 255));
                Mat maskRed = new Mat();
                CvInvoke.InRange(hsv, lowerRed, upperRed, maskRed);

                // Применяем морфологические операции
                CvInvoke.Dilate(maskRed, maskRed, null, new Point(1, 1), 1, BorderType.Constant, default);

                int redCount = CountNonZero(maskRed);
                double totalArea = roi.Width * roi.Height;

                return redCount / totalArea > 0.3;
            }
        }

        private bool HasSpeedLimitDigit(Mat image, Rectangle rect, Rectangle roi)
        {
            using (Mat cropped = new Mat())
            {
                CvInvoke.CopyMakeBorder(image, cropped, roi.Y, image.Rows - roi.Bottom, roi.X, image.Cols - roi.Right, BorderType.Constant, new MCvScalar(0));

                Mat gray = new Mat();
                CvInvoke.CvtColor(cropped, gray, ColorConversion.Bgr2Gray);
                CvInvoke.Threshold(gray, gray, 200, 255, ThresholdType.Binary);

                VectorOfVectorOfPoint digitContours = new VectorOfVectorOfPoint();
                CvInvoke.FindContours(gray, digitContours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

                for (int i = 0; i < digitContours.Size; i++)
                {
                    VectorOfPoint approx = new VectorOfPoint();
                    double peri = CvInvoke.ArcLength(digitContours[i], true);
                    CvInvoke.ApproxPolyDP(digitContours[i], approx, 0.04 * peri, true);

                    // Цифры обычно имеют 4-9 углов
                    if (approx.Size >= 4 && approx.Size <= 9)
                        return true;
                }

                return false;
            }
        }

        private bool HasPedestrianPattern(Mat image, Rectangle rect, Rectangle roi)
        {
            using (Mat cropped = new Mat())
            {
                CvInvoke.CopyMakeBorder(image, cropped, roi.Y, image.Rows - roi.Bottom, roi.X, image.Cols - roi.Right, BorderType.Constant, new MCvScalar(0));

                Mat gray = new Mat();
                CvInvoke.CvtColor(cropped, gray, ColorConversion.Bgr2Gray);
                CvInvoke.Threshold(gray, gray, 200, 255, ThresholdType.Binary);

                // Ищем горизонтальные линии (полосы пешеходного перехода)
                Mat edges = new Mat();
                CvInvoke.Canny(gray, edges, 50, 150);

                LineSegment2D[] lines = CvInvoke.HoughLinesP(edges, 1, Math.PI / 180, 100, 30, 10);
                int horizontalLines = 0;

                foreach (var line in lines)
                {
                    float angle = Math.Abs(line.P1.Y - line.P2.Y);
                    if (angle < 10)
                        horizontalLines++;
                }

                return horizontalLines >= 3;
            }
        }

        private bool AnalyzeInterior(Mat image, Rectangle roi, string shape)
        {
            using (Mat cropped = new Mat())
            {
                CvInvoke.CopyMakeBorder(image, cropped, roi.Y, image.Rows - roi.Y - roi.Height,
                    roi.X, image.Cols - roi.X - roi.Width, BorderType.Constant, new MCvScalar());

                Mat grayCropped = new Mat();
                CvInvoke.CvtColor(cropped, grayCropped, ColorConversion.Bgr2Gray);
                CvInvoke.Threshold(grayCropped, grayCropped, 128, 255, ThresholdType.Binary);

                // Поиск внутренних контуров
                VectorOfVectorOfPoint innerContours = new VectorOfVectorOfPoint();
                CvInvoke.FindContours(grayCropped, innerContours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

                int innerCornerCount = 0;
                for (int j = 0; j < innerContours.Size; j++)
                {
                    VectorOfPoint approx = new VectorOfPoint();
                    double peri = CvInvoke.ArcLength(innerContours[j], true);
                    CvInvoke.ApproxPolyDP(innerContours[j], approx, 0.04 * peri, true);

                    if (approx.Size > 2)
                        innerCornerCount += approx.Size;
                }

                Console.WriteLine($"Внутренние углы: {innerCornerCount}");

                // Если много внутренних контуров — это может быть цифра или полосы
                return innerCornerCount > 10;
            }
        }

      
    }
}
