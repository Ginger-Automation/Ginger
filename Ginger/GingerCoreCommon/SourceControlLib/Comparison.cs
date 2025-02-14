#region License
/*
Copyright © 2014-2025 European Support Limited

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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

#nullable enable
namespace Amdocs.Ginger.Common.SourceControlLib
{
    public sealed class Comparison : INotifyPropertyChanged
    {
        public enum StateType
        {
            Unmodified,
            Modified,
            Added,
            Deleted
        }

        public string Name { get; }
        public StateType State { get; }

        public bool HasChildComparisons { get; } = false;
        public ICollection<Comparison> ChildComparisons { get; }

        public bool HasSiblingComparison => SiblingComparison != null;
        public Comparison SiblingComparison { get; private set; } = null!;

        public bool HasParentComparison => ParentComparison != null;
        public Comparison ParentComparison { get; private set; } = null!;

        public bool HasData { get; } = false;
        public object? Data { get; }
        public string DataAsString => $"{Data}";

        public Type? DataType { get; }

        private bool _selected;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsSelectionEnabled { get; set; } = true;

        public bool Selected
        {
            get => _selected;
            set
            {
                if (!IsSelectionEnabled)
                {
                    return;
                }

                _selected = value;

                if ((State == StateType.Added || State == StateType.Deleted) && HasChildComparisons)
                {
                    foreach (Comparison nestedChange in ChildComparisons)
                    {
                        nestedChange.Selected = _selected;
                    }
                }

                bool parentIsNotCollection =
                    ParentComparison == null ||
                    ParentComparison.DataType == null ||
                    !ParentComparison.DataType.IsAssignableTo(typeof(System.Collections.ICollection));

                if (_selected && HasSiblingComparison && parentIsNotCollection)
                {
                    SiblingComparison.Selected = false;
                }
                PropertyChangedEventHandler? propertyChangedEventHandler = PropertyChanged;
                propertyChangedEventHandler?.Invoke(this, new PropertyChangedEventArgs(nameof(Selected)));
                NotifyParentOfPropertyChange();
            }
        }

        public Comparison(string name, StateType state, ICollection<Comparison> childComparisons, Type dataType)
        {
            Name = name;
            State = state;
            ChildComparisons = childComparisons;
            HasChildComparisons = true;
            foreach (Comparison childComparison in childComparisons)
            {
                childComparison.ParentComparison = this;
            }
            Data = null!;
            DataType = dataType;
        }

        public Comparison(string name, StateType state, ICollection<Comparison> childComparisons, object? data)
        {
            Name = name;
            State = state;
            ChildComparisons = childComparisons;
            HasChildComparisons = true;
            foreach (Comparison childComparison in childComparisons)
            {
                childComparison.ParentComparison = this;
            }
            Data = data;
            HasData = true;
        }

        public Comparison(string name, StateType state, object? data)
        {
            Name = name;
            State = state;
            Data = data;
            HasData = true;
            ChildComparisons = null!;
        }

        private void NotifyParentOfPropertyChange()
        {
            if (ParentComparison != null)
            {
                ParentComparison.OnChildPropertyChange();
            }
        }

        private void OnChildPropertyChange()
        {
            PropertyChangedEventHandler? handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(nameof(ChildComparisons)));
            NotifyParentOfPropertyChange();
        }

        public void SetSiblingComparison(Comparison siblingComparison)
        {
            SiblingComparison = siblingComparison;
            SiblingComparison.SiblingComparison = this;
        }

        public int UnselectedComparisonCount()
        {
            return (int)UnselectedComparisonCountPrivate();
        }

        private double UnselectedComparisonCountPrivate()
        {
            if (State == StateType.Unmodified)
            {
                return 0;
            }
            else if (State == StateType.Modified)
            {
                Dictionary<string, List<Comparison>> uniqueNameComparisons = [];
                foreach (Comparison childComparison in ChildComparisons)
                {
                    if (uniqueNameComparisons.ContainsKey(childComparison.Name))
                    {
                        uniqueNameComparisons[childComparison.Name].Add(childComparison);
                    }
                    else
                    {
                        uniqueNameComparisons.Add(childComparison.Name, [childComparison]);
                    }
                }

                double unselectedChildComparisonCount = 0;
                foreach (KeyValuePair<string, List<Comparison>> nameComparison in uniqueNameComparisons)
                {
                    unselectedChildComparisonCount += nameComparison.Value.Aggregate(0.0, (total, c) => total + c.UnselectedComparisonCountPrivate());
                }

                return unselectedChildComparisonCount;
            }
            else
            {
                double unselectedComparisonCount;
                if (Selected || (HasSiblingComparison && SiblingComparison.Selected))
                {
                    unselectedComparisonCount = 0;
                }
                else
                {
                    if (HasSiblingComparison)
                    {
                        unselectedComparisonCount = 0.5;
                    }
                    else
                    {
                        unselectedComparisonCount = 0;
                    }
                }
                return unselectedComparisonCount;
            }
        }
    }
}
