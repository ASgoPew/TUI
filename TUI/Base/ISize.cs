using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI
{
    public interface ISize
    {
        int Value { get; }
    }
    public class Absolute : ISize
    {
        public int Value { get; }

        public Absolute(int value) =>
            Value = value;
    }
    public class Relative : ISize
    {
        public int Value { get; }

        public Relative(int value) =>
            Value = value;
    }
}
