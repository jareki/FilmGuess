using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using System.IO;

namespace FilmGuess.Models
{
    class GameRound
    {
        public List<FilmData> Films { get; set; }
        public int right_answer { get; set; }
        public string ImagePath { get; set; }
        public Stream ImgStream { get; set; }
        Random rnd;

        public GameRound()
        {
            Films = new List<FilmData>();
            rnd = new Random();
            right_answer = rnd.Next(0, 4);
        }        

        public async Task GenerateRound(int max_votecount)
        {
            FilmData film;     
            try
            {
                for (int i = 0; i < 4; i++)
                {
                    if (i == right_answer)
                    {
                        Films.Add(DbManager.SelectRandomFilm(max_votecount, 100, App.is_imdb));

                        var image = DbManager.SelectRandomImage(Films[i].filmID);

                        string kp_uri = string.Format(App.res.GetString("ImgPath"), image.image);
                        string uri;
                        if (ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1)) //is mobile
                            uri = string.Format(App.res.GetString("ImageResizer"), kp_uri);
                        else
                            uri = kp_uri;
                        try
                        {
                            using (HttpClient client = new HttpClient())
                            {
                                var data = await client.GetByteArrayAsync(uri);
                                var temp = ApplicationData.Current.TemporaryFolder;
                                var file = await temp.CreateFileAsync($"{Guid.NewGuid().ToString()}.jpg", CreationCollisionOption.GenerateUniqueName);
                                using (var stream = await file.OpenStreamForWriteAsync())
                                {
                                    stream.Write(data, 0, data.Length);
                                    stream.Flush();
                                }
                                ImagePath = file.Name;
                            }
                        }
                        catch
                        {
                            ImagePath = "";
                        }
                        //ImagePath = image.image;   
                    }
                    else
                    {
                        do
                            film = DbManager.SelectRandomFilm(max_votecount - 1, App.is_imdb);
                        while (Films.Contains(film));

                        Films.Add(film);
                    }
                }               
            }
            catch (FilmException e)
            {
                await e.ExceptionHandle();
            }
            finally
            {
                GC.Collect();
            }
        }
    }
}
