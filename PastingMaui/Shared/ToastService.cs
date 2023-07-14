using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace PastingMaui.Shared
{
    internal class ToastService : IToastService
    {

        SemaphoreSlim toastListSema = new SemaphoreSlim(1);

        private ObservableCollection<ToastData> toasts;

        public ObservableCollection<ToastData> Toasts { get { return toasts; } }

        public void EnumerateToasts(Action<ObservableCollection<ToastData>> action)
        {
            toastListSema.Wait();
            action.Invoke(toasts);
            toastListSema.Release();
        }

        public ToastService() { 
            toasts = new ObservableCollection<ToastData>();
        }

        public PropertyChangedEventHandler rerender;

        public PropertyChangedEventHandler UpdateRendered {
            get {
                return rerender;
            }
            set {
                rerender = value;
            }
        }

        public event Action<ObservableCollection<Toast>> 
            dToast;

        public void AddToast(string title, string message, ToastType type)
        {
            var toast = new ToastData(title, message, type);
            InitializeToast(toast);
        }

        public void AddToast(ToastData toast)
        {
            InitializeToast(toast);
        }

        public void InitializeToast(ToastData toast)
        {
            toast.deleteToast += () =>
            {
                toastListSema.Wait();
                toasts.Remove(toast);
                toastListSema.Release();
            };
            toast.PropertyChanged += UpdateRendered;

            toastListSema.Wait();
            toasts.Add(toast);
            toastListSema.Release();
        }

        public ToastData GetToastById(string id)
        {
            toastListSema.Wait();
            var toast = toasts.Where(toast => toast.Id.Equals(id)).FirstOrDefault();
            toastListSema.Release();
            return toast;
        }

        public void UpdateToast(string id)
        {
            var toastFound = GetToastById(id);
            if (toastFound != null)
            {
                // update the toast

                //toastFound.updateToast()
            }
        }

        public void RemoveToast(string id)
        {
            var toastFound = GetToastById(id);
            if (toastFound != null)
            {
                toastListSema.Wait();
                toasts.Remove(toastFound);
                toastListSema.Release();
            }
        }

    }
}
