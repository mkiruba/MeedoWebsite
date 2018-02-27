using Nop.Core.Configuration;
using System;
using System.Runtime.CompilerServices;

namespace Nop.Plugin.Widgets.FacebookPixel
{
    public class FacebookPixelSettings : ISettings
    {
        public string FacebookPixelScriptFirstPart { get; set; }
        public string FacebookPixelScriptLastPart { get; set; }
        
        public FacebookPixelSettings()
        {
        }
    }
}
