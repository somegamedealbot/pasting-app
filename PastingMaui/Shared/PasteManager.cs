﻿using PastingMaui.Data;
using System.Collections.ObjectModel;

namespace PastingMaui.Shared
{
    internal class PasteManager : IPasteManager
    {

        public ObservableCollection<Paste> pasteList = new();

        private readonly ReaderWriterLockSlim pasteListLock = new();

        public event EventHandler OnChangesHandlers;

        public event EventHandler OnChanges { 
            add { OnChangesHandlers += value; } 
            remove { OnChangesHandlers -= value; } 
        }

        public Paste AddPaste(Stream streamData, BasePacketInfo packetInfo)
        {
            pasteListLock.EnterWriteLock();
            var paste = new Paste(streamData, packetInfo);
            pasteList.Add(paste);
            OnChangesHandlers?.Invoke(this, null);
            pasteListLock.ExitWriteLock();
            return paste;
        }

        public Paste AddPaste(Paste paste)
        {
            pasteListLock.EnterWriteLock();
            pasteList.Add(paste);
            OnChangesHandlers?.Invoke(this, null);
            pasteListLock.ExitWriteLock();
            return paste;
        }

        public bool RemovePaste(Paste paste)
        {
            pasteListLock.EnterWriteLock();
            var isRemoved = pasteList.Remove(paste);
            OnChangesHandlers?.Invoke(this, null);
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

