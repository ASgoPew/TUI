using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaUI.Widgets;

namespace TerrariaUI
{
    public class RatingApplication : Application
    {
        public RatingApplication(string name)
            : base($"{name}_rating", Generate)
        {
        }

        public static Panel Generate(string name)
        {
            return null;
        }
    }
}
