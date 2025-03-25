using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace mp3player
{
    internal class MusicController
    {
        public string directoryPath { get; set; }
        private DirectoryInfo directoryInfo { get; set; }
        private FileInfo[] files { get; set; }

        public List<Song> songsList;

        public ObservableCollection<Song> songs;

        //public MusicController(string DirectoryPath)
        //{
        //    this.directoryPath = DirectoryPath;
        //    directoryInfo = new DirectoryInfo(DirectoryPath);
        //    songsList = new List<Song>();
        //    songs = new ObservableCollection<Song>();
        //}
        public MusicController() { }

        public List<Song> listMusicFiles(string DirectoryPath)
        {
            this.directoryPath = DirectoryPath;
            directoryInfo = new DirectoryInfo(DirectoryPath);
            songsList = new List<Song>();
            songs = new ObservableCollection<Song>();


            //
            files = directoryInfo.GetFiles("*.mp3");
            songsList.Clear();
            foreach (FileInfo file in files)
            {
                //songsList.Add(file.FullName);
                try
                {
                    TagLib.File tagFile = TagLib.File.Create(file.FullName);
                    string title = "";
                    string author = "";
                    string length = "-";
                    TagLib.IPicture image;
                    if (tagFile.Tag.Title != null) { title = tagFile.Tag.Title; } else { title = file.Name; }
                    if (tagFile.Tag.Performers[0] != null) { author = tagFile.Tag.Performers[0]; } else { author = "unknown"; }
                    if (tagFile.Tag.Pictures[0] != null) { image = tagFile.Tag.Pictures[0]; } else { image = null; }
                    //if (tagFile.Length > 0) { length = tagFile.Length.ToString(); }
                    MediaPlayer mediaPlayer = new MediaPlayer();
                    mediaPlayer.Open(new Uri(file.FullName));
                    //var totalDurationTime = TimeSpan.FromSeconds(mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds);
                    //length = new DateTime(totalDurationTime.Ticks).ToString("mm:ss");
                    
                    songsList.Add(new Song(file.FullName, title, author, length, image));

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }


            }
            
            return songsList;
        }


        public ObservableCollection<Song> getCollection()
        {
            files = directoryInfo.GetFiles("*.mp3");
            songs.Clear();
            foreach (FileInfo file in files)
            {
                //songsList.Add(file.FullName);
                try
                {
                    TagLib.File tagFile = TagLib.File.Create(file.FullName);
                    string title = "";
                    string author = "";
                    string length = "-";
                    TagLib.IPicture image;
                    if (tagFile.Tag.Title != null) { title = tagFile.Tag.Title; } else { title = file.Name; }
                    if (tagFile.Tag.Performers[0] != null) { author = tagFile.Tag.Performers[0]; } else { author = "unknown"; }
                    if (tagFile.Tag.Pictures[0] != null) { image = tagFile.Tag.Pictures[0]; } else { image = null; }
                    //if (tagFile.Length > 0) { length = tagFile.Length.ToString(); }
                    MediaPlayer mediaPlayer = new MediaPlayer();
                    mediaPlayer.Open(new Uri(file.FullName));
                    //var totalDurationTime = TimeSpan.FromSeconds(mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds);
                    //length = new DateTime(totalDurationTime.Ticks).ToString("mm:ss");

                    songs.Add(new Song(file.FullName, title, author, length, image));

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }


            }

            return songs;
        }

        public bool isPathCorrect(string path)
        {
            bool correct = false;
            try
            {
                DirectoryInfo tempDirInfo = new DirectoryInfo(path);
                FileInfo[] tempFiles = tempDirInfo.GetFiles("*.mp3");
                if(tempFiles.Count() > 0)
                {
                    correct = true;
                }
            } catch { }
            return correct;
        }
    }
}
