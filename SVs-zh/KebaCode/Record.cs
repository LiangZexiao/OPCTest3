using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SVs
{
    public struct Record
    {
        public Record(string i,string v,string t)
        {
            this.item = i;
            this.value = v;
            this.type = t;
        }

        public string Item { get { return this.item; } }
        public string Value { get { return this.value; } }
        public string Type { get { return this.type; } }

        private string item;
        private string value;
        private string type;
    }
}
