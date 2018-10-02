using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

namespace LoadKinopoisk
{
    class WebConnect
    {
        HttpClient client;
        Uri base_address;

        public WebConnect(string baseadress)
        {
            base_address = new Uri(baseadress);
        }

        async Task<string> GetResponse(string param_string)
        {
            try
            {
                using (client = new HttpClient { BaseAddress = base_address })
                {
                    return await client.GetStringAsync(param_string);
                }
            }
            catch
            {
                Console.WriteLine("Unable to get Response from server");
                return string.Empty;
            }
        }

        public async Task<FilmData> GetFilm(int filmId)
        {
            string responseData = await GetResponse($"getFilm?filmID={filmId}");
            try
            {
                return JsonConvert.DeserializeObject<FilmData>(responseData);
            }
            catch
            {
                Console.WriteLine($"Unable to get Film={filmId} from server response");
                return null;
            }
        }
        public async Task<ImageData> GetImages(int filmId)
        {
            ImageData data;
            string responseData = await GetResponse($"getGallery?filmID={filmId}");
            try
            {
                data = JsonConvert.DeserializeObject<ImageData>(responseData);
                data.filmID = filmId;
                return data;
            }
            catch
            {
                Console.WriteLine($"Unable to get Posters={filmId} from server response");
                return null;
            }
        }
    }
}
