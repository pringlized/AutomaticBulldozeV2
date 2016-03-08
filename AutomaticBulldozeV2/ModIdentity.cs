#if DEBUG
using System.Reflection;
#endif
using ICities;

namespace AutomaticBulldozeV2
{
    public class ModIdentity : IUserMod
    {
        public string Name => "Automatic Bulldoze v2";

        public string Description
        {
            get
            {
                return "Automatically destroys abandoned and burned buildings"
#if DEBUG
                        + "DEBUG version " + Assembly.GetExecutingAssembly().GetName().Version
#endif
                    ;
            }
        }
    }
}
