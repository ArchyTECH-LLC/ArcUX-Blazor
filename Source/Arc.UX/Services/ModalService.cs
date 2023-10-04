using ArchyTECH.Core.Results;
using Microsoft.AspNetCore.Components;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using Arc.UX.Components;

namespace Arc.UX.Services;

    public interface IModalService
    {
        void Dismiss(ArcModal modal);
        ObservableCollection<ModalService.ModalReference> Modals { get; }
        event Action<ModalService.ModalReference> OnModalChanged;
        Task<Result> Show<TPage>(string title, dynamic parameters = null) where TPage : ComponentBase;
    }

    public class ModalService : IModalService
    {
        public ObservableCollection<ModalReference> Modals { get; set; } = new();
        public event Action<ModalReference> OnModalChanged = _ => { };

        public async Task<Result> Show<TPage>(string title, dynamic parameters = null) where TPage : ComponentBase
        {
            var modalReference = new ModalReference();
            var modalContent = new RenderFragment(builder =>
            {
                var i = 0;
                builder.OpenComponent<TPage>(i);

                if (parameters != null)
                {

                    foreach (PropertyDescriptor parameter in TypeDescriptor.GetProperties(parameters))
                    {
                        i++;
                        object parameterValue = parameter.GetValue(parameters);
                        builder.AddAttribute(i, parameter.Name, parameterValue);

                        Debug.WriteLine($"Modal Content Parameter: '{parameter.Name}' = '{parameterValue}'");
                    }
                }

                builder.CloseComponent();
            });

            var modalResult = new TaskCompletionSource<Result>();
            modalReference.Fragment = builder =>
            {
                builder.OpenComponent<ArcModal>(0);
                builder.AddAttribute(1, "Title", title);
                builder.AddAttribute(2, "Body", modalContent);
                builder.AddAttribute(3, "Result", modalResult);
                builder.AddComponentReferenceCapture(4, reference => modalReference.Instance = (ArcModal)reference);
                builder.CloseComponent();
            };

            Modals.Add(modalReference);
            OnModalChanged(modalReference);

            return await modalResult.Task;
        }

        public class ModalReference
        {
            public RenderFragment? Fragment { get; set; }
            public ArcModal? Instance { get; set; }
        }

        public void Dismiss(ArcModal modal)
        {
            var reference = Modals.FirstOrDefault(modalReference => modalReference.Instance == modal);
            if (reference != null) Modals.Remove(reference);
            OnModalChanged(reference);
        }
    }