using FilmGuess.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace FilmGuess
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class GamePage : Page
	{
		GameRound round;
		DispatcherTimer show_answer_timer, time_type_timer;
		Stopwatch gametime;
		Gametype game_type;
		int time_sec;
		bool answerBtnFlag, exitBtnFlag;

		public GamePage()
		{
			this.InitializeComponent();            
			round = null;

			show_answer_timer = new DispatcherTimer();
			show_answer_timer.Interval = new TimeSpan(0, 0, 1);
			show_answer_timer.Tick += Show_answer_timer_Tick;

			time_type_timer = new DispatcherTimer();
			time_type_timer.Interval = new TimeSpan(0, 0, 1);
			time_type_timer.Tick += Time_type_timer_Tick;

			gametime = new Stopwatch();
		}

		private async void Time_type_timer_Tick(object sender, object e)
		{
			time_sec--;
			if (time_sec <= 0)
				await EndGame();
			else
			{
				CountDownBar.Value = time_sec;
				DateTime time = new DateTime();
				time = time.AddSeconds(time_sec);
				TimeTxt.Text = $"{time:mm:ss}";
			}
		}

		private async void Show_answer_timer_Tick(object sender, object e)
		{
			Answer1Btn.Background = myBrush;
			Answer2Btn.Background = myBrush;
			Answer3Btn.Background = myBrush;
			Answer4Btn.Background = myBrush;

			show_answer_timer.Stop();
			LoadRing.IsActive = true;
			await StartRound();
			LoadRing.IsActive = false;
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e) //on loading the page
		{
			answerBtnFlag = false; //disable buttons
			exitBtnFlag = false;

			game_type = (Gametype)(e.Parameter);
			await StartGame();
			Window.Current.Activated += Current_Activated; //start lost focus observing after load game
			await StartRound();            
		}

		protected override async void OnNavigatedFrom(NavigationEventArgs e) //on leaving the page
		{
			int count = SettingsManager.LaunchCount;
			if (count == 5 || (count + 1) % 15 == 0)
			{
				SettingsManager.LaunchCount++;
				
				var dlg = new MessageDialog(App.res.GetString("MsgRateApp"));
				dlg.Commands.Add(new UICommand { Label = App.res.GetString("MsgRateAppYes"), Id = 0 });
				dlg.Commands.Add(new UICommand { Label = App.res.GetString("MsgRateAppNo"), Id = 1 });

				var res = await dlg.ShowAsync();
				if ((int)res?.Id == 0) //yes                    
					await Launcher.LaunchUriAsync(new Uri($"ms-windows-store://review/?PFN={Package.Current.Id.FamilyName}"));
			}
			base.OnNavigatedFrom(e);
		}

		private async void Current_Activated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
		{
			bool answer_flag = false; //if game was paused, when playing answer animation
			bool timetype_flag = false; //if game was paused, when type is Time

			if (e.WindowActivationState==Windows.UI.Core.CoreWindowActivationState.Deactivated) //focus lost
			{
				if (show_answer_timer.IsEnabled)
				{
					show_answer_timer.Stop();
					answer_flag = true;
				}
				if (time_type_timer.IsEnabled)
				{
					time_type_timer.Stop();
					timetype_flag = true;
				}
				if (game_type==Gametype.lives3)
					CountDown.Pause();

				var dlg = new MessageDialog(App.res.GetString("MsgPaused"));
				dlg.Commands.Add(new UICommand { Label = App.res.GetString("MsgPausedYes"), Id = 0 });
				dlg.Commands.Add(new UICommand { Label = App.res.GetString("MsgPausedNo"), Id = 1 });

				var res = await dlg.ShowAsync();
				if (res == null) return;
				if ((int)res.Id == 0) //yes
				{
					if (game_type == Gametype.lives3)
						CountDown.Resume();
					if (answer_flag)
						show_answer_timer.Start();
					if (timetype_flag)
						time_type_timer.Start();
				}
				else
					await EndGame(true);
			}
		}

		private async void ExitBtn_Click(object sender, RoutedEventArgs e)
		{
			if (exitBtnFlag)
				await CancelGame();
		}

		private void AnswerBtn_Click(object sender, RoutedEventArgs e)
		{
			if (!answerBtnFlag) //btn disabled
				return;

			answerBtnFlag = false;
			exitBtnFlag = false;

			if (game_type == Gametype.lives3)
				CountDown.Pause();

			var btn = (Button)sender;
			btn.Background = RedBrush;

			switch (round.right_answer)
			{
				case 0:
					Answer1Btn.Background = GreenBrush;
					break;
				case 1:
					Answer2Btn.Background = GreenBrush;
					break;
				case 2:
					Answer3Btn.Background = GreenBrush;
					break;
				case 3:
					Answer4Btn.Background = GreenBrush;
					break;
			}

			CheckRound(int.Parse(btn.Tag.ToString()));
			show_answer_timer.Start();
		}

		private void ShowLives()
		{
			LiveTxt.Text = "";
			int live = App.game.lives;
			for (int i=0;i<3;i++)
			{
				if (live > 0)
				{
					LiveTxt.Text += "\xE00B ";
					live--;
				}
				else
					LiveTxt.Text += "\xE007 ";
			}
		}

		private async Task StartGame()
		{
			ShotImg.Source = null;
			LoadRing.IsActive = true;

			Answer1Btn.Background = myBrush;
			Answer2Btn.Background = myBrush;
			Answer3Btn.Background = myBrush;
			Answer4Btn.Background = myBrush;

			var files = await ApplicationData.Current.TemporaryFolder.GetFilesAsync();
			foreach (var file in files)
				await file.DeleteAsync();
			await Task.Run(() => App.game = new Game());

			if (game_type == Gametype.lives3)
				ShowLives();
			else
				TimeTxt.Visibility = Visibility.Visible;

			LoadRing.IsActive = false;
			if (game_type==Gametype.lives3)
			{
				CountDownBar.Minimum = 0;
				CountDownBar.Maximum = 11;
				CountDownBar.SetValue(RelativePanel.LeftOfProperty, LiveTxt);
			}
			else
			{
				CountDownBar.Minimum = 0;
				CountDownBar.Maximum = 180;
				CountDownBar.SetValue(RelativePanel.LeftOfProperty, TimeTxt);

				time_sec = 180;
				time_type_timer.Start();
			}

			gametime.Start();
		}

		private async Task StartRound()
		{
			await Task.Run(() => round = App.game.Get());

			answerBtnFlag = true; //enable buttons
			exitBtnFlag = true;                        

			if (round?.Films == null || App.game.lives < 1)
				await EndGame();
			else
			{
				/* var image = new BitmapImage();
				string kp_uri = string.Format(App.res.GetString("ImgPath"), round.ImagePath);
				string uri;
				if (ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1)) //is mobile
					uri = string.Format(App.res.GetString("ImageResizer"), kp_uri);
				else
					uri = kp_uri;
				
				image.UriSource = new Uri(uri);
				ShotImg.Source = image; */
				if (!string.IsNullOrEmpty(round.ImagePath))
				{
					var file = await ApplicationData.Current.TemporaryFolder.GetFileAsync(round.ImagePath);
					var image = new BitmapImage();
					var stream = await file.OpenStreamForReadAsync();
					image.SetSource(stream.AsRandomAccessStream());

					ShotImg.Source = image;
				}
				else
				{
					Window.Current.Activated -= Current_Activated; //delete lost focus observing to avoid duplication
					ShotImg_ImageFailed(this, null);
					return;
				}

				if (App.is_imdb)
				{
					Answer1Btn.Content = $"{round.Films[0].nameEN} ({round.Films[0].year})";
					Answer2Btn.Content = $"{round.Films[1].nameEN} ({round.Films[1].year})";
					Answer3Btn.Content = $"{round.Films[2].nameEN} ({round.Films[2].year})";
					Answer4Btn.Content = $"{round.Films[3].nameEN} ({round.Films[3].year})";
				}
				else
				{
					Answer1Btn.Content = $"{round.Films[0].nameRU} ({round.Films[0].year})";
					Answer2Btn.Content = $"{round.Films[1].nameRU} ({round.Films[1].year})";
					Answer3Btn.Content = $"{round.Films[2].nameRU} ({round.Films[2].year})";
					Answer4Btn.Content = $"{round.Films[3].nameRU} ({round.Films[3].year})";
				}
				if (game_type == Gametype.lives3)
					CountDown.Begin();
			}
		}

		private void CheckRound(int answer)
		{
			if (answer == round.right_answer)
			{
				SettingsManager.Guessed++;
				App.game.answers++;
				if (game_type == Gametype.lives3)
					App.game.score += (int)CountDownBar.Value;
				else
					App.game.score += 19 - (int)(CountDownBar.Value / 10);
			}
			else
			{
				SettingsManager.NotGuessed++;
				StatManager.FilmNotGuessed(round.Films[round.right_answer].filmID.ToString());

				if (game_type == Gametype.lives3)
				{
					App.game.lives--;
					ShowLives();
				}
			}
		}

		private async Task CancelGame()
		{
			Window.Current.Activated -= Current_Activated; //delete lost focus observing to avoid duplication
			if (game_type == Gametype.lives3)
				CountDown.Pause();
			bool answer_flag = false; //if game was paused, when playing answer animation
			bool timetype_flag = false; //if game was paused, when type is Time
			if (show_answer_timer.IsEnabled)
			{
				answer_flag = true;
				show_answer_timer.Stop();
			}
			if (time_type_timer.IsEnabled)
			{
				timetype_flag = true;
				time_type_timer.Stop();
			}

			var dlg =
				new MessageDialog(string.Format(App.res.GetString("MsgCanceling"), App.game.answers, App.game.score));
			dlg.Commands.Add(new UICommand { Label = App.res.GetString("MsgCancelingYes"), Id = 0 });
			dlg.Commands.Add(new UICommand { Label = App.res.GetString("MsgCancelingNo"), Id = 1 });
			var res = await dlg.ShowAsync();

			if ((int)res.Id == 0)
			{
				App.game.Dispose();
				if (game_type == Gametype.lives3)
					CountDown.Stop();
				if (show_answer_timer.IsEnabled)
					show_answer_timer.Stop();

				gametime.Stop();
				gametime.Reset();

				Frame frame = Window.Current.Content as Frame;
				frame.Navigate(typeof(MainPage));
			}
			else
			{
				if (game_type == Gametype.lives3)
					CountDown.Resume();
				if (answer_flag)
					show_answer_timer.Start();
				if (timetype_flag)
					time_type_timer.Start();
			}
		}

		private async Task EndGame(bool terminate=false)
		{
			if (show_answer_timer.IsEnabled)
				show_answer_timer.Stop();
			if (time_type_timer.IsEnabled)
				time_type_timer.Stop();

			App.game.Dispose();
			if (game_type == Gametype.lives3)
				CountDown.Stop();
			Window.Current.Activated -= Current_Activated; //delete lost focus observing to avoid duplication            

			try
			{
				GC.Collect();
				var date = DateTime.Now;
				var score = new Scores
				{
					Score = App.game.score,
					Date = DateTime.Now.ToString("dd.MM.yy HH:mm"),
					GameType = (int)game_type
				};

				SettingsManager.GamesPlayed++;
				StatManager.GameResult(score, gametime.Elapsed.ToString(@"mm\:ss"));                
				
				gametime.Stop();
				gametime.Reset();

				DbManager.InsertScore(score);
			}
			catch (Exception exc)
			{
				StatManager.Exception("fail_end_game", exc.Message);
			}
			
			if (!terminate)
			{
				var dlg =
					new MessageDialog(string.Format(App.res.GetString("MsgFinished"), App.game.answers, App.game.score));
				dlg.Commands.Add(new UICommand { Label = App.res.GetString("MsgFinishedExit"), Id = 0 });
				dlg.Commands.Add(new UICommand { Label = App.res.GetString("MsgFinishedRetry"), Id = 1 });
				var res = await dlg.ShowAsync();

				if ((int)res.Id == 1)
				{
					//Window.Current.Activated += Current_Activated; //return lost focus observing
					/*await StartGame();
					await StartRound();*/

					Frame frame2 = Window.Current.Content as Frame;
					frame2.Navigate(typeof(GamePage),game_type);

					return;
				}
			}
			Frame frame = Window.Current.Content as Frame;
			frame.Navigate(typeof(MainPage));
		}
	   

		private async void CountDown_Completed(object sender, object e)
		{
			await EndGame();
		}

		private async void ShotImg_ImageFailed(object sender, ExceptionRoutedEventArgs e)
		{
			Window.Current.Activated -= Current_Activated; //delete lost focus observing to avoid duplication
			var dlg =
				new MessageDialog(App.res.GetString("MsgNoInternet"));
			dlg.Commands.Add(new UICommand { Label = App.res.GetString("MsgNoInternetOk"), Id = 0 });

			StatManager.Exception("fail_load_img");
			await dlg.ShowAsync();
			await EndGame(true);
		}
	}
}
