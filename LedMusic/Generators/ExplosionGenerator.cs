﻿using LedMusic.Interfaces;
using LedMusic.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LedMusic.Generators
{
    class ExplosionGenerator : INotifyPropertyChanged, IAnimatable, IGenerator
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private ObservableCollection<AnimatedProperty> _animatedProperties = new ObservableCollection<AnimatedProperty>();
        public ObservableCollection<AnimatedProperty> AnimatedProperties
        {
            get { return _animatedProperties; }
            set
            {
                _animatedProperties = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<IController> _controllers = new ObservableCollection<IController>();
        public ObservableCollection<IController> Controllers
        {
            get { return _controllers; }
            set
            {
                _controllers = value;
                NotifyPropertyChanged();
            }
        }

        private List<Particle> particles = new List<Particle>();

        public ExplosionGenerator()
        {

        }

        public Color[] getSample(int frameNumber)
        {
            return new Color[GlobalProperties.Instance.LedCount];
        }

        private class Particle
        {
            double Velocity { get; set; }
            double Alpha { get; set; }
            double Hue { get; set; }
            double Saturation { get; set; } 
            double Value { get; set; }

            public Particle(double initialVelocity, double initialAlpha, double h, double s, double v)
            {
                Velocity = initialVelocity;
                Alpha = initialAlpha;
                Hue = h;
                Saturation = s;
                Value = v;
            }
        }

    }
}
