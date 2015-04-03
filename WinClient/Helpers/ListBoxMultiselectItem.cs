using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Relational.Octapus.WinClient.Helpers
{
    public class ListBoxMultiselectItem<T>
    {

        public ListBoxMultiselectItem(T value)
        {
            this.Value = value;
        }

        public bool IsSelected { get; set; }
        public T Value { get; set; }
    }
}
