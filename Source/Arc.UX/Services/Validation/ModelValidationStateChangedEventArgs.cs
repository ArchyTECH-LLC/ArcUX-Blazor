namespace Arc.UX.Services.Validation;

public class ModelValidationStateChangedEventArgs
{
    public ModelValidationStateChangedEventArgs(bool isModelValid)
    {
        IsModelValid = isModelValid;
    }

    public bool IsModelValid { get; }
}