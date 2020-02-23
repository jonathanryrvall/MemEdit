using MemEditGUI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemEditGUI.ViewModels
{
    public class InputBoxViewModel : ViewModelBase
    {
        public DelegateCommand EditRamCommand { get; set; }
        public DelegateCommand EditFileCommand { get; set; }
        public Action CloseAction { get; set; }

        public InputBoxViewModel()
        {
            EditRamCommand = new DelegateCommand(EditRamButton_Click);
            EditFileCommand = new DelegateCommand(EditFileButton_Click);
        }

        /// <summary>
        /// Edit ram button click
        /// </summary>
        private void EditRamButton_Click()
        {
            SelectProcessView window = new SelectProcessView();
            window.Show();
            CloseAction();
        }

        /// <summary>
        /// Edit file button click
        /// </summary>
        private void EditFileButton_Click()
        {
            EditMemoryView w = new EditMemoryView();
            w.Show();
            CloseAction();
        }



    }
}