using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;



namespace FrequentWordCalculator
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index(string id)
        {
            // for edit view
            if (!string.IsNullOrEmpty(id))
            {
                // Set the name of the table
                TableManager TableManagerObj = new TableManager("wordFrequencyCount");

                // Retrieve the WordFrequencyCount object where RowKey eq value of id
                List<WordFrequencyCount> SutdentListObj = TableManagerObj.RetrieveEntity<WordFrequencyCount>("RowKey eq '" + id + "'");

                WordFrequencyCount WordFrequencyCountObj = SutdentListObj.FirstOrDefault();
                return View(WordFrequencyCountObj);
            }

            // new entry view
            return View(new WordFrequencyCount());
        }


        /*upload file and evaluate words from book*/
        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                try
                {
                    /*Upload file*/
                    string path = Path.Combine(Server.MapPath("~/UploadedFile"),
                                               Path.GetFileName(file.FileName));
                    file.SaveAs(path);
                    ViewBag.Message = "File uploaded successfully";

                    /*dictionary to hold string of words with frequency of occurance*/
                    Dictionary<string, int> WordCounts = new Dictionary<string, int>();

                    /*dictionary to hold  words with scrabble score*/
                    Dictionary<string, int> ScWordCounts = new Dictionary<string, int>();

                    var content = System.IO.File.ReadAllText(path);
                    var wordPattern = new Regex(@"\w+");
                    foreach (Match match in wordPattern.Matches(content))
                    {
                        if (!WordCounts.ContainsKey(match.Value))
                            WordCounts.Add(match.Value, 1);
                        else
                            WordCounts[match.Value]++;
                    }
                    //sort words with order of most frequent occurance 
                    var items = WordCounts.OrderByDescending(x => x.Value);

                    //calculate score
                    int Wscore = 0;
                    foreach (var item in items)
                    {
                        Wscore = ScoreTest(item.Key);
                        ScWordCounts.Add(item.Key, Wscore);
                    }
                    //sort words with order of high scrabble score
                    var sortedScoreList = ScWordCounts.OrderByDescending(x => x.Value);


                    //save in DB(azure storage)
                    WordFrequencyCount WordFrequencyCountObj = new WordFrequencyCount();

                    WordFrequencyCountObj.localFilePath = path;

                    WordFrequencyCountObj.mostFrequentWord = " { " + items.First().Key + "} occured {" + items.First().Value + "} times ";
                    //WordFrequencyCountObj.occurance = items.First().Value;

                    WordFrequencyCountObj.mostFrequentSevenCharacterWord = " { " + items.Where(key => key.Key.Length == 7).First().Key + "} occured {" + items.Where(key => key.Key.Length == 7).First().Value + "} times ";
                    //WordFrequencyCountObj.occured = items.Where(key => key.Key.Length == 7).First().Value;

                    WordFrequencyCountObj.highScoreScrabblerWord = " { " + sortedScoreList.First().Key + "} with a score of {" + sortedScoreList.First().Value + "}";
                    //WordFrequencyCountObj.sc_occured = sortedScoreList.First().Value;

                    // Insert
                    if (!string.IsNullOrEmpty(path))
                    {
                        WordFrequencyCountObj.PartitionKey = "wordFrequencyCount";
                        WordFrequencyCountObj.RowKey = Guid.NewGuid().ToString();
                        TableManager TableManagerObj = new TableManager("wordFrequencyCount");
                        TableManagerObj.InsertEntity<WordFrequencyCount>(WordFrequencyCountObj, true);
                    }

                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            }
            else
            {
                ViewBag.Message = "You have not specified a file.";
            }
            return RedirectToAction("Get");
        }

        public ActionResult Get()
        {
            TableManager TableManagerObj = new TableManager("wordFrequencyCount");
            // Get all WordFrequencyCount object, pass null as query
            List<WordFrequencyCount> WordFrequencyCountObj = TableManagerObj.RetrieveEntity<WordFrequencyCount>(null);
            return View(WordFrequencyCountObj);
        }

        /*letters holds the score for each letter*/
        private Dictionary<char, int> LetterValues = new Dictionary<char, int> {
        {'A',1}, {'E',1}, {'I',1}, {'O',1}, {'U',1}, {'L',1}, {'N',1}, {'R',1}, {'S',1}, {'T',1},
        {'D',2}, {'G',2},
        {'B',3}, {'C',3}, {'M',3}, {'P',3},
        {'F',4}, {'H',4}, {'V',4}, {'W',4}, {'Y',4},
        {'K',5},
        {'J',8}, {'X',8},
        {'Q',10}, {'Z',10}
    };

        /*Score calcualtes the score for scrabble word*/
        public int ScoreTest(string input)
        {
            return input.ToUpper()
                       .Where(LetterValues.ContainsKey)
                       .Sum(character => LetterValues[character]);
        }

    }
}