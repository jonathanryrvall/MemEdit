using MemEditGUI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemEditGUI.ViewModels
{
    /// <summary>
    /// View model for main window where the user selects what to edit
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        public DelegateCommand EditRamCommand { get; set; }
        public DelegateCommand EditFileCommand { get; set; }
        public DelegateCommand AboutCommand { get; set; }
        public Action CloseAction { get; set; }

        public MainViewModel()
        {
            EditRamCommand = new DelegateCommand(EditRamButton_Click);
            EditFileCommand = new DelegateCommand(EditFileButton_Click);
            AboutCommand = new DelegateCommand(AboutButton_Click);
        }

        /// <summary>
        /// Edit ram button click
        /// </summary>
        private void EditRamButton_Click()
        {
            new SelectProcessView().Show();
            CloseAction();
        }

        /// <summary>
        /// Edit file button click
        /// </summary>
        private void EditFileButton_Click()
        {
            new SelectFileView().Show();
            CloseAction();
        }

        /// <summary>
        /// About button click
        /// </summary>
        private void AboutButton_Click()
        {
            new AboutView().Show();
        }



    }
}
