#region License
/*
Copyright © 2014-2026 European Support Limited
 
Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
 
http://www.apache.org/licenses/LICENSE-2.0
 
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion
using System.ComponentModel;

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
