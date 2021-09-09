using System;

namespace JBAUtils
{
    public class Table : Attribute
    {
        public int TableIndex { get; set; }
        public Table(int tableIndex = 0)
        {
            TableIndex = tableIndex;
        }
    }
}
