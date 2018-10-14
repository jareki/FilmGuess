using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParserKinopoisk
{
    class SeleniumConnect : IDisposable
    {
        IWebDriver driver;
        string baseAddress;

        public SeleniumConnect(string uri)
        {
            driver = new InternetExplorerDriver();
            //driver.Navigate().GoToUrl("http://google.ru");
            baseAddress = uri;
        }
        
        public List<FilmData> GetFilmsList(int year, int page_num)
        {
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl(Path.Combine(baseAddress,$"m_act[year]/{year}/m_act[all]/page/page/{page_num}/"));
            Thread.Sleep(3000);
            var films = new List<FilmData>();
            var films_divs = FindElements(driver,".//div[contains(@class,'item _NO_HIGHLIGHT_')]");                
            films.Clear();

            foreach (var film_node in films_divs)
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

        FilmData GetFilmInfo(IWebElement element)
        {
            int left, right;

            var film = new FilmData();
            film.filmID = film.year = film.ratingVoteCount = film.ratingIMDbVoteCount = 0;
            film.rating = film.ratingIMDb = 0;

            string name_ru = FindElement(element,".//div[@class='name']//a")?.Text;
            string name_en_year = FindElement(element, ".//div[@class='name']//span")?.Text;
            string rating = FindElement(element, ".//div[contains(@class,'numVote')]")?.Text;
            string rating_imdb = FindElement(element, ".//div[contains(@class,'imdb')]")?.Text;
            string imdb_votecount = FindElement(element, ".//div[contains(@class,'imdb')]//span")?.Text;

            name_ru = name_ru?.Replace("&nbsp;", "");
            name_en_year = name_en_year?.Replace("&nbsp;", "");
            rating_imdb = rating_imdb?.Replace(imdb_votecount, "")
                                     .Replace("&nbsp;", "");
            imdb_votecount = imdb_votecount?.Replace("&nbsp;", "");
            rating = rating?.Replace("&nbsp;", "");

            try
            {

                film.filmID = int.Parse(element.GetAttribute("Id").Substring(3));
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
        public List<FilmShot> GetImageList(int film_id)
        {
            driver.Navigate().GoToUrl(Path.Combine(baseAddress,$"{film_id}/stills/"));
            Thread.Sleep(3000);
            var table_element = driver.FindElement(By.XPath("//table[@class='fotos']"));
            if (table_element != null)
            {
                var images = new List<FilmShot>();
                images.Clear();

                var images_element = table_element.FindElements(By.XPath(".//img"));
                foreach (var image_node in images_element)
                {
                    var image = new FilmShot();
                    image.filmid = film_id;
                    var str = image_node.GetAttribute("src").Replace("sm_","");
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

        public void Dispose()
        {
            driver.Quit();
        }

        IWebElement FindElement(IWebElement element, string xpath)
        {
            try
            {
                return element.FindElement(By.XPath(xpath));
            }
            catch
            {
                return null;
            }
        }

        IWebElement FindElement(IWebDriver driver, string xpath)
        {
            try
            {
                return driver.FindElement(By.XPath(xpath));
            }
            catch
            {
                return null;
            }
        }

        IReadOnlyCollection<IWebElement> FindElements(IWebElement element, string xpath)
        {
            try
            {
                return element.FindElements(By.XPath(xpath));
            }
            catch
            {
                return null;
            }
        }

        IReadOnlyCollection<IWebElement> FindElements(IWebDriver driver, string xpath)
        {
            try
            {
                return driver.FindElements(By.XPath(xpath));
            }
            catch
            {
                return null;
            }
        }
    }
}
