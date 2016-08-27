using LedMusic.Interfaces;
using LedMusic.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Input;
using System.Windows.Media;

namespace LedMusic.Viewmodels
{
    class MainModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #region Properties
        private ObservableCollection<Layer> _layers;
        public ObservableCollection<Layer> Layers
        {
            get { return _layers; }
            set
            {
                _layers = value;
                NotifyPropertyChanged();
            }
        }

        private Layer _currentLayer;
        public Layer CurrentLayer
        {
            get { return _currentLayer; }
            set
            {
                _currentLayer = value;
                NotifyPropertyChanged();
            }
        }

        private IAnimatable _currentIAnimatable;
        public IAnimatable CurrentAnimatable
        {
            get { return _currentIAnimatable; }
            set
            {
                _currentIAnimatable = value;
                NotifyPropertyChanged();
            }
        }

        private double _trackWidth = 1000;
        public double TrackWidth
        {
            get { return _trackWidth; }
            set
            {
                _trackWidth = value;
                NotifyPropertyChanged();
            }
        }

        private double _gridLineDistance = 100d;
        public double GridLineDistance
        {
            get { return _gridLineDistance; }
            set
            {
                _gridLineDistance = value;
                NotifyPropertyChanged();
            }
        }

        private double _viewportOffset = 0d;
        public double ViewportOffset
        {
            get { return _viewportOffset; }
            set
            {
                _viewportOffset = value;
                NotifyPropertyChanged();
            }
        }

        private double _playerPosition = 0; //IN PIXELS!!!
        public double PlayerPosition
        {
            get { return _playerPosition; }
            set
            {
                _playerPosition = value;
                NotifyPropertyChanged();
            }
        }

        private int _currentFrame = 0;
        public int CurrentFrame
        {
            get { return _currentFrame; }
            set
            {
                _currentFrame = value;
                NotifyPropertyChanged();
            }
        }

        private GradientStopCollection _stripPreview;
        public GradientStopCollection StripPreview
        {
            get { return _stripPreview; }
            set
            {
                _stripPreview = value;
                NotifyPropertyChanged();
            }
        }

        public List<string> AvailableGenerators { get; private set; }

        public RelayCommand<string> CmdAnimate { get; private set; }
        public RelayCommand CmdOpenMusicFile { get; private set; }
        public RelayCommand CmdPlay { get; private set; }
        public RelayCommand CmdPause { get; private set; }
        public RelayCommand CmdStop { get; private set; }
        public RelayCommand CmdChooseGenerator { get; private set; }
        #endregion

        #region Constructor
        private static MainModel instance;
        private MainModel() {
            _layers = new ObservableCollection<Layer>();
            PropertyChanged += this_PropertyChanged;
            BassEngine.Instance.PropertyChanged += BassEngine_PropertyChanged;
            GlobalProperties.Instance.PropertyChanged += GlobalProperties_PropertyChanged;

            CmdAnimate = new RelayCommand<string>(cmdAnimate_Execute, cmdAnimate_CanExecute);
            CmdOpenMusicFile = new RelayCommand(cmdOpenMusicFile_Execute);
            CmdPlay = new RelayCommand(
                () => { BassEngine.Instance.Play(); },
                () => { return BassEngine.Instance.CanPlay; });
            CmdPause = new RelayCommand(
                () => { BassEngine.Instance.Pause(); },
                () => { return BassEngine.Instance.CanPause; });
            CmdStop = new RelayCommand(
                () => { BassEngine.Instance.Stop(); },
                () => { return BassEngine.Instance.CanStop; });
            CmdChooseGenerator = new RelayCommand(cmdChooseGenerator_Execute, () => { return CurrentLayer != null; });

            fillGeneratorsList();
        }

