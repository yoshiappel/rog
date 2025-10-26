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
        private bool isPlaying = false;
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
            player.MediaEnded += Player_MediaEnded;
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

        private void Player_MediaEnded(object? sender, EventArgs e)
        {
            timer.Stop();
            isPlaying = false;
            isPaused = false;
            PlayPauseButton.Content = "▶";
            ProgressSlider.Value = 0;
            NowPlaying.Text = "Playback finished.";
            SaveSession(0, true);
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Audio Files|*.mp3;*.wav;*.wma;*.m4a|All Files|*.*"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    // reset everything UI and player
                    timer.Stop();
                    player.Stop();
                    player.Close(); 
                    currentFile = dlg.FileName;
                    ProgressSlider.Value = 0;
                    isPlaying = false;
                    isPaused = false;
                    hasPlayed = false;
                    PlayPauseButton.Content = "▶";
                    NowPlaying.Text = $"{System.IO.Path.GetFileName(currentFile)}";

                    // reset resume position
                    SaveSession(0, true);

                    // for autoplaying the new song
                    //player.Open(new Uri(currentFile));
                    //player.Play();
                    //isPlaying = true;
                    //hasPlayed = true;
                    //timer.Start();
                    //PlayPauseButton.Content = "⏸ Pause";
                    //NowPlaying.Text = $"Playing: {System.IO.Path.GetFileName(currentFile)}";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading song: {ex.Message}");
                }
            }
        }

        // show play or pause
        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
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

            if (!isPlaying)
            {
                player.Play();
                timer.Start();
                isPlaying = true;
                hasPlayed = true;
                isPaused = false;
                PlayPauseButton.Content = "❚❚";
                NowPlaying.Text = $"{System.IO.Path.GetFileName(currentFile)}";
            }
            else
            {
                player.Pause();
                isPlaying = false;
                isPaused = true;
                SaveSession();
                PlayPauseButton.Content = "▶";
                NowPlaying.Text = $"{System.IO.Path.GetFileName(currentFile)}";
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
                isPlaying = false;
                ProgressSlider.Value = 0;
                NowPlaying.Text = "Stopped.";
                PlayPauseButton.Content = "▶";
                player.Close();
            }
        }

        private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isDragging)
                return;
        }

        private void SaveSession(double? overridePos = null, bool force = false)
        {
            try
            {
                // We can always save if we have a file path — even if player.Source == null
                if (!string.IsNullOrEmpty(currentFile))
                {
                    double position = overridePos ?? 0;

                    // If we're not forcing and player has a valid source, use its position
                    if (!force && player.Source != null && player.NaturalDuration.HasTimeSpan)
                    {
                        position = player.Position.TotalSeconds;
                    }

                    File.WriteAllText(ResumeFile, $"{currentFile}|{position}");
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
                        NowPlaying.Text = $"{Path.GetFileName(currentFile)}";
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
