using System.Collections.Generic;

namespace EDC.MachineLearning.Samples.ImageRecognition
{
    public class SearchResponse
    {
        public bool Has_More { get; set; }
        public string Log_Id { get; set; }
        public int Result_Num { get; set; }
        public List<SearchResult> Result { get; set; }
    }
}
