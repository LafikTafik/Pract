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
        public List<DetectedObject> lidarObjects = new List<DetectedObject>(); // ��������� ���� ��� �������� LiDAR
        private LiDARSimulator lidarSimulator = new LiDARSimulator();
        private ObstacleDetector obstacleDetector = new ObstacleDetector();
        private DecisionMaker decisionMaker = new DecisionMaker();
        private int roadLineOffset = 0; // �������� ��������
        private bool isPaused = false;

        public SimulationForm()
        {
            InitializeComponent();
            pictureBox1.Paint += pictureBox1_Paint;
        }

        public void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // ������� ���������
            int roadWidth = 400; // ������ ������
            int roadLineOffset = 0;
            int roadX = pictureBox1.Width / 2 - roadWidth / 2; // ����������� ����� PictureBox

            // ����� ������
            g.FillRectangle(Brushes.Gray, roadX, 0, roadWidth, pictureBox1.Height);

            Pen linePen = new Pen(Color.White, 5); // ������� ����� �����
            int lineX = roadX + roadWidth / 2; // ����������� ��� ������
            for (int y = 0; y < pictureBox1.Height + 50; y += 50)
            {
                int currentY = y - roadLineOffset;

                // ������������ ������ ������� ������
                if (currentY >= -50 && currentY <= pictureBox1.Height)
                {
                    g.DrawLine(linePen, lineX, currentY, lineX, currentY + 20);
                }
            }

            // �������
            g.FillRectangle(Brushes.Brown, 0, 0, roadX, pictureBox1.Height); // ����� �������
            g.FillRectangle(Brushes.Brown, roadX + roadWidth, 0, pictureBox1.Width - roadX - roadWidth, pictureBox1.Height); // ������ �������

            // ������
            g.FillRectangle(Brushes.Green, -40, 0, roadX, pictureBox1.Height); // ����� �����
            g.FillRectangle(Brushes.Green, roadX + roadWidth + 40, 0, pictureBox1.Width - roadX - roadWidth, pictureBox1.Height); // ������ �����

            // ����� LiDAR
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

                    g.FillEllipse(brush, obj.Position.X, obj.Position.Y, 30, 30); // ������ ����� ��������
                }
            }

            // ���������� (����� ������)
            int carX = roadX + roadWidth - 100; // ������ �������� �� ������ ������� ������
            int carY = (pictureBox1.Height / 2) + 80; // �� ������ ������
            PointF[] carTriangle =
            {
                new PointF(carX, carY + 25),          // ����� ������ ����
                new PointF(carX + 30, carY + 25),     // ������ ������ ����
                new PointF(carX + 15, carY)           // �������� ������������
              };
            g.FillPolygon(Brushes.Blue, carTriangle); // ����������
        }

        private void UpdateDataGridView(List<DetectedObject> obstacles)
        {
            dataGridView1.Rows.Clear();

            foreach (var obj in obstacles)
            {
                string typeStr = obj.Type switch
                {
                    ObjectType.Car => "����������",
                    ObjectType.OncomingCar => "��������� ����������",
                    ObjectType.Pedestrian => "�������",
                    _ => "�����������"
                };

                float carY = (pictureBox1.Height / 2) + 80; // ���������� � ������ ������
                float distance = Math.Abs(obj.Position.Y - carY); // ���������� �� ����������

                dataGridView1.Rows.Add(typeStr, distance, obj.Speed);
                dataGridView1.Rows.Add(typeStr, obj.Position.Y, obj.Speed);
            }
        }

        public void SimulationForm_Load(object sender, EventArgs e)
        {
            dataGridView1.Columns.Clear();

            // ��������� �������
            dataGridView1.Columns.Add("ObjectType", "���");
            dataGridView1.Columns.Add("Distance", "���������� (�)");
            dataGridView1.Columns.Add("Speed", "�������� (��/�)");
        }

        public void roundedButton1_Click(object sender, EventArgs e)
        {
            MainForm mainForm = new MainForm();
            mainForm.Show();
            this.Close(); // ��� Hide(), ���� ������ ��������� ���������
        }

        public void roundedButton2_Click(object sender, EventArgs e)
        {
            if (timer == null)
            {
                timer = new System.Windows.Forms.Timer();
                timer.Interval = 50; // ���������� ������ 50 ��
                timer.Tick += Timer_Tick;
                timer.Start();
            }
        }

        public void Timer_Tick(object sender, EventArgs e)
        {
            float carY = (pictureBox1.Height / 2) + 80;
            // �������� ��������� ������
            int roadWidth = 400;
            int roadX = pictureBox1.Width / 2 - roadWidth / 2;

            // ���������� ����� �������
            if (lidarObjects.Count < 10 && _random.Next(0, 100) < 30)
            {
                lidarObjects.AddRange(lidarSimulator.GenerateTestPoints(roadX, pictureBox1.Height));
            }

            // ������� ��� ������� ����
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

            // ������� �������, ������� ����� �� ������ �������
            lidarObjects = lidarObjects
                .Where(o =>
                {
                    switch (o.Type)
                    {
                        case ObjectType.Car:
                        case ObjectType.OncomingCar:
                            return o.Position.Y < pictureBox1.Height;
                        case ObjectType.Pedestrian:
                            return o.Position.X < pictureBox1.Width + 20; // �������, ����� ������� ������� ������
                        default:
                            return true;
                    }
                })
                .ToList();
            // ��������� PictureBox
            pictureBox1.Invalidate();

            // ��������� DataGridView
            var obstacles = obstacleDetector.DetectObstacles(lidarObjects);
            UpdateDataGridView(obstacles);

            // ��������� ������������
            string decision = decisionMaker.MakeDecision(obstacles, carY);

            labelDecision.Text = $"������������: {decision}";

            // ��������� �����
            switch (decision)
            {
                case "����� �� ��������� ������":
                    labelDecision.BackColor = Color.Green;
                    labelDecision.ForeColor = Color.White;
                    break;
                case "����� �� �������":
                    labelDecision.BackColor = Color.Orange;
                    labelDecision.ForeColor = Color.Black;
                    break;
                case "�������� ��������":
                    labelDecision.BackColor = Color.Red;
                    labelDecision.ForeColor = Color.White;
                    break;
                default:
                    labelDecision.BackColor = SystemColors.Control;
                    labelDecision.ForeColor = SystemColors.ControlText;
                    break;
            }
            // ��������� �������� ��������
            roadLineOffset += 3;
            if (roadLineOffset > 50)
                roadLineOffset = 0;

            // �������������� PictureBox
            pictureBox1.Invalidate();
        }

        private void UpdateDecisionLabel(string decision)
        {
            switch (decision)
            {
                case "����� �� ��������� ������":
                    labelDecision.BackColor = Color.Green;
                    labelDecision.ForeColor = Color.White;
                    break;
                case "����� �� �������":
                    labelDecision.BackColor = Color.Orange;
                    labelDecision.ForeColor = Color.Black;
                    break;
                case "�������� ��������":
                    labelDecision.BackColor = Color.Red;
                    labelDecision.ForeColor = Color.White;
                    break;
                default:
                    labelDecision.BackColor = SystemColors.Control;
                    labelDecision.ForeColor = SystemColors.ControlText;
                    break;
            }

            labelDecision.Text = $"������������: {decision}";
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
                    labelDecision.Text = "��������� ��������������";
                    labelDecision.BackColor = Color.Gray;
                }
                else
                {
                    timer.Start();
                    string decision = decisionMaker.MakeDecision(obstacles, carY);
                    UpdateDecisionLabel(decision); // ��������� ������������
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
