using ArchyTECH.Core.Extensions;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Arc.UX.Services.Validation
{

    /// <summary>
    /// Original source Blazored.FluentValidation : https://github.com/Blazored/FluentValidation/tree/main/src/Blazored.FluentValidation
    /// </summary>

    public class FluentValidator : ComponentBase, IDisposable
    {
        [Inject]
        private  IServiceProvider ServiceProvider { get; set; } = null!;

        [CascadingParameter]
        private EditContext? EditContext { get; set; }

        public bool? IsModelValid { get; protected set; }

        [Parameter]
        public IValidator? Validator { get; set; }

        [Parameter]
        public EventCallback<ModelValidationStateChangedEventArgs> OnModelStateValidated { get; set; }

        protected ValidationMessageStore? ValidationMessageStore { get; private set; }

        [Parameter]
        public Action<ValidationStrategy<object>> ValidationOptions { get; set; } = options => options.IncludeAllRuleSets();

        protected override bool ShouldRender()
        {
            return EditContext != null && Validator != null;
        }

        protected override async Task OnInitializedAsync()
        {

            if (EditContext == null)
            {
                throw new InvalidOperationException($"{nameof(FluentValidation)} requires a cascading " +
                    $"parameter of type {nameof(EditContext)}. For example, you can use {nameof(FluentValidation)} " +
                    $"inside an {nameof(EditForm)}.");
            }

            ValidationMessageStore = new ValidationMessageStore(EditContext);

            Validator ??= ServiceProvider.GetFluentValidationValidator(EditContext.Model);

            // If there is no fluent validator for this model, we can't validate
            if (Validator == null)
            {
                return;
            }

            EditContext.OnValidationRequested += ValidateModelAsync;
            EditContext.OnFieldChanged += ValidateField;
            await CheckModelStateValidationAsync();
        }

        /// <summary>
        /// Deregisters event handlers on dispose
        /// </summary>
        public void Dispose()
        {
            EditContext!.OnValidationRequested -= ValidateModelAsync;
            EditContext!.OnFieldChanged -= ValidateField;
        }

        /// <summary>
        /// Silently determines if the model state
        /// </summary>
        /// <param name="isModelStateValid">Optional parameter to skip validating the entire model state</param>
        protected async Task CheckModelStateValidationAsync(bool? isModelStateValid = null)
        {
            var wasValid = IsModelValid;

            // Skip revalidating the entire model if already calculated
            if (isModelStateValid.HasValue)
            {
                IsModelValid = isModelStateValid.Value;
            }
            else
            {
                var modelValidationResults = await GetModelValidationErrors();
                IsModelValid = modelValidationResults.Errors.None();
            }

            // Only raise change notification if the validation state has changed
            if (wasValid != IsModelValid)
            {
                await NotifyModelStateChanged();
            }
        }

        protected async Task NotifyModelStateChanged()
        {
            await OnModelStateValidated.InvokeAsync(new ModelValidationStateChangedEventArgs(IsModelValid ?? false));
        }

        protected virtual async Task<ValidationResult> GetModelValidationErrors()
        {
            var context = ValidationContext<object>.CreateWithOptions(EditContext!.Model, ValidationOptions);
            var validationResults = await Validator!.ValidateAsync(context);
            return validationResults;
        }

        protected virtual async void ValidateModelAsync(object? sender, ValidationRequestedEventArgs e)
        {
            var validationResults = await GetModelValidationErrors();

            ValidationMessageStore!.Clear();
            foreach (var validationResult in validationResults.Errors)
            {
                ValidationMessageStore.AddValidationError(validationResult, EditContext!);
            }

            var isValid = validationResults.Errors.None();
            await CheckModelStateValidationAsync(isValid);
        }

        protected virtual async void ValidateField(object? sender, FieldChangedEventArgs e)
        {
            var fieldIdentifier = e.FieldIdentifier;
            var properties = new[] { fieldIdentifier.FieldName };
            var validationContext = new ValidationContext<object>(EditContext!.Model, new PropertyChain(), new MemberNameValidatorSelector(properties));

            var fieldValidationResults = await Validator!.ValidateAsync(validationContext);

            ValidationMessageStore!.Clear(fieldIdentifier);
            ValidationMessageStore.Add(fieldIdentifier, fieldValidationResults.Errors.Select(error => error.ErrorMessage));


            await CheckModelStateValidationAsync();
        }
    }
}
