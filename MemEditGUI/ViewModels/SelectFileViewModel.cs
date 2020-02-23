using System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MemEditGUI.Models;
using MemEditGUI.Views;

namespace MemEditGUI.ViewModels
{
    class SelectFileViewModel : ViewModelBase
    {
        public DelegateCommand OkCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand BrowseCommand { get; set; }
        public Action CloseAction { get; set; }

        private string path;


        public string Path
        {
            get { return path; }
            set { SetProperty(ref path, value); }
        }

        public SelectFileViewModel()
        {
            OkCommand = new DelegateCommand(OkButton_Click);
            CancelCommand = new DelegateCommand(CancelButton_Click);
            BrowseCommand = new DelegateCommand(BrowseButton_Click);
        }

        /// <summary>
        /// Edit ram button click
        /// </summary>
        private void OkButton_Click()
        {
            if (!File.Exists(path))
            {
                MessageBox.Show("File does not exist!");
                return;
            }
            GlobalStates.Instance.SelectMemorySource(new MemorySource(path));
            new EditMemoryView().Show();
            CloseAction();
        }

        /// <summary>
        /// Edit file button click
        /// </summary>
        private void CancelButton_Click()
        {
            new MainView().Show();
            CloseAction();
        }


        /// <summary>
        /// Browse for file button click
        /// </summary>
        private void BrowseButton_Click()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Path = ofd.FileName;
            }
        }



    }
}