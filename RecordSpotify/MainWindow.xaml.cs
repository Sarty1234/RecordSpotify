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
        }





        private static WasapiLoopbackCapture _capture;
        private static LameMP3FileWriter _writer;
        private static string _outputFilePath = "audio\\{filename}.mp3";

        private void StartAudioCapture(string filename)
        {
            _capture = new WasapiLoopbackCapture();
            _writer = new LameMP3FileWriter(_outputFilePath.Replace("{filename}", filename), _capture.WaveFormat, 128);

            _capture.DataAvailable += OnDataAvailable;
            _capture.RecordingStopped += RecordingStopped;

            _capture.StartRecording();
        }

        private void StopAudioCapture()
        {
            _capture.StopRecording();
        }

        private static void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            _writer.Write(e.Buffer, 0, e.BytesRecorded);
            _writer.Flush();
        }
        
        private static void RecordingStopped(object sender, StoppedEventArgs e)
        {
            _writer?.Dispose();
            _capture?.Dispose();
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