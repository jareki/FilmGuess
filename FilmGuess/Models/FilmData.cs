using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmGuess.Models
{
    class FilmData: IEquatable<FilmData>
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

        public bool Equals(FilmData other)
        {
            if (other == null)
                return false;

            FilmData obj = other as FilmData;
            if (obj == null)
                return false;

            return obj.filmID == this.filmID;

        }
    }
}
