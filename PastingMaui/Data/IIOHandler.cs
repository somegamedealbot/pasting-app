using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PastingMaui.Data
{
    public class IIOHandler
    {
        public class OnReadThreadEndArgs : EventArgs
        {
        }

        public class OnWriteThreadEndArgs : EventArgs { }

        public delegate void OnReadThreadEndHandler(object sender, OnReadThreadEndArgs args);

        public delegate void OnWriteThreadEndHandler(object sender, OnWriteThreadEndArgs args);

        public event OnReadThreadEndHandler _OnReadThreadEndHandler;

        public event OnWriteThreadEndHandler _OnWriteThreadEndHandler;

        public event OnWriteThreadEndHandler OnWriteThreadEnd
        {
            add
            {
                _OnWriteThreadEndHandler += value;
            }
            remove
            {
                _OnWriteThreadEndHandler -= value;
            }
        }

        public event OnReadThreadEndHandler OnReadThreadEnd
        {
            add
            {
                _OnReadThreadEndHandler += value;
            }
            remove
            {
                _OnReadThreadEndHandler -= value;
            }
        }

        public void CallOnReadEnd()
        {
            _OnReadThreadEndHandler.Invoke(this, null);
        }

        public void CallOnWriteEnd()
        {
            _OnWriteThreadEndHandler.Invoke(this, null);
        }

    }
}
