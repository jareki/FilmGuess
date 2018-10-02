using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserKinopoisk
{
    [Table("FilmData")]
    public class FilmData
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        [Unique]
        public int filmID { get; set; }
        public string nameRU { get; set; }
        public string nameEN { get; set; }
        public int year { get; set; }

        public double rating { get; set; }
        public double ratingIMDb { get; set; }
        public int ratingVoteCount { get; set; }
        public int ratingIMDbVoteCount { get; set; }
        
        [Ignore]
        public bool IsGood
        {
            get
            {
                if (filmID > 0 && year > 0 && (nameEN != string.Empty || nameRU != string.Empty) && !nameRU.Contains("сериал") && !nameRU.Contains("видео"))
                {

                    if (ratingVoteCount + ratingIMDbVoteCount > 2000 &&
                        (rating > 5 || ratingIMDb > 5))
                        return true;
                    else
                        return false;
                }
                else return false;
            }
        }

        int ParseInt(string str)
        {
            if (!String.IsNullOrEmpty(str))
                return int.Parse(str.Replace(" ", ""));
            return 0;
        }
    }
}
