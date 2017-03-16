using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SVs
{
    public struct User
    {
        public User(int i, string n)
        {
            this.id = i;
            this.name = n;
        }

        public int ID { get { return this.id; } }
        public string Name { get { return this.name; } }

        int id;
        string name;
    }
}
