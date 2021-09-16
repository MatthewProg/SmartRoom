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
        private readonly PackagesManager _packagesManager;
        private Dictionary<Models.MacroModel, Tuple<Task, CancellationTokenSource>> _tasks;

        public ObservableCollection<Models.MacroModel> Macros { get; private set; }

        public MacrosManager(PackagesManager packagesManager)
        {
            _packagesManager = packagesManager;
            _tasks = new Dictionary<Models.MacroModel, Tuple<Task, CancellationTokenSource>>();
            Macros = new ObservableCollection<Models.MacroModel>();
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
            var task = Task.Run(async () => await RunMacro(macro, cts.Token), cts.Token);
            _tasks[macro] = new Tuple<Task, CancellationTokenSource>(task, cts);
        }

        public void PauseMacro(Models.MacroModel macro) => macro.Running = false;

        public void StopMacro(Models.MacroModel macro)
        {
            macro.Running = false;
            if (_tasks.ContainsKey(macro) == false) return;

            if (_tasks[macro]?.Item1.IsCompleted == false)
            {
                _tasks[macro].Item2.Cancel();
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

        private async Task RunMacro(Models.MacroModel macro, CancellationToken token)
        {
            var pause = new CancellationTokenSource();
            void PropChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                if (e.PropertyName == "Running")
                {
                    var obj = sender as Models.MacroModel;
                    if (obj.Running == false) pause = new CancellationTokenSource();
                    else pause.Cancel();
                }
            }

            macro.PropertyChanged += PropChanged;
            do
            {
                if (macro.Items.Count == 0)
                    await Delay(-1, pause.Token);

                foreach (var item in macro.Items)
                {
                    if (item.Enabled == false)
                        continue;

                    if (item is Models.SwitchModel)
                        _packagesManager.QueueSetValues(item as Models.SwitchModel);
                    else if(item is Models.DelayMacroItemModel)
                        await Delay((item as Models.DelayMacroItemModel).Delay, pause.Token);

                    if (token.IsCancellationRequested)
                        return;

                    if (macro.Running == false)
                        await Delay(-1, pause.Token);
                }
            } while (macro.Repeat);
        }
        private async Task Delay(int mili, CancellationToken token)
        {
            try
            {
                await Task.Delay(mili, token);
            }
            catch (System.OperationCanceledException) when (token.IsCancellationRequested)
            {
                return;
            }
        }
    }
}