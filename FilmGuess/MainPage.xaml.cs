using FilmGuess.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FilmGuess
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void NewGameBtn_Click(object sender, RoutedEventArgs e)
        {
            Frame frame = Window.Current.Content as Frame;
            frame.Navigate(typeof(GameTypePage));
        }

        private void OptionsBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ResultsBtn_Click(object sender, RoutedEventArgs e)
        {
            Frame frame = Window.Current.Content as Frame;
            frame.Navigate(typeof(ResultsPage));
            StatManager.PageLoaded("ResultsPage");
        }

        private void AboutBtn_Click(object sender, RoutedEventArgs e)
        {
            Frame frame = Window.Current.Content as Frame;
            frame.Navigate(typeof(AboutPage));
            StatManager.PageLoaded("AboutPage");
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            App.dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;
            StatTxt.Text = string.Format(App.res.GetString("StatTxt"),
                                        SettingsManager.GamesPlayed, SettingsManager.Guessed, SettingsManager.NotGuessed);
        }
    }
}
