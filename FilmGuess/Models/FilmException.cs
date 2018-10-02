using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FilmGuess.Models
{
    class FilmException:Exception
    {
        public FilmException() : base() { }
        public FilmException(string msg) : base(msg) { }
        public FilmException(string msg, Exception inner) : base(msg, inner) { }

        public async Task ExceptionHandle()
        {
            var dialog = new MessageDialog(Message);
            dialog.Commands.Add(new UICommand { Label = "Exit", Id = 0 });
            var res = await dialog.ShowAsync();

            StatManager.Exception(Message);

            Frame thisFrame = Window.Current.Content as Frame;
            thisFrame.Navigate(typeof(MainPage));
        }
    }
}
