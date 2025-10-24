using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace rog
{
    public partial class MainWindow : Window
    {
        private MediaPlayer player = new MediaPlayer();
        private string currentFile = "";
        private bool isPaused = false;
        private bool hasPlayed = false;
        // song length slider
        private bool isDragging = false;
        private DispatcherTimer timer;
        // path for resumefile
        private readonly string ResumeFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "rog_resume.txt"
        );

        public MainWindow()
        {
            InitializeComponent();
            SetupTimer();
            LoadLastSession();
            player.MediaOpened += Player_MediaOpened;
        }

        private void SetupTimer()
        {
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (player.Source != null && player.NaturalDuration.HasTimeSpan && !isDragging)
            {
                ProgressSlider.Maximum = player.NaturalDuration.TimeSpan.TotalSeconds;
                ProgressSlider.Value = player.Position.TotalSeconds;
            }

            if (hasPlayed && player.Source != null)
            {
                SaveSession();
            }
        }

        private void Player_MediaOpened(object? sender, EventArgs e)
        {
            // check if the resume file has value
            if (File.Exists(ResumeFile))
            {
                var data = File.ReadAllText(ResumeFile).Split('|');
                if (data.Length == 2 && File.Exists(data[0]))
                {
                    double seconds;
                    if (double.TryParse(data[1], out seconds) && seconds > 0)
                    {
                        player.Position = TimeSpan.FromSeconds(seconds);
                    }
                }
            }
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Audio Files|*.mp3;*.wav;*.wma;*.m4a|All Files|*.*"
            };

            if (dlg.ShowDialog() == true)
            {
                currentFile = dlg.FileName;
                NowPlaying.Text = $"Loaded: {Path.GetFileName(currentFile)}";
                isPaused = false;
                hasPlayed = false;
                SaveSession(0);
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentFile))
            {
                NowPlaying.Text = "Please load a file first.";
                return;
            }

            if (player.Source == null)
            {
                player.Open(new Uri(currentFile));
            }

            player.Play();
            hasPlayed = true;
            timer.Start();
            isPaused = false;
            NowPlaying.Text = $"Playing: {Path.GetFileName(currentFile)}";
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            if (player.CanPause)
            {
                player.Pause();
                isPaused = true;
                SaveSession();
                NowPlaying.Text = $"Paused: {Path.GetFileName(currentFile)}";
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (player.Source != null)
            {
                SaveSession(0);
                player.Stop();
                timer.Stop();
                isPaused = false;
                ProgressSlider.Value = 0;
                NowPlaying.Text = "Stopped.";
            }
        }

        private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isDragging)
                return;
        }

        private void SaveSession(double? overridePos = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(currentFile) && player.Source != null)
                {
                    double position = overridePos ?? player.Position.TotalSeconds;
                    if (player.NaturalDuration.HasTimeSpan && position > 0)
                    {
                        File.WriteAllText(ResumeFile, $"{currentFile}|{position}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error saving session: {e.Message}");
            }
        }

        private void LoadLastSession()
        {
            try
            {
                if (File.Exists(ResumeFile))
                {
                    var data = File.ReadAllText(ResumeFile).Split('|');
                    if (data.Length == 2 && File.Exists(data[0]))
                    {
                        currentFile = data[0];
                        NowPlaying.Text = $"Resume: {Path.GetFileName(currentFile)}";
                        isPaused = true;
                    }
                }
            }
            catch (Exception e) 
            {
                Console.WriteLine($"Error loading session: {e.Message}");
            }
        }
    }
}
