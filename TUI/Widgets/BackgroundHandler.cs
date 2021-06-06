using System;
using System.Collections.Generic;
using TerrariaUI.Base;

namespace TerrariaUI.Widgets
{
    public class BackgroundHandler : RootVisualObject
    {
        private int _Layer;
        public override int Layer => _Layer;
        public override bool Orderable => false;

        public BackgroundHandler(string name, int layer = Int32.MaxValue)
            : base(name, 0, 0, 0, 0, provider: new MainTileProvider(0))
        {
            Style.FixedSize = false;
            _Layer = layer;
        }

        protected override (int, int) GetSizeNative() => (TUI.MaxTilesX, TUI.MaxTilesY);

        public override dynamic Tile(int x, int y) => null;

        protected override void ApplyThisNative() { }

        public override VisualObject Draw(int dx = 0, int dy = 0, int width = -1, int height = -1,
            HashSet<int> targetPlayers = null, bool? drawWithSection = null, bool? frameSection = null) => this;
    }
}
