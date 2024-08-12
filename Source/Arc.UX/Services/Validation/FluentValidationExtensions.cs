using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace Arc.UX.Services.Validation;

public static class FluentValidationExtensions
{
    private static readonly char[] Separators = { '.', '[' };
    private static readonly List<string> ScannedAssembly = new List<string>();
    private static readonly List<AssemblyScanner.AssemblyScanResult> AssemblyScanResults = new List<AssemblyScanner.AssemblyScanResult>();


    public static void AddValidationError(this ValidationMessageStore validationMessageStore, ValidationFailure validationFailure, EditContext editContext)
    {
        var fieldIdentifier = ToFieldIdentifier(editContext, validationFailure.PropertyName);
        validationMessageStore.Add(fieldIdentifier, validationFailure.ErrorMessage);
    }



    public static IValidator GetFluentValidationValidator(this IServiceProvider serviceProvider, object model)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(model.GetType());
        if (serviceProvider != null)
        {
            try
            {
                if (serviceProvider.GetService(validatorType) is IValidator validator)
                {
                    return validator;
                }
            }
            catch (Exception)
            {
            }
        }


        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(i => !ScannedAssembly.Contains(i.FullName)))
        {
            try
            {
                AssemblyScanResults.AddRange(AssemblyScanner.FindValidatorsInAssembly(assembly));
            }
            catch (Exception)
            {
            }

            ScannedAssembly.Add(assembly.FullName);
        }


        var interfaceValidatorType = typeof(IValidator<>).MakeGenericType(model.GetType());

        Type modelValidatorType = AssemblyScanResults.FirstOrDefault(i => interfaceValidatorType.IsAssignableFrom(i.InterfaceType))?.ValidatorType;

        if (modelValidatorType == null)
        {
            return null;
        }

        return (IValidator)ActivatorUtilities.CreateInstance(serviceProvider, modelValidatorType);
    }

    private static FieldIdentifier ToFieldIdentifier(in EditContext editContext, in string propertyPath)
    {
        // This code is taken from an article by Steve Sanderson (https://blog.stevensanderson.com/2019/09/04/blazor-fluentvalidation/)
        // all credit goes to him for this code.

        // This method parses property paths like 'SomeProp.MyCollection[123].ChildProp'
        // and returns a FieldIdentifier which is an (instance, propName) pair. For example,
        // it would return the pair (SomeProp.MyCollection[123], "ChildProp"). It traverses
        // as far into the propertyPath as it can go until it finds any null instance.

        var obj = editContext.Model;
        var nextTokenEnd = propertyPath.IndexOfAny(Separators);

        // Optimize for a scenario when parsing isn't needed.
        if (nextTokenEnd < 0)
        {
            return new FieldIdentifier(obj, propertyPath);
        }

        ReadOnlySpan<char> propertyPathAsSpan = propertyPath;

        while (true)
        {
            var nextToken = propertyPathAsSpan.Slice(0, nextTokenEnd);
            propertyPathAsSpan = propertyPathAsSpan.Slice(nextTokenEnd + 1);

            object newObj;
            if (nextToken.EndsWith("]"))
            {
                // It's an indexer
                // This code assumes C# conventions (one indexer named Item with one param)
                nextToken = nextToken.Slice(0, nextToken.Length - 1);
                var prop = obj.GetType().GetProperty("Item");

                if (prop is object)
                {
                    // we've got an Item property
                    var indexerType = prop.GetIndexParameters()[0].ParameterType;
                    var indexerValue = Convert.ChangeType(nextToken.ToString(), indexerType);
                    newObj = prop.GetValue(obj, new object[] { indexerValue });
                }
                else
                {
                    // If there is no Item property
                    // Try to cast the object to array
                    if (obj is object[] array)
                    {
                        var indexerValue = int.Parse(nextToken);
                        newObj = array[indexerValue];
                    }
                    else
                    {
                        throw new InvalidOperationException($"Could not find indexer on object of type {obj.GetType().FullName}.");
                    }
                }
            }
            else
            {
                // It's a regular property
                var prop = obj.GetType().GetProperty(nextToken.ToString());
                if (prop == null)
                {
                    throw new InvalidOperationException($"Could not find property named {nextToken.ToString()} on object of type {obj.GetType().FullName}.");
                }
                newObj = prop.GetValue(obj);
            }

            if (newObj == null)
            {
                // This is as far as we can go
                return new FieldIdentifier(obj, nextToken.ToString());
            }

            obj = newObj;

            nextTokenEnd = propertyPathAsSpan.IndexOfAny(Separators);
            if (nextTokenEnd < 0)
            {
                return new FieldIdentifier(obj, propertyPathAsSpan.ToString());
            }
        }
    }
}