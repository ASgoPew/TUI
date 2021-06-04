using System;

namespace TerrariaUI.Hooks.Args
{
    public class GetPlaceStyleArgs : EventArgs
    {
        public int Item { get; }
        public int PlaceStyle { get; set; } = -1;

        public GetPlaceStyleArgs(int item)
        {
            Item = item;
        }
    }
}
