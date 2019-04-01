using System;

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

        public bool RootAcquire { get; set; } = true;
        public bool BeginRequire { get; set; } = true;
        public bool UseOutsideTouches { get; set; } = false;
        public bool Ordered { get; set; } = false;
        public bool UseBegin { get; set; } = true;
        public bool UseMoving { get; set; } = false;
        public bool UseEnd { get; set; } = false;
        //public bool Orderable { get; set; } = true;

        /*public UIConfiguration(bool rootAcquire = true, bool beginRequire = true, bool useOutsideTouches = false, bool ordered = false, bool useBegin = true, bool useMoving = false, bool useEnd = false, GridConfiguration gridConfig = null, PaddingConfig paddingConfig = null, LockConfig lockConfig = null)
        {
            RootAcquire = rootAcquire;
            BeginRequire = beginRequire;
            UseOutsideTouches = useOutsideTouches;
            Ordered = ordered;
            UseBegin = useBegin;
            UseMoving = useMoving;
            UseEnd = useEnd;
            Grid = gridConfig;
            Padding = paddingConfig;
            Lock = lockConfig;
        }*/

        public object Clone()
        {
            UIConfiguration result = this.MemberwiseClone() as UIConfiguration;
            result.Grid = Grid?.Clone() as GridConfiguration;
            result.Lock = Lock?.Clone() as LockConfig;
            result.Permission = Permission is String ? (Permission != null ? String.Copy((string)Permission) : null) : Permission;
            result.Padding = Padding?.Clone() as PaddingConfig;
            return result;
        }
    }
}
