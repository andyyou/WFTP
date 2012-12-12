using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;

namespace WFTP.Pages
{
    public class FileItem : INotifyPropertyChanged
    {
        private string _target_path;
        private bool _is_replace;
        public FileInfo File { set; get; }

        public string TargetPath
        {
            get
            {
                return _target_path;
            }
            set
            {
                _target_path = value;
                RaisePropertyChanged("TargetPath");
            }
        }
        public bool IsReplace
        {
            get
            {
                return _is_replace;
            }
            set
            {
                _is_replace = value;
                RaisePropertyChanged("IsReplace");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(String propertyName)
        {
            if ((PropertyChanged != null))
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
    public class AdminItem : INotifyPropertyChanged
    {
        private bool _isAdmin;
        private int _rank;

        public bool IsAdmin
        {
            get
            {
                return _isAdmin;
            }
            set
            {
                _isAdmin = value;
                RaisePropertyChanged("IsAdmin");
            }
        }
        public int Rank
        {
            get
            {
                return _rank;
            }
            set
            {
                _rank = value;
                RaisePropertyChanged("Rank");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(String propertyName)
        {
            if ((PropertyChanged != null))
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
