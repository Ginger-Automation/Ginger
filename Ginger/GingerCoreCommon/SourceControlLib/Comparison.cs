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
                        nestedChange.Selected = _selected;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Selected)));
            }
        }

        public Comparison(string name, StateType state, ICollection<Comparison> childComparisons, Type dataType)
        {
            Name = name;
            State = state;
            ChildComparisons = childComparisons;
            HasChildComparisons = true;
            Data = null!;
            DataType = dataType;
        }

        public Comparison(string name, StateType state, ICollection<Comparison> childComparisons, object? data)
        {
            Name = name;
            State = state;
            ChildComparisons = childComparisons;
            HasChildComparisons = true;
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

        public bool CanBeMerged()
        {
            if (State == StateType.Unmodified)
            {
                return true;
            }
            else if (State == StateType.Modified)
            {
                Dictionary<string, List<Comparison>> uniqueNameComparisons = new();
                foreach(Comparison childComparison in ChildComparisons)
                {
                    if (uniqueNameComparisons.ContainsKey(childComparison.Name))
                        uniqueNameComparisons[childComparison.Name].Add(childComparison);
                    else
                        uniqueNameComparisons.Add(childComparison.Name, new List<Comparison>() { childComparison });
                }

                foreach(KeyValuePair<string,List<Comparison>> nameComparison in uniqueNameComparisons)
                {
                    if(!nameComparison.Value.Any(c => c.CanBeMerged()))
                    {
                        return false;
                    }
                }

                return true;
            }
            else
            {
                return Selected;
            }
        }
    }
}
