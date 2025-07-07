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
            System.Diagnostics.Debug.WriteLine("Текущая папка: " + System.IO.Directory.GetCurrentDirectory());
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files (*.jpg;*.png)|*.jpg;*.png";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    Mat image = CvInvoke.Imread(ofd.FileName, ImreadModes.Color);
                    if (image.IsEmpty)
                    {
                        MessageBox.Show("Ошибка: Не удалось загрузить изображение");
                        return;
                    }

                    picboxsign.Image = image.ToBitmap();

                    Mat resultImage;
                    string signName = DetectTrafficSign(image, out resultImage);
                    picboxsign.Image = resultImage.ToBitmap(); // отображаем результат с обводкой
                    labelSign.Text = $"Обнаружен: {signName}";
                }
            }
        }
        private string DetectTrafficSign(Mat image, out Mat resultImage)
        {
            resultImage = image.Clone(); // Клонируем изображение для отрисовки

            if (image.IsEmpty)
                return "Неизвестный";

            // Предварительная обработка
            Mat gray = new Mat();
            CvInvoke.CvtColor(image, gray, ColorConversion.Bgr2Gray);
            CvInvoke.EqualizeHist(gray, gray);

            Mat binary = new Mat();
            CvInvoke.Threshold(gray, binary, 150, 255, ThresholdType.BinaryInv);
            CvInvoke.GaussianBlur(binary, binary, new Size(5, 5), 0);

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(binary, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            
            for (int i = 0; i < contours.Size; i++)
            {
                VectorOfPoint contour = contours[i];
                double perimeter = CvInvoke.ArcLength(contour, true);
                VectorOfPoint approx = new VectorOfPoint();
                CvInvoke.ApproxPolyDP(contour, approx, 0.04 * perimeter, true);

                int corners = approx.Size;
                Rectangle roi = CvInvoke.BoundingRectangle(approx);

                // Проверка корректности ROI
                double area = CvInvoke.ContourArea(contour);
                if (area < 1000) continue;

                using (Mat cropped = new Mat())
                {
                    int top = Math.Max(0, roi.Y);
                    int bottom = Math.Max(0, image.Rows - roi.Y - roi.Height);
                    int left = Math.Max(0, roi.X);
                    int right = Math.Max(0, image.Cols - roi.X - roi.Width);

                    CvInvoke.CopyMakeBorder(image, cropped, top, bottom, left, right, BorderType.Constant, new MCvScalar(0));

                    // Теперь approx и corners доступны
                    string shape = GetShape(corners, roi);
                    string color = GetDominantColor(image, roi);

                    string matchedSign = MatchWithTemplates(cropped);
                    if (matchedSign != "Неизвестный")
                    {
                        CvInvoke.DrawContours(resultImage, contours, i, new MCvScalar(0, 255, 0), 2);
                        return matchedSign;
                    }
                }
            }

            return "Неизвестный";
        }

        private string GetShape(int corners, Rectangle rect)
        {
            double aspectRatio = (double)rect.Width / rect.Height;

            if (corners >= 8 && Math.Abs(aspectRatio - 1) < 0.2)
                return "octagon"; // Стоп

            if (corners == 3)
                return "triangle"; // Уступите дорогу

            if (corners == 4 && Math.Abs(aspectRatio - 1) < 0.2)
                return "square"; // Движение прямо

            if (corners > 6 && Math.Abs(aspectRatio - 1) < 0.1)
                return "circle"; // Ограничение скорости

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
            using (Mat cropped = new Mat())
            {
                // Корректное вырезание ROI
                int top = Math.Max(0, roi.Y);
                int bottom = Math.Max(0, image.Rows - roi.Y - roi.Height);
                int left = Math.Max(0, roi.X);
                int right = Math.Max(0, image.Cols - roi.X - roi.Width);

                CvInvoke.CopyMakeBorder(image, cropped, top, bottom, left, right, BorderType.Constant, new MCvScalar(0));

                // Переводим в HSV
                Mat hsv = new Mat();
                CvInvoke.CvtColor(cropped, hsv, ColorConversion.Bgr2Hsv);

                // Красный цвет (два диапазона в HSV)
                ScalarArray lowerRed1 = new ScalarArray(new MCvScalar(0, 100, 100));
                ScalarArray upperRed1 = new ScalarArray(new MCvScalar(10, 255, 255));
                ScalarArray lowerRed2 = new ScalarArray(new MCvScalar(170, 100, 100));
                ScalarArray upperRed2 = new ScalarArray(new MCvScalar(180, 255, 255));

                Mat maskRed1 = new Mat(), maskRed2 = new Mat();
                CvInvoke.InRange(hsv, lowerRed1, upperRed1, maskRed1);
                CvInvoke.InRange(hsv, lowerRed2, upperRed2, maskRed2);
                CvInvoke.Add(maskRed1, maskRed2, maskRed1); // объединяем оба красных диапазона

                int redCount = CountNonZero(maskRed1);
                int totalPixels = roi.Width * roi.Height;

                double redRatio = redCount / (double)totalPixels;
                if (redRatio > 0.3)
                    return "red";

                return "unknown";
            }
        }


        private bool IsInvertedTriangle(VectorOfPoint approx)
        {
            if (approx.Size < 3)
                return false;

            try
            {
                Point p1 = approx[0];
                Point p2 = approx[1];
                Point p3 = approx[2];

                int lowestY = Math.Max(p1.Y, Math.Max(p2.Y, p3.Y));
                return p1.Y == lowestY || p2.Y == lowestY || p3.Y == lowestY;
            }
            catch
            {
                return false;
            }
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

        private bool HasWhiteCenter(Mat image, Rectangle roi)
        {
            using (Mat cropped = new Mat())
            {
                int top = Math.Max(0, roi.Y);
                int bottom = Math.Max(0, image.Rows - roi.Y - roi.Height);
                int left = Math.Max(0, roi.X);
                int right = Math.Max(0, image.Cols - roi.X - roi.Width);

                CvInvoke.CopyMakeBorder(image, cropped, top, bottom, left, right, BorderType.Constant, new MCvScalar(0));

                Mat gray = new Mat();
                CvInvoke.CvtColor(cropped, gray, ColorConversion.Bgr2Gray);

                Mat whiteMask = new Mat();
                CvInvoke.Threshold(gray, whiteMask, 200, 255, ThresholdType.Binary);

                int whitePixels = CountNonZero(whiteMask);
                double ratio = whitePixels / (double)(roi.Width * roi.Height);

                return ratio > 0.6;
            }
        }

        private bool HasRedBorder(Mat image, Rectangle rect, Rectangle roi)
        {
            // Проверка: ROI должен быть внутри изображения
            if (roi.Width <= 0 || roi.Height <= 0 ||
                roi.X < 0 || roi.Y < 0 ||
                roi.X + roi.Width > image.Cols || roi.Y + roi.Height > image.Rows)
            {
                System.Diagnostics.Debug.WriteLine("ROI вне допустимых границ");
                return false;
            }

            using (Mat cropped = new Mat())
            {
                CvInvoke.CopyMakeBorder(image, cropped, roi.Y, image.Rows - roi.Bottom, roi.X, image.Cols - roi.Right, BorderType.Constant, new MCvScalar(0));

                Mat hsv = new Mat();
                CvInvoke.CvtColor(cropped, hsv, ColorConversion.Bgr2Hsv);

                // Диапазон красного цвета (2 диапазона для HSV)
                ScalarArray lowerRed1 = new ScalarArray(new MCvScalar(0, 100, 100));
                ScalarArray upperRed1 = new ScalarArray(new MCvScalar(10, 255, 255));
                ScalarArray lowerRed2 = new ScalarArray(new MCvScalar(170, 100, 100));
                ScalarArray upperRed2 = new ScalarArray(new MCvScalar(180, 255, 255));

                Mat maskRed1 = new Mat();
                CvInvoke.InRange(hsv, lowerRed1, upperRed1, maskRed1);
                Mat maskRed2 = new Mat();
                CvInvoke.InRange(hsv, lowerRed2, upperRed2, maskRed2);

                CvInvoke.Add(maskRed1, maskRed2, maskRed1); // Объединяем оба красных диапазона

                // Улучшаем маску с помощью морфологических операций
                Mat element = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(-1, -1));

                // ✅ Здесь исправляем вызов Dilate и Erode — добавляем anchor
                CvInvoke.Dilate(maskRed1, maskRed1, element, new Point(-1, -1), 1, BorderType.Constant, new MCvScalar());
                CvInvoke.Erode(maskRed1, maskRed1, element, new Point(-1, -1), 1, BorderType.Constant, new MCvScalar());

                int redCount = CountNonZero(maskRed1);
                double totalPixels = roi.Width * roi.Height;
                double redRatio = redCount / totalPixels;

                System.Diagnostics.Debug.WriteLine($"Красные пиксели: {redCount}, Всего: {totalPixels}, Доля: {redRatio:F2}");

                return redRatio > 0.4 && redCount > 500;
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

                Mat edges = new Mat();
                CvInvoke.Canny(gray, edges, 50, 150);

                LineSegment2D[] lines = CvInvoke.HoughLinesP(edges, 1, Math.PI / 180, 100, 30, 10);

                int horizontalLines = 0;
                foreach (var line in lines)
                {
                    float angle = Math.Abs(line.P1.Y - line.P2.Y);
                    if (angle < 10) horizontalLines++;
                }

                return horizontalLines >= 3;
            }
        }
        private string MatchWithTemplates(Mat cropped)
        {
            var templates = new Dictionary<string, string>
            {
                {"Стоп", "templates/stop.png"},
                {"Ограничение скорости", "templates/speed_limit.png"},
                {"Уступите дорогу", "templates/yield.png"},
                {"Пешеходный переход", "templates/pedestrian.png"}
            };

            double bestMatch = 0.2 ;
            string matchedSign = "Неизвестный";

            foreach (var pair in templates)
            {
                string signName = pair.Key;
                string path = pair.Value;

                if (!System.IO.File.Exists(path))
                {
                    System.Diagnostics.Debug.WriteLine($"Файл не найден: {path}");
                    continue;
                }

                using (Mat template = CvInvoke.Imread(path, ImreadModes.Color))
                {
                    if (template.IsEmpty)
                    {
                        System.Diagnostics.Debug.WriteLine($"{signName}: Шаблон пустой");
                        continue;
                    }

                    double matchValue = TemplateMatch(cropped, template);
                    System.Diagnostics.Debug.WriteLine($"{signName}: {matchValue:F2}");

                    if (matchValue > bestMatch)
                    {
                        bestMatch = matchValue;
                        matchedSign = signName;
                    }
                }
            }

            return matchedSign;
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
        private double TemplateMatch(Mat source, Mat template)
        {
            if (source.IsEmpty || template.IsEmpty)
                return 0;

            // Переводим в серый
            Mat sourceGray = new Mat();
            CvInvoke.CvtColor(source, sourceGray, ColorConversion.Bgr2Gray);

            Mat templateGray = new Mat();
            CvInvoke.CvtColor(template, templateGray, ColorConversion.Bgr2Gray);

            // Масштабируем шаблон под ROI
            Mat resizedTemplate = new Mat();
            CvInvoke.Resize(templateGray, resizedTemplate, new Size(sourceGray.Width, sourceGray.Height));

            // Вычисляем корреляцию
            Mat result = new Mat();
            CvInvoke.MatchTemplate(sourceGray, resizedTemplate, result, TemplateMatchingType.CcoeffNormed);

            double minVal = 0, maxVal = 0;
            Point minLoc = new Point(), maxLoc = new Point();

            CvInvoke.MinMaxLoc(result, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

            return maxVal;
        }
        private string DetectTrafficSignWithTemplates(Mat image, Rectangle roi)
        {
            using (Mat cropped = new Mat())
            {
                int top = Math.Max(0, roi.Y);
                int bottom = Math.Max(0, image.Rows - roi.Y - roi.Height);
                int left = Math.Max(0, roi.X);
                int right = Math.Max(0, image.Cols - roi.X - roi.Width);

                CvInvoke.CopyMakeBorder(image, cropped, top, bottom, left, right, BorderType.Constant, new MCvScalar(0));

                // Шаблоны
                var templates = new Dictionary<string, string>
        {
            {"Стоп", "templates/stop.png"},
            {"Ограничение скорости", "templates/speed_limit.png"},
            {"Уступите дорогу", "templates/yield.png"},
            {"Пешеходный переход", "templates/pedestrian.png"}
        };

                double bestMatch = 0.5; // Минимум совпадения
                string matchedSign = "Неизвестный знак";

                foreach (var pair in templates)
                {
                    string signName = pair.Key;
                    string path = pair.Value;

                    if (!System.IO.File.Exists(path))
                        continue;

                    using (Mat template = CvInvoke.Imread(path, ImreadModes.Color))
                    {
                        if (template.IsEmpty)
                            continue;

                        double matchValue = TemplateMatch(cropped, template);
                        System.Diagnostics.Debug.WriteLine($"{signName}: {matchValue:F2}");

                        if (matchValue > bestMatch)
                        {
                            bestMatch = matchValue;
                            matchedSign = signName;
                        }
                    }
                }

                return matchedSign;
            }
        }
    }
}
