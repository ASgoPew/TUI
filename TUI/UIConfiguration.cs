using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI
{
    public class UIConfiguration : ICloneable
    {
        public GridConfiguration Grid { get; set; }
        public LockConfig Lock { get; set; }
        public string Permission { get; set; }
        public PaddingConfig Padding { get; set; }

        public bool Ordered { get; set; } = false;
        public bool RootAcquire { get; set; } = true;
        public bool BeginRequire { get; set; } = false;
        public bool UseOutsideTouches { get; set; } = false;
        public bool UseBegin { get; set; } = true;
        public bool UseMoving { get; set; } = false;
        public bool UseEnd { get; set; } = false;

        public object Clone()
        {
            UIConfiguration result = MemberwiseClone() as UIConfiguration;
            result.Grid = (GridConfiguration)Grid?.Clone();
            result.Lock = (LockConfig)Lock?.Clone();
            result.Permission = Permission != null ? String.Copy(Permission) : null;
            result.Padding = (PaddingConfig)Padding?.Clone();
            return result;
        }
    }
}
