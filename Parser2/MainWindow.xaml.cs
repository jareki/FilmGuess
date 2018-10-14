using HtmlAgilityPack;
using ParserKinopoisk;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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

        const int start_year = 2018;

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

                      var film_imgs = connect.GetImageList(html, film.filmID);
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
            
            int year = DateTime.Now.Year;
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
            var images = new List<FilmShot>();
            
            //DbManager db = new DbManager("data.db");
            //films = db.SelectNoImgFilms();
            connect = new WebConnect("https://www.kinopoisk.ru/film/");
            
            
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

            DbManager db = new DbManager("data.db");
            foreach (var film in films)
                db.Insert(film);
            db.Insert(images);
            db.Close();

            Txt.Text += "\nFINISHED!!!!";
            //Console.ReadKey();
        }
    

    public MainWindow()
        {
            InitializeComponent();
        }

        private void web_Navigated(object sender, NavigationEventArgs e)
        {
            SetSilent(web, true);
        }

        public static void SetSilent(WebBrowser browser, bool silent)
        {
            if (browser == null)
                throw new ArgumentNullException("browser");

            // get an IWebBrowser2 from the document
            IOleServiceProvider sp = browser.Document as IOleServiceProvider;
            if (sp != null)
            {
                Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                if (webBrowser != null)
                {
                    webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
                }
            }
        }


        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }
    }
}
