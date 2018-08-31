using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumberBoxDemo.ViewModels {
    class MainViewModel : INotifyPropertyChanged {

        private double _myValue1 = 12.5;
        private double? _myValue2 = 16.8;
        private int _myValue3 = 42;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public double MyValue1 {
            get { return _myValue1; }
            set {
                if (_myValue1 != value) {
                    _myValue1 = value;
                    OnPropertyChanged(nameof(MyValue1));
                }
            }
        }

        public double? MyValue2 {
            get { return _myValue2; }
            set {
                if (_myValue2 != value) {
                    _myValue2 = value;
                    OnPropertyChanged(nameof(MyValue2));
                }
            }
        }

        public int MyValue3 {
            get { return _myValue3; }
            set {
                if (_myValue3 != value) {
                    _myValue3 = value;
                    OnPropertyChanged(nameof(MyValue3));
                }
            }
        }

    }
}
