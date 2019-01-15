// <copyright file="XunFeiCloudService" company="EDC">
// Copyright (c) Edison Zhou Corporation 2019. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace EDC.ML.Samples.TTS.Services
{
    public class XunFeiCloudTtsService
    {
        // Header Type : audio/mpeg
        private const string AUDIO_MPEG_TYPE = "audio/mpeg";
        // AppID, AppKey 从讯飞开放云平台获取
        public string AppID { get; set; } = "5c3806f5";
        public string ApiKey { get; set; } = "a99fff9fc537d883a18a37bf56dc8f28";
        // 要进行合成的文字
        public string TextToSpeech { get; set; } = "这是一段测试文字";
        // 讯飞TTS服务API地址
        public string ServiceUrl { get; set; } = "http://api.xfyun.cn/v1/service/v1/tts";
        // aue = raw, 音频文件保存类型为 wav
        // aue = lame, 音频文件保存类型为 mp3
        public string AUE { get; set; } = "raw";
        // 音频采样率，可选值：audio/L16;rate=8000，audio/L16;rate=16000
        public string AUF { get; set; } = "audio/L16;rate=16000";
        // 发音人，可选值：详见控制台-我的应用-在线语音合成服务管理-发音人授权管理
        public string VoiceName { get; set; } = "xiaoyan";
        // 引擎类型，可选值：aisound（普通效果），intp65（中文），intp65_en（英文），
        // mtts（小语种，需配合小语种发音人使用），x（优化效果），
        // 默认为intp65
        public string EngineType { get; set; } = "intp65";
        // 语速，可选值：[0-100]，默认为50
        public string Speed { get; set; } = "50";
        // 音量，可选值：[0-100]，默认为50
        public string Volume { get; set; } = "50";
        // 音高，可选值：[0-100]，默认为50
        public string Pitch { get; set; } = "50";
        // URL加密后的TextToSpeech
        public string Bodys { get; set; }
        // 要保存的文件夹路径
        public string SavePath { get; set; } = "./Output/";
        // 要保存的文件名
        public string FileName { get; set; } = $"TTS-{ Guid.NewGuid().ToString() }";

        static XunFeiCloudTtsService()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public XunFeiCloudTtsService()
        { }

        public XunFeiCloudTtsService(string appID, string apiKey, string serviceUrl)
        {
            AppID = appID;
            ApiKey = apiKey;
            ServiceUrl = serviceUrl;
        }

        public string GetTtsResult()
        {
            SetTextDataBodys();

            string param = "{\"aue\":\"" + AUE + "\",\"auf\":\"" + AUF + "\",\"voice_name\":\"" + VoiceName + "\",\"engine_type\":\""
                + EngineType + "\",\"speed\":\"" + Speed + "\",\"volume\":\"" + Volume + "\",\"pitch\":\"" + Pitch + "\"}";
            // 获取十位的时间戳
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            string curTime = Convert.ToInt64(ts.TotalSeconds).ToString();
            // 对参数先utf-8然后用base64编码
            byte[] paramData = Encoding.UTF8.GetBytes(param);
            string paraBase64 = Convert.ToBase64String(paramData);
            // 形成签名
            string checkSum = Md5(ApiKey + curTime + paraBase64);
            // 设置HttpHeaders
            var ttsRequest = (HttpWebRequest)WebRequest.Create(ServiceUrl);
            ttsRequest.Method = "POST";
            ttsRequest.ContentType = "application/x-www-form-urlencoded";
            ttsRequest.Headers.Add("X-Param", paraBase64);
            ttsRequest.Headers.Add("X-CurTime", curTime);
            ttsRequest.Headers.Add("X-Appid", AppID);
            ttsRequest.Headers.Add("X-CheckSum", checkSum);

            using (Stream requestStream = ttsRequest.GetRequestStream())
            {
                using (StreamWriter streamWriter = new StreamWriter(requestStream, Encoding.GetEncoding("gb2312")))
                {
                    streamWriter.Write(Bodys);
                }
            }

            string responseText = string.Empty;
            HttpWebResponse ttsResponse = ttsRequest.GetResponse() as HttpWebResponse;
            using (Stream responseStream = ttsResponse.GetResponseStream())
            {
                using (StreamReader streamReader = new StreamReader(responseStream, Encoding.GetEncoding("UTF-8")))
                {
                    string headerType = ttsResponse.Headers["Content-Type"];
                    if (headerType.Equals(AUDIO_MPEG_TYPE))
                    {
                        responseText = GetSuccessResponseText(ttsResponse);
                    }
                    else
                    {
                        responseText = streamReader.ReadToEnd();
                    }
                }
            }

            return responseText;
        }

        #region 私有辅助方法
        /// <summary>
        /// 获取请求成功的响应文本
        /// </summary>
        /// <param name="ttsResponse">HttpWebResponse</param>
        /// <returns>响应Headers文本</returns>
        private string GetSuccessResponseText(HttpWebResponse ttsResponse)
        {
            string responseText = string.Empty;
            using (Stream stream = ttsResponse.GetResponseStream())
            {
                MemoryStream memoryStream = StreamToMemoryStream(stream);
                if (!Directory.Exists(SavePath))
                {
                    Directory.CreateDirectory(SavePath);
                }

                string fileType = string.Empty;
                switch (AUE.ToLower())
                {
                    case "raw":
                        fileType = "wav";
                        break;
                    case "lame":
                        fileType = "lame";
                        break;
                }

                File.WriteAllBytes($"{SavePath}{FileName}.{fileType}", streamTobyte(memoryStream));
                responseText = ttsResponse.Headers.ToString();
            }

            return responseText;
        }

        /// <summary>
        ///  对要合成语音的文字先用utf-8然后进行URL加密
        /// </summary>
        private void SetTextDataBodys()
        {
            byte[] textData = Encoding.UTF8.GetBytes(TextToSpeech);
            TextToSpeech = HttpUtility.UrlEncode(textData);
            Bodys = string.Format("text={0}", TextToSpeech);
        }

        /// <summary>
        /// 生成令牌 ：X-CheckSum
        /// 计算方法：MD5(apiKey + curTime + param)，三个值拼接的字符串，进行MD5哈希计算（32位小写），其中apiKey由讯飞提供，调用方管理。
        /// </summary>
        /// <param name="token">apiKey + curTime + param</param>
        /// <returns>X-CheckSum</returns>
        private string Md5(string token)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] bytes = Encoding.UTF8.GetBytes(token);
            bytes = md5.ComputeHash(bytes);
            md5.Clear();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(Convert.ToString(bytes[i], 16).PadLeft(2, '0'));
            }

            return sb.ToString().PadLeft(32, '0');
        }

        /// <summary>
        /// 将流转换为缓存流
        /// </summary>
        /// <param name="instream">输入流</param>
        /// <returns>输出流</returns>
        private MemoryStream StreamToMemoryStream(Stream instream)
        {
            MemoryStream outstream = new MemoryStream();
            const int bufferLen = 4096;
            byte[] buffer = new byte[bufferLen];
            int count = 0;
            while ((count = instream.Read(buffer, 0, bufferLen)) > 0)
            {
                outstream.Write(buffer, 0, count);
            }

            return outstream;
        }

        /// <summary>
        /// 把缓存流转换成字节组
        /// </summary>
        /// <param name="memoryStream">缓存刘</param>
        /// <returns>字节数组</returns>
        private byte[] streamTobyte(MemoryStream memoryStream)
        {
            byte[] buffer = new byte[memoryStream.Length];
            memoryStream.Seek(0, SeekOrigin.Begin);
            memoryStream.Read(buffer, 0, buffer.Length);

            return buffer;
        }
        #endregion
    }
}
