using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.Common
{
    public sealed class Observable<T> : INotifyPropertyChanged
    {
        private T _value;

        public event PropertyChangedEventHandler PropertyChanged;
        
        public T Value 
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        public object ValueAsObject
        {
            get => _value;
            set 
            {
                _value = (T)value;
                OnPropertyChanged(nameof(ValueAsObject));
            }
        }

        public Observable(T value)
        {
            _value = value;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(sender: this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
