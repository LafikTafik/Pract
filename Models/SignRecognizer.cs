using Emgu.CV.CvEnum;
using Emgu.CV;
using NAMI.Foms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAMI.Models
{
    public static class SignRecognizer
    {
        public static string RecognizeSign(Mat image)
        {
            Mat resultImage;
            // Твоя реализация распознавания
            var recognizer = new SignRecognitionForm(); // можно передавать параметры
            return recognizer.DetectTrafficSign(image, out resultImage); // метод должен быть публичным
        }

        public static string RecognizeFromFile(string filePath)
        {
            using (Mat image = CvInvoke.Imread(filePath, ImreadModes.Color))
            {
                if (image.IsEmpty)
                    return "Неизвестный";

                return RecognizeSign(image);
            }
        }
    }
}
