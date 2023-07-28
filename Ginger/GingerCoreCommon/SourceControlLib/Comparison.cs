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
        public enum State
        {
            Unmodified,
            Modified,
            Added,
            Deleted
        }

        public string Name { get; }
        public State StateType { get; }

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
                if ((StateType == State.Added || StateType == State.Deleted) && HasChildComparisons)
                {
                    foreach (Comparison nestedChange in ChildComparisons)
                        nestedChange.Selected = _selected;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Selected)));
            }
        }

        public Comparison(string name, State state, ICollection<Comparison> childComparisons, Type dataType)
        {
            Name = name;
            StateType = state;
            ChildComparisons = childComparisons;
            HasChildComparisons = true;
            Data = null!;
            DataType = dataType;
        }

        public Comparison(string name, State state, ICollection<Comparison> childComparisons, object? data)
        {
            Name = name;
            StateType = state;
            ChildComparisons = childComparisons;
            HasChildComparisons = true;
            Data = data;
            HasData = true;
        }

        public Comparison(string name, State state, object? data)
        {
            Name = name;
            StateType = state;
            Data = data;
            HasData = true;
            ChildComparisons = null!;
        }
    }
}
