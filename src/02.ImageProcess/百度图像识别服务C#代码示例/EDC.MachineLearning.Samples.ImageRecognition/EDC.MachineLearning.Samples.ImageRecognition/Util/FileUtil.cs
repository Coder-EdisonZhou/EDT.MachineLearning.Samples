using System.Collections.Specialized;
using System.IO;

namespace EDC.MachineLearning.Samples.ImageRecognition
{
    public static class FileUtil
    {
        /// <summary>
        /// 遍历 rootdir目录下的所有文件
        /// </summary>
        /// <param name="rootdir">目录名称</param>
        /// <returns>该目录下的所有文件</returns>
        public static StringCollection GetAllFiles(string rootdir, string fileExtension = ".jpg")
        {
            StringCollection result = new StringCollection();
            GetAllFiles(rootdir, fileExtension, result);
            return result;
        }

        /// <summary>
        /// 作为遍历文件函数的子函数
        /// </summary>
        /// <param name="parentDir">目录名称</param>
        /// <param name="result">该目录下的所有文件</param>
        private static void GetAllFiles(string parentDir, string fileExtension, StringCollection result)
        {
            //获取目录parentDir下的所有的文件，并过滤得到所有的文本文件
            string[] file = Directory.GetFiles(parentDir, fileExtension);
            for (int i = 0; i < file.Length; i++)
            {
                result.Add(file[i]); 
            }
        }
    }
}
