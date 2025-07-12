using Emgu.CV.CvEnum;
using Emgu.CV;
using NAMI.Foms;
using NAMI.Models;
using NAMI.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV.Util;
using System.Text.RegularExpressions;

namespace NAMI
{
    public partial class SimulationForm : Form
    {
        private Random _random = new Random();
        private System.Windows.Forms.Timer timer;
        public List<DetectedObject> lidarObjects = new List<DetectedObject>(); // Добавляем поле для объектов LiDAR
        private LiDARSimulator lidarSimulator = new LiDARSimulator();
        private ObstacleDetector obstacleDetector = new ObstacleDetector();
        private DecisionMaker decisionMaker = new DecisionMaker();
        private int roadLineOffset = 0; // Смещение разметки
        private bool isPaused = false;
        private Bitmap carImage;
        private Bitmap oncomingCarImage;
        private Bitmap pedestrianImage;
        private Bitmap outcarImage;

        private float speedFactor = 1.0f;
        private float targetX ;
        private float CarX ; // текущая X-координата автомобиля

        public SimulationForm()
        {
            InitializeComponent();
            pictureBox1.Paint += pictureBox1_Paint;
            carImage = ResizeImage(new Bitmap(Image.FromFile("images/car.png")), new Size(70, 110));
            oncomingCarImage = ResizeImage(new Bitmap(Image.FromFile("images/oncoming_car.png")), new Size(70, 110));
            pedestrianImage = ResizeImage(new Bitmap(Image.FromFile("images/pedestrian.png")), new Size(25, 25));
            outcarImage = ResizeImage(new Bitmap(Image.FromFile("images/outcar.png")), new Size(70, 110));
            int roadWidth = 400;
            int roadX = pictureBox1.Width / 2 - roadWidth / 2;
            int CarX = roadX + roadWidth - 120; // Справа по оси X
            int carY = (pictureBox1.Height / 2) + 200; // Центр экрана
            targetX = CarX;
            //++++++++++++
            signDisplayTimer = new System.Windows.Forms.Timer();
            signDisplayTimer.Interval = SignDisplayIntervalMs;
            signDisplayTimer.Tick += DisplayRandomSign;

            // Загрузка путей к знакам
            string signsFolder = "signs/";
            if (Directory.Exists(signsFolder))
            {
                signPaths = Directory.GetFiles(signsFolder, "*.png")
                    .Concat(Directory.GetFiles(signsFolder, "*.jpg"))
                    .ToArray();
            }
            else
            {
                MessageBox.Show("Папка 'signs/' не найдена!");
            }

            signDisplayTimer.Start(); // запуск показа случайных знаков
        }
        

        private Bitmap ResizeImage(Bitmap original, Size newSize)
        {
            Bitmap resized = new Bitmap(original, newSize);
            return resized;
        }


        private Bitmap LoadImage(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                System.Diagnostics.Debug.WriteLine($"Файл не найден: {path}");
                return null;
            }

            try
            {
                return new Bitmap(Image.FromFile(path));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки: {ex.Message}");
                return null;
            }
        }

        private Bitmap GetImageForObjectType(ObjectType type)
        {
            switch (type)
            {
                case ObjectType.Car:
                    return carImage;
                case ObjectType.OncomingCar:
                    return oncomingCarImage;
                case ObjectType.Pedestrian:
                    return pedestrianImage;
                default:
                    return null;
            }
        }

        public void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Размеры элементов
            int roadWidth = 400; // Ширина дороги
            int roadLineOffset = 0;
            int roadX = pictureBox1.Width / 2 - roadWidth / 2; // Центральная часть PictureBox

            // Общая дорога
            g.FillRectangle(Brushes.Gray, roadX, 0, roadWidth, pictureBox1.Height);

            Pen linePen = new Pen(Color.White, 5); // Толстая белая линия
            int lineX = roadX + roadWidth / 2; // Центральная ось дороги
            for (int y = 0; y < pictureBox1.Height + 50; y += 50)
            {
                int currentY = y - roadLineOffset;

                // Ограничиваем только область дороги
                if (currentY >= -50 && currentY <= pictureBox1.Height)
                {
                    g.DrawLine(linePen, lineX, currentY, lineX, currentY + 20);
                }
            }

