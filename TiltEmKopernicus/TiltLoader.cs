﻿using Kopernicus;
using Kopernicus.Configuration;

namespace TiltEmKopernicus
{
    [RequireConfigType(ConfigType.Node)]
    [ParserTargetExternal("Body", "Properties", "Kopernicus")]
    public class TiltLoader : BaseLoader
    {
        [ParserTarget("tiltx")]
        public NumericParser<double> TiltX
        {
            get => generatedBody.Get("tiltx", 0f);
            set => generatedBody.Set("tiltx", value.Value);
        }

        [ParserTarget("tiltz")]
        public NumericParser<double> TiltZ
        {
            get => generatedBody.Get("tiltz", 0f);
            set => generatedBody.Set("tiltz", value.Value);
        }
    }
}
