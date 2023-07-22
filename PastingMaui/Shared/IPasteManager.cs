using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PastingMaui.Shared
{
    public interface IPasteManager
    {

        public void AddPaste(Paste paste);

        public void RemovePaste(Paste paste);

        public void ActionOnList(Action<ObservableCollection<Paste>> action);



    }
}
