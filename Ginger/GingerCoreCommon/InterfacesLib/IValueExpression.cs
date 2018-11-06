namespace Amdocs.Ginger.Common
{
    public interface IValueExpression
    {
        string Value { get; set; }
        string ValueCalculated { get; }
    }
}
