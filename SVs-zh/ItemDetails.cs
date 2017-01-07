using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace SVs
{
    public class ItemDetails
    {
        public ItemDetails(string i, int h, VarEnum t)
        {
            this.id = i;
            this.handle = h;
            this.type = t;
        }

        //properties
        public string ID { get { return this.id;} }
        public int Handle { get { return this.handle; } }
        public VarEnum TypeItem { get { return this.type; } }

        //private
        private string id;
        private int handle;
        private VarEnum type;
    }
}