            // Обочины
            g.FillRectangle(Brushes.Brown, 0, 0, roadX, pictureBox1.Height); // Левая обочина
            g.FillRectangle(Brushes.Brown, roadX + roadWidth, 0, pictureBox1.Width - roadX - roadWidth, pictureBox1.Height); // Правая обочина

            // Газоны
            g.FillRectangle(Brushes.Green, -40, 0, roadX, pictureBox1.Height); // Левый газон
            g.FillRectangle(Brushes.Green, roadX + roadWidth + 40, 0, pictureBox1.Width - roadX - roadWidth, pictureBox1.Height); // Правый газон

            // Точки LiDAR
            foreach (var obj in lidarObjects)
            {
                if (obj.Position.Y >= 0 && obj.Position.Y <= pictureBox1.Height)
                {
                    Bitmap bmp = GetImageForObjectType(obj.Type);
                    if (bmp != null)
                    {
                        int drawX = (int)obj.Position.X - bmp.Width / 2; // центрируем изображение
                        int drawY = (int)obj.Position.Y - bmp.Height / 2;

                        g.DrawImage(bmp, new Rectangle(drawX, drawY, bmp.Width, bmp.Height));
                    }
                    else
                    {
                        // Резервная отрисовка — если изображение не найдено
                        g.FillEllipse(Brushes.Gray, obj.Position.X, obj.Position.Y, 30, 30);
                    }
                }
            }

            //int carX = roadX + roadWidth - 120; // Справа по оси X
            //int carY = (pictureBox1.Height / 2) + 200; // Центр экрана
            //e.Graphics.DrawImage(outcarImage, new Rectangle(carX, carY, outcarImage.Width, outcarImage.Height));
            int drawCarX = (int)CarX; // ✅ Используем CarX из класса
            int drawCarY = (pictureBox1.Height / 2) + 80;

