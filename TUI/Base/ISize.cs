using System;

namespace TUI.Base
{
    public interface ISize
    {
        int Value { get; }

        bool IsAbsolute { get; }
        bool IsRelative { get; }
    }
    public class Absolute : ISize
    {
        int InternalValue { get; }

        public int Value => InternalValue;
        public bool IsAbsolute => true;
        public bool IsRelative => false;

        public Absolute(int value)
        {
            if (value < 0)
                throw new ArgumentException("Negative absolute size in grid");

            InternalValue = value;
        }
    }
    public class Relative : ISize
    {
        int InternalValue { get; }
        public bool IsAbsolute => false;
        public bool IsRelative => true;

        public int Value => InternalValue - 1000000;

        public Relative(int value)
        {
            if (value < 0)
                throw new ArgumentException("Negative relative size in grid");
            else if (value > 100)
                throw new ArgumentException("Relative size more than 100 in grid");

            InternalValue = value + 1000000;
        }
    }
}
