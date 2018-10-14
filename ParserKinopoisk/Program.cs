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
        const int start_year = 2018;
        const int page_count = 2;

        static async Task DownloadDataToDb()
        {
            //load data
            Console.WriteLine("Parsing kinopoisk...");
            List<FilmData> films = await FilmsLoader.GetFilms(start_year, page_count);
            Console.WriteLine($"{films.Count} films has been parsed");
            List<FilmShot> images = await ImagesLoader.GetImages(films);
            Console.WriteLine($"{images.Count} images has been parsed");

            //delete films without images
            films = (from f in films
                     where images.Any(img => img.filmid == f.filmID)
                     select f).ToList();

            Console.WriteLine("Write to db");
            //write to db

            DbManager db = new DbManager("data.db");
            db.Insert(films);
            db.Insert(images);
            db.Close();
        }
        
        static void Main(string[] args)
        {
            Task t = new Task(async() => await DownloadDataToDb());
            t.Start();
            Task.WaitAll(t);

            Console.WriteLine("FINISHED!!!!");
            Console.ReadKey();
        }
    }
}
