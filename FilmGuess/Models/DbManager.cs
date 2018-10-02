using SQLite.Net;
using SQLite.Net.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace FilmGuess.Models
{
    class DbManager
    {
        static string dbname = "data.db";
        static string path;
        static SQLiteConnection connection;

        static private object dbLock = new object();

        public static async Task<bool> Initialize()
        {
            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                path = Path.Combine(localFolder.Path, dbname);
                if (await localFolder.TryGetItemAsync(dbname) == null)
                    await Create();
                System.Threading.Monitor.TryEnter(dbLock,2500);
                connection = new SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex);
                return true;
            }
            catch
            {
                throw new FilmException(App.res.GetString("ErrorDb"));
            }
            finally
            {
                System.Threading.Monitor.Exit(dbLock);                
            }

        }

        public static async Task Create()
        {
            StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFolder assets = await appInstalledFolder.GetFolderAsync("Assets"); 
                       
            var db = await assets.GetFileAsync(dbname);
            var localFolder = ApplicationData.Current.LocalFolder;
            await db.CopyAsync(localFolder,dbname,NameCollisionOption.ReplaceExisting);
        }

        public static async Task UpdateDB()
        {
            try
            {
                //copy scores from db to list<scores>
                var query = from p in connection.Table<Scores>()
                            select p;
                var scores = query.ToList();
                connection.Close();

                //copy db to localfolder
                await Create();

                //insert scores in new db
                connection = new SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path);
                if (scores != null)
                    connection.InsertAll(scores);
            }
            catch
            {
                throw new FilmException(App.res.GetString("ErrorDbUpdate"));
            }
        }

        public static void Close()
        {
            lock (dbLock)
            {
                if (connection != null)
                    connection.Close();
            }
            GC.Collect();
        }
        
        public static void InsertScore(Scores ob)
        {
            lock (dbLock)
            {
                if (ob.Score > 0)
                    connection.Insert(ob);
            }
        }

        public static void DeleteScores()
        {
            lock (dbLock)
                connection.DeleteAll<Scores>();
        }

        public static List<Scores> SelectScore(Gametype type)
        {
            int i = (int)type;
            lock (dbLock)
            {
                var query = (from p in connection.Table<Scores>()
                             where (p.GameType == i)
                             orderby p.Score descending
                             select p).Take(25);

                List<Scores> temp = new List<Scores>();
                foreach (var item in query)
                    temp.Add(item);
                return temp;
            }
        }        

        public static FilmData SelectRandomFilm(int max_votecount, bool is_imdb)
        {
            int min_votecount = 10 * max_votecount / 100;
            Random rnd = new Random();
            TableQuery<FilmData> query;
            List<FilmData> list;
            lock (dbLock)
            {
                if (is_imdb)
                {
                    query = from p in connection.Table<FilmData>()
                            where (p.nameEN != null && p.nameEN != "") && p.ratingIMDbVoteCount < max_votecount && p.ratingIMDbVoteCount >= min_votecount
                            select p;
                    list = query.ToList().OrderByDescending(p => p.ratingIMDbVoteCount).Skip(100).ToList();
                }
                else
                {
                    query = from p in connection.Table<FilmData>()
                            where (p.nameRU != null && p.nameRU != "") && p.ratingVoteCount < max_votecount && p.ratingVoteCount >= min_votecount
                            select p;
                    list = query.ToList().OrderByDescending(p => p.ratingVoteCount).Skip(100).ToList();
                }

                //var list = query.ToList().Skip(100);
                if (list.Count() > 0)
                    return list.ElementAt(rnd.Next(0, list.Count()));
                else
                    return null;
            }
        }

        public static FilmData SelectRandomFilm(int max_votecount, int choose_count, bool is_imdb)
        {
            int min_votecount = 10 * max_votecount / 100;
            Random rnd = new Random();
            TableQuery<FilmData> query;
            List<FilmData> list;
            lock (dbLock)
            {
                if (is_imdb)
                {
                    query = from p in connection.Table<FilmData>()
                            where (p.nameEN != null && p.nameEN != "") && p.ratingIMDbVoteCount < max_votecount && p.ratingIMDbVoteCount >= min_votecount
                            select p;
                    list = query.ToList().OrderByDescending(p => p.ratingIMDbVoteCount).Take(choose_count).ToList();
                }
                else
                {
                    query = from p in connection.Table<FilmData>()
                            where (p.nameRU != null && p.nameRU != "") && p.ratingVoteCount < max_votecount && p.ratingVoteCount >= min_votecount
                            select p;
                    list = query.ToList().OrderByDescending(p => p.ratingVoteCount).Take(choose_count).ToList();
                }
                
                if (list.Count() > 0)
                    return list.ElementAt(rnd.Next(0, list.Count()));
                else
                    return null;
            }
        }

        public static FilmShot SelectRandomImage(int filmid)
        {
            Random rnd = new Random();
            lock (dbLock)
            {
                var query = from p in connection.Table<FilmShot>()
                            where p.filmid == filmid
                            select p;

                if (query.Count() > 0)
                    return query.ElementAt(rnd.Next(0, query.Count()));
                else
                    return null;
            }
        }
    }
}
