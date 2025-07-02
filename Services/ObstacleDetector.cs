using NAMI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAMI.Services
{
    public class ObstacleDetector
    {
        private const int MaxDistanceToShow = 400; // Показываем только близкие объекты

        public List<DetectedObject> DetectObstacles(List<DetectedObject> lidarObjects)
        {
            var obstacles = new List<DetectedObject>();

            foreach (var obj in lidarObjects)
            {
                // Фильтруем по Y — показываем только близкие
                if (obj.Position.Y < MaxDistanceToShow)
                {
                    obstacles.Add(obj); // сохраняем оригинальный тип
                }
            }

            return obstacles;
        }
    }
}