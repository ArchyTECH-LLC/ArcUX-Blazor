using Microsoft.AspNetCore.Components.Forms;

namespace Arc.UX.Services.Validation
{
    public class BootstrapCssProvider : FieldCssClassProvider
    {
        protected HashSet<FieldIdentifier> ValidatedFields { get; } = new();

        public void MarkFieldValidated(in FieldIdentifier fieldIdentifier)
        {
            ValidatedFields.Add(fieldIdentifier);
        }

        public void MarkFieldUnvalidated(in FieldIdentifier fieldIdentifier)
        {
            ValidatedFields.Remove(fieldIdentifier);
        }

        public override string GetFieldCssClass(EditContext editContext, in FieldIdentifier fieldIdentifier)
        {
            var isValidated = ValidatedFields.Contains(fieldIdentifier);

            var validatedCssClass = isValidated ? "is-valid" : string.Empty;
            
            var isInvalid = editContext
                .GetValidationMessages(fieldIdentifier)
                .Any();

            return isInvalid ?  "is-invalid": validatedCssClass;
        }
    }

}
