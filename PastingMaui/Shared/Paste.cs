

using PastingMaui.Data;

namespace PastingMaui.Shared
{
    public class Paste
    {

        Stream data;
        string explicitData;
        public BasePacketInfo pasteInfo
        {
            get; private set;
        }

        public async Task SetPaste()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Clipboard.SetTextAsync(explicitData);

            });
        }

        public enum PasteState
        {
            InProgress, Completed, Incomplete
        }

        public SemaphoreSlim pasteSemaphore;

        PasteState currentState;

        private event EventHandler OnCompleteActions;

        public event EventHandler OnComplete
        {
            add
            {
                OnCompleteActions += value;
            }

            remove
            {
                OnCompleteActions -= value;
            }
        }

        public PasteState State
        {
            get
            {
                pasteSemaphore.Wait();
                var state = currentState;
                pasteSemaphore.Release();
                return state;
            }
            set
            {
                pasteSemaphore.Wait();
                currentState = value;
                pasteSemaphore.Release();
            }
        }
        public Paste(Stream receivedData, BasePacketInfo packetInfo) {
            data = receivedData;
            pasteInfo = packetInfo;
            pasteSemaphore = new SemaphoreSlim(1);
            currentState = PasteState.InProgress;
        }

        public void CompletePaste()
        {
            pasteSemaphore.Wait();
            currentState = PasteState.Completed;
            if (pasteInfo.IsText)
            {
                if (data != null)
                {
                    data.Position = 0; // temporary fix of reading extra null characters
                    // not sure what causes the reading of null characters
                }
            }
            else
            {
                data.Flush();
                data.Close();
            }
            OnCompleteActions?.Invoke(this, null);
            pasteSemaphore.Release();
        }

        public void InCompletePaste() // used in case of an error I/O error of saving or reading packet
        {
            if (State == PasteState.InProgress)
            {
                pasteSemaphore.Wait();
                currentState = PasteState.Incomplete;
                data.Close();
                pasteSemaphore.Release();
            }
        }

        public bool IsComplete()
        {
            return State == PasteState.Completed;
        }

        public string StringData
        {
            get
            {
                if (IsComplete()) { 
                    if (explicitData == null)
                    {
                        pasteSemaphore.Wait();
                        if (data == null) {
                            Console.WriteLine("error point");
                        }
                        using (var reader = new StreamReader(data))
                            explicitData = reader.ReadToEnd();
                        // data.Seek(0, SeekOrigin.Begin);
                        // close data stream here?
                        pasteSemaphore.Release();
                    }
                    return explicitData;
                }
                return null;
            }
        }
    }
}
