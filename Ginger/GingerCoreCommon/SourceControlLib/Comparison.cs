using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public bool Selected
        {
            get => _selected;
            set
            {
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
            }
        }

        public Comparison(string name, StateType state, ICollection<Comparison> childComparisons, Type dataType)
        {
            Name = name;
            State = state;
            ChildComparisons = childComparisons;
            HasChildComparisons = true;
            foreach(Comparison childComparison in childComparisons)
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
                Dictionary<string, List<Comparison>> uniqueNameComparisons = new();
                foreach (Comparison childComparison in ChildComparisons)
                {
                    if (uniqueNameComparisons.ContainsKey(childComparison.Name))
                    {
                        uniqueNameComparisons[childComparison.Name].Add(childComparison);
                    }
                    else
                    {
                        uniqueNameComparisons.Add(childComparison.Name, new List<Comparison>() { childComparison });
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
