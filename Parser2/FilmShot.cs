using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserKinopoisk
{
    [Table("FilmShot")]
    class FilmShot
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public int filmid { get; set; }
        public string image { get; set; }
    }
}
