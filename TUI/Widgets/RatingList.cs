﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaUI.Base;

namespace TerrariaUI.Widgets
{
    public class RatingList : VisualContainer
    {
        public RatingList(int x, int y, int width, int height, string name, ContainerStyle style = null)
            : base(x, y, width, height, null, style)
        {
            Name = name;


        }

        protected override void DBReadNative(BinaryReader br)
        {
            
        }
    }
}
