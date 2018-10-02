using SQLite.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ParserKinopoisk
{
    class DbManager
    {
        string path;
        SQLiteConnection connection;

        public DbManager(string db_name)

        {
            var localFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            path = Path.Combine(localFolder, db_name);
            if (!File.Exists(path))
            {
                CreateDb();
            }
            connection = new SQLiteConnection(new SQLite.Net.Platform.Win32.SQLitePlatformWin32(), path);
        }

        public void Close() => connection.Close();

        public void CreateDb()
        {
            File.Create(path).Close();
            connection = new SQLiteConnection(new SQLite.Net.Platform.Win32.SQLitePlatformWin32(), path);
            connection.CreateTable<FilmData>();
            connection.CreateTable<FilmShot>();

            connection.Close();
        }

        public void MergeDb(DbManager db)
        {
            //
        }

        public void Insert(FilmData ob) => connection.Insert(ob);
        public void Insert(FilmShot ob) => connection.Insert(ob);

        public void Insert(List<FilmShot> obs) => connection.InsertAll(obs);

        public void DeleteFim(int id) => connection.Delete<FilmData>(id);

        public FilmData SelectFilm(int film_id)
        {
            var query = from p in connection.Table<FilmData>()
                        where p.filmID == film_id
                        select p;

            return query.FirstOrDefault();
        }

        public FilmShot SelectImage(int film_id)
        {
            var query = from p in connection.Table<FilmShot>()
                        where p.filmid == film_id
                        select p;
            if (query.Count() == 0)
                return null;

            Random rnd = new Random();
            return query.ElementAt(rnd.Next(query.Count() - 1));
        }

        public List<FilmData> SelectNoImgFilms()
        {
            return connection.Query<FilmData>("Select * from `FilmData` where filmid not in (select filmid from `FilmShot`)");
        }

        public int[] SelectIds()
        {
            var query = from p in connection.Table<FilmData>()
                        select p.filmID;

            return query.ToArray();

        }
    }
}
