using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserKinopoisk
{
    public static class FilmsLoader
    {
        public static async Task<List<FilmData>> GetFilms(int year, int pagecount)
        {
            HtmlDocument html = new HtmlDocument();
            using (var driver = new KPWebDriver(3))
            {
                var films = new List<FilmData>();
                films.Clear();

                for (int i = 1; i < pagecount; i++)
                {
                    html.LoadHtml(await driver.LoadFilmsPage(year, i));
                    var films_html = html.DocumentNode.SelectNodes("//div[@class='item _NO_HIGHLIGHT_']").ToArray();
                    
                    foreach (var film_node in films_html)
                    {

                        var film = GetFilmInfo(film_node);
                        try
                        {
                            if (film.IsGood)
                                films.Add(film);
                        }
                        catch
                        {
                            Console.WriteLine();
                        }
                    }                    
                }
                return films;
            }            
        }

        static FilmData GetFilmInfo(HtmlNode inner_html)
        {
            int left, right;

            var film = new FilmData();
            film.filmID = film.year = film.ratingVoteCount = film.ratingIMDbVoteCount = 0;
            film.rating = film.ratingIMDb = 0;

            string name_ru = inner_html.SelectSingleNode(".//div[@class='name']//a")?.InnerText;
            string name_en_year = inner_html.SelectSingleNode(".//div[@class='name']//span")?.InnerText;
            string rating = inner_html.SelectSingleNode(".//div[contains(@class,'numVote')]")?.InnerText;
            string rating_imdb = inner_html.SelectSingleNode(".//div[contains(@class,'imdb')]")?.InnerText;
            string imdb_votecount = inner_html.SelectSingleNode(".//div[contains(@class,'imdb')]//span")?.InnerText;

            name_ru = name_ru?.Replace("&nbsp;", "");
            name_en_year = name_en_year?.Replace("&nbsp;", "");
            rating_imdb = rating_imdb?.Replace(imdb_votecount, "")
                                     .Replace("&nbsp;", "");
            imdb_votecount = imdb_votecount?.Replace("&nbsp;", "");
            rating = rating?.Replace("&nbsp;", "");

            try
            {

                film.filmID = int.Parse(inner_html.Id.Substring(3));
                film.nameRU = name_ru;
                film.nameEN = name_en_year?.Substring(0, name_en_year.IndexOf('(') - 1);
                film.year = int.Parse(name_en_year.Substring(name_en_year.IndexOf('(') + 1, 4));

                if (rating != null)
                {
                    film.rating = double.Parse(rating.Substring(0, rating.IndexOf('(') - 1));

                    left = rating.IndexOf('(');
                    right = rating.IndexOf(')');
                    film.ratingVoteCount = int.Parse(rating.Substring(left + 1, right - left - 1));
                }
                if (rating_imdb != null)
                    film.ratingIMDb = double.Parse(rating_imdb.Substring(6));
                if (imdb_votecount != null)
                    film.ratingIMDbVoteCount = int.Parse(imdb_votecount);
            }
            catch
            {
                return null;
            }

            return film;
        }

    }
}
