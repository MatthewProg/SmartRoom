using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartRoom.Managers
{
    public class MacrosManager
    {
        private readonly Interfaces.IPackagesManager _packagesManager;
        private Dictionary<Models.MacroModel, Tuple<Task, CancellationTokenSource>> _tasks;

        public ViewModels.MacrosViewModel MacrosViewModel { get; private set; }

        public MacrosManager(Interfaces.IPackagesManager packagesManager, ViewModels.MacrosViewModel macros)
        {
            _packagesManager = packagesManager;
            _tasks = new Dictionary<Models.MacroModel, Tuple<Task, CancellationTokenSource>>();
            MacrosViewModel = macros;
        }

        public void StartMacro(Models.MacroModel macro)
        {
            if (macro.Enabled == false)
                return;

            macro.Running = true;
            if (_tasks.ContainsKey(macro) == true)
            {
                if (_tasks[macro]?.Item1.IsCompleted == true)
                {
                    _tasks[macro].Item1.Dispose();
                    _tasks[macro].Item2.Dispose();
                }
                else
                    return;
            }

            var cts = new CancellationTokenSource();
            var task = Task.Run(() => RunMacro(macro, cts.Token), cts.Token);
            _tasks[macro] = new Tuple<Task, CancellationTokenSource>(task, cts);
        }

        public void PauseMacro(Models.MacroModel macro) => macro.Running = false;

        public void StopMacro(Models.MacroModel macro)
        {
            if (_tasks.ContainsKey(macro) == false) return;

            if (_tasks[macro]?.Item1.IsCompleted == false)
            {
                _tasks[macro].Item2.Cancel();
                macro.Running = false;
                _tasks[macro].Item1.Wait();
            }

            _tasks[macro].Item1.Dispose();
            _tasks[macro].Item2.Dispose();
            _tasks.Remove(macro);
        }

        public bool IsMacroRunning(Models.MacroModel macro) => (macro.Running == true &&
                                                                _tasks.ContainsKey(macro) == true &&
                                                                _tasks[macro]?.Item1.IsCompleted == false);

        public bool IsMacroPaused(Models.MacroModel macro) => (macro.Running == false &&
                                                                _tasks.ContainsKey(macro) == true &&
                                                                _tasks[macro]?.Item1.IsCompleted == false);

        public bool IsMacroStopped(Models.MacroModel macro) => (_tasks.ContainsKey(macro) == false ||
                                                                _tasks[macro]?.Item1.IsCompleted == true);

        private void RunMacro(Models.MacroModel macro, CancellationToken token)
        {
            var pause = new CancellationTokenSource();
            void PropChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                if (e.PropertyName == "Running")
                {
                    var obj = sender as Models.MacroModel;
                    if (obj.Running == true || token.IsCancellationRequested)
                        pause.Cancel();
                }
            }

            macro.PropertyChanged += PropChanged;
            do
            {
                if (macro.Items.Count == 0)
                {
                    Delay(-1, pause.Token);
                    pause = new CancellationTokenSource();
                }

                foreach (var item in macro.Items)
                {
                    if (item.Enabled == false)
                        continue;

                    if (item is Models.SwitchModel)
                        _packagesManager.SetValue(item as Models.SwitchModel);
                    else if(item is Models.DelayMacroItemModel)
                        Delay((item as Models.DelayMacroItemModel).Delay, pause.Token);

                    if (token.IsCancellationRequested)
                    {
                        macro.Running = false;
                        return;
                    }

                    if (macro.Running == false)
                    {
                        Delay(-1, pause.Token);
                        pause = new CancellationTokenSource();
                    }
                }
            } while (macro.Repeat);
            macro.Running = false;
        }
        private void Delay(int mili, CancellationToken token)
        {
            try
            {
                Task.Delay(mili, token).Wait(token);
            }
            catch (System.OperationCanceledException) when (token.IsCancellationRequested)
            {
                return;
            }
        }
    }
}