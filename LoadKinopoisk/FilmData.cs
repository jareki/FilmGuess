using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net.Attributes;

namespace LoadKinopoisk
{
    public class RatingData
    {
        public double rating { get; set; }
        public double ratingIMDb { get; set; }
        public string ratingVoteCount { get; set; }
        public string ratingIMDbVoteCount { get; set; }
    }
    [Table("FilmData")]
    public class FilmData
    {
        [PrimaryKey,AutoIncrement]
        public int id { get; set; }
        [Unique]
        public int filmID { get; set; }
        [Ignore]
        public RatingData ratingData { get; set; }
        public string nameRU { get; set; }
        public string nameEN { get; set; }
        public int year { get; set; }

        public double rating { get; set; }
        public double ratingIMDb { get; set; }
        public int ratingVoteCount { get; set; }
        public int ratingIMDbVoteCount { get; set; }

        public bool IsGood
        {
            get
            {
                if (ratingData != null)
                {
                    if (ParseInt(ratingData.ratingVoteCount) + ParseInt(ratingData.ratingIMDbVoteCount) > 2000 &&
                        (ratingData?.rating > 5 || ratingData?.ratingIMDb > 5))
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
        }

        public void Fill()
        {
            rating = ratingData.rating;
            ratingIMDb = ratingData.ratingIMDb;
            ratingVoteCount = ParseInt(ratingData.ratingVoteCount);
            ratingIMDbVoteCount = ParseInt(ratingData.ratingIMDbVoteCount);
        }

        int ParseInt (string str)
        {
            if (!String.IsNullOrEmpty(str))
                return int.Parse(str.Replace(" ", ""));
            return 0;
        }
    }
    
}