        public static MainModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MainModel();
                }
                return instance;
            }
        }
        #endregion

        #region PropertyChangedEvents
        private void this_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "TrackWidth")
            {
                updateGrid();
                updateChannelPosition();
            }
        }

        private void BassEngine_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ChannelLength")
                updateGrid();
            if (e.PropertyName == "ChannelPosition")
            {
                updateChannelPosition();
                calculateCurrentFrame();
                updatePreviewStrip();
            }
        }

        private void GlobalProperties_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BPM" || e.PropertyName == "BeatOffset")
                updateGrid();
        }
        #endregion

        public void updateGrid()
        {

            double trackDuration = BassEngine.Instance.ChannelLength;
            if (trackDuration == 0)
                return;

            double BPM = GlobalProperties.Instance.BPM;

            //Width in pixels of 1s
            double secondWidth = (TrackWidth / trackDuration);

            double beatDuration = 60 / BPM;
            double barDuration = 4 * beatDuration;

            double beatWidth = beatDuration * secondWidth;
            double barWidth = barDuration * secondWidth;

            GridLineDistance = beatWidth >= 30 ? beatWidth : barWidth;
            ViewportOffset = GlobalProperties.Instance.BeatOffset * beatWidth;

        }

        public void updateChannelPosition()
        {

            double trackDuration = BassEngine.Instance.ChannelLength;
            if (trackDuration == 0)
                return;

            double secondWidth = (TrackWidth / trackDuration);
            PlayerPosition = BassEngine.Instance.ChannelPosition * secondWidth;

        }

        private void calculateCurrentFrame()
        {
            int fps = GlobalProperties.Instance.FPS;
            double time = BassEngine.Instance.ChannelPosition;
            CurrentFrame = (int)Math.Floor(time * fps);
        }

        private void fillGeneratorsList()
        {
            List<string> generatorNames = new List<string>();
            Type[] gens = Assembly.GetCallingAssembly().GetTypes().Where(t => string.Equals(t.Namespace, "LedMusic.Generators", StringComparison.Ordinal)).ToArray();
            foreach (Type t in gens)
            {
                if (t.GetInterfaces().Contains(typeof(IGenerator)))
                    generatorNames.Add(t.Name);
            }
            AvailableGenerators = generatorNames;
        }

        public void ChangeAnimatableParameter(double value, string propertyName)
        {

            if (isPropertyAnimated(propertyName, CurrentAnimatable.AnimatedProperties))
            {
                AnimatedProperty ap;
                getAnimatedPropertyReference(propertyName, CurrentAnimatable.AnimatedProperties, out ap);
                if (ap == null)
                    return;

                Keyframe kf;
                if (containsKeyframe(CurrentFrame, ap.Keyframes, out kf))
                {
                    kf.Value = value;
                }
                else
                {
                    ap.Keyframes.Add(new Keyframe(CurrentFrame, value));
                }
            }
            else
            {
                PropertyInfo pi = CurrentAnimatable.GetType().GetProperty(propertyName);
                if (pi == null)
                    return;

                if (pi.PropertyType == typeof(int))
                {
                    pi.SetValue(CurrentAnimatable, (int)value);
                }
                else if (pi.PropertyType == typeof(double))
                {
                    pi.SetValue(CurrentAnimatable, value);
                }
                else if (pi.PropertyType == typeof(bool))
                {
                    pi.SetValue(CurrentAnimatable, value > 0.5 ? true : false);
                }
            }

        }

        private bool isPropertyAnimated(string propertyName, IEnumerable<AnimatedProperty> animatedProperties)
        {

            if (animatedProperties == null)
                return false;

            foreach (AnimatedProperty ap in animatedProperties)
            {
                if (ap.PropertyName == propertyName)
                    return true;
            }
            return false;
        }

        private void getAnimatedPropertyReference(string propertyName, IEnumerable<AnimatedProperty> animatedProperties, out AnimatedProperty ap)
        {

            if (animatedProperties == null)
            {
                ap = null;
                return;
            }

            foreach (AnimatedProperty _ap in animatedProperties)
            {
                if (_ap.PropertyName == propertyName)
                {
                    ap = _ap;
                    return;
                }
            }
            ap = null;
        }

        #region Mixdown and Rendering
        public void updatePreviewStrip()
        {

            GradientStopCollection gsc = new GradientStopCollection();
            int ledCount = GlobalProperties.Instance.LedCount;
            ColorRGB[] ledColors = getColorsByFrame(CurrentFrame);

            for (int i = 0; i < ledCount; i++)
            {
                gsc.Add(new GradientStop(System.Windows.Media.Color.FromArgb(
                    Convert.ToByte(ledColors[i].A * 255), ledColors[i].R, ledColors[i].G, ledColors[i].B),
                    i * 1.0 / (ledCount - 1)));
            }

            StripPreview = gsc;

        }

        private ColorRGB[] getColorsByFrame(int frame)
        {

            //Copy the layers list so changing the properties doesnt change them in the UI
            //-> avoids creating keyframes
            List<Layer> localLayers = new List<Layer>(Layers);
            int ledCount = GlobalProperties.Instance.LedCount;
            ColorRGB[] final = new ColorRGB[ledCount];

            for (int i = 0; i < ledCount; i++)
            {
                final[i] = new ColorRGB(1, 0, 0, 0);
            }

            for (int i = localLayers.Count - 1; i >= 0; i--)
            {
                Layer l = localLayers[i];
                IGenerator g = l.Generator;

                //Set properties according to the keyframes
                IAnimatable a = (IAnimatable)g;
                applyKeyframes(ref a, frame);

                foreach (IController con in a.Controllers)
                {
                    IAnimatable aCon = (IAnimatable)con;
                    if (aCon != null)
                        applyKeyframes(ref aCon, frame);
                    setPropertyValue(ref a, con.PropertyName, con.getValueAt(frame));
                }

                Models.Color[] thisLayerColor = g.getSample(frame);

                for (int j = 0; j < ledCount; j++)
                {
                    if (thisLayerColor[j] == null)
                    {
                        Type t = thisLayerColor.GetType().GetElementType();
                        if (t == typeof(ColorRGB))
                        {
                            thisLayerColor[j] = new ColorRGB(0, 0, 0, 0);
                        } else
                        {
                            thisLayerColor[j] = new ColorHSV(0, 0, 0, 0);
                        }
                    }
                }

                for (int j = 0; j < ledCount; j++)
                {
                    ColorRGB crgb = thisLayerColor[j].getColorRGB();
                    crgb.A = crgb.A * l.Alpha;
                    final[j] = crgb.Overlay(final[j]);
                }                

            }

            return final;

        }

        private void applyKeyframes(ref IAnimatable a, int frame)
        {
            foreach (AnimatedProperty ap in a.AnimatedProperties)
            {

                for (int i = 0; i < a.Controllers.Count; i++)
                {
                    if (a.Controllers[i] is IAnimatable)
                    {
                        IAnimatable c = (IAnimatable)a.Controllers[i];
                        applyKeyframes(ref c, frame);
                        a.Controllers[i] = (IController)c;
                    }
                }

                Keyframe k = getKeyframeAt(frame, ap.Keyframes);

                double value = 0;

                if (k == null)
                {
                    Keyframe pK = findPreviousKeyframe(frame, ap.Keyframes);
                    Keyframe nK = findNextKeyframe(frame, ap.Keyframes);

                    if (pK == null)
                    {
                        value = nK.Value;
                    }
                    else if (nK == null)
                    {
                        value = pK.Value;
                    }
                    else
                    {
                        double m = (nK.Value - pK.Value) / (nK.Frame - pK.Frame);
                        value = pK.Value + (frame - pK.Frame) * m;
                    }
                }
                else
                {
                    value = k.Value;
                }

                setPropertyValue(ref a, ap.PropertyName, value);

            }
        }

        private void setPropertyValue(ref IAnimatable a, string propertyName, double value)
        {
            PropertyInfo pi = a.GetType().GetProperty(propertyName);

            if (pi == null)
                return;

            if (pi.PropertyType == typeof(int))
            {
                pi.SetValue(a, Convert.ToInt32(value));
            } else if (pi.PropertyType == typeof(bool))
            {
                pi.SetValue(a, value > 0.5 ? true : false);
            } else if (pi.PropertyType == typeof(double))
            {
                pi.SetValue(a, value);
            }
        }

        private Keyframe getKeyframeAt(int frame, IEnumerable<Keyframe> keyframes)
        {
            foreach (Keyframe k in keyframes)
            {
                if (k.Frame == frame)
                    return k;
            }
            return null;
        }

        private Keyframe findPreviousKeyframe(int frame, IEnumerable<Keyframe> keyframes)
        {
            Keyframe closestKeyframe = null;
            int diff = 0;

            foreach (Keyframe k in keyframes)
            {
                if (k.Frame < frame && (frame - k.Frame < diff || diff == 0))
                {
                    closestKeyframe = k;
                    diff = frame - k.Frame;
                }
            }

            return closestKeyframe;
        }

        private Keyframe findNextKeyframe(int frame, IEnumerable<Keyframe> keyframes)
        {
            Keyframe closestKeyframe = null;
            int diff = 0;

            foreach (Keyframe k in keyframes)
            {
                if (k.Frame > frame && (k.Frame - frame < diff || diff == 0))
                {
                    closestKeyframe = k;
                    diff = k.Frame - frame;
                }
            }

            return closestKeyframe;
        }

        private bool containsKeyframe(int frame, IEnumerable<Keyframe> keyframes, out Keyframe k)
        {
            foreach (Keyframe kf in keyframes)
            {
                if (kf.Frame == frame)
                {
                    k = kf;
                    return true;
                }
            }
            k = null;
            return false;
        }
        #endregion

        #region Commands
        private void cmdAnimate_Execute(string propertyName)
        {

            PropertyInfo pi = CurrentLayer.Generator.GetType().GetProperty(propertyName);
            if (pi == null)
                return;

            double value = Convert.ToDouble(pi.GetValue(CurrentLayer.Generator));

            IAnimatable gen = (IAnimatable)(CurrentLayer.Generator);

            double minValue = 0;
            double maxValue = 0;

            foreach (PropertyModel pm in gen.AnimatableProperties)
            {
                if (pm.Name == propertyName)
                {
                    minValue = pm.MinValue;
                    maxValue = pm.MaxValue;
                }
            }

            gen.AnimatedProperties.Add(new AnimatedProperty(propertyName, new Keyframe(CurrentFrame, value), minValue, maxValue));

        }

        private bool cmdAnimate_CanExecute(string propertyName)
        {
            return !isPropertyAnimated(propertyName, ((IAnimatable)CurrentLayer.Generator).AnimatedProperties);
        }

        private void cmdOpenMusicFile_Execute()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.CheckFileExists = true;
            ofd.Filter = "Audio files (*.wav;*.mp3)|*.wav;*mp3";
            if (ofd.ShowDialog() == true)
            {
                BassEngine.Instance.OpenFile(ofd.FileName);
            }
        }

        private void cmdChooseGenerator_Execute()
        {
            ChooseGeneratorWindow cgw = new ChooseGeneratorWindow();
            cgw.DataContext = this;
            if (cgw.ShowDialog() == true)
            {
                foreach (Layer l in Layers)
                {
                    if (l.LayerNumber == CurrentLayer.LayerNumber)
                    {
                        l.Generator = (IGenerator)Activator.CreateInstance(
                            Type.GetType("LedMusic.Generators." + cgw.lbGenerators.SelectedItem));
                        return;
                    }
                }
            }
        }
        #endregion

        #region Saving/Loading stuff
        public void Save()
        {

            if (GlobalProperties.Instance.CurrentProjectFile == null ||
                GlobalProperties.Instance.CurrentProjectFile == "")
                SaveAs();

            List<Layer> layersList = new List<Layer>(_layers);
            FileStream fs = new FileStream(GlobalProperties.Instance.CurrentProjectFile, FileMode.Create);

            Savefile s = new Savefile(layersList);
            IFormatter f = new BinaryFormatter();
            f.Serialize(fs, s);
            fs.Close();

        }

        public void SaveAs()
        {

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.CheckFileExists = false;
            sfd.CheckPathExists = true;
            sfd.AddExtension = true;
            sfd.DefaultExt = ".lmp";

            if (sfd.ShowDialog() == true)
            {
                GlobalProperties.Instance.CurrentProjectFile = sfd.FileName;
                Save();
            }


        }

        public void Open()
        {

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.Filter = "LedMusic project files (*.lmp)|*.lmp";

            if (ofd.ShowDialog() == true)
            {
                Stream fs = ofd.OpenFile();
                IFormatter f = new BinaryFormatter();
                Savefile s = (Savefile)f.Deserialize(fs);
                fs.Close();

                GlobalProperties.Instance.CurrentProjectFile = ofd.FileName;
                GlobalProperties.Instance.LedCount = s.ledCount;
                GlobalProperties.Instance.BPM = s.bpm;
                GlobalProperties.Instance.BeatOffset = s.beatOffset;
                GlobalProperties.Instance.FPS = s.fps;

                BassEngine.Instance.OpenFile(s.musicFile);

                Layers = new ObservableCollection<Layer>(s.layers);

            }

        }
        #endregion

    }
}
