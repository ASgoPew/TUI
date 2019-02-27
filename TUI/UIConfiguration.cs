using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI
{
    public class UIConfiguration<T> : ICloneable
        where T : VisualDOM<T>
    {
        public GridConfiguration Grid { get; set; }
        public PaddingConfig Padding { get; set; }
        public LockConfig Lock { get; set; }
        public string Permission { get; set; }
        public Func<T, T> CustomUpdate { get; set; }
        public Func<T, Touch<T>, bool> CustomCanTouch { get; set; }
        public Func<T, T> CustomApply { get; set; }

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
            UIConfiguration<T> result = MemberwiseClone() as UIConfiguration<T>;
            result.Grid = (GridConfiguration)Grid?.Clone();
            result.Lock = (LockConfig)Lock?.Clone();
            result.Permission = Permission != null ? String.Copy(Permission) : null;
            result.Padding = (PaddingConfig)Padding?.Clone();
            return result;
        }
    }

    public class UIConfiguration : UIConfiguration<VisualObject> { }
}
