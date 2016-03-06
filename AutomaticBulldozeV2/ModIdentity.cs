using System.Reflection;
using ICities;

namespace AutomaticBulldozeV2
{
    public class ModIdentity : IUserMod
    {
        public string Name => "Automatic Bulldoze V2";

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
