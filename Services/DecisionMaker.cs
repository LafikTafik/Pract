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
            float closestPedestrianDistance = float.MaxValue;

            foreach (var obj in obstacles)
            {
                float distanceToCar = Math.Abs(obj.Position.Y - carY);

                if (distanceToCar < PedestrianWarningDistance && obj.Type == ObjectType.Pedestrian)
                {
                    closestPedestrianDistance = Math.Min(closestPedestrianDistance, distanceToCar);
                }

                if (distanceToCar < SafeDistance)
                {
                    switch (obj.Type)
                    {
                        case ObjectType.Car:
                            hasCar = true;
                            break;
                        case ObjectType.OncomingCar:
                            hasOncomingCar = true;
                            break;
                    }
                }
            }

            if (closestPedestrianDistance < PedestrianWarningDistance)
            {
                return "Пешеход - Сбросьте скорость";
            }

            if (hasCar && !hasOncomingCar)
            {
                return "Обгон по встречной полосе";
            }

            if (hasCar && hasOncomingCar)
            {
                return "Обгон по обочине";
            }

            return "Движение разрешено";
        }
    }
}
