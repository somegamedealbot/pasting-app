using System.Collections.ObjectModel;

namespace PastingMaui.Shared
{
    internal class PasteManager : IPasteManager
    {

        public ObservableCollection<Paste> pasteList = new();

        private readonly ReaderWriterLockSlim pasteListLock = new();

        public Paste AddPaste(Stream streamData)
        {
            pasteListLock.EnterWriteLock();
            var paste = new Paste(streamData);
            pasteList.Add(paste);
            pasteListLock.ExitWriteLock();
            return paste;
        }

        public Paste AddPaste(Paste paste)
        {
            pasteListLock.EnterWriteLock();
            pasteList.Add(paste);
            pasteListLock.ExitWriteLock();
            return paste;
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

