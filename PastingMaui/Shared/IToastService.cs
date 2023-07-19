using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PastingMaui.Shared
{
    public interface IToastService
    {
        public ObservableCollection<ToastData> Toasts
        {
            get;
        }

        public PropertyChangedEventHandler UpdateRendered
        {
            get; set;
        }

        public void EnumerateToasts(Action<ObservableCollection<ToastData>> action);

        public void AddToast(string title, string message, ToastType type);

        public void AddToast(ToastData toast);

        public void UpdateToast(string id);

        public void RemoveToast(string id);
    }
}
