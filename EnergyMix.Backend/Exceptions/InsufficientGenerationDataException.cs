namespace EnergyMix.Backend.Exceptions;

public sealed class InsufficientGenerationDataException : Exception
{
    public InsufficientGenerationDataException(string message)
        : base(message)
    {
    }
}
