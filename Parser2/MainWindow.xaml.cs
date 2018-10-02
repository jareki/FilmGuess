using HtmlAgilityPack;
using ParserKinopoisk;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Parser2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static BlockingCollection<FilmShot> imagedata;

        const int start_year = 2017;

        Task<bool> LoadImageData(FilmData film)
        {
            WebConnect connect = new WebConnect("https://www.kinopoisk.ru/film/");
            Txt.Text += $"\nGetting images of film={film.filmID}...";
            TaskCompletionSource<bool> tsc = new TaskCompletionSource<bool>();

            LoadCompletedEventHandler handler = null;
            handler = (o, s) =>
                  {
                      web.LoadCompleted -= handler;
                      dynamic doc = web.Document;
                      string html = doc.documentElement.InnerHtml;

                      var film_imgs = connect.GetImageList(html, film.filmID).Result;
                      if (film_imgs != null)
                          foreach (var item in film_imgs)
                              imagedata.Add(item);
                      tsc.SetResult(true);
                  };
            web.LoadCompleted += handler;
            web.Navigate($"https://www.kinopoisk.ru/film/{film.filmID}/stills/");            
            return tsc.Task;
        }
        

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            imagedata = new BlockingCollection<FilmShot>();

            WebConnect connect = new WebConnect("https://www.kinopoisk.ru/lists/ord/rating_kp/");
            var films = new List<FilmData>();
            /*
             * int year = DateTime.Now.Year;
            for (int y = start_year; y <= year; y++)
            {
                for (int num = 1; num <= 10; num++)
                {
                    Txt.Text+=$"\nParsing page {num} of kinopoisk...";
                    films.AddRange(await connect.GetFilmsList(y, num));
                    //await Task.Delay(10 * 1000);
                }
            }

            Txt.Text+="\n---------------------------------------------------";
            */
            DbManager db = new DbManager("data1.db");
            films = db.SelectNoImgFilms();
            connect = new WebConnect("https://www.kinopoisk.ru/film/");
            var images = new List<FilmShot>();

            //Parallel.ForEach(films, LoadImageData);
            foreach (var film in films)
            {
                try
                {
                    await LoadImageData(film);
                }
                catch { }
                await Task.Delay(3*1000);
            }

            images = imagedata.ToList();

            Txt.Text += "\nWriting database...";

            //DbManager db = new DbManager("data1.db");
            //foreach (var film in films)
            //    db.Insert(film);
            db.Insert(images);
            db.Close();

            Txt.Text += "\nFINISHED!!!!";
            //Console.ReadKey();
        }
    

    public MainWindow()
        {
            InitializeComponent();
        }
    }
}
