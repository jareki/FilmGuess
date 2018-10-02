using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmGuess.Models
{
    class FilmShot
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public int filmid { get; set; }
        [Unique]
        public string image { get; set; }
    }
}
