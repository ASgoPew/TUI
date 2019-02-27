using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI
{
    public class TileProvider
    {
        ITileCollection _Provider;
        int _X;
        int _Y;

        public TileProvider(ITileCollection provider, int x, int y)
        {
            _Provider = provider;
            _X = x;
            _Y = y;
        }

        public ITile this[int x, int y]
        {
            get => _Provider[_X + x, _Y + y];
        }
    }
}
