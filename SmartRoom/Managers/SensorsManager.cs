using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SmartRoom.Interfaces;
using SmartRoom.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace SmartRoom.Managers
{
    public class SensorsManager
    {
        private readonly IPackagesManager _pkgManager;
        private readonly SensorsViewModel _sensors;
        private Extensions.UniqueDictionary<Models.SensorModel, Timer> _timers;

        public bool IsPaused { get; private set; }

        public SensorsManager(IPackagesManager pkgManager, SensorsViewModel sensors)
        {
            _timers = new Extensions.UniqueDictionary<Models.SensorModel, Timer>();
            _pkgManager = pkgManager;
            _sensors = sensors;
            _sensors.Sensors.CollectionChanged += SensorsCollectionChanged;
            foreach (var s in _sensors.Sensors)
                s.PropertyChanged += SensorPropertyChanged;
        }

        private void SensorPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Refresh" || e.PropertyName == "Input" || e.PropertyName == "IsId")
                RefreshSensor(sender as Models.SensorModel);
        }

        private void SensorsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems != null)
                {
                    foreach (Models.SensorModel s in e.OldItems)
                    {
                        if (_timers.TryGetValue(s, out Timer t))
                        {
                            t.Elapsed -= TimerElapsed;
                            t.Stop();
                        }
                        _timers.RemoveKey(s);
                    }
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                if (e.NewItems != null)
                    foreach (Models.SensorModel s in e.NewItems)
                    {
                        s.PropertyChanged += SensorPropertyChanged;
                        RefreshSensor(s);
                    }
        }

        public void PauseAll()
        {
            foreach (var t in _timers.Values)
                t.Stop();

            IsPaused = true;
        }

        public void RestartAll()
        {
            IsPaused = false;
            foreach (var t in _timers.Values)
            {
                t.Stop();
                t.Start();
                TimerElapsed(t, null);
            }
        }

        public void RefreshSensor(Models.SensorModel model)
        {
            IsPaused = false;

            if(_timers.TryGetValue(model, out Timer t))
            {
                t.Stop();
                t.Interval = model.Refresh.TotalMilliseconds;
                t.Start();
                TimerElapsed(t, null);
            }
            else
            {
                var timer = new Timer(model.Refresh.TotalMilliseconds);
                timer.AutoReset = true;
                timer.Elapsed += TimerElapsed;
                _timers.Add(model, timer);
                timer.Start();
                TimerElapsed(timer, null);
            }
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_timers.TryGetKey(sender as Timer, out Models.SensorModel s))
                _pkgManager.GetData(s);
        }
    }
}