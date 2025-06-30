using NAMI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace NAMI.Services
{
    class SignRecognizer
    {

        public TrafficSign Recognize(Bitmap image)
        {
            if (image != null)
            {
                
                return new TrafficSign { Type = "Stop" };
            }

            return null;
        }
    }
}
