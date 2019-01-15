using EDC.ML.Samples.TTS.Services;
using System;

namespace EDC.ML.Samples.TTS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            XunFeiCloudTtsService ttsService = new XunFeiCloudTtsService();
            ttsService.TextToSpeech = "大家好我是第一个语音合成！";
            ttsService.SavePath = "./TTS-Result/";
            ttsService.FileName = "Hello-TTS";
            ttsService.Speed = "60";

            var result = ttsService.GetTtsResult();
            Console.WriteLine(result);

            Console.ReadKey();
        }
    }
}
