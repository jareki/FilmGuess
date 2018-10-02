using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadKinopoisk
{
    public class ImageData
    {
        public Gallery gallery { get; set; }
        public int filmID { get; set; }

        public List<FilmShot> images { get; set; }

        public void Fill()
        {
            images = new List<FilmShot>();
            foreach (var item in gallery.kadr)
            {
                images.Add(new FilmShot { filmid = filmID, image = item.image });
            }
        }
    }

    public class Gallery
    {
        public Kadr[] kadr { get; set; }
    }

    public class Kadr
    {
        public string image { get; set; }
        public string socialURL { get; set; }
    }

    public class FilmShot
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public int filmid { get; set; }
        [Unique]
        public string image { get; set; }
    }
}
