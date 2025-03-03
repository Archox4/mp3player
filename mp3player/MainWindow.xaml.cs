using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;
using TagLib.Mpeg;
using static MaterialDesignThemes.Wpf.Theme;

namespace mp3player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MusicController musicController;
        private MediaPlayer musicPlayer;
        private List<Song> songQueue;
        public List<Song> songList;
        private List<Song> songHistory;
        private List<Song> songListRandom;
        private string state;
        TimeSpan timeSpan;
        DispatcherTimer timer;
        Song playingSong;
        private int lastPos;
        private int historyPos;
        private int randomPos;
        private string playMode;
        static Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();

            musicController = new MusicController(@"C:\Users\w\Music\");
            musicController.listMusicFiles();
            songList = musicController.listMusicFiles();
            songQueue = new List<Song>();
            songHistory = new List<Song>();
            songListRandom = new List<Song>();


            timer = new DispatcherTimer();
            listView.ItemsSource = songList;
            musicPlayer = new MediaPlayer();
            musicPlayer.MediaEnded += MusicPlayer_MediaEnded;
            
            state = "notplaying";


            musicPlayer.Volume = (double)Properties.Settings.Default["volume"];
            sliderBarVolume.Value = (double)Properties.Settings.Default["volume"];
            playMode = (string)Properties.Settings.Default["playMode"];
            if(playMode == "random")
            {
                songListRandom = songList;
                Shuffle(songListRandom);

                randomPos = -1;
            }

            test1.Content = playMode;
            setBackgrounds();

            //lastPos = -1;
            lastPos = 0;
            queueListView.ItemsSource = songQueue;

        }
        
        private void setBackgrounds()
        {
            // play mode button icon
            if(playMode == "normal")
            {
                Uri resourceUri = new Uri("Images/playNormal.png", UriKind.Relative);
                StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);
                BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
                var brush = new ImageBrush();
                brush.ImageSource = temp;
                btnPlayMode.Background = brush;
            }
            else
            {
                Uri resourceUri = new Uri("Images/playRandom.png", UriKind.Relative);
                StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);
                BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
                var brush = new ImageBrush();
                brush.ImageSource = temp;
                btnPlayMode.Background = brush;
            }
            //
        }

        protected void HandleDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
            var song = ((ListViewItem)sender).Content as Song;
            if(song != null)
            {
                PlayFile(song.path);
                playingSong = song;

                lastPos = getSongPosition(song.path);

                if (songHistory.Count < 50)
                {
                    songHistory.Add(playingSong);
                }
                else
                {
                    songHistory.RemoveAt(0);
                    songHistory.Add(playingSong);
                }
                historyPos = songHistory.Count - 1;

            }
        }
        
        private void PlayFile(String url)
        {

            if (state != "playing")
            {
                musicPlayer.Open(new Uri(url));
            }
            else if(state == "paused")
            {
                musicPlayer.Play();
                state = "playing";
                //setPlayBtnBackground();
            }
            else if(state == "playing")
            {
                state = "paused";
                musicPlayer.Open(new Uri(url));
               // setPlayBtnBackground();
            }
            //
            lastPos = getSongPosition(url);
            //
            musicPlayer.MediaOpened += MusicPlayer_MediaOpened;
        }

        private void tickEvent(object sender, EventArgs e)
        {
            sliderBar.Value = musicPlayer.Position.TotalMilliseconds;
            var pastTime = TimeSpan.FromMilliseconds(musicPlayer.Position.TotalMilliseconds);
            songLengthPast.Content = new DateTime(pastTime.Ticks).ToString("mm:ss");
        }

        private void MusicPlayer_MediaOpened(object sender, EventArgs args)
        {
            var totalDurationTime = TimeSpan.FromMilliseconds(musicPlayer.NaturalDuration.TimeSpan.TotalMilliseconds);

            sliderBar.Maximum = totalDurationTime.TotalMilliseconds;
            sliderBar.Minimum = 0;

            songLength.Content = new DateTime(totalDurationTime.Ticks).ToString("mm:ss");
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += new EventHandler(tickEvent);
            timer.Start();

            musicPlayer.Play();
            state = "playing";

            lbSongName.Content = playingSong.name;
            lbSongAuthor.Content = playingSong.author;
            if (playingSong.image != null)
            {
                MemoryStream ms = new MemoryStream(playingSong.image.Data.Data);
                ms.Seek(0, SeekOrigin.Begin);
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.EndInit();

                songImg.Source = bitmap;
            }

            sliderBar.Value = 0;
            songLengthPast.Content = "00:00";

            //setPlayBtnBackground();
        }

        private void MusicPlayer_MediaEnded(object sender, EventArgs args)
        {
            playNextSong();
        }

        private void btPlayOnClick(object sender, RoutedEventArgs e)
        {
            
            if (state == "playing")
            {
                musicPlayer.Pause();
                state = "paused";

                //setPlayBtnBackground();
            } 
            else if(state == "notplaying")
            {
                PlayFile(songList.FirstOrDefault().path);
                playingSong = songList.FirstOrDefault();
                state = "playing";

               // setPlayBtnBackground();
            }
            else if(state == "paused")
            {
                musicPlayer.Play();
                state = "playing";

                //setPlayBtnBackground();
            }

            if(playMode == "random")
            {
                randomPos = 0;
                test1.Content = "randomPos" + randomPos;
            }


        }

        private void volumeBarValChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            musicPlayer.Volume = sliderBarVolume.Value;
            Properties.Settings.Default["volume"] = (double)sliderBarVolume.Value;
            Properties.Settings.Default.Save();
        }

        //private void btnPlayMousEnter(object sender, MouseEventArgs e)
        //{
        //    if(state == "playing")
        //    {
        //        Uri resourceUri = new Uri("Images/btnPause2.png", UriKind.Relative);
        //        StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);
        //        BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
        //        var brush = new ImageBrush();
        //        brush.ImageSource = temp;
        //        btPlay.Background = brush;
        //    } else
        //    {
        //        Uri resourceUri = new Uri("Images/btnPlay2.png", UriKind.Relative);
        //        StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);
        //        BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
        //        var brush = new ImageBrush();
        //        brush.ImageSource = temp;
        //        btPlay.Background = brush;
        //    }
        //}

        //private void btnPlayMousLeave(object sender, MouseEventArgs e)
        //{
        //    if (state == "playing")
        //    {
        //        Uri resourceUri = new Uri("Images/btnPause.png", UriKind.Relative);
        //        StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);
        //        BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
        //        var brush = new ImageBrush();
        //        brush.ImageSource = temp;
        //        btPlay.Background = brush;
        //    }
        //    else
        //    {
        //        Uri resourceUri = new Uri("Images/btnPlay.png", UriKind.Relative);
        //        StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);
        //        BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
        //        var brush = new ImageBrush();
        //        brush.ImageSource = temp;
        //        btPlay.Background = brush;
        //    }
        //}

        //private void setPlayBtnBackground()
        //{
        //    if (state == "playing")
        //    {
        //        Uri resourceUri = new Uri("Images/btnPause.png", UriKind.Relative);
        //        StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);
        //        BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
        //        var brush = new ImageBrush();
        //        brush.ImageSource = temp;
        //        btPlay.Background = brush;
        //    }
        //    else if (state == "notplaying")
        //    {
        //        Uri resourceUri = new Uri("Images/btnPlay.png", UriKind.Relative);
        //        StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);
        //        BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
        //        var brush = new ImageBrush();
        //        brush.ImageSource = temp;
        //        btPlay.Background = brush;
        //    }
        //    else if (state == "paused")
        //    {
        //        Uri resourceUri = new Uri("Images/btnPlay.png", UriKind.Relative);
        //        StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);
        //        BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
        //        var brush = new ImageBrush();
        //        brush.ImageSource = temp;
        //        btPlay.Background = brush;
        //    }
        //}

        private void sliderBar_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            
            if (state != "notplaying")
            {
                musicPlayer.Pause();
                timer.Stop();
            }
        }

        private void sliderBar_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (state != "notplaying")
            {

                musicPlayer.Position = TimeSpan.FromMilliseconds(sliderBar.Value);
                musicPlayer.Play();
                timer.Start();
                state = "playing";
                //setPlayBtnBackground();
            }
        }

        private int getSongPosition(string path)
        {
            int pos = -1;
            for (int i = 0; i < songList.Count; i++)
            {
                if(songList[i].path == path)
                {
                    pos = i; break;
                }
            }
            return pos;
        }

        private void btnAddToQueue(object sender, RoutedEventArgs e)
        {
            var btn = sender as System.Windows.Controls.Button;
            if(btn?.DataContext is Song song)
            {
                songQueue.Add(song);
            }
            queueListView.Items.Refresh();
        }

        private void OpenQueueBtnClick(object sender, RoutedEventArgs e)
        {
            if(queueListView.Opacity == 0)
            {
                var animation = (Storyboard)FindResource("PopUpAnimation");
                queueListView.Visibility = Visibility.Visible;
                animation.Begin(queueListView);
            }
            else
            {
                var animation = (Storyboard)FindResource("PopDownAnimation");
                queueListView.Visibility = Visibility.Hidden;
                animation.Begin(queueListView);
            }
            
        }

        private void btnRemoveFromQueue(object sender, RoutedEventArgs e)
        {
            var btn = sender as System.Windows.Controls.Button;
            if (btn?.DataContext is Song song)
            {
                songQueue.Remove(song);
            }
            queueListView.Items.Refresh();
        }

        private void buttonSkipNext(object sender, RoutedEventArgs e)
        {
            playNextSong();
        }

        private void playNextSong()
        {
            sliderBar.Value = 0;
            timer.Stop();
            state = "notplaying";
            //setPlayBtnBackground();

            if (songQueue.Count != 0)
            {
                PlayFile(songQueue.First().path);
                playingSong = songQueue.First();
                songQueue.RemoveAt(0);
                queueListView.Items.Refresh();

                // history
                if (songHistory.Count < 50)
                {
                    songHistory.Add(playingSong);
                }
                else
                {
                    songHistory.RemoveAt(0);
                    songHistory.Add(playingSong);
                }
                //

            }
            else
            {
                if(playMode == "random")
                {
                    if(randomPos + 1 < songListRandom.Count)
                    {
                        PlayFile(songListRandom.ElementAt(randomPos + 1).path);
                        playingSong = songListRandom.ElementAt(randomPos + 1);
                        randomPos++;
                    }
                    else
                    {
                        randomPos = -1;
                        Shuffle(songListRandom);
                        PlayFile(songListRandom.ElementAt(randomPos + 1).path);
                        playingSong = songListRandom.ElementAt(randomPos + 1);
                        randomPos++;
                    }


                    test1.Content = "randomPos" + randomPos;
                    
                }
                else
                {
                    int pos = lastPos;
                    if (pos == songList.Count - 1)
                    {
                        pos = -1;
                        lastPos = pos;
                    }

                    if (pos + 1 < songList.Count)
                    {
                        PlayFile(songList.ElementAt(pos + 1).path);
                        playingSong = songList.ElementAt(pos + 1);
                    }
                    else
                    {
                        PlayFile(songList.FirstOrDefault().path);
                        playingSong = songList.FirstOrDefault();
                    }

                    // history
                    if (songHistory.Count < 50)
                    {
                        songHistory.Add(playingSong);
                    }
                    else
                    {
                        songHistory.RemoveAt(0);
                        songHistory.Add(playingSong);
                    }
                    historyPos = songHistory.Count - 1;
                }

                

            }
        }

        static void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private void buttonPrevious(object sender, RoutedEventArgs e){
            test1.Content = historyPos + ", count: " + songHistory.Count;

            if (songHistory.Count > 0)
            {
                historyPos = songHistory.Count - 1;
                Song currSong = songHistory.ElementAt(historyPos);
                PlayFile(currSong.path);
                playingSong = currSong;
                songHistory.RemoveAt(historyPos);
            }
            else
            {
                timer.Stop();
                musicPlayer.Pause();
                musicPlayer.Position = TimeSpan.FromMilliseconds(0);
                musicPlayer.Play();
                timer.Start();
            }
        }

        private void changePlayMode(object sender, RoutedEventArgs e)
        {

            if(playMode == "normal")
            {
                playMode = "random";
                setBackgrounds();

                songListRandom = songList;
                Shuffle(songListRandom);

                randomPos = -1;
            }
            else if(playMode == "random")
            {
                playMode = "normal";
                setBackgrounds();
            }

            // set value in settings
            Properties.Settings.Default["playMode"] = playMode;
            Properties.Settings.Default.Save();
            //
        }
    }
}
