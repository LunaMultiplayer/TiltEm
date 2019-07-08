using Kopernicus;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.Configuration.Parsing;

namespace TiltEmKopernicus
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
