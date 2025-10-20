using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Media;

namespace rog
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MediaPlayer player = new MediaPlayer();
        private string currentFile = "";
        private bool isPaused = false;
        public MainWindow()
        {
            InitializeComponent();
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
                NowPlaying.Text = $"Loaded: {System.IO.Path.GetFileName(currentFile)}";
            }
        }
        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentFile))
            {
                NowPlaying.Text = "Please load a file first.";
                return;
            }

            if (!isPaused)
            {
                player.Open(new Uri(currentFile));
            }

            player.Play();
            isPaused = false;
            NowPlaying.Text = $"Playing: {System.IO.Path.GetFileName(currentFile)}";
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            if (player.CanPause)
            {
                player.Pause();
                isPaused = true;
                NowPlaying.Text = $"Paused: {System.IO.Path.GetFileName(currentFile)}";
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            player.Stop();
            isPaused = false;
            NowPlaying.Text = "Stopped.";
        }
    }
}