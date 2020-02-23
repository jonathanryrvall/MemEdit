using MemEditGUI.Models;
using MemEditGUI.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemEditGUI.ViewModels
{
    public class SelectProcessViewModel : ViewModelBase
    {
        public DelegateCommand UpdateCommand { get; set; }
        public RelayCommand SelectProcessCommand { get; set; }
        public Action CloseAction { get; set; }


        private ObservableCollection<MemorySource> processes;

        public ObservableCollection<MemorySource> Processes
        {
            get { return processes; }
            set { SetProperty(ref processes, value); }
        }
       

        public SelectProcessViewModel()
        {
            UpdateCommand = new DelegateCommand(UpdateButton_Click);
            SelectProcessCommand = new RelayCommand(SelectProcessButton_Click);

            Task.Run(() =>
            {
                UpdateList();
            });
        }

        /// <summary>
        /// Force update list of processes
        /// </summary>
        private void UpdateButton_Click()
        {
            Task.Run(() =>
            {
                UpdateList();
            });
        }

        /// <summary>
        /// Select a process to edit memory of
        /// </summary>
        private void SelectProcessButton_Click(object selectedProcess)
        {
            GlobalStates.Instance.SelectMemorySource((MemorySource)selectedProcess);
            new EditMemoryView().Show();
            CloseAction();
        }

        /// <summary>
        /// Update list of processes
        /// </summary>
        private void UpdateList()
        {
            Process[] allProcesses = Process.GetProcesses();
            Processes = new ObservableCollection<MemorySource>();
            foreach (Process process in allProcesses)
            {
                if (process.ProcessName == "svchost")
                {
                    continue;
                }

                MemorySource memorySource = new MemorySource(process);
                
                App.Current.Dispatcher.BeginInvoke(new Action(delegate ()
                {
                   

                    Processes.Add(memorySource);
                }));
                
            }
        }
     

    }
}