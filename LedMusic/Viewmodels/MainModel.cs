using LedMusic.Interfaces;
using LedMusic.Models;
using LedMusic.StaticStuff;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
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

        private IAnimatable _currentAnimatable;
        public IAnimatable CurrentAnimatable
        {
            get { return _currentAnimatable; }
            set
            {
                saveAnimatable(_currentAnimatable);
                _currentAnimatable = value;
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

        private double _scrollViewerHorizontalOffset;
        public double ScrollViewerHorizontalOffset
        {
            get { return _scrollViewerHorizontalOffset; }
            set
            {
                _scrollViewerHorizontalOffset = value;
                NotifyPropertyChanged();
            }
        }

        private Brush _beatIndicatorBackground = Brushes.Black;
        public Brush BeatIndicatorBackground
        {
            get { return _beatIndicatorBackground; }
            set
            {
                _beatIndicatorBackground = value;
                NotifyPropertyChanged();
            }
        }

        public List<string> AvailableGenerators { get; private set; }
        public List<string> AvailableControllers { get; private set; }

        private int copyFrame = 0;
        private Dictionary<string, List<Keyframe>> clipboard = new Dictionary<string, List<Keyframe>>();

        SerialPort sp;
        private const bool outputToSerial = false;
        private volatile bool isOutputtingToSerial = false;

        private BeatAnimatable beatAnimatable = new BeatAnimatable(); 

        public RelayCommand<string> CmdAnimate { get; private set; }
        public RelayCommand<string> CmdRemoveAnimation { get; private set; }
        public RelayCommand<string> CmdEditController { get; private set; }
        public RelayCommand<string> CmdRemoveController { get; private set; }
        public RelayCommand CmdOpenMusicFile { get; private set; }
        public RelayCommand CmdPlay { get; private set; }
        public RelayCommand CmdPause { get; private set; }
        public RelayCommand CmdStop { get; private set; }
        public RelayCommand CmdChooseGenerator { get; private set; }
        public RelayCommand CmdMoveLayerUp { get; private set; }
        public RelayCommand CmdMoveLayerDown { get; private set; }
        public RelayCommand CmdAddLayer { get; private set; }
        public RelayCommand CmdRemoveLayer { get; private set; }
        public RelayCommand CmdRenameLayer { get; private set; }
        #endregion

        #region Constructor
        private static MainModel instance;
        private MainModel() {
            _layers = new ObservableCollection<Layer>();
            PropertyChanged += this_PropertyChanged;
            BassEngine.Instance.PropertyChanged += BassEngine_PropertyChanged;
            GlobalProperties.Instance.PropertyChanged += GlobalProperties_PropertyChanged;

            CmdAnimate = new RelayCommand<string>(cmdAnimate_Execute, cmdAnimate_CanExecute);
            CmdRemoveAnimation = new RelayCommand<string>(cmdRemoveAnimation_Execute,
                (s) => { return isPropertyAnimated(s, CurrentAnimatable.AnimatedProperties); });
            CmdEditController = new RelayCommand<string>(cmdEditController_Execute,
                (s) => { return !isPropertyAnimated(s, CurrentAnimatable.AnimatedProperties); });
            CmdRemoveController = new RelayCommand<string>(cmdRemoveController_Execute,
                (s) => { return hasController(s); });
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
            CmdAddLayer = new RelayCommand(cmdAddLayer_Execute);
            CmdRemoveLayer = new RelayCommand(cmdRemoveLayer_Execute, () => { return CurrentLayer != null; });
            CmdMoveLayerUp = new RelayCommand(cmdMoveLayerUp_Execute,
                () => { return (CurrentLayer != null) && (CurrentLayer.LayerNumber > 0); });
            CmdMoveLayerDown = new RelayCommand(cmdMoveLayerDown_Execute,
                () => { return (CurrentLayer != null) && (CurrentLayer.LayerNumber < Layers.Count - 1); });
            CmdRenameLayer = new RelayCommand(cmdRenameLayer_Execute, () => { return CurrentLayer != null; });

            fillGeneratorsList();
            fillControllersList();
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
            } else if (e.PropertyName == "CurrentAnimatable")
            {
                PropertiesHelper.updateAnimatableProperties(ref _currentAnimatable);
            } else if (e.PropertyName == "CurrentFrame")
            {
                updateChannelPosition();
                updateBeatDisplay();
                updatePreviewStrip();
            }
        }

        private void BassEngine_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ChannelLength")
                updateGrid();
            if (e.PropertyName == "ChannelPosition")
                calculateCurrentFrame();
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
            PlayerPosition = ((double)CurrentFrame / GlobalProperties.Instance.FPS) * secondWidth;

        }

        private void updateBeatDisplay()
        {
            double t = (double)CurrentFrame / GlobalProperties.Instance.FPS;
            byte v = Convert.ToByte(255 * (1 - BeatHelper.getBeatNumberByTime(t) + Math.Floor(BeatHelper.getBeatNumberByTime(t))));
            BeatIndicatorBackground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, v, 0));
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

        private void fillControllersList()
        {
            List<string> controllerNames = new List<string>();
            Type[] gens = Assembly.GetCallingAssembly().GetTypes().Where(t => string.Equals(t.Namespace, "LedMusic.Controller", StringComparison.Ordinal)).ToArray();
            foreach (Type t in gens)
            {
                if (t.GetInterfaces().Contains(typeof(IController)))
                    controllerNames.Add(t.Name);
            }
            AvailableControllers = controllerNames;
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

        private void saveAnimatable(IAnimatable a)
        {

            if (a == null)
                return;

            if (a.Id == CurrentAnimatable.Id)
            {
                a = CurrentAnimatable;
            } else
            {
                foreach (IController c in a.Controllers)
                {
                    if (c is IAnimatable)
                    {
                        saveAnimatable((IAnimatable)c);
                    }
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

            if (outputToSerial && (sp == null || sp.IsOpen == false))
            {
                sp = new SerialPort("COM3", 115200);
                sp.Open();
            }

            GradientStopCollection gsc = new GradientStopCollection();
            int ledCount = GlobalProperties.Instance.LedCount;
            ColorRGB[] ledColors = getColorsByFrame(CurrentFrame);

            byte[] data = new byte[3 * ledCount];
            for (int i = 0; i < ledCount; i++)
            {
                data[3 * i] = ledColors[i].R;
                data[3 * i + 1] = ledColors[i].G;
                data[3 * i + 2] = ledColors[i].B;
            }

            if (outputToSerial && sp.IsOpen)
                Task.Run(() =>
                {
                    while (isOutputtingToSerial)
                        Thread.Sleep(1);
                    isOutputtingToSerial = true;
                    sp.Write(data, 0, 3 * ledCount);
                    isOutputtingToSerial = false;
                });

            for (int i = 0; i < ledCount; i++)
            {
                gsc.Add(new GradientStop(System.Windows.Media.Color.FromRgb(ledColors[i].R, ledColors[i].G, ledColors[i].B),
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
                final[i] = new ColorRGB(0, 0, 0);
            }

            var taskList = new List<Task<SampleTaskReturn>>();
            

            foreach (Layer l in localLayers)
            {
                taskList.Add(Task.Factory.StartNew(() => getLayerSample(l, frame)));
            }

            Task.WaitAll(taskList.ToArray());

            List<SampleTaskReturn> returns = new List<SampleTaskReturn>();
            foreach (Task<SampleTaskReturn> t in taskList)
            {
                returns.Add(t.Result);
            }
            returns.Sort();

            for (int l = localLayers.Count - 1; l >= 0; l--)
            {
                for (int j = 0; j < ledCount; j++)
                {
                    ColorHSV chsv = returns[l].Leds[j].getColorHSV();
                    chsv.V = chsv.V * localLayers[l].Alpha;
                    final[j] = chsv.getColorRGB().Overlay(final[j]);
                }
            }

            return final;

        }

        private SampleTaskReturn getLayerSample(Layer l, int frame)
        {

            int ledCount = GlobalProperties.Instance.LedCount;
            IGenerator g = l.Generator;

            //Set properties according to the keyframes
            IAnimatable a = (IAnimatable)g;
            setProperties(ref a, frame);

            Models.Color[] thisLayerColor = g.getSample(frame);

            for (int j = 0; j < ledCount; j++)
            {
                if (thisLayerColor[j] == null)
                {
                    Type t = thisLayerColor.GetType().GetElementType();
                    if (t == typeof(ColorRGB))
                    {
                        thisLayerColor[j] = new ColorRGB(0, 0, 0);
                    }
                    else
                    {
                        thisLayerColor[j] = new ColorHSV(0, 0, 0);
                    }
                }
            }

            return new SampleTaskReturn(l.LayerNumber, thisLayerColor);
        }

        private void setProperties(ref IAnimatable a, int frame)
        {
            applyKeyframes(ref a, frame);

            foreach (IController con in a.Controllers)
            {
                IAnimatable aCon = (IAnimatable)con;
                if (aCon != null)
                {
                    setProperties(ref aCon, frame);
                    applyKeyframes(ref aCon, frame);
                }
                setPropertyValue(ref a, con.PropertyName, con.getValueAt(frame));
            }
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
                        switch (pK.Mode)
                        {
                            case KeyframeMode.LINEAR:
                                double m = (nK.Value - pK.Value) / (nK.Frame - pK.Frame);
                                value = pK.Value + (frame - pK.Frame) * m;
                                break;
                            case KeyframeMode.HOLD:
                                value = pK.Value;
                                break;
                        }
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

            PropertyInfo pi = CurrentAnimatable.GetType().GetProperty(propertyName);
            if (pi == null)
                return;

            double value = Convert.ToDouble(pi.GetValue(CurrentAnimatable));

            bool log = false;
            double minValue = 0;
            double maxValue = 0;

            foreach (PropertyModel pm in CurrentAnimatable.AnimatableProperties)
            {
                if (pm.Name == propertyName)
                {
                    log = pm.Logarithmic;
                    minValue = pm.MinValue;
                    maxValue = pm.MaxValue;
                }
            }

            CurrentAnimatable.AnimatedProperties.Add(new AnimatedProperty(propertyName, new Keyframe(CurrentFrame, value), minValue, maxValue, log));

        }

        private bool cmdAnimate_CanExecute(string propertyName)
        {
            return !isPropertyAnimated(propertyName, CurrentAnimatable.AnimatedProperties) && !hasController(propertyName);
        }

        private void cmdRemoveAnimation_Execute(string propertyName)
        {
            AnimatedProperty toRemove = null;
            foreach (AnimatedProperty ap in CurrentAnimatable.AnimatedProperties)
            {
                if (ap.PropertyName == propertyName)
                {
                    toRemove = ap;
                    break;
                }
            }
            if (toRemove != null)
                CurrentAnimatable.AnimatedProperties.Remove(toRemove);
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

        private void cmdEditController_Execute(string propertyName)
        {

            if (!hasController(propertyName))
            {
                ChooseControllerWindow ccw = new ChooseControllerWindow();
                ccw.DataContext = this;
                if (ccw.ShowDialog() == false)
                    return;

                IController c = (IController)Activator.CreateInstance(
                        Type.GetType("LedMusic.Controller." + ccw.lbControllers.SelectedItem));
                PropertyModel p = getPropertyByName(CurrentAnimatable.AnimatableProperties, propertyName);
                c.initialize(p.Name, p.MinValue, p.MaxValue);

                CurrentAnimatable.Controllers.Add(c);
                CurrentAnimatable = (IAnimatable)c;
            } else
            {
                IController c;
                getControllerReference(propertyName, out c);
                CurrentAnimatable = (IAnimatable)c;
            }

        }

        private void cmdRemoveController_Execute(string propertyName)
        {
            IController c = null;
            getControllerReference(propertyName, out c);
            if (c != null)
                CurrentAnimatable.Controllers.Remove(c);
        }

        private PropertyModel getPropertyByName(IEnumerable<PropertyModel> properties, string propertyName)
        {
            foreach (PropertyModel pm in properties)
            {
                if (pm.Name == propertyName)
                    return pm;
            }
            return null;
        }

        private bool hasController(string propertyName)
        {

            IController c = null;
            getControllerReference(propertyName, out c);
            return c != null;

        }

        private void getControllerReference(string propertyName, out IController controller)
        {

            foreach (IController c in CurrentAnimatable.Controllers)
            {
                if (c.PropertyName == propertyName)
                {
                    controller = c;
                    return;
                }
            }

            controller = null;

        }

        private void cmdAddLayer_Execute()
        {
            ChooseGeneratorWindow cgw = new ChooseGeneratorWindow();
            cgw.DataContext = this;
            if (cgw.ShowDialog() == false)
                return;
            string generatorName = (string)cgw.lbGenerators.SelectedItem;

            if (generatorName == null || generatorName == "")
                return;

            Layers.Add(new Layer(Layers.Count, (IGenerator)Activator.CreateInstance(
                Type.GetType("LedMusic.Generators." + generatorName))));

        }

        private void cmdRemoveLayer_Execute()
        {
            int layerNumber = CurrentLayer.LayerNumber;
            Layers.Remove(CurrentLayer);

            foreach (Layer l in Layers)
            {
                if (l.LayerNumber > layerNumber)
                    l.LayerNumber -= 1;
            }
            Layers.Sort();
        }

        private void cmdMoveLayerUp_Execute()
        {
            int layerNumber = CurrentLayer.LayerNumber;

            if (layerNumber == 0)
                return;

            foreach (Layer l in Layers)
            {
                if (l.LayerNumber == layerNumber - 1)
                    l.LayerNumber = layerNumber;
            }
            Layers.ElementAt(CurrentLayer.LayerNumber).LayerNumber -= 1;
            Layers.Sort();
        }

        private void cmdMoveLayerDown_Execute()
        {
            int layerNumber = CurrentLayer.LayerNumber;

            if (layerNumber == Layers.Count - 1)
                return;

            foreach (Layer l in Layers)
            {
                if (l.LayerNumber == layerNumber + 1)
                    l.LayerNumber = layerNumber;
            }
            Layers.ElementAt(CurrentLayer.LayerNumber).LayerNumber += 1;
            Layers.Sort();
        }

        private void cmdRenameLayer_Execute()
        {
            RenameDialog rd = new RenameDialog();
            rd.textBox.Text = CurrentLayer.LayerName;
            if (rd.ShowDialog() == false)
                return;
            Layers.ElementAt(CurrentLayer.LayerNumber).LayerName = rd.textBox.Text;
        }
        #endregion

        #region Saving/Loading stuff
        public void Save()
        {

            if (GlobalProperties.Instance.CurrentProjectFile == null ||
                GlobalProperties.Instance.CurrentProjectFile == "")
            {
                SaveAs();
                return;
            }

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

        #region Keyframe Manipulation
        public void MoveKeyframes(int deltaFrames)
        {

            if (CurrentAnimatable == null)
                return;

            foreach (AnimatedProperty ap in CurrentAnimatable.AnimatedProperties)
            {
                foreach (Keyframe k in ap.Keyframes)
                {
                    if (k.IsSelected)
                    {
                        k.Frame += deltaFrames;
                        k.JustDragged = true;
                    }
                }
            }
        }

        public void UnselectAllKeyframes()
        {

            if (CurrentAnimatable == null)
                return;

            foreach (AnimatedProperty ap in CurrentAnimatable.AnimatedProperties)
            {
                foreach (Keyframe k in ap.Keyframes)
                {
                    if (k.IsSelected)
                    {
                        if (k.JustDragged)
                        {
                            k.JustDragged = false;
                        } else
                        {
                            k.IsSelected = false;
                        }
                    }
                }
            }
        }

        public void SelectKeyframeRange()
        {

            foreach (AnimatedProperty ap in CurrentAnimatable.AnimatedProperties)
            {
                //find first and last Keyframe and select all Keyframes in between them
                ap.Keyframes.Sort();
                bool select = false;

                foreach (Keyframe k in ap.Keyframes)
                {

                    if (!select && k.IsSelected)
                        select = true;
                    else if (select && !k.IsSelected)
                        k.IsSelected = true;
                    else if (select && k.IsSelected)
                        break;

                }
            }

        }

        public void DeleteKeyframes()
        {

            List<AnimatedProperty> toRemoveAP = new List<AnimatedProperty>();

            foreach (AnimatedProperty ap in CurrentAnimatable.AnimatedProperties)
            {
                List<Keyframe> toRemove = ap.Keyframes.Where((k) => { return k.IsSelected; }).ToList();
                foreach (Keyframe k in toRemove)
                {
                    ap.Keyframes.Remove(k);
                }
                if (ap.Keyframes.Count == 0)
                    toRemoveAP.Add(ap);
            }

            foreach (AnimatedProperty ap in toRemoveAP)
            {
                CurrentAnimatable.AnimatedProperties.Remove(ap);
            }

        }

        public void CopyKeyframes()
        {

            clipboard.Clear();
            copyFrame = CurrentFrame;
            
            foreach (AnimatedProperty ap in CurrentAnimatable.AnimatedProperties)
            {
                List<Keyframe> tempList = new List<Keyframe>();
                foreach (Keyframe k in ap.Keyframes)
                {
                    if (k.IsSelected)
                        tempList.Add(k.Copy());
                }
                if (tempList.Count > 0)
                    clipboard.Add(ap.PropertyName, tempList);
            }

        }

        public void PasteKeyframes()
        {

            int delta = CurrentFrame - copyFrame;

            foreach (AnimatedProperty ap in CurrentAnimatable.AnimatedProperties)
            {
                if (clipboard.ContainsKey(ap.PropertyName))
                {
                    foreach (Keyframe k in clipboard[ap.PropertyName])
                    {
                        Keyframe kf = null;
                        k.Frame += delta;
                        if (containsKeyframe(k.Frame, ap.Keyframes, out kf))
                        {
                            kf.Value = k.Value;
                        } else
                        {
                            ap.Keyframes.Add(k.Copy());
                        }
                    }
                }
            }

        }

        public bool MultipleKeyframesSelected()
        {

            if (CurrentAnimatable == null)
                return false;

            foreach (AnimatedProperty ap in CurrentAnimatable.AnimatedProperties)
            {
                bool hasSelectedKeyframe = false;
                foreach (Keyframe k in ap.Keyframes)
                {
                    if (k.IsSelected)
                    {
                        if (hasSelectedKeyframe)
                        {
                            return true;
                        } else
                        {
                            hasSelectedKeyframe = true;
                        }
                    }
                }
            }

            return false;

        }
        #endregion

        private class SampleTaskReturn : IComparable<SampleTaskReturn>
        {

            public int LayerNumber { get; set; }
            public Models.Color[] Leds { get; set; }

            public SampleTaskReturn(int layerNumber, Models.Color[] leds)
            {
                LayerNumber = layerNumber;
                Leds = leds;
            }

            public int CompareTo(SampleTaskReturn other)
            {
                return LayerNumber - other.LayerNumber;
            }

        }

    }
}
