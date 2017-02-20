using LedMusic.Controller;
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
                //saveAnimatable(_currentAnimatable);
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

        //private BeatAnimatable beatAnimatable = new BeatAnimatable(); 

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
            SoundEngine.Instance.PropertyChanged += SoundEngine_PropertyChanged;
            GlobalProperties.Instance.PropertyChanged += GlobalProperties_PropertyChanged;

            CmdAnimate = new RelayCommand<string>(cmdAnimate_Execute, cmdAnimate_CanExecute);
            CmdRemoveAnimation = new RelayCommand<string>(
                cmdRemoveAnimation_Execute,
                (s) => isPropertyAnimated(s, CurrentAnimatable));
            CmdEditController = new RelayCommand<string>(
                cmdEditController_Execute,
                (s) => !isPropertyAnimated(s, CurrentAnimatable));
            CmdRemoveController = new RelayCommand<string>(
                cmdRemoveController_Execute,
                (s) => hasController(s));
            CmdOpenMusicFile = new RelayCommand(
                cmdOpenMusicFile_Execute);
            CmdPlay = new RelayCommand(
                () => SoundEngine.Instance.Play(),
                () => SoundEngine.Instance.CanPlay);
            CmdPause = new RelayCommand(
                () => SoundEngine.Instance.Pause(),
                () => SoundEngine.Instance.CanPause);
            CmdStop = new RelayCommand(
                () => SoundEngine.Instance.Stop(),
                () => SoundEngine.Instance.CanStop);
            CmdChooseGenerator = new RelayCommand(
                cmdChooseGenerator_Execute,
                () => CurrentLayer != null);
            CmdAddLayer = new RelayCommand(
                cmdAddLayer_Execute);
            CmdRemoveLayer = new RelayCommand(
                cmdRemoveLayer_Execute,
                () => CurrentLayer != null);
            CmdMoveLayerUp = new RelayCommand(
                cmdMoveLayerUp_Execute,
                () => (CurrentLayer != null) && (CurrentLayer.LayerNumber > 0));
            CmdMoveLayerDown = new RelayCommand(
                cmdMoveLayerDown_Execute,
                () => (CurrentLayer != null) && (CurrentLayer.LayerNumber < Layers.Count - 1));
            CmdRenameLayer = new RelayCommand(
                cmdRenameLayer_Execute,
                () => CurrentLayer != null);

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
            } else if (e.PropertyName == "CurrentFrame")
            {
                updateChannelPosition();
                updateBeatDisplay();
                updatePreviewStrip();
                SoundEngine.Instance.CalculateFft();
            }
        }

        private void SoundEngine_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Length")
                updateGrid();
            if (e.PropertyName == "Position")
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

            double trackDuration = SoundEngine.Instance.Length.TotalSeconds;
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

            double trackDuration = SoundEngine.Instance.Length.TotalSeconds;
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
            double time = SoundEngine.Instance.Position.TotalSeconds;
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
                if (t.BaseType == typeof(ControllerBase))
                    controllerNames.Add(t.Name);
            }
            AvailableControllers = controllerNames;
        }

        public void ChangeAnimatableParameter(double value, string propertyName)
        {

            PropertyModel pm = CurrentAnimatable.AnimatableProperties.GetProperty(propertyName);

            if (isPropertyAnimated(propertyName, CurrentAnimatable))
            {
                Keyframe kf = pm.Keyframes.FirstOrDefault(k => k.Frame == CurrentFrame);
                if (kf != null)
                {
                    kf.Value = value;
                }
                else
                {
                    pm.Keyframes.Add(new Keyframe(CurrentFrame, value));
                }
            }
            else
            {
                pm.PercentageValue = value;
            }

        }

        private bool isPropertyAnimated(PropertyModel pm)
        {
            if (pm == null)
                return false;
            else
                return pm.Keyframes.Count > 0;
        }
        private bool isPropertyAnimated(string propertyName, IAnimatable a)
        {
            if (propertyName == null || a == null)
                return false;

            return isPropertyAnimated(a.AnimatableProperties.GetProperty(propertyName));
        }

        private bool hasController(string propertyName)
        {
            if (CurrentAnimatable == null || propertyName == null)
                return false;

            PropertyModel pm = CurrentAnimatable.AnimatableProperties.GetProperty(propertyName);

            if (pm == null)
                return false;
            else
                return pm.Controller != null;
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

            int ledCount = GlobalProperties.Instance.LedCount;
            ColorRGB[] final = new ColorRGB[ledCount];

            for (int i = 0; i < ledCount; i++)
            {
                final[i] = new ColorRGB(0, 0, 0);
            }

            var taskList = new List<Task<SampleTaskReturn>>();
            

            foreach (Layer l in Layers)
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

            for (int l = Layers.Count - 1; l >= 0; l--)
            {
                for (int j = 0; j < ledCount; j++)
                {
                    ColorHSV chsv = returns[l].Leds[j].getColorHSV();
                    chsv.V = chsv.V * Layers[l].Alpha;
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
            applyKeyframes(a, frame);
            applyControllers(a, frame);

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

        private void applyKeyframes(IAnimatable a, int frame)
        {
            foreach (PropertyModel pm in a.AnimatableProperties)
            {

                if (pm.Controller != null)
                {
                    IAnimatable c = pm.Controller;
                    applyKeyframes(c, frame);
                    pm.Controller = (ControllerBase)c;
                }

                if (pm.Keyframes != null && pm.Keyframes.Count > 0)
                {

                    Keyframe k = pm.Keyframes.FirstOrDefault(kf => kf.Frame == frame);

                    double value = 0;

                    if (k == null)
                    {
                        Keyframe previousKeyframe = pm.Keyframes.LastOrDefault(kf => kf.Frame < frame);
                        Keyframe nextKeyframe = pm.Keyframes.FirstOrDefault(kf => kf.Frame > frame);

                        if (previousKeyframe == null)
                        {
                            value = nextKeyframe.Value;
                        }
                        else if (nextKeyframe == null)
                        {
                            value = previousKeyframe.Value;
                        }
                        else
                        {
                            switch (previousKeyframe.Mode)
                            {
                                case KeyframeMode.LINEAR:
                                    double m = (nextKeyframe.Value - previousKeyframe.Value) / (nextKeyframe.Frame - previousKeyframe.Frame);
                                    value = previousKeyframe.Value + (frame - previousKeyframe.Frame) * m;
                                    break;
                                case KeyframeMode.HOLD:
                                    value = previousKeyframe.Value;
                                    break;
                            }
                        }
                    }
                    else
                    {
                        value = k.Value;
                    }

                    pm.RenderValuePercentage = value;

                }
            }
        }

        private void applyControllers(IAnimatable a, int frame)
        {
            foreach (PropertyModel pm in a.AnimatableProperties)
            {
                if (pm.Controller != null)
                {
                    applyControllers(pm.Controller, frame);
                    pm.RenderValuePercentage = pm.Controller.GetValueAt(frame);
                }
            }
        }
        #endregion

        #region Commands
        private void cmdAnimate_Execute(string propertyName)
        {
            PropertyModel pm = CurrentAnimatable.AnimatableProperties.GetProperty(propertyName);
            pm.Keyframes.Add(new Keyframe(CurrentFrame, pm.PercentageValue));
        }

        private bool cmdAnimate_CanExecute(string propertyName)
        {
            if (propertyName == null || CurrentAnimatable == null)
                return false;

            PropertyModel pm = CurrentAnimatable.AnimatableProperties.GetProperty(propertyName);
            return pm != null && !isPropertyAnimated(pm) && pm.Controller == null;
        }

        private void cmdRemoveAnimation_Execute(string propertyName)
        {
            CurrentAnimatable.AnimatableProperties.GetProperty(propertyName).Keyframes.Clear();
        }

        private async void cmdOpenMusicFile_Execute()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.CheckFileExists = true;
            ofd.Filter = "Audio files (*.wav;*.mp3)|*.wav;*mp3";
            if (ofd.ShowDialog() == true)
            {
                await SoundEngine.Instance.OpenFile(ofd.FileName);
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

            PropertyModel pm = CurrentAnimatable.AnimatableProperties.GetProperty(propertyName);

            if (pm.Controller == null)
            {
                ChooseControllerWindow ccw = new ChooseControllerWindow();
                ccw.DataContext = this;
                if (ccw.ShowDialog() == false)
                    return;

                ControllerBase c = (ControllerBase)Activator.CreateInstance(
                        Type.GetType("LedMusic.Controller." + ccw.lbControllers.SelectedItem));
                pm.Controller = c;

                CurrentAnimatable = c;
            } else
            {
                CurrentAnimatable = pm.Controller;
            }

        }

        private void cmdRemoveController_Execute(string propertyName)
        {
            CurrentAnimatable.AnimatableProperties.GetProperty(propertyName).Controller = null;
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

        public async void Open()
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

                await SoundEngine.Instance.OpenFile(s.musicFile);

                Layers = new ObservableCollection<Layer>(s.layers);

            }

        }
        #endregion

        #region Keyframe Manipulation
        public void MoveKeyframes(int deltaFrames)
        {

            if (CurrentAnimatable == null)
                return;

            foreach (PropertyModel pm in CurrentAnimatable.AnimatableProperties)
            {
                foreach (Keyframe k in pm.Keyframes)
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

            foreach (PropertyModel ap in CurrentAnimatable.AnimatableProperties)
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

            foreach (PropertyModel ap in CurrentAnimatable.AnimatableProperties)
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
            foreach (PropertyModel pm in CurrentAnimatable.AnimatableProperties)
            {
                List<Keyframe> toRemove = pm.Keyframes.Where((k) => { return k.IsSelected; }).ToList();
                foreach (Keyframe k in toRemove)
                {
                    pm.Keyframes.Remove(k);
                }
            }
        }

        public void CopyKeyframes()
        {

            clipboard.Clear();
            copyFrame = CurrentFrame;
            
            foreach (PropertyModel pm in CurrentAnimatable.AnimatableProperties)
            {
                List<Keyframe> tempList = new List<Keyframe>();
                foreach (Keyframe k in pm.Keyframes)
                {
                    if (k.IsSelected)
                        tempList.Add(k.Copy());
                }
                if (tempList.Count > 0)
                    clipboard.Add(pm.Name, tempList);
            }

        }

        public void PasteKeyframes()
        {

            int delta = CurrentFrame - copyFrame;

            foreach (PropertyModel pm in CurrentAnimatable.AnimatableProperties)
            {
                if (clipboard.ContainsKey(pm.Name))
                {
                    foreach (Keyframe k in clipboard[pm.Name])
                    {
                        Keyframe kf = pm.Keyframes.FirstOrDefault(lkf => lkf.Frame == k.Frame);
                        k.Frame += delta;
                        if (kf != null)
                        {
                            kf.Value = k.Value;
                        } else
                        {
                            pm.Keyframes.Add(k.Copy());
                        }
                    }
                }
            }

        }

        public bool MultipleKeyframesSelected()
        {

            if (CurrentAnimatable == null)
                return false;

            foreach (PropertyModel pm in CurrentAnimatable.AnimatableProperties)
            {
                bool hasSelectedKeyframe = false;
                foreach (Keyframe k in pm.Keyframes)
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
