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

            // Шаг 1: Преобразование изображения
            Mat resized = new Mat();
            CvInvoke.Resize(image, resized, new Size(500, 500));

            Mat hsv = new Mat();
            CvInvoke.CvtColor(resized, hsv, ColorConversion.Bgr2Hsv);

            // Шаг 2: Поиск контуров
            Mat gray = new Mat();
            CvInvoke.CvtColor(resized, gray, ColorConversion.Bgr2Gray);

            Mat binary = new Mat();
            CvInvoke.Threshold(gray, binary, 150, 255, ThresholdType.BinaryInv);

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(binary, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

            for (int i = 0; i < contours.Size; i++)
            {
                VectorOfPoint contour = contours[i];
                VectorOfPoint approx = new VectorOfPoint();
                double perimeter = CvInvoke.ArcLength(contour, true);
                CvInvoke.ApproxPolyDP(contour, approx, 0.04 * perimeter, true);

                int corners = approx.Size;
                Rectangle rect = CvInvoke.BoundingRectangle(approx);

                if (rect.Width < 30 || rect.Height < 30) continue;

                // Шаг 3: Анализ формы
                string shape = GetShape(corners, rect);

                // Шаг 4: Анализ цвета
                string color = GetDominantColor(resized, rect);

                // Шаг 5: Комбинируем форму и цвет для определения знака
                switch (shape)
                {
                    case "octagon" when color == "red":
                        return "Стоп";

                    case "circle" when color == "red":
                        return "Ограничение скорости";

                    case "triangle" when color == "red" && IsInvertedTriangle(resized, rect):
                        return "Уступите дорогу";

                    case "circle" when color == "blue":
                        return "Пешеходный переход";

                    case "square" when color == "blue":
                        return "Движение прямо";

                    case "square" when color == "green":
                        return "Направление";

                    default:
                        return "Неизвестный знак";
                }
            }
            return "Не найдено";
        }

        private string GetShape(int corners, Rectangle rect)
        {
            double aspectRatio = (double)rect.Width / rect.Height;

            if (corners >= 8 && Math.Abs(aspectRatio - 1) < 0.2)
                return "octagon"; // Восьмиугольник

            if (corners == 3)
                return "triangle";

            if (corners == 4 && rect.Width > rect.Height * 1.2)
                return "rectangle_long";

            if (corners == 4 && Math.Abs(aspectRatio - 1) < 0.2)
                return "square";

            if (corners == 4 && aspectRatio < 0.6)
                return "horizontal_rectangle";

            if (corners == 4 && aspectRatio > 1.5)
                return "vertical_rectangle";

            if (corners > 6)
                return "circle";

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
            // Создаём новый Mat для копирования ROI
            using (Mat cropped = new Mat())
            {
                // Копируем ROI с использованием CopyMakeBorder
                CvInvoke.CopyMakeBorder(image, cropped, roi.Top, roi.Bottom, roi.Left, roi.Right, BorderType.Constant, new MCvScalar(0));

                // Переводим в HSV
                Mat hsv = new Mat();
                CvInvoke.CvtColor(cropped, hsv, ColorConversion.Bgr2Hsv);

                // Диапазон красного цвета
                ScalarArray lowerRed = new ScalarArray(new MCvScalar(0, 100, 100));
                ScalarArray upperRed = new ScalarArray(new MCvScalar(10, 255, 255));
                Mat maskRed = new Mat();
                CvInvoke.InRange(hsv, lowerRed, upperRed, maskRed);
                int redCount = CountNonZero(maskRed);

                // Диапазон синего цвета
                ScalarArray lowerBlue = new ScalarArray(new MCvScalar(100, 150, 50));
                ScalarArray upperBlue = new ScalarArray(new MCvScalar(140, 255, 255));
                Mat maskBlue = new Mat();
                CvInvoke.InRange(hsv, lowerBlue, upperBlue, maskBlue);
                int blueCount = CountNonZero(maskBlue);

                // Диапазон белого цвета
                Mat grayMask = new Mat();
                CvInvoke.CvtColor(cropped, grayMask, ColorConversion.Bgr2Gray);
                CvInvoke.Threshold(grayMask, grayMask, 200, 255, ThresholdType.Binary);
                int whiteCount = CountNonZero(grayMask);

                if (redCount > blueCount && redCount > whiteCount)
                    return "red";
                else if (blueCount > whiteCount)
                    return "blue";

                return "white";
            }
        }

        private int CountColor(Mat hsv, ScalarArray lower, ScalarArray upper)
        {
            Mat mask = new Mat();
            CvInvoke.InRange(hsv, lower, upper, mask);

            VectorOfPoint nonZeroPoints = new VectorOfPoint();
            CvInvoke.FindNonZero(mask, nonZeroPoints);

            int count = nonZeroPoints.Size;
            return count; // Используем Size вместо Length
        }

        private bool IsInvertedTriangle(Mat image, Rectangle rect)
        {
            // Извлекаем ROI и находим контуры
            Mat roi = new Mat(image, rect);
            Mat gray = new Mat();
            CvInvoke.CvtColor(roi, gray, ColorConversion.Bgr2Gray);

            VectorOfVectorOfPoint triangleContours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(gray, triangleContours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

            for (int i = 0; i < triangleContours.Size; i++)
            {
                VectorOfPoint approx = new VectorOfPoint();
                double peri = CvInvoke.ArcLength(triangleContours[i], true);
                CvInvoke.ApproxPolyDP(triangleContours[i], approx, 0.04 * peri, true);

                if (approx.Size == 3)
                {
                    // Проверяем ориентацию треугольника
                    Point p1 = approx[0];
                    Point p2 = approx[1];
                    Point p3 = approx[2];

                    // Если вершина треугольника внизу → перевёрнутый
                    int topY = Math.Min(p1.Y, Math.Min(p2.Y, p3.Y));
                    return topY == p1.Y || topY == p2.Y || topY == p3.Y;
                }
            }

            return false;
        }
    }

}
