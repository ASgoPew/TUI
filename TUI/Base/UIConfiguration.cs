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
        public PaddingConfig Padding { get; set; }
        public LockConfig Lock { get; set; }
        // rather string for tshock or int for rank
        public object Permission { get; set; }
        public Func<VisualObject, VisualObject> CustomUpdate { get; set; }
        public Func<VisualObject, Touch, bool> CustomCanTouch { get; set; }
        public Func<VisualObject, VisualObject> CustomApply { get; set; }

        public bool Ordered { get; set; } = false;
        //public bool Orderable { get; set; } = true;
        public bool RootAcquire { get; set; } = true;
        public bool BeginRequire { get; set; } = true;
        public bool UseOutsideTouches { get; set; } = false;
        public bool UseBegin { get; set; } = true;
        public bool UseMoving { get; set; } = false;
        public bool UseEnd { get; set; } = false;

        public object Clone()
        {
            UIConfiguration result = this.MemberwiseClone() as UIConfiguration;
            result.Grid = (GridConfiguration)Grid?.Clone();
            result.Lock = (LockConfig)Lock?.Clone();
            result.Permission = Permission is String ? (Permission != null ? String.Copy((string)Permission) : null) : Permission;
            result.Padding = (PaddingConfig)Padding?.Clone();
            return result;
        }
    }
}
