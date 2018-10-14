using OpenQA.Selenium;
using OpenQA.Selenium.IE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserKinopoisk
{
    public class KPWebDriver : IDisposable
    {
        IWebDriver _driver;
        int _waitmseconds;

        public KPWebDriver(int waitseconds)
        {            
            _driver = new InternetExplorerDriver();
            _driver.Manage().Window.Maximize();
            _waitmseconds = waitseconds * 1000;
        }

        public void Dispose()
        {
            _driver.Close();
            _driver.Quit();
        }

        public async Task<string> LoadFilmsPage(int year, int page_num)
        {
            try
            {
                _driver.Navigate().GoToUrl($"https://www.kinopoisk.ru/lists/ord/rating_kp/m_act[year]/{year}/m_act[all]/page/page/{page_num}/");
                await Task.Delay(_waitmseconds);
                return _driver.PageSource;
            }
            catch { return null; }
        }

        public async Task<string> LoadImagePage(int film_id)
        {
            try
            {
                _driver.Navigate().GoToUrl($"https://www.kinopoisk.ru/film/{film_id}/stills/");
                await Task.Delay(_waitmseconds);
                return _driver.PageSource;
            }
            catch { return null; }
        }
    }
}
