using SQLite.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LoadKinopoisk
{
    class DbManager
    {
        string dbname = "data.db";
        string path;
        SQLiteConnection connection;

        public DbManager()

        {
            var localFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            path = Path.Combine(localFolder, dbname);
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

        public void Insert(FilmData ob)
        {
            connection.Insert(ob);
        }

        public void Insert(List<FilmShot> ob)
        {
            connection.InsertAll(ob);
        }

        public void DeleteFim(int id) => connection.Delete<FilmData>(id);

        public FilmData SelectFilm(int film_id)
        {
            var query = from p in connection.Table<FilmData>()
                        where p.filmID==film_id
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

        public int[] SelectIds()
        {
            var query = from p in connection.Table<FilmData>()
                        select p.filmID;

            return query.ToArray();

        }

        public void UpdateImages()
        {
            var query1 = from p in connection.Table<FilmShot>()
                         orderby p.id
                         select p;
            var temp = query1.ToList();
            string str;
            for (int i=0;i<temp.Count;i++)
            {
                str = temp[i].image;
                str = str.Substring(4); //kadr{0}.jpg
                temp[i].image = str.Substring(0, str.LastIndexOf('.'));
            }
            connection.UpdateAll(temp);
        }
    }
}
