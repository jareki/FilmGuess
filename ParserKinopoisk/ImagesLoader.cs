using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserKinopoisk
{
    public static class ImagesLoader
    {
        public static async Task<List<FilmShot>> GetImages(List<FilmData> films)
        {
            using (var driver = new KPWebDriver(3))
            {
                HtmlDocument html = new HtmlDocument();
                var images = new List<FilmShot>();
                foreach (var film in films)
                {
                    try
                    {
                        html.LoadHtml(await driver.LoadImagePage(film.filmID));
                        images.AddRange(LoadImagesData(html, film.filmID));
                    }
                    catch { }
                }
                return images;
            }
        }

        public static async Task<List<FilmShot>> GetImages(int film_id)
        {
            using (var driver = new KPWebDriver(3))
            {
                try
                {
                    HtmlDocument html = new HtmlDocument();
                    html.LoadHtml(await driver.LoadImagePage(film_id));
                    return LoadImagesData(html, film_id);
                }
                catch { return null; }
                
            }
        }

        static List<FilmShot> LoadImagesData(HtmlDocument html, int film_id)
        {
            var table_html = html.DocumentNode.SelectSingleNode("//table[contains(@class,'fotos')]");
            if (table_html != null)
            {
                var images = new List<FilmShot>();
                images.Clear();

                var images_html = table_html.SelectNodes(".//img").ToArray();
                foreach (var image_node in images_html)
                {
                    var image = new FilmShot();
                    image.filmid = film_id;
                    var str = image_node.GetAttributeValue("src", "").Replace("sm_", "");
                    int left = str.LastIndexOf('/');
                    int right = str.LastIndexOf('.');
                    image.image = str.Substring(left, right - left);

                    images.Add(image);
                }
                return images;
            }
            else
                return null;
        }

    }
}
