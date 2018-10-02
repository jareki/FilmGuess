using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LoadKinopoisk
{
    class Program
    {
        static BlockingCollection<FilmData> filmdata;
        static BlockingCollection<ImageData> imagedata;
        static int counter = 0;

        const int start_id = 700000;
        const int end_id = 1000000;

        static void LoadFilmData(int id)
        {
            WebConnect web = new WebConnect("http://api.kinopoisk.cf");
            FilmData item;
            item = web.GetFilm(id).Result;
            if (item?.IsGood == true)
            {
                item.Fill();
                filmdata.Add(item);
            }
            else
                Thread.Sleep(100);

            counter++;
            if (counter % 100 == 0)
                Console.WriteLine(counter);

        }

        static void SaveFilmData()
        {
            DbManager db = new DbManager();
            FilmData item;

            while (!filmdata.IsCompleted)
            {
                if (filmdata.TryTake(out item))
                    db.Insert(item);
            }
        }
        
        static void LoadImageData(int id)
        {
            WebConnect web = new WebConnect("http://api.kinopoisk.cf");
            ImageData item = web.GetImages(id).Result;

            if (item == null)
                item = new ImageData { filmID = id };
            if (item?.gallery?.kadr != null)
                item.Fill();

            imagedata.Add(item);

            counter++;
            if (counter % 100 == 0)
                Console.WriteLine(counter);
        }

        static void SaveImageData()
        {
            DbManager db = new DbManager();
            ImageData item;
            while (!imagedata.IsCompleted)
            {
                if (imagedata.TryTake(out item))
                {
                    if (item?.images != null)
                        db.Insert(item.images);
                    else
                    {
                        db.DeleteFim(item.filmID);
                        Console.WriteLine($"Film={item.filmID} has been deleted cause imageless!");
                    }
                }
            }
            db.Close();
        }
        
        static void LoadFilmsFromKp()
        {
            filmdata = new BlockingCollection<FilmData>(10);
            Task s = new Task(SaveFilmData);
            s.Start();

            for (int i = start_id; i < end_id; i += 1000)
            {
                Console.WriteLine($"Start adding film data from {i} to {i + 1000}");

                counter = 0;
                Parallel.For(i, i + 1000, LoadFilmData);

                Console.WriteLine($"Data from {i} to {i + 1000} had been added");
                Console.WriteLine("-------------------------------------------------");
                Console.WriteLine("----------------Waiting 10 seconds----------------");
                Console.WriteLine("-------------------------------------------------");
                Thread.Sleep(1000 * 10);
            }

            filmdata.CompleteAdding();
            try
            {
                Task.WaitAll(s);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                s.Dispose();
                filmdata.Dispose();
            }

            Console.WriteLine();
            Console.WriteLine("ADDING FILM DATA IS COMPLETED!!!!!!!!!!!!!!!");
        }

        static void LoadImagesFromKp()
        {
            imagedata = new BlockingCollection<ImageData>(10);
            Task s = new Task(SaveImageData);
            s.Start();

            DbManager db = new DbManager();
            int[] ids = db.SelectIds();
            db.Close();

            Console.WriteLine($"Start adding image data");
            counter = 0;            
            
            Parallel.ForEach(ids, LoadImageData);
            imagedata.CompleteAdding();
            try
            {
                Task.WaitAll(s);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                s.Dispose();
                imagedata.Dispose();
            }

            Console.WriteLine();
            Console.WriteLine("ADDING IMAGE DATA IS COMPLETED!!!!!!!!!!!!!!!");
        }
        
        static void Main(string[] args)
        {
            //1.
            //LoadFilmsFromKp();
            //2.
            //LoadImagesFromKp();

            DbManager db = new DbManager();
            db.UpdateImages();
            db.Close();

            Console.WriteLine("Finished!");            
            Console.ReadKey();

        }
    }
}
