﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ParserKinopoisk
{
    class WebConnect
    {
        Uri baseAddress;

        public WebConnect(string uri)
        {
            baseAddress = new Uri(uri);
        }

        async Task<string> GetResponse(string param_string)
        {
            try
            {
                using (var httpclient = new HttpClient { BaseAddress = baseAddress })
                {
                    //httpclient.DefaultRequestHeaders.Add("User-Agent",
                    //             "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident / 6.0)");
                    //httpclient.DefaultRequestHeaders.Add("Host", "www.kinopoisk.ru:443");
                    //httpclient.DefaultRequestHeaders.Add("Accept", "text / html, application / xhtml + xml, application / xml; q = 0.9,image / webp,image / apng,*/*;q=0.8");
                    //httpclient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                    //httpclient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,uk;q=0.8,ru;q=0.7");
                    //httpclient.DefaultRequestHeaders.Add("Cookie", "PHPSESSID=acves29a8mep728e8usca6mpd6; yandex_gid=143; desktop_session_key=2aa1ff6214bd69f49034817443ba339d549e81b0ec0df6b651a1ad63525285b7ef46c47688421a6e9cdba252f184feb2873991729962cc29f3a6e497db4206c7da8b74c32666eae10c14df3b2b455a51; desktop_session_key.sig=wr4KOKP6CZWaSd4Bzhysufn9R3g; yandexuid=7839672051513866813; my_perpages=%5B%5D; _ym_uid=1518637208818912632; Session_id=3%3A1520689404.5.0.1520689404790%3AsVWOWw%3A11C.1%7C1110000014810420.-1.0%7C30%3A170284.99.JRTf0qI9vm_puxjBreNHI8VGN-8; uid=14810420; mykp_button=edit_main; tc=49; mobile=no; noflash=true; refresh_yandexuid=7839672051513866813; _ym_isad=1; loc2=win; user_country=ua; _ym_visorc_22663942=b");
                    //httpclient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                    httpclient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.181 Safari/537.36");
                    //httpclient.DefaultRequestHeaders.Add("X-Compress", "null");
                    return await httpclient.GetStringAsync(param_string);
                }
            }
            catch
            {
                return String.Empty;
            }
        }

        public async Task<List<FilmData>> GetFilmsList(int year, int page_num)
        {
            string response_data = await GetResponse($"m_act[year]/{year}/m_act[all]/page/page/{page_num}/");

            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(response_data);

            var films_html = html.DocumentNode.SelectNodes("//div[@class='item _NO_HIGHLIGHT_']").ToArray();
            var films = new List<FilmData>();
            films.Clear();

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
            return films;
        }

        FilmData GetFilmInfo(HtmlNode inner_html)
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
                if (rating_imdb!=null)
                    film.ratingIMDb = double.Parse(rating_imdb.Substring(6));
                if (imdb_votecount!=null)
                    film.ratingIMDbVoteCount = int.Parse(imdb_votecount);
            }
            catch
            {
                return null;
            }            

            return film;
        }
        public async Task<List<FilmShot>> GetImageList(string htmltext, int filmid)
        {
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(htmltext);

            var table_html = html.DocumentNode.SelectSingleNode("//table[@class='fotos']");
            if (table_html != null)
            {
                var images = new List<FilmShot>();
                images.Clear();

                var images_html = table_html.SelectNodes(".//img").ToArray();
                foreach (var image_node in images_html)
                {
                    var image = new FilmShot();
                    image.filmid = filmid;
                    var str = image_node.GetAttributeValue("src", "").Replace("sm_","");
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