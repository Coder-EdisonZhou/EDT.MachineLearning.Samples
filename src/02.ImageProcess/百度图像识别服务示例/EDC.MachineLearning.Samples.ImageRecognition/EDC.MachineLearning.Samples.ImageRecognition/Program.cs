using Baidu.Aip.ImageClassify;
using Baidu.Aip.ImageSearch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EDC.MachineLearning.Samples.ImageRecognition
{
    public class Program
    {
        private static ImageSearch aipSearchClient;
        private static ImageClassify aipClassifyClient;

        static Program()
        {
            aipSearchClient = new ImageSearch(BaiduAipSimSettings.ApiKey, BaiduAipSimSettings.SecretKey);
            aipClassifyClient = new ImageClassify(BaiduAipRecogSettings.ApiKey, BaiduAipRecogSettings.SecretKey);
        }

        public static void Main(string[] args)
        {
            var filePath = @"Images\沙发2018.jpg";

            // 图片识别Demo
            //ImageClassifyDemo(filePath);
            // 相似图片Demo
            SimilarImageDemo(filePath);

            Console.ReadKey();
        }

        private static void ImageClassifyDemo(string filePath)
        {
            var image = File.ReadAllBytes(filePath);

            try
            {
                var result = aipClassifyClient.AdvancedGeneral(image);
                Console.WriteLine("Api Response :");
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void SimilarImageDemo(string filePath)
        {
            //// 导入
            //var importOptions = new Dictionary<string, object>
            //{
            //    { "brief", "{\"id\":\"none\", \"name\":\"温馨风\", \"url\":\"http://www.edisonchou.com/images/test1.jpg\"}" },
            //    { "tags", "101,1"}
            //};
            //ImportToLib(filePath, importOptions);

            //Console.WriteLine();

            // 检索
            var searchOptions = new Dictionary<string, object>{
                    {"tag_logic", "0"},
                    {"pn", "0"},
                    {"rn", "10"}
                };
            SimilarSearchFromLib(filePath, searchOptions);
        }

        /// <summary>
        /// 多张图片入库
        /// </summary>
        private static void ImportAllToLib()
        {
            var files = FileUtil.GetAllFiles(@"Images\");
            foreach (var file in files)
            {
                ImportToLib(file);
            }
        }

        /// <summary>
        /// 单张图片入库
        /// </summary>
        private static void ImportToLib(string filePath, Dictionary<string, object> options = null)
        {
            var image = File.ReadAllBytes(filePath);
            
            try
            {
                var result = aipSearchClient.SimilarAdd(image, options);
                Console.WriteLine("Api Response :");
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 相似图检索
        /// </summary>
        private static void SimilarSearchFromLib(string filePath, Dictionary<string, object> options = null)
        {
            var image = File.ReadAllBytes(filePath);

            try
            {
                var response = aipSearchClient.SimilarSearch(image, options).ToObject<SearchResponse>();
                var result = response.Result.Take(5);
                Console.WriteLine("Similarity Result :");
                foreach (var item in result)
                {
                    Console.WriteLine("{0}:{1}", item.Brief, item.Score);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
