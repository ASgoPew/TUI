using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI.Base
{
    public class ChildCollection<T> : List<T>
    {
        public Alignment? Alignment { get; set; }
        public Direction? Direction { get; set; }
        public Side? Side { get; set; }
    }
}
