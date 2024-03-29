﻿using System;

namespace TerrariaUI.Hooks
{
    public class RemoveProviderArgs : EventArgs
    {
        public dynamic Provider { get; set; }

        public RemoveProviderArgs(dynamic provider)
        {
            Provider = provider;
        }
    }
}