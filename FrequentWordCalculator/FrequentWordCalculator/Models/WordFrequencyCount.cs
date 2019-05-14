using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage.Table;

namespace FrequentWordCalculator
{
    public class WordFrequencyCount : TableEntity
    {
        public WordFrequencyCount() { }

        public string localFilePath { get; set; }
        public string mostFrequentWord { get; set; }
        //public Int64 occurance { get; set; }
        public string mostFrequentSevenCharacterWord { get; set; }
        //public Int64 occured { get; set; }

        public string highScoreScrabblerWord { get; set; }
        //public Int64 sc_occured { get; set; }
    }
}