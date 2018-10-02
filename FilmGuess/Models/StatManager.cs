using Microsoft.HockeyApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmGuess.Models
{
    static class StatManager
    {
        static public void GameResult(Scores score, string elapsed_time)
        {
            HockeyClient.Current.TrackEvent("gameresult",
                new Dictionary<string, string> { { "date", score.Date },
                                                 { "score", score.Score.ToString() },
                                                 {"gametype", score.GameType.ToString()},
                                                 {"elapsed_time", elapsed_time } });
        }
        static public void DbUpdated(string old_ver, string new_ver)
        {
            HockeyClient.Current.TrackEvent("db_update",
                new Dictionary<string, string> { { "oldversion", old_ver },
                                                 { "newversion", new_ver } });
        }

        static public void Exception(string name, string msg = "")
        {
            HockeyClient.Current.TrackEvent(name,
                new Dictionary<string, string> { { "message", msg } });
        }

        static public void PageLoaded(string pagename)
        {
            HockeyClient.Current.TrackEvent(pagename);
        }

        static public void  FilmNotGuessed(string filmid)
        {
            HockeyClient.Current.TrackEvent("FilmNotGuessed",
                new Dictionary<string, string> { { "filmid", filmid}});
        }
    }
}
