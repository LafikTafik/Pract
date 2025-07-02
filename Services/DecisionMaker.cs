using NAMI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NAMI.Services
{
    public class DecisionMaker
    {
        private const int SafeDistance = 200; // если объект ближе, чем это значение — опасность
        private const int PedestrianWarningDistance = 300;

        public string MakeDecision(List<DetectedObject> obstacles, float carY)
        {
            bool hasCar = false;
            bool hasOncomingCar = false;
            float closestPedestrianDistance = float.MaxValue; // начальное значение

            foreach (var obj in obstacles)
            {
                if (obj.Position.Y < SafeDistance)
                {
                    switch (obj.Type)
                    {
                        case ObjectType.Car:
                            hasCar = true;
                            break;
                        case ObjectType.OncomingCar:
                            hasOncomingCar = true;
                            break;
                        case ObjectType.Pedestrian:
                            // Рассчитываем расстояние до пешехода
                            float distanceToPedestrian = Math.Abs(obj.Position.Y - carY);
                            if (distanceToPedestrian < closestPedestrianDistance)
                            {
                                closestPedestrianDistance = distanceToPedestrian;
                            }
                            break;
                    }
                }
            }

            // Приоритеты:
            if (closestPedestrianDistance < PedestrianWarningDistance)
            {
                return "Сбросьте скорость";
            }

            if (hasCar && !hasOncomingCar)
            {
                return "Обгон по встречной полосе";
            }

            if (hasCar && hasOncomingCar)
            {
                return "Обгон по обочине";
            }

            // Если ничего не нашли, вернём дефолтное сообщение
            return "Движение разрешено";
        }
    }
}
