using Grasshopper.Kernel;
using System;
using System.Drawing;
using System.Reflection;

namespace ReLifeCycleGHPlugin
{
    public class ReLifeCycleGHPluginInfo : GH_AssemblyInfo
    {
        public override string Name => "ReLifeCycle";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => GetIcon();

        private Bitmap GetIcon()
        {
            string resourceName = "ReLifeCycleGHPlugin.Resources.ReLifeCycleLogoIcon.png";
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                return stream != null ? new Bitmap(stream) : null;
            }
        }

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "Grasshopper plugin for responsible material use assessment (environmental impact, circularity and financial impact)";

        public override Guid Id => new Guid("e6ef5a70-1e7d-44a9-89bb-7593586ca194");

        //Return a string identifying you or your company.
        public override string AuthorName => "Pim van Rijsbergen";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "pvanrijsbergen@gmail.com";

        //Return a string representing the version.  This returns the same version as the assembly.
        public override string AssemblyVersion => GetType().Assembly.GetName().Version.ToString();
    }
}