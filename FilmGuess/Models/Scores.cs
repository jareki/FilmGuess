using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmGuess.Models
{
    public enum Gametype { lives3, minutes3};
    class Scores
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Date { get; set; }
        public int Score { get; set; }
        public int GameType { get; set; }
    }
}
