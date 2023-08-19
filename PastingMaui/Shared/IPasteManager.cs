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

        public Paste AddPaste(Stream streamData);

        public Paste AddPaste(Paste paste);

        public bool RemovePaste(Paste paste);

        public void ActionOnList(Action<ObservableCollection<Paste>> action);

        public event EventHandler OnChanges;

    }
}
