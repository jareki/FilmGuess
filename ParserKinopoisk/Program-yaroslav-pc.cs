using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParserKinopoisk
{
    class Program
    {
       // static BlockingCollection<FilmData> filmdata;
        static BlockingCollection<FilmShot> imagedata;

        const int start_year = 2018;

        static void LoadImageData(FilmData film)
        {
            WebConnect connect = new WebConnect("https://www.kinopoisk.ru/film/");
            Console.WriteLine($"Getting images of film={film.filmID}...");
            var film_imgs = connect.GetImageList(film.filmID).Result;
            if (film_imgs != null)
                foreach (var item in film_imgs)
                    imagedata.Add(item);
        }

        static void Main(string[] args)
        {
            imagedata = new BlockingCollection<FilmShot>();

            WebConnect connect = new WebConnect("https://www.kinopoisk.ru/lists/ord/rating_kp/");
            var films = new List<FilmData>();
            int year = DateTime.Now.Year;
            for (int y = start_year; y <= year; y++)
            {
                for (int num = 1; num < 11; num++)
                {
                    Console.WriteLine($"Parsing page {num} of kinopoisk...");
                    films.AddRange(connect.GetFilmsList(y, num).Result);
                    Thread.Sleep(1000);
                }
            }

            Console.WriteLine("---------------------------------------------------");

            connect = new WebConnect("https://www.kinopoisk.ru/film/");
            var images = new List<FilmShot>();

            Thread.Sleep(10 * 1000);
            //Parallel.ForEach(films, LoadImageData);
            foreach (var film in films)
            {
                LoadImageData(film);
                Thread.Sleep(10*1000);
            }


            images = imagedata.ToList();

            Console.WriteLine("Writing database...");

            DbManager db = new DbManager("data99.db");
            foreach (var film in films)
                db.Insert(film);
            db.Insert(images);
            db.Close();

            Console.WriteLine("FINISHED!!!!");
            Console.ReadKey();
        }
    }
}
