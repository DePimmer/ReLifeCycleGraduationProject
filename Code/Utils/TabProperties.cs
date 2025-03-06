using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;

namespace ReLifeCycleGHPlugin.Utils
{
    public class TabProperties : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            var server = Grasshopper.Instances.ComponentServer;

            server.AddCategoryShortName("ReLifeCycle", "RLC");
            server.AddCategorySymbolName("ReLifeCycle", 'R');

            // Load the embedded icon for the Grasshopper ribbon
            Bitmap icon = GetEmbeddedIcon("ReLifeCycleGHPlugin.Resources.ReLifeCycleLogoIcon.png");
            if (icon != null)
            {
                server.AddCategoryIcon("ReLifeCycle", icon);
            }

            return GH_LoadingInstruction.Proceed;
        }

        private Bitmap GetEmbeddedIcon(string resourcePath)
        {
            // Load the embedded icon from the assembly resources
            using (var stream = typeof(TabProperties).Assembly.GetManifestResourceStream(resourcePath))
            {
                if (stream != null)
                {
                    return new Bitmap(stream);
                }
            }

            // Return null if the resource cannot be found
            return null;
        }
    }
}
