namespace Amdocs.Ginger.CoreNET.RunLib.CLILib.Interfaces
{
    /// <summary>
    /// Represents an interface for CLI options validation.
    /// </summary>
    internal interface IOptionValidation
    {
        /// <summary>
        /// Validates the option.
        /// </summary>
        /// <returns>True if the option is valid; otherwise, false.</returns>
        bool Validate();
    }
}