            if (outcarImage != null)
            {
              e.Graphics.DrawImage(outcarImage, new Rectangle(drawCarX, drawCarY, outcarImage.Width, outcarImage.Height));
            }

        }

        private void UpdateDataGridView(List<DetectedObject> obstacles)
        {
            dataGridView1.Rows.Clear();

            foreach (var obj in obstacles)
            {
                string typeStr = obj.Type switch
                {
                    ObjectType.Car => "Автомобиль",
                    ObjectType.OncomingCar => "Встречный автомобиль",
                    ObjectType.Pedestrian => "Пешеход",
                    _ => "Неизвестный"
                };

                float carY = (pictureBox1.Height / 2) + 80; // автомобиль в центре экрана
                float distance = Math.Abs(obj.Position.Y - carY); // расстояние до автомобиля

                dataGridView1.Rows.Add(typeStr, distance, obj.Speed +60);
                dataGridView1.Rows.Add(typeStr, obj.Position.Y, obj.Speed +60 );
            }
        }

        public void SimulationForm_Load(object sender, EventArgs e)
        {
            dataGridView1.Columns.Clear();

            // Добавляем столбцы
            dataGridView1.Columns.Add("ObjectType", "Тип");
            dataGridView1.Columns.Add("Distance", "Расстояние (м)");
            dataGridView1.Columns.Add("Speed", "Скорость (км/ч)");
        }

        public void roundedButton1_Click(object sender, EventArgs e)
        {
            MainForm mainForm = new MainForm();
            mainForm.Show();
            this.Close(); // или Hide(), если хотите сохранить состояние
        }

        public void roundedButton2_Click(object sender, EventArgs e)
        {
            if (timer == null)
            {
                timer = new System.Windows.Forms.Timer();
                timer.Interval = 50; // Обновление каждые 50 мс
                timer.Tick += Timer_Tick;
                timer.Start();
            }
        }

        private bool CheckCollision(DetectedObject a, DetectedObject b)
        {
            RectangleF rectA = new RectangleF(a.Position.X, a.Position.Y, a.Size.Width, a.Size.Height);
            RectangleF rectB = new RectangleF(b.Position.X, b.Position.Y, b.Size.Width, b.Size.Height);

            return rectA.IntersectsWith(rectB);
        }

        private const int SpawnCooldownMs = 5000; // 1 секунда
        private DateTime lastCarSpawnTime = DateTime.MinValue;
        private DateTime lastOncomingCarSpawnTime = DateTime.MinValue;





        public void Timer_Tick(object sender, EventArgs e)
        {
            bool canSpawnCar = (DateTime.Now - lastCarSpawnTime).TotalMilliseconds > SpawnCooldownMs;
            bool canSpawnOncoming = (DateTime.Now - lastOncomingCarSpawnTime).TotalMilliseconds > SpawnCooldownMs;
            // Плавное движение к целевой позиции
            if (Math.Abs(CarX - targetX) > 1)
            {
                CarX += (targetX - CarX) * 0.1f * speedFactor;
            }
            float carY = (pictureBox1.Height / 2) + 80;
            // Получаем параметры дороги
            int roadWidth = 400;
            int roadX = pictureBox1.Width / 2 - roadWidth / 2;
            int LaneOffset = 100;

            int lane1X = roadX + roadWidth - LaneOffset;
            int lane2X = roadX + LaneOffset;

            int carCount = lidarObjects.Count(o => o.Type == ObjectType.Car || o.Type == ObjectType.OncomingCar);
            int maxCars = 6;


            if (carCount < maxCars && _random.Next(0, 100) < 30)
            {
                lidarObjects.AddRange(lidarSimulator.GenerateTestPoints(roadX, pictureBox1.Height));
            }
            if (IsPathBlocked(CarX, carY))
            {
                speedFactor = 0.3f; // медленнее
                labelDecision.Text = "Препятствие рядом — снижение скорости";
            }
            else
            {
                speedFactor = 1.0f;
            }
            // Генерируем новые объекты
            //if (lidarObjects.Count < 10 && _random.Next(0, 100) < 30)
            //{
            //lidarObjects.AddRange(lidarSimulator.GenerateTestPoints(roadX, pictureBox1.Height));
            //}

            if (_random.Next(0, 100) < 20 && lidarObjects.Count < 8 && !IsPositionOccupied(lane1X) && canSpawnCar)
            {
                lidarObjects.AddRange(lidarSimulator.GenerateTestPoints(roadX, pictureBox1.Height));
            }

            // Встречная машина
            if (_random.Next(0, 100) < 20 && lidarObjects.Count < 8 && !IsPositionOccupied(lane2X) && canSpawnOncoming)
            {
                lidarObjects.AddRange(lidarSimulator.GenerateTestPoints(roadX, pictureBox1.Height));
            }

            // Пешеход
            if (_random.Next(0, 100) < 5)
            {
                lidarObjects.AddRange(lidarSimulator.GenerateTestPoints(roadX, pictureBox1.Height));
            }


            // Двигаем все объекты вниз
            for (int i = 0; i < lidarObjects.Count; i++)
            {
                var obj = lidarObjects[i];
                switch (obj.Type)
                {
                    case ObjectType.Car:
                    case ObjectType.OncomingCar:
                        obj.Position = new PointF(obj.Position.X, obj.Position.Y + obj.Speed * 0.6f);
                        break;
                    case ObjectType.Pedestrian:
                        obj.Position = new PointF(obj.Position.X + obj.Speed * 0.3f, obj.Position.Y);
                        break;
                }
            }

            for (int i = 0; i < lidarObjects.Count; i++)
            {
                var objA = lidarObjects[i];

                for (int j = i + 1; j < lidarObjects.Count; j++)
                {
                    var objB = lidarObjects[j];
                    if (CheckCollision(objA, objB))
                    {
                        System.Diagnostics.Debug.WriteLine("Объекты столкнулись!");
                        // Можно немного сместить один из них
                        objB.Position = new PointF(objB.Position.X + 20, objB.Position.Y);
                    }
                }
            }

            // Удаляем объекты, которые вышли за нижнюю границу
            lidarObjects = lidarObjects
            .Where(o =>
            {
                switch (o.Type)
                {
                    case ObjectType.Car:
                    case ObjectType.OncomingCar:
                        return o.Position.Y < pictureBox1.Height;
                    case ObjectType.Pedestrian:
                        return o.Position.X < pictureBox1.Width + 20; // удаляем, когда пешеход перешёл дорогу
                    default:
                        return true;
                }
            })
            .ToList();
            // Обновляем PictureBox
            pictureBox1.Invalidate();

            // Обновляем DataGridView
            var obstacles = obstacleDetector.DetectObstacles(lidarObjects);
            UpdateDataGridView(obstacles);

            // Обновляем рекомендацию
            string decision = decisionMaker.MakeDecision(obstacles, carY);

            labelDecision.Text = $"Рекомендация: {decision}";

            if (CheckCollisionWithOthers(CarX, carY))
            {
                // Если уже сталкиваемся → останавливаем автомобиль
                speedFactor = 0.0f;
                labelDecision.Text = "Остановка: столкновение";
                labelDecision.BackColor = Color.Red;
            }
            else
            {
                // Установка цвета
                switch (decision)
                {
                    case "Обгон по встречной полосе":
                        labelDecision.BackColor = Color.Green;
                        labelDecision.ForeColor = Color.White;
                        targetX = roadX + 140;
                        // Увеличь скорость немного
                        foreach (var obj in lidarObjects)
                        {
                            if (obj.Type == ObjectType.Car || obj.Type == ObjectType.OncomingCar)
                                obj.Speed = Math.Min(obj.InitialSpeed * 1.1f, obj.InitialSpeed * 1.5f);
                        }
                        break;

                    case "Обгон по обочине":
                        labelDecision.BackColor = Color.Orange;
                        labelDecision.ForeColor = Color.Black;
                        targetX = roadX + roadWidth - 20;
                        // Уменьши скорость на время манёвра
                        foreach (var obj in lidarObjects)
                        {
                            if (obj.Type == ObjectType.Car || obj.Type == ObjectType.OncomingCar)
                                obj.Speed = Math.Max(obj.InitialSpeed * 0.5f, obj.Speed * 0.98f);
                        }
                        break;

                    case "Пешеход - Сбросьте скорость":
                        labelDecision.BackColor = Color.Red;
                        labelDecision.ForeColor = Color.White;

                        // Замедлим все машины
                        foreach (var obj in lidarObjects)
                        {
                            if (obj.Type == ObjectType.Car || obj.Type == ObjectType.OncomingCar)
                                obj.Speed *= 0.95f;
                        }
                        break;

                    default:
                        float defaultCarX = roadX + roadWidth - 120;
                        labelDecision.BackColor = SystemColors.Control;
                        labelDecision.ForeColor = SystemColors.ControlText;
                        targetX = defaultCarX;

                        // Восстановим скорость
                        foreach (var obj in lidarObjects)
                        {
                            if ((obj.Type == ObjectType.Car || obj.Type == ObjectType.OncomingCar) && obj.Speed < obj.InitialSpeed)
                            {
                                obj.Speed += obj.InitialSpeed * 0.2f;
                                if (obj.Speed > obj.InitialSpeed)
                                    obj.Speed = obj.InitialSpeed;
                            }
                        }
                        break;
                }
            }
           


            CarX += (targetX - CarX) * 0.1f;
            // Обновляем смещение разметки
            roadLineOffset += 3;
            if (roadLineOffset > 50)
                roadLineOffset = 0;

            // Перерисовываем PictureBox
            pictureBox1.Invalidate();
        }
        private bool IsPositionOccupied(float laneX)
        {
            foreach (var obj in lidarObjects)
            {
                if (Math.Abs(obj.Position.X - laneX) < 20 &&
                    obj.Position.Y < pictureBox1.Height + 110)
                {
                    return true;
                }
            }
            return false;
        }

        private void UpdateDecisionLabel(string decision)
        {
            switch (decision)
            {
                case "Обгон по встречной полосе":
                    labelDecision.BackColor = Color.Green;
                    labelDecision.ForeColor = Color.White;
                    break;
                case "Обгон по обочине":
                    labelDecision.BackColor = Color.Orange;
                    labelDecision.ForeColor = Color.Black;
                    break;
                case "Пешеход - Сбросьте скорость":
                    labelDecision.BackColor = Color.Red;
                    labelDecision.ForeColor = Color.White;
                    break;
                default:
                    labelDecision.BackColor = SystemColors.Control;
                    labelDecision.ForeColor = SystemColors.ControlText;
                    break;
            }

            labelDecision.Text = $"Рекомендация: {decision}";
        }
        private void roundedButton3_Click(object sender, EventArgs e)
        {
            float carY = (pictureBox1.Height / 2) + 80;
            var obstacles = obstacleDetector.DetectObstacles(lidarObjects);
            if (timer != null)
            {
                isPaused = !isPaused;

                if (isPaused)
                {
                    timer.Stop();
                    labelDecision.Text = "Симуляция приостановлена";
                    labelDecision.BackColor = Color.Gray;
                }
                else
                {
                    timer.Start();
                    string decision = decisionMaker.MakeDecision(obstacles, carY);
                    UpdateDecisionLabel(decision); // обновляем рекомендацию
                }
            }
        }

        private const int SelfCarWidth = 70;
        private const int SelfCarHeight = 110;
        private bool CheckCollisionWithOthers(float carX, float carY)
        {
            foreach (var obj in lidarObjects)
            {
                RectangleF objRect = new RectangleF(obj.Position.X, obj.Position.Y, obj.Size.Width, obj.Size.Height);
                RectangleF selfRect = new RectangleF(carX, carY, SelfCarWidth, SelfCarHeight);

                if (objRect.IntersectsWith(selfRect))
                {
                    return true; // обнаружено столкновение
                }
            }
            return false;
        }
        private bool IsPathBlocked(float carX, float carY)
        {
            foreach (var obj in lidarObjects)
            {
                if (obj.Type == ObjectType.Car || obj.Type == ObjectType.OncomingCar)
                {
                    float distance = Math.Abs(obj.Position.Y - carY);
                    if (distance < 80 && Math.Abs(obj.Position.X - carX) < 50)
                        return true;
                }
            }
            return false;
        }
        private void roundedButton4_Click(object sender, EventArgs e)
        {
            SignRecognitionForm signForm = new SignRecognitionForm();
            signForm.Show();
        }
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        private System.Windows.Forms.Timer signDisplayTimer; // второй таймер для показа знаков
        private string[] signPaths;
        private DateTime lastSignTime = DateTime.MinValue;
        private const int SignDisplayIntervalMs = 15000; // 15 секунд между показами

        private void DisplayRandomSign(object sender, EventArgs e)
        {
            if (signPaths == null || signPaths.Length == 0)
                return;

            string randomSignPath = signPaths[_random.Next(signPaths.Length)];

            try
            {
                // Показываем изображение
                pictureBoxSign.Image?.Dispose();
                pictureBoxSign.Image = new Bitmap(randomSignPath);

                // Распознаём через Emgu CV
                using (Mat image = CvInvoke.Imread(randomSignPath, ImreadModes.Color))
                {
                    if (!image.IsEmpty)
                    {
                        string result = SignRecognizer.RecognizeSign(image);
                        labelSignDecision.Text = $"Распознано: {result}";

                        // Можно менять цвет метки в зависимости от типа знака
                        switch (result)
                        {
                            case "Стоп":
                                labelSignDecision.BackColor = Color.Red;
                                break;
                            case string s when s.StartsWith("Ограничение скорости"):
                                labelSignDecision.BackColor = Color.Blue;
                                break;
                            case "Пешеходный переход":
                                labelSignDecision.BackColor = Color.Yellow;
                                break;
                            default:
                                labelSignDecision.BackColor = SystemColors.Control;
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при обработке знака: {ex.Message}");
                labelSignDecision.Text = "Ошибка загрузки";
            }
        }
    }
}
