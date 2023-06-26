using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace PastingMaui.Shared
{

    public enum ToastType
    {
        Alert,
        DisplayLoading
    }

    public class ToastData : ComponentBase, INotifyPropertyChanged
    {
        private string title;

        private string message;

        private string id;

        private ToastType type;

        public bool isVisible
        {
            get; set;
        }

        public event Action deleteToast;

        public event PropertyChangedEventHandler PropertyChanged;

        public EventHandler<string> changeToastContent; 


        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        public string Id
        {
            get { return id; }
        }

        public ToastType Type
        {
            get { return type; }
        }

/*        protected override void OnInitialized()
        {

        }*/

        public ToastData()
        {

        }

        public ToastData(string title, string message, ToastType type) : base()
        {
            this.title = title;
            this.message = message;
            id = Guid.NewGuid().ToString();
            this.type = type;
            isVisible = true; 

            if (type == ToastType.Alert)
            {
                TimeAlert();
            }
            // BuildToast(title, message);
        }

        public void AfterTime()
        {
            deleteThisToast();
        }

        public async void TimeAlert()
        {
            await Task.Delay(3000);
            isVisible = !isVisible;
            OnPropertyChange("visibility");
            await Task.Delay(750);

            AfterTime();
            /*var timer = Microsoft.Maui.Controls.Application.Current.Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.
            timer.Tick += (sender, args) => { };
            timer.Start();*/
            
        }

        public void OnPropertyChange(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public void deleteThisToast()
        {
            deleteToast?.Invoke();
        }

        public void updateToast(string title)
        {
            this.title = title;
            OnPropertyChange("title");
        }

        public void updateToast(string title, string message, ToastType type)
        {
            this.title = title;
            this.message = message;
            OnPropertyChange(title); this.type = type;

        }

        /*private void BuildToast(string title, string message) {
            // add more options later

            Title = title;
            Message = message;

        }*/

    }
}
