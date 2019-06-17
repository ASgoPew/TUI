using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI.Base
{
    public interface IInput
    {
        object Value { get; }
    }

    public interface IInput<T>
    {
        Input<T> Input { get; }
        T GetValue();
        void SetValue(T value);
    }
}
