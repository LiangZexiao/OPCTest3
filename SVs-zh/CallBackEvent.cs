using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SVs
{
    public class CallBackEvent : EventArgs 
    {
        public CallBackEvent(int h, object v)
        {
            handle = h;
            value = v;
        }
        //properties
        public int Handle { get { return this.handle; } }
        public object Value { get { return this.value; } }

        //private
        private int handle;
        private object value;
    }
}
