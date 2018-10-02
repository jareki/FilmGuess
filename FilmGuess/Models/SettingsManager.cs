using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace FilmGuess.Models
{
    class SettingsManager
    {
        static public ApplicationDataContainer local;
        static SettingsManager()
        {
            local = ApplicationData.Current.LocalSettings;
            var localfolder = ApplicationData.Current.LocalFolder;


            if (Version=="")
            {
                var ver = Package.Current.Id.Version;
                Version = $"{ver.Major}.{ver.Minor}.{ver.Build}";
            }
        }

        static public int LaunchCount
        {
            get
            {
                return (int)(local.Values["launch_count"] ?? 0);
            }
            set
            {
                local.Values["launch_count"] = value;
            }
        }

        static public int GamesPlayed
        {
            get
            {
                return (int)(local.Values["games_played"] ?? 0);
            }
            set
            {
                local.Values["games_played"] = value;
            }
        }

        static public int Guessed
        {
            get
            {
                return (int)(local.Values["guessed"] ?? 0);
            }
            set
            {
                local.Values["guessed"] = value;
            }
        }

        static public int NotGuessed
        {
            get
            {
                return (int)(local.Values["not_guessed"] ?? 0);
            }
            set
            {
                local.Values["not_guessed"] = value;
            }
        }

        static public string Version
        {
            get
            {
                return (string)(local.Values["version"] ?? "");
            }
            set
            {
                local.Values["version"] = value;
            }
        }
    }
}
