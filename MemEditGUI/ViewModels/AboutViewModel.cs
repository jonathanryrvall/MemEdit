using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MemEditGUI.ViewModels
{
    /// <summary>
    /// View model for about page
    /// </summary>
    public class AboutViewModel : ViewModelBase
    {
        public DelegateCommand CloseCommand { get; set; }
        public Action CloseAction { get; set; }

        //private string path;



        public AboutViewModel()
        {
            CloseCommand = new DelegateCommand(CloseButton_Click);
        }


        /// <summary>
        /// Build date
        /// </summary>
        public string BuildDate
        {
            get
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                DateTime buildDate = new DateTime(2000, 1, 1).AddDays(version.Build).AddSeconds(version.Revision * 2);

                string date = buildDate.ToString("yyyy-MM-dd HH:mm");
                return "Build " + date;
            }
        }

        /// <summary>
        /// Build date
        /// </summary>
        public string VersionNumber
        {
            get
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                return "Version " + version.Major.ToString() + "." + version.Minor.ToString() + "." + version.Build.ToString() + "." + version.Revision.ToString();
            }
        }
        /// <summary>
        /// Close button click
        /// </summary>
        private void CloseButton_Click()
        {
            CloseAction();
        }



    }
}