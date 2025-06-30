using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAMI.Models
{
    public class SensorData
    {
        public string ID { get; set; }
        public byte DLC { get; set; }
        public byte[] Data { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
