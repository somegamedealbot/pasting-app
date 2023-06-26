using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PastingMaui.Shared
{
    internal class ToastService
    {
        private ObservableCollection<ToastData> toasts;

        public ObservableCollection<ToastData> Toasts { get { return toasts; } }

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
            toasts.Add(toast);
            toast.deleteToast += () =>
            {
                toasts.Remove(toast);
            };
            toast.PropertyChanged += UpdateRendered;
        }

        public ToastData GetToastById(string id)
        {
            return toasts.Where(toast => toast.Id.Equals(id)).FirstOrDefault();
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
                toasts.Remove(toastFound);
            }
        }

    }
}
