// Libraries
using NAudio.Gui;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
// Libraries

namespace dataMusic
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static ProgressBar musicNumberProgressBar;
        public static ItemsControl other;

        public static Slider musicVolume;

        public MainWindow()
        {
            InitializeComponent(); // Load the frame

            this.MouseDown += (s, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                    this.DragMove();
            }; // Allow dragging the window with the mouse

            musicProcess.importantFolders(); // Create important folders (musics)
            otherMusicPanel.ItemsSource = musicProcess.listOfMusicAll(); // List all music in the musics folder when the window is created

            musicNumberProgressBar = musicProgressBar; // Define necessary variables for MusicProcess
            other = otherMusicPanel; // Define necessary variables for MusicProcess

            libsPanel.ItemsSource = musicProcess.listLibrary(); // Fill libsLibrary with music libraries

            this.DataContext = new mixedOrLoopBinding(); // TwoWay DataBinding for mixed or loop playback mode

            musicVolume = musicVolumeSlider;
            musicVolumeSlider.Value = 0.5f;

            musicProcess.GetMusicProgressAsync(); // Synchronize the progress bar for music
        }

        private void exitToMusic(object sender, RoutedEventArgs e) // Add function to the exit button
        {
            Environment.Exit(0);
        }
        private void iconedToWindow(object sender, RoutedEventArgs e) // Add function to the minimize button
        {
            WindowState = WindowState.Minimized; // Minimize the MainWindow
        }

        private void goToHome(object sender, RoutedEventArgs e) // Add function to the button for listing all music or returning home
        {
            if (MainWindow.other.ItemsSource != null) //
                MainWindow.other.ItemsSource = null;  // Clear existing items in Other
            else                                      //
                MainWindow.other.Items.Clear();       //

            otherMusicPanel.ItemsSource = musicProcess.listOfMusicAll(); // Replace cleared items with components from listOfMusicAll
        }

        private void downloandsMusic(object sender, RoutedEventArgs e) // Add function to the music download button
        {
            var process = Process.Start("downloander.exe"); // Start the required program
            process.WaitForExit(); // Stop DataMusic until the program closes

            libsPanel.ItemsSource = musicProcess.listLibrary();
        }
        private void beforeMusic(object sender, RoutedEventArgs e) // Add function to the previous music button
        {
            if (musicProcess.isLoopOrMixed != true) // Is mix mode enabled?
            {
                musicProcess.beforeMusicPlaying(); // If not, go to the previous music normally
            }
            else
            {
                musicProcess.randomMusic(); // If yes, play randomly
            }
        }
        private void afterMusic(object sender, RoutedEventArgs e) // Add function to the next music button
        {
            if (musicProcess.isLoopOrMixed != true)
            {
                musicProcess.afterMusicPlaying(); // Go to the next music
            }
            else
            {
                musicProcess.randomMusic();
            }
        }
        private void newLibCrate(object sender, RoutedEventArgs e) // Add function to the create library button
        {
            var process = Process.Start("musicLibsCreater.exe"); // Start the required program
            process.WaitForExit(); // Stop DataMusic until the program closes

            libsPanel.ItemsSource = musicProcess.listLibrary();
        }
        private void musicOfPlayOrStop(object sender, RoutedEventArgs e) // Button to pause or resume music
        {
            musicProcess.playOrStop(playOrStop); // Run the required function
        }

        private void search_TextChanged(object sender, TextChangedEventArgs e) // Add function to the search bar
        {
            if (MainWindow.other.ItemsSource != null)
                MainWindow.other.ItemsSource = null;
            else
                MainWindow.other.Items.Clear();

            List<Canvas> theMusics = new List<Canvas>(); // List of searched music
            List<string> thePlaylist = new List<string>(); // For the CurrectPlayList in MusicProcess

            foreach (string theMusic in musicProcess.currectPlayList) // Loop through the current playlist
            {
                if (theMusic.ToLower().Contains(search.Text.ToLower())) // Search with the text in search
                {
                    thePlaylist.Add(theMusic);

                    // Component creation for Other
                    Canvas the = new Canvas();
                    the.Width = 118;
                    the.Height = 118;

                    Image theMusicPP = new Image();
                    theMusicPP.Source = new BitmapImage(new System.Uri("/musicPP.png", System.UriKind.RelativeOrAbsolute));
                    theMusicPP.Width = 39;
                    theMusicPP.Height = 39;
                    Canvas.SetLeft(theMusicPP, 15);
                    Canvas.SetTop(theMusicPP, 5);
                    the.Children.Add(theMusicPP);

                    TextBlock theMusicName = new TextBlock();
                    theMusicName.Text = theMusic.Replace("musics\\", "").Replace(".mp3", "");
                    theMusicName.Width = 55;
                    theMusicName.Height = 113;
                    Canvas.SetLeft(theMusicName, 58);
                    Canvas.SetTop(theMusicName, 5);
                    theMusicName.TextWrapping = TextWrapping.Wrap;
                    the.Children.Add(theMusicName);

                    Button thePlaying = new Button();
                    thePlaying.Content = "Play";
                    thePlaying.Width = 50;
                    thePlaying.Height = 20;

                    thePlaying.Background = Brushes.DarkGray;
                    thePlaying.Foreground = Brushes.White;
                    thePlaying.BorderBrush = Brushes.Transparent;

                    Canvas.SetLeft(thePlaying, 4);
                    Canvas.SetTop(thePlaying, 75);

                    thePlaying.Click += async (sendaer, ea) => { await musicProcess.PlayMusicAsync(theMusic); musicProcess.currectPlaying = theMusic; };

                    the.Children.Add(thePlaying);

                    theMusics.Add(the);
                    // Component creation for Other
                }
            }

            other.ItemsSource = theMusics; // Send components to other

            musicProcess.currectPlayList = thePlaylist; // Assign to CurrectPlayList in MusicProcess
        }

        private void musicProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) // Function when music ends (same as afterMusic)
        {
            if (e.NewValue >= 99.9)
            {
                if (musicProcess.isLoopOrMixed)
                {
                    musicProcess.randomMusic();
                }
                else
                {
                    musicProcess.afterMusicPlaying();
                }
            }
        }

        private void musicVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) // Assigning a function to the volume level selector
        {
            if (musicProcess.audioFile != null)
            {
                musicProcess.audioFile.Volume = (float)e.NewValue;
            }
        }
    }

    class musicProcess // Special class for music operations. Library: NAudio
    {
        private static WaveOutEvent outputDevice; // Output device
        public static AudioFileReader audioFile; // Music file

        public static List<string> currectPlayList = new List<string>(); // Current playlist

        public static string currectPlaying; // Current playing music

        public static bool isLoopOrMixed = false; // Is mixing mode enabled

        public static void importantFolders() // Creates important files
        {
            Directory.CreateDirectory("musics");
        }

        public static List<Canvas> listOfMusicAll() // Lists all music and assigns components to other
        {
            List<Canvas> list = new List<Canvas>();

            string[] asd = Directory.GetFileSystemEntries("musics", "*.mp3", SearchOption.AllDirectories);

            currectPlaying = null;

            foreach (string s in asd)
            {
                currectPlayList.Add(s);

                Canvas the = new Canvas();
                the.Width = 118;
                the.Height = 118;

                Image theMusicPP = new Image();
                theMusicPP.Source = new BitmapImage(new System.Uri("/musicPP.png", System.UriKind.RelativeOrAbsolute));
                theMusicPP.Width = 39;
                theMusicPP.Height = 39;
                Canvas.SetLeft(theMusicPP, 15);
                Canvas.SetTop(theMusicPP, 5);
                the.Children.Add(theMusicPP);

                TextBlock theMusicName = new TextBlock();
                theMusicName.Text = s.Replace("musics\\", "").Replace(".mp3", "");
                theMusicName.Width = 55;
                theMusicName.Height = 113;
                Canvas.SetLeft(theMusicName, 58);
                Canvas.SetTop(theMusicName, 5);
                theMusicName.TextWrapping = TextWrapping.Wrap;
                the.Children.Add(theMusicName);

                Button thePlaying = new Button();
                thePlaying.Content = "Play";
                thePlaying.Width = 50;
                thePlaying.Height = 20;

                thePlaying.Background = Brushes.DarkGray;
                thePlaying.Foreground = Brushes.White;
                thePlaying.BorderBrush = Brushes.Transparent;

                Canvas.SetLeft(thePlaying, 4);
                Canvas.SetTop(thePlaying, 75);

                thePlaying.Click += async (sender, e) => { await PlayMusicAsync(s); currectPlaying = s; };

                the.Children.Add(thePlaying);

                list.Add(the);
            }

            return list;
        }

        public static List<Canvas> listLibrary() // List playlists in the library
        {
            List<Canvas> list = new List<Canvas>();
            string[] libs = Directory.GetDirectories("musics");

            foreach (string lib in libs)
            {
                Canvas the = new Canvas();
                the.Width = 150;
                the.Height = 40;
                the.Background = Brushes.Transparent;

                Image libsImage = new Image();
                libsImage.Source = new BitmapImage(new Uri("/musicPP.png", UriKind.RelativeOrAbsolute));
                libsImage.Width = 42;
                libsImage.Height = 42;
                Canvas.SetLeft(libsImage, 8);
                Canvas.SetTop(libsImage, 3);
                the.Children.Add(libsImage);

                TextBlock libsName = new TextBlock();
                libsName.Text = lib.Replace("musics\\", "");
                libsName.FontSize = 14;
                libsName.TextAlignment = TextAlignment.Center;
                libsName.Width = 60;
                libsName.Height = 42;
                Canvas.SetLeft(libsName, 50);
                Canvas.SetTop(libsName, 3);
                libsName.TextWrapping = TextWrapping.Wrap;
                the.Children.Add(libsName);

                Button viewLib = new Button();
                viewLib.Content = "Show";
                viewLib.Width = 50;
                viewLib.Height = 20;
                Canvas.SetLeft(viewLib, 115);
                Canvas.SetTop(viewLib, 10);
                viewLib.Background = Brushes.DarkGray;
                viewLib.Foreground = Brushes.White;
                viewLib.BorderBrush = Brushes.Transparent;

                viewLib.Click += (sender, e) =>
                {
                    if (MainWindow.other.ItemsSource != null)
                        MainWindow.other.ItemsSource = null;
                    else
                        MainWindow.other.Items.Clear();

                    string[] songs = Directory.GetFiles(lib, "*.mp3", SearchOption.TopDirectoryOnly);

                    currectPlayList = new List<string>();

                    foreach (string song in songs)
                    {
                        currectPlayList.Add(song);
                    }

                    foreach (string s in songs)
                    {
                        Canvas songItem = new Canvas();
                        songItem.Width = 118;
                        songItem.Height = 118;

                        Image songPP = new Image();
                        songPP.Source = new BitmapImage(new Uri("/musicPP.png", UriKind.RelativeOrAbsolute));
                        songPP.Width = 39;
                        songPP.Height = 39;
                        Canvas.SetLeft(songPP, 15);
                        Canvas.SetTop(songPP, 5);
                        songItem.Children.Add(songPP);

                        TextBlock songName = new TextBlock();
                        songName.Text = Path.GetFileNameWithoutExtension(s);
                        songName.Width = 55;
                        songName.Height = 113;
                        Canvas.SetLeft(songName, 58);
                        Canvas.SetTop(songName, 5);
                        songName.TextWrapping = TextWrapping.Wrap;
                        songItem.Children.Add(songName);

                        Button playButton = new Button();
                        playButton.Content = "Play";
                        playButton.Width = 50;
                        playButton.Height = 20;
                        playButton.Background = Brushes.DarkGray;
                        playButton.Foreground = Brushes.White;
                        playButton.BorderBrush = Brushes.Transparent;
                        Canvas.SetLeft(playButton, 4);
                        Canvas.SetTop(playButton, 75);

                        playButton.Click += async (sendear, ea) => { await PlayMusicAsync(s); currectPlaying = s; };
                        songItem.Children.Add(playButton);

                        MainWindow.other.Items.Add(songItem);
                    }
                };

                the.Children.Add(viewLib);
                list.Add(the);
            }

            return list;
        }

        public static async Task PlayMusicAsync(string filePath) // Play the music file at the given file path
        {
            if (outputDevice != null && outputDevice.PlaybackState == PlaybackState.Playing)
            {
                outputDevice.Stop();
                outputDevice.Dispose();
                audioFile.Dispose();
            }

            try
            {
                audioFile = new AudioFileReader(filePath);
                audioFile.Volume = (float)MainWindow.musicVolume.Value;
                outputDevice = new WaveOutEvent();
                
                outputDevice.Init(audioFile);
                outputDevice.Play();

                await Task.CompletedTask;
            }
            catch (Exception ex) { }
        }

        public static void playOrStop(Button sender) // Playback and pause function
        {
            if (outputDevice != null && outputDevice.PlaybackState == PlaybackState.Paused)
            {
                outputDevice.Play();

                sender.Content = "Pause";
            }
            else if (outputDevice != null && outputDevice.PlaybackState == PlaybackState.Playing)
            {
                outputDevice.Pause();

                sender.Content = "Play";
            }
        }

        public static async Task GetMusicProgressAsync() // Function for music progress percentage
        {
            while (true)
            {
                double percentage = 0;

                if (audioFile != null)
                {
                    double current = audioFile.CurrentTime.TotalSeconds;
                    double total = audioFile.TotalTime.TotalSeconds;

                    if (total > 0)
                        percentage = (current / total) * 100.0;

                    MainWindow.musicNumberProgressBar.Value = percentage;
                }

                await Task.Delay(10);
            }
        }

        public static void afterMusicPlaying() // Play the next music
        {
            if (currectPlayList.Count > 0 && currectPlaying != null)
            {
                int currectMusicIndex = currectPlayList.IndexOf(currectPlaying);

                if ((currectMusicIndex + 1) > currectPlayList.Count - 1)
                {
                    PlayMusicAsync(currectPlayList[0]);

                    currectPlaying = currectPlayList[0];
                }
                else
                {
                    PlayMusicAsync(currectPlayList[currectMusicIndex + 1]);

                    currectPlaying = currectPlayList[currectMusicIndex + 1];
                }
            }
        }

        public static void beforeMusicPlaying() // Play the previous music
        {
            if (currectPlayList.Count > 0 && currectPlaying != null)
            {
                int currectMusicIndex = currectPlayList.IndexOf(currectPlaying);

                if ((currectMusicIndex - 1) < 0)
                {
                    PlayMusicAsync(currectPlayList[currectPlayList.Count - 1]);

                    currectPlaying = currectPlayList[currectPlayList.Count - 1];
                }
                else
                {
                    PlayMusicAsync(currectPlayList[currectMusicIndex - 1]);

                    currectPlaying = currectPlayList[currectMusicIndex - 1];
                }
            }
        }
        public static async void randomMusic() // Play a random music from the current playlist
        {
            if (currectPlayList.Count > 0 && currectPlaying != null)
            {
                int currectMusicIndex = new Random().Next(0, currectPlayList.Count);

                currectPlaying = currectPlayList[currectMusicIndex];

                await PlayMusicAsync(currectPlayList[currectMusicIndex]);
            }
        }
    }

    // DataBinding for loopOrMixed for mixed playback 
    public class mixedOrLoopBinding : INotifyPropertyChanged
    {
        private bool _isLoopOrMixed = false;

        public bool IsLoopOrMixed
        {
            get => _isLoopOrMixed;
            set
            {
                if (_isLoopOrMixed != value)
                {
                    _isLoopOrMixed = value;
                    musicProcess.isLoopOrMixed = value; // Connection established
                    OnPropertyChanged(nameof(IsLoopOrMixed));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    // DataBinding for loopOrMixed for mixed playback 
}