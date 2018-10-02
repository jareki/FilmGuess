using FilmGuess.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace FilmGuess
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ResultsPage : Page
    {
        ObservableCollection<Scores> Scores;

        public ResultsPage()
        {
            this.InitializeComponent();
            Scores = new ObservableCollection<Scores>();
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            Frame thisFrame = Window.Current.Content as Frame;
            thisFrame.Navigate(typeof(MainPage));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            GameTypeCombo.SelectedIndex = 0;
        }

        private async void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg =
                new MessageDialog(App.res.GetString("MsgDelScores"));
            dlg.Commands.Add(new UICommand { Label = App.res.GetString("MsgDelScoresYes"), Id = 0 });
            dlg.Commands.Add(new UICommand { Label = App.res.GetString("MsgDelScoresNo"), Id = 1 });
            var res = await dlg.ShowAsync();

            if ((int)res.Id == 0)//yes
            {
                DbManager.DeleteScores();
                Scores.Clear();

                SettingsManager.GamesPlayed = 0;
                SettingsManager.Guessed = 0;
                SettingsManager.NotGuessed = 0;
            }
        }

        private async void GameTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {      
            Gametype game_type = (Gametype)GameTypeCombo.SelectedIndex;
            Scores.Clear();
            try
            {
                await DbManager.Initialize();
                var temp = DbManager.SelectScore(game_type);
                temp.ForEach(p => Scores.Add(p));
            }
            catch (FilmException) { }
        }        
    }
}
