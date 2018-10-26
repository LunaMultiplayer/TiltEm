using Kopernicus;
using Kopernicus.Configuration;

namespace TiltEm.Kopernicus
{
    [RequireConfigType(ConfigType.Node)]
    [ParserTargetExternal("Body", "Properties", "Kopernicus")]
    public class TiltReader : BaseLoader
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
