using Kopernicus;
using Kopernicus.Configuration;

namespace TiltEmKopernicus
{
    [RequireConfigType(ConfigType.Node)]
    [ParserTargetExternal("Body", "Properties", "Kopernicus")]
    public class TiltLoader : BaseLoader
    {
        [ParserTarget("tilt")]
        public NumericParser<float> Tilt
        {
            get => generatedBody.Get("tilt", 0f);
            set => generatedBody.Set("tilt", value.Value);
        }
    }
}
