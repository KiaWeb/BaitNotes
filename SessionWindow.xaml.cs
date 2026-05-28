using Microsoft.Win32;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media;

namespace BaitNotes
{
    public partial class SessionWindow : Window
    {
        private WasapiLoopbackCapture? capture;
        private WaveFileWriter? writer;
        private MediaPlayer bgmPlayer = new MediaPlayer();
        private string sessionFolder = "";
        private string? outputFile = null;
        private bool isRecording = false;
        private bool recordedSomething = false;

        public SessionWindow()
        {
            InitializeComponent();
            LoadAudioDevices();

            StatusText.Text = "Idle";
        }

        private string GetSessionTimestamp()
        {
            return DateTime.Now.ToString("yyyy-MM-dd_hh-mm-tt");
        }

        private void CreateSessionFolderIfNeeded()
        {
            if (!string.IsNullOrWhiteSpace(sessionFolder))
                return;

            string timestamp = GetSessionTimestamp();

            sessionFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "BaitNotes Sessions",
                timestamp
            );

            Directory.CreateDirectory(sessionFolder);
        }

        private void LoadAudioDevices()
        {
            var enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

            foreach (var device in devices)
            {
                AudioDeviceBox.Items.Add(device);
            }

            AudioDeviceBox.DisplayMemberPath = "FriendlyName";

            if (AudioDeviceBox.Items.Count > 0)
                AudioDeviceBox.SelectedIndex = 0;
        }

        private void StartRecording()
        {
            if (AudioDeviceBox.SelectedItem is not MMDevice device)
            {
                MessageBox.Show("No audio device selected.");
                return;
            }

            CreateSessionFolderIfNeeded();

            string timestamp = GetSessionTimestamp();
            outputFile = Path.Combine(sessionFolder, timestamp + ".wav");

            capture = new WasapiLoopbackCapture(device);
            writer = new WaveFileWriter(outputFile, capture.WaveFormat);

            capture.DataAvailable += (s, e) =>
            {
                writer?.Write(e.Buffer, 0, e.BytesRecorded);
                writer?.Flush();
            };

            capture.RecordingStopped += (s, e) =>
            {
                writer?.Dispose();
                writer = null;

                capture?.Dispose();
                capture = null;
            };

            capture.StartRecording();

            isRecording = true;
            recordedSomething = true;

            RecordButton.Content = "■ Stop Recording";
            RecordButton.Background = Brushes.DarkGray;

            StatusText.Text = "Recording desktop audio...";
        }

        private void StopRecording()
        {
            if (capture != null)
            {
                capture.StopRecording();
            }

            isRecording = false;

            RecordButton.Content = "● Record";
            RecordButton.Background = new SolidColorBrush(Color.FromRgb(170, 34, 34));

            StatusText.Text = "Recording stopped.";
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isRecording)
            {
                StartRecording();
            }
            else
            {
                StopRecording();
            }
        }

        public void EndSession()
        {
            bool savedText = false;
            bool savedAudio = false;

            if (isRecording)
            {
                StopRecording();
            }

            string notes = NotesBox.Text.Trim();

            if (!string.IsNullOrWhiteSpace(notes))
            {
                CreateSessionFolderIfNeeded();

                string timestamp = GetSessionTimestamp();
                string textPath = Path.Combine(sessionFolder, timestamp + ".txt");

                File.WriteAllText(textPath, notes);

                savedText = true;
            }

            if (recordedSomething && outputFile != null && File.Exists(outputFile))
            {
                savedAudio = true;
            }

            if (savedText || savedAudio)
            {
                MessageBox.Show($"Text and audio saved to \"{sessionFolder}\"");
            }
            bgmPlayer.Stop();
        }
        private void PlayBgm(string path)
        {
            bgmPlayer.Open(new Uri(path, UriKind.RelativeOrAbsolute));
            bgmPlayer.Volume = 0.20;

            bgmPlayer.MediaEnded += (s, e) =>
            {
                bgmPlayer.Position = TimeSpan.Zero;
                bgmPlayer.Play();
            };

            bgmPlayer.Play();
        }
        private void BgmBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BgmBox.SelectedItem is not ComboBoxItem item)
                return;

            string choice = item.Content.ToString();

            bgmPlayer.Stop();

            if (choice == "None")
                return;

            if (choice == "Custom")
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Audio Files|*.mp3;*.wav;*.wma";

                if (dialog.ShowDialog() == true)
                {
                    PlayBgm(dialog.FileName);
                }

                return;
            }

            if (choice == "Desert")
            {
                PlayBgm("Assets\\desert.wav");
            }
            else if (choice == "Water")
            {
                PlayBgm("Assets\\water.wav");
            }
        }
    }
}