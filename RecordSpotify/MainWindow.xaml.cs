using NAudio.Wave;
using NAudio.Lame;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Threading.Tasks;

namespace RecordSpotify
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            UpdateStatusLabel();
        }


        enum RecordingStatus
        {
            Recording,
            WaitingToStart,
            WaitingToStop,
            Stopped
        }


        private RecordingStatus recordingStatus = RecordingStatus.Stopped;
        private void ToggleRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            if (recordingStatus == RecordingStatus.Recording)
            {
                recordingStatus = RecordingStatus.WaitingToStop;
            }
            else if (recordingStatus == RecordingStatus.WaitingToStart)
            {
                recordingStatus = RecordingStatus.Stopped;
            }
            else if (recordingStatus == RecordingStatus.WaitingToStop)
            {
                recordingStatus = RecordingStatus.WaitingToStart;
            }
            else if (recordingStatus == RecordingStatus.Stopped)
            {
                recordingStatus = RecordingStatus.WaitingToStart;
            }

            UpdateStatusLabel();
        }

        private void NewSongStarted(string songname)
        {
            CurrentSongLabel.Content = $"{songname}";

            if (recordingStatus == RecordingStatus.Recording)
            {
                StopAudioCapture();
                SaveFinishedRecordToDB();

                if (IsFirstTimeRecordingThisSong(songname))
                {
                    StartAudioCapture(songname);
                }
                else
                {
                    // //////////////     send skip request
                    recordingStatus = RecordingStatus.WaitingToStart;
                }
            }
            else if (recordingStatus == RecordingStatus.WaitingToStart)
            {
                StartAudioCapture(songname);
                recordingStatus = RecordingStatus.Recording;
            }
            else if (recordingStatus == RecordingStatus.WaitingToStop)
            {
                StopAudioCapture();
                recordingStatus = RecordingStatus.Stopped;
            }

            UpdateStatusLabel();
        }

        private void UpdateStatusLabel()
        {
            if (recordingStatus == RecordingStatus.Recording)
            {
                StatusLabel.Content = "Status: Recording";
            }
            else if (recordingStatus == RecordingStatus.WaitingToStart)
            {
                StatusLabel.Content = "Status: Waiting next song to start";
            }
            else if (recordingStatus == RecordingStatus.WaitingToStop)
            {
                StatusLabel.Content = "Status: Waiting next song to stop";
            }
            else if (recordingStatus == RecordingStatus.Stopped)
            {
                StatusLabel.Content = "Status: Stopped";
            }
        }


        private bool IsFirstTimeRecordingThisSong(string name)
        {
            // ///////////////// Add actual check
            return true;
        }

        private void SaveFinishedRecordToDB()
        {
            // ///////////////// Add actual save
        }




        private static WasapiLoopbackCapture _capture;
        private static LameMP3FileWriter _writer;
        private static string _outputFilePath = "audio";
        private void StartAudioCapture(string filename)
        {
            string songPath = _outputFilePath + $"\\{filename}.mp3";
            Directory.CreateDirectory(_outputFilePath);

            _capture = new WasapiLoopbackCapture();
            _writer = new LameMP3FileWriter(songPath, _capture.WaveFormat, 128);


            _capture.DataAvailable += OnDataAvailable;

            _capture.StartRecording();
        }

        private void StopAudioCapture()
        {
            if (_capture != null)
            {
                _capture.DataAvailable -= OnDataAvailable;

                _capture.StopRecording();
                _capture.Dispose();
                _capture = null;
            }

            if (_writer != null)
            {
                _writer.Flush();
                _writer.Dispose();
                _writer = null;
            }
        }

        private static void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            if (_writer == null) return;

            _writer.Write(e.Buffer, 0, e.BytesRecorded);
        }



        const string InjectionFilePath = "jsinjection.js";
        private async void CopyInjectionButton_Click(object sender, RoutedEventArgs e)
        {
            string jsinjectionText = File.ReadAllText(InjectionFilePath);
            Clipboard.SetText(jsinjectionText);

            string temp = CopyInjectionButton.Content.ToString();
            CopyInjectionButton.Content = "Coppied to clipboard";
            CopyInjectionButton.IsEnabled = false;

            await Task.Delay(1000);

            CopyInjectionButton.Content = temp;
            CopyInjectionButton.IsEnabled = true;
        }
    }
}