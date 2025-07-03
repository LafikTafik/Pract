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

        public SimulationForm()
        {
            InitializeComponent();
            pictureBox1.Paint += pictureBox1_Paint;
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
                    Brush brush = obj.Type switch
                    {
                        ObjectType.Car => Brushes.Red,
                        ObjectType.OncomingCar => Brushes.Yellow,
                        ObjectType.Pedestrian => Brushes.Green,
                        _ => Brushes.Black
                    };

                    g.FillEllipse(brush, obj.Position.X, obj.Position.Y, 30, 30); // Размер точек увеличен
                }
            }

            // Автомобиль (точка зрения)
            int carX = roadX + roadWidth - 100; // Машину помещаем на правой стороне дороги
            int carY = (pictureBox1.Height / 2) + 80; // По центру экрана
            PointF[] carTriangle =
            {
                new PointF(carX, carY + 25),          // Левый нижний угол
                new PointF(carX + 30, carY + 25),     // Правый нижний угол
                new PointF(carX + 15, carY)           // Верхушка треугольника
              };
            g.FillPolygon(Brushes.Blue, carTriangle); // Автомобиль
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

                dataGridView1.Rows.Add(typeStr, distance, obj.Speed);
                dataGridView1.Rows.Add(typeStr, obj.Position.Y, obj.Speed);
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

        public void Timer_Tick(object sender, EventArgs e)
        {
            float carY = (pictureBox1.Height / 2) + 80;
            // Получаем параметры дороги
            int roadWidth = 400;
            int roadX = pictureBox1.Width / 2 - roadWidth / 2;

            // Генерируем новые объекты
            if (lidarObjects.Count < 10 && _random.Next(0, 100) < 30)
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

            // Установка цвета
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
                case "Сбросьте скорость":
                    labelDecision.BackColor = Color.Red;
                    labelDecision.ForeColor = Color.White;
                    break;
                default:
                    labelDecision.BackColor = SystemColors.Control;
                    labelDecision.ForeColor = SystemColors.ControlText;
                    break;
            }
            // Обновляем смещение разметки
            roadLineOffset += 3;
            if (roadLineOffset > 50)
                roadLineOffset = 0;

            // Перерисовываем PictureBox
            pictureBox1.Invalidate();
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
                case "Сбросьте скорость":
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

        private void roundedButton4_Click(object sender, EventArgs e)
        {
            SignRecognitionForm signForm = new SignRecognitionForm();
            signForm.Show();
        }
    }
}
