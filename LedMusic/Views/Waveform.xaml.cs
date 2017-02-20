using LedMusic.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LedMusic.Views
{
    /// <summary>
    /// Interaction logic for Waveform.xaml
    /// </summary>
    public partial class Waveform : UserControl, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public WaveformModel WaveformModel
        {
            get { return (WaveformModel)GetValue(WaveformModelProperty); }
            set { SetValue(WaveformModelProperty, value); }
        }
        public static readonly DependencyProperty WaveformModelProperty = DependencyProperty.Register(
                "WaveformModel", typeof(WaveformModel), typeof(Waveform));

        public Waveform()
        {
            InitializeComponent();

            var desc = DependencyPropertyDescriptor.FromProperty(WaveformModelProperty, typeof(Waveform));
            if (desc != null)
                desc.AddValueChanged(this, (s, e) => {
                    if (WaveformModel != null)
                        WaveformModel.PropertyChanged += WaveformModel_PropertyChanged;
                    });
        }

        private async void WaveformModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Samples")
                await updateWaveformAsync();
        }

        public async Task updateWaveformAsync()
        {

            Debug.WriteLine("Redrawing waveform");

            if (!Dispatcher.CheckAccess())
            {
                await Dispatcher.InvokeAsync(async () => await updateWaveformAsync());
                return;
            }

            List<Point> points = new List<Point>();
            double centerHeight = PART_Canvas.RenderSize.Height / 2d;
            double canvasWidth = PART_Canvas.RenderSize.Width;
            int sampleCount = WaveformModel.Samples.Length;
            float value = 0;

            if (canvasWidth == 0 || sampleCount == 0)
                return;

            points.Add(new Point(0, centerHeight));
            points.Add(new Point(0, centerHeight));

            for (int x = 0; x < canvasWidth; x++)
            {
                value = WaveformModel.Samples[(int)Math.Floor(x * sampleCount / canvasWidth)];
                points.Add(new Point(x, centerHeight + value * centerHeight));
            }

            for (int x = (int)canvasWidth - 1; x >= 0; x--)
            {
                value = WaveformModel.Samples[(int)Math.Floor(x * sampleCount / canvasWidth)];
                points.Add(new Point(x, centerHeight - value * centerHeight));
            }

            PolyLineSegment lineSegment = new PolyLineSegment(points, false);

            PathFigure figure = new PathFigure();
            figure.Segments.Add(lineSegment);

            PathGeometry geometry = new PathGeometry();
            geometry.Figures.Add(figure);

            PART_Path.Data = geometry;
            PART_Canvas.CacheMode = new BitmapCache(1.0);
        }

        protected override async void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            var cache = PART_Canvas.CacheMode as BitmapCache;
            if (cache != null)
            {
                cache.RenderAtScale = 1.0;
                await updateWaveformAsync();
            }
        }

    }

}