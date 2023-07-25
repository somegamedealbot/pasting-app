

namespace PastingMaui.Shared
{
    public class Paste
    {

        public enum PasteState
        {
            Incomplete, InProgress, Completed
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

        Stream data;
        string explicitData;

        public Paste(Stream receivedData) {
            data = receivedData;
            pasteSemaphore = new SemaphoreSlim(1);
            currentState = PasteState.Incomplete;
        }

        public void CompletePaste()
        {
            pasteSemaphore.Wait();
            currentState = PasteState.Completed;
            OnCompleteActions.Invoke(this, null);
            pasteSemaphore.Release();
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
                        using (var reader = new StreamReader(data))
                            explicitData = reader.ReadToEnd();
                        data.Seek(0, SeekOrigin.Begin);
                        pasteSemaphore.Release();
                    }
                    return explicitData;
                }
                return null;
            }
        }
    }
}
