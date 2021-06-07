using System.Collections.Generic;
using System.Linq;
using TerrariaUI.Base.Style;

namespace TerrariaUI.Base
{

    /// <summary>
    /// Touching and drawing settings for VisualObject.
    /// </summary>
    public class UIConfiguration
    {
        /// <summary>
        /// Touching this node would prevent touches on it or on the whole root for some time.
        /// </summary>
        public Lock Lock { get; set; }
        /// <summary>
        /// Object that should be used for checking if user can touch this node (permission string for TShock).
        /// </summary>
        public object Permission { get; set; }
        /// <summary>
        /// Collection of custom callbacks.
        /// </summary>
        public CustomCallbacks Custom { get; set; } = new CustomCallbacks();
        /// <summary>
        /// Once node is touched all future touches within the same session will pass to this node.
        /// </summary>
        public bool SessionAcquire { get; set; } = true;
        /// <summary>
        /// Allows to touch this node only if current session began with touching it.
        /// </summary>
        public bool BeginRequire { get; set; } = true;
        /// <summary>
        /// Only for nodes with SessionAcquire. Passes touches even if they are not inside of this object.
        /// </summary>
        public bool UseOutsideTouches { get; set; } = false;
        /// <summary>
        /// Touching child node would place it on top of Child array layer so that it would be drawn
        /// above other objects with the same layer and check for touching first.
        /// </summary>
        public bool Ordered { get; set; } = false;
        /// <summary>
        /// Allows to touch this node if touch.State == TouchState.Begin. True by default.
        /// </summary>
        public bool UseBegin { get; set; } = true;
        /// <summary>
        /// Allows to touch this node if touch.State == TouchState.Moving. False by default.
        /// </summary>
        public bool UseMoving { get; set; } = false;
        /// <summary>
        /// Allows to touch this node if touch.State == TouchState.End. False by default.
        /// </summary>
        public bool UseEnd { get; set; } = false;

        /// <summary>
        /// Touching and drawing settings for VisualObject.
        /// </summary>
        public UIConfiguration() { }

        /// <summary>
        /// Touching and drawing settings for VisualObject.
        /// </summary>
        public UIConfiguration(UIConfiguration configuration)
        {
            this.Lock = new Lock(configuration.Lock);
            this.Permission = configuration.Permission;
            this.Custom = new CustomCallbacks(configuration.Custom);
            this.SessionAcquire = configuration.SessionAcquire;
            this.BeginRequire = configuration.BeginRequire;
            this.UseOutsideTouches = configuration.UseOutsideTouches;
            this.Ordered = configuration.Ordered;
            this.UseBegin = configuration.UseBegin;
            this.UseMoving = configuration.UseMoving;
            this.UseEnd = configuration.UseEnd;
        }
    }
}
