using NAudio.Lame;
using NAudio.Wave;
using System.IO;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RecordSpotify
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HttpListener _listener;

        public MainWindow()
        {
            InitializeComponent();

            StartServer();
            UpdateStatusLabel();
        }


        private void StartServer()
        {
            string prefix = "http://localhost:2034/";
            _listener = new HttpListener();
            _listener.Prefixes.Add(prefix);
            _listener.Start();

            Task.Run(() => ListenLoop());
        }


        private async Task ListenLoop()
        {
            while (_listener.IsListening)
            {
                try
                {
                    HttpListenerContext context = await _listener.GetContextAsync();
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;

                    // Handle CORS Preflight request (Chrome requires this for local requests)
                    if (request.HttpMethod == "OPTIONS")
                    {
                        response.AddHeader("Access-Control-Allow-Origin", "*");
                        response.AddHeader("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
                        response.AddHeader("Access-Control-Allow-Headers", "Content-Type");
                        response.StatusCode = (int)HttpStatusCode.OK;
                        response.Close();
                        continue;
                    }

                    if (request.HttpMethod == "POST")
                    {
                        // 1. Read incoming data from JS fetch
                        string incomingData = "";
                        using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                        {
                            incomingData = await reader.ReadToEndAsync();
                        }

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            CurrentSongLabel.Content = $"{incomingData}";
                        });

                        string responseMessage = "OK";


                        // 2. Check for duplicates
                        string songPath = _outputFilePath + $"\\{incomingData}.mp3";
                        if (File.Exists(songPath))
                        {
                            responseMessage = "skip";
                        }
                        else
                        {
                            NewSongStarted(incomingData);
                        }


                        // 4. Send response back to Chrome JS
                        byte[] buffer = Encoding.UTF8.GetBytes(responseMessage);
                        response.ContentType = "text/plain";
                        response.AddHeader("Access-Control-Allow-Origin", "*"); // Allow Chrome to read it
                        response.ContentLength64 = buffer.Length;

                        using (Stream output = response.OutputStream)
                        {
                            await output.WriteAsync(buffer, 0, buffer.Length);
                        }
                    }

                    response.Close();
                }
                catch (Exception ex)
                {
                    // Handle server closed or errors
                    System.Diagnostics.Debug.WriteLine($"Server Error: {ex.Message}");
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _listener?.Stop();
            StopAudioCapture();
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
            if (recordingStatus == RecordingStatus.Recording)
            {
                StopAudioCapture();
                StartAudioCapture(songname);
            }
            else if (recordingStatus == RecordingStatus.WaitingToStart)
            {
                recordingStatus = RecordingStatus.Recording;
                StartAudioCapture(songname);
            }
            else if (recordingStatus == RecordingStatus.WaitingToStop)
            {
                recordingStatus = RecordingStatus.Stopped;
                StopAudioCapture();
            }


            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateStatusLabel();
            });
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