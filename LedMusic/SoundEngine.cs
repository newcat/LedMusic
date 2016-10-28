using CSCore;
using CSCore.Codecs;
using CSCore.CoreAudioAPI;
using CSCore.DSP;
using CSCore.SoundOut;
using CSCore.Streams;
using LedMusic.Viewmodels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace LedMusic
{
    class SoundEngine : INotifyPropertyChanged
    {

        #region INotifyProperyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Properties
        private ObservableCollection<MMDevice> _devices = new ObservableCollection<MMDevice>();
        public ObservableCollection<MMDevice> Devices
        {
            get { return _devices; }
            set
            {
                _devices = value;
                NotifyPropertyChanged();
            }
        }

        private int _volume = 80;
        public int Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                if (soundOut != null)
                    soundOut.Volume = Math.Min(1f, Math.Max(value / 100f, 0f));
                NotifyPropertyChanged();
            }
        }

        public PlaybackState PlaybackState
        {
            get
            {
                if (soundOut != null)
                    return soundOut.PlaybackState;
                return PlaybackState.Stopped;
            }
        }

        public bool CanPlay
        {
            get {
                return soundOut != null && waveSource != null && !buildingWaveform &&
                  (PlaybackState == PlaybackState.Stopped || PlaybackState == PlaybackState.Paused);
            }
        }

        public bool CanPause
        {
            get
            {
                return soundOut != null && waveSource != null && PlaybackState == PlaybackState.Playing;
            }
        }

        public bool CanStop
        {
            get
            {
                return soundOut != null && waveSource != null && PlaybackState == PlaybackState.Playing;
            }
        }

        public TimeSpan Position
        {
            get
            {
                if (waveSource != null)
                    return waveSource.GetPosition();
                return TimeSpan.Zero;
            }
            set
            {
                if (waveSource != null && value.TotalMilliseconds >= 0 && value < Length)
                    waveSource.SetPosition(value);
            }
        }

        public TimeSpan Length
        {
            get
            {
                if (waveSource != null)
                    return waveSource.GetLength();
                return TimeSpan.Zero;
            }
        }

        private List<float> _samples = new List<float>();
        public List<float> Samples
        {
            get { return _samples; }
            set
            {
                _samples = value;
                NotifyPropertyChanged();
            }
        }

        public float[] CurrentFftData
        {
            get { return currentFfTData; }
        }

        private string _file = "";
        public string File
        {
            get { return _file; }
            private set
            {
                _file = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        #region Constants
        private const int BLOCK_SIZE = 1000;
        public const FftSize FFT_SIZE = FftSize.Fft2048;
        #endregion

        #region Fields
        private readonly DispatcherTimer positionTimer = new DispatcherTimer(DispatcherPriority.Render);
        private readonly FftProvider fftProvider = new FftProvider(2, FFT_SIZE);
        private ISoundOut soundOut;
        private IWaveSource waveSource;
        private bool buildingWaveform = false;
        private float[] currentFfTData = new float[(int)FFT_SIZE];
        #endregion

        #region Constructor
        private static readonly SoundEngine _instance = new SoundEngine();
        public static SoundEngine Instance
        {
            get
            {
                return _instance;
            }
        }

        private SoundEngine()
        {
            loadDevices();

            positionTimer.Interval = TimeSpan.FromMilliseconds(1000 / 60d);
            positionTimer.Tick += PositionTimer_Tick;
            positionTimer.IsEnabled = true;
        }
        #endregion

        #region Public Methods
        public async Task<bool> OpenFile(string filename)
        {

            CleanupPlayback();

            File = filename;

            waveSource = CodecFactory.Instance.GetCodec(filename).ToSampleSource().ToWaveSource();

            if (!waveSource.CanSeek)
            {
                Debug.WriteLine("WaveSource doesnt support seeking.");
                return false;
            }

            soundOut = new WasapiOut(false, AudioClientShareMode.Shared, 10, System.Threading.ThreadPriority.AboveNormal);
            soundOut.Initialize(waveSource);
            soundOut.Stopped += SoundOut_Stopped;
            Volume = _volume;

            //var notificationSource = new SingleBlockNotificationStream(waveSource.ToSampleSource());
            //notificationSource.SingleBlockRead += (s, e) => fftProvider.Add(e.Left, e.Right);

            buildingWaveform = true;
            ISampleSource sampleBuildingSource = CodecFactory.Instance.GetCodec(filename).ToSampleSource().ToMono();
            await Task.Run(() => createSampleList(sampleBuildingSource));
            buildingWaveform = false;

            soundOut.Play();

            return true;

        }

        private void SoundOut_Stopped(object sender, PlaybackStoppedEventArgs e)
        {
            if (e.HasError)
                Debug.WriteLine(e.Exception);
        }

        public void Play()
        {
            if (CanPlay)
                soundOut.Play();
        }

        public void Pause()
        {
            if (CanPause)
                soundOut.Pause();
        }

        public void Stop()
        {
            if (CanStop)
            {
                soundOut.Stop();
                waveSource.SetPosition(TimeSpan.Zero);
            }
        }

        public float GetCurrentSample()
        {

            double currentTime = Position.TotalSeconds;
            if (currentTime <= 0)
                return 0f;

            int sampleNumber = (int)Math.Floor((currentTime * waveSource.WaveFormat.SampleRate) / BLOCK_SIZE);

            if (sampleNumber >= 0 && sampleNumber < Samples.Count)
                return Samples[sampleNumber];
            else
                return 0f;

        }

        public int GetFftBandIndex(float frequency)
        {
            int fftSize = (int)FFT_SIZE;
            double f = waveSource.WaveFormat.SampleRate / 2.0;
            return (int)((frequency / f) * (fftSize / 2));
        }

        public void CalculateFft()
        {
            if (!fftProvider.GetFftData(currentFfTData))
            {
                Debug.WriteLine("GetFftData returned false");
            }
        }

        public void CleanupPlayback()
        {
            if (soundOut != null)
            {
                soundOut.Dispose();
                soundOut = null;
            }

            if (waveSource != null)
            {
                waveSource.Dispose();
                waveSource = null;
            }
        }
        #endregion

        #region Private Methods
        private void loadDevices()
        {
            using (var mmdeviceEnumerator = new MMDeviceEnumerator())
            {
                using (var mmdeviceCollection = mmdeviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active))
                {
                    foreach (var device in mmdeviceCollection)
                        Devices.Add(device);
                }
            }
        }

        private void createSampleList(ISampleSource sampleSource)
        {            

            if (sampleSource == null)
                throw new ArgumentNullException("sampleSource");

            var newSamples = new List<float>();
            var buffer = new float[BLOCK_SIZE];
            float blockMaxValue;

            while (sampleSource.Read(buffer, 0, BLOCK_SIZE) > 0)
            {
                blockMaxValue = 0;
                for (int i = 0; i < buffer.Length; i++)
                {
                    if (Math.Abs(buffer[i]) > blockMaxValue)
                        blockMaxValue = Math.Abs(buffer[i]);
                }
                newSamples.Add(blockMaxValue);
            }

            Samples = newSamples;

        }

        private void PositionTimer_Tick(object sender, EventArgs e)
        {
            if (PlaybackState == PlaybackState.Playing)
                NotifyPropertyChanged("Position");
        }
        #endregion

    }
}
