using System.Collections.ObjectModel;

namespace PastingMaui.Shared
{
    internal class PasteList
    {

        public ObservableCollection<Paste> pasteList = new ObservableCollection<Paste>();

        private ReaderWriterLockSlim pasteListLock = new ReaderWriterLockSlim();

        public void AddPaste(Paste paste)
        {
            pasteListLock.EnterWriteLock();
            pasteList.Add(paste);
            pasteListLock.ExitWriteLock();
        }

        public bool RemovePaste(Paste paste)
        {
            pasteListLock.EnterWriteLock();
            var isRemoved = pasteList.Remove(paste);
            pasteListLock.ExitWriteLock();
            return isRemoved;
        }

        public void ActionOnList(Action<ObservableCollection<Paste>> action)
        {
            pasteListLock.EnterReadLock();
            action(pasteList);
            pasteListLock.ExitReadLock();
        }

    }
}
