using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using System.Drawing;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System.Linq;

namespace ReLifeCycleGHPlugin
{
    //   ██████╗██████╗ ███████╗ █████╗ ████████╗███████╗    ██████╗ ██╗   ██╗██╗██╗     ██████╗ ██╗███╗   ██╗ ██████╗ 
    //  ██╔════╝██╔══██╗██╔════╝██╔══██╗╚══██╔══╝██╔════╝    ██╔══██╗██║   ██║██║██║     ██╔══██╗██║████╗  ██║██╔════╝ 
    //  ██║     ██████╔╝█████╗  ███████║   ██║   █████╗      ██████╔╝██║   ██║██║██║     ██║  ██║██║██╔██╗ ██║██║  ███╗
    //  ██║     ██╔══██╗██╔══╝  ██╔══██║   ██║   ██╔══╝      ██╔══██╗██║   ██║██║██║     ██║  ██║██║██║╚██╗██║██║   ██║
    //  ╚██████╗██║  ██║███████╗██║  ██║   ██║   ███████╗    ██████╔╝╚██████╔╝██║███████╗██████╔╝██║██║ ╚████║╚██████╔╝
    //   ╚═════╝╚═╝  ╚═╝╚══════╝╚═╝  ╚═╝   ╚═╝   ╚══════╝    ╚═════╝  ╚═════╝ ╚═╝╚══════╝╚═════╝ ╚═╝╚═╝  ╚═══╝ ╚═════╝ 
    //  

    public class CreateBuildingComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreateBuildingComponent class.
        /// </summary>
        public CreateBuildingComponent()
          : base("Create Building", "CreateB",
              "Create building from building elements",
              "ReLifeCycle", "   Building")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Custom name for the building", GH_ParamAccess.item, "");
            pManager.AddIntegerParameter("Building Lifespan [years]", "L", "Functional lifespan of the building", GH_ParamAccess.item, 75);

            // Add building function parameter with a set of fixed options
            var buildingFunction = new Grasshopper.Kernel.Parameters.Param_Integer
            {
                Name = "Building Function",
                NickName = "F",
                Description = "Function of the building. Possible values: " +
                "\n" +
                "\n 0 = Residential function - ground based " +
                "\n 1 = Residential function - stacked " +
                "\n 2 = Office function " +
                "\n 3 = Educational function" +
                "\n 4 = Healthcare function",
                Access = GH_ParamAccess.item
            };

            buildingFunction.AddNamedValue("Residential function - ground based", 0);
            buildingFunction.AddNamedValue("Residential function - stacked", 1);
            buildingFunction.AddNamedValue("Office function", 2);
            buildingFunction.AddNamedValue("Educational function", 3);
            buildingFunction.AddNamedValue("Healthcare function", 4);

            // Add the building function parameter to the parameter manager
            pManager.AddParameter(buildingFunction);

            // Set the default value
            buildingFunction.SetPersistentData(0); // Default to "Residential function - ground based"

            pManager.AddNumberParameter("Gross Floor Area [m2]", "GFA", "Total floor area of the building in m2", GH_ParamAccess.item);
            pManager.AddGenericParameter("Building Element Data", "BED", "Data for the building elements. Drag the Buidling Element Data output from the Building Element components to this input", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Building Data", "BD", "Data for the complete building", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Building Information", "BI", "General building information", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Building Materials", "BM", "List of materials per building element", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Building Geometry", "BG", "List of geometry per building element", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Create input variables
            string name = "";
            GH_Structure<IGH_Goo> buildingElementData = new GH_Structure<IGH_Goo>();
            int functionalLifespan = 75;
            int buildingFunction = 0;
            double gfa = 0.0;

            // Retrieve data from input parameters
            if (!DA.GetData(0, ref name)) return;
            if (!DA.GetData(1, ref functionalLifespan)) return;
            if (!DA.GetData(2, ref buildingFunction)) return;
            if (!DA.GetData(3, ref gfa)) return;
            if (!DA.GetDataTree(4, out buildingElementData)) return;

            // Combine multiple building element data trees into one tree "buildingData" with structure {B;A} where B is the index per building element and A the index per data element
            // buildingData will be used to retrieve data for performing the environmental impact calculations
            GH_Structure<IGH_Goo> buildingData = new GH_Structure<IGH_Goo>();
            int BDStartIndexB = 0;

            foreach (IGH_Param input in Params.Input[4].Sources)
            {
                // Retrieve building element data trees from building element data input
                GH_Structure<IGH_Goo> buildingElementTree = input.VolatileData as GH_Structure<IGH_Goo>;

                if (buildingElementTree != null)
                {
                    foreach (GH_Path path in buildingElementTree.Paths)
                    {
                        // Create a new path for the output tree with structure {B;A}
                        GH_Path newPath = new GH_Path(BDStartIndexB, path.Indices[0]);

                        // Add branches to the buildingData tree
                        buildingData.AppendRange(buildingElementTree.get_Branch(path).Cast<IGH_Goo>(), newPath);
                    }
                    BDStartIndexB++;
                }
            }


//////////////////////////////////////////////////////////////////////
// --- Retrieve building materials and place in a tree --
//////////////////////////////////////////////////////////////////////


            // Create buildingMaterials tree to include the material and nlsfb class per building element
            GH_Structure<IGH_Goo> buildingMaterials = new GH_Structure<IGH_Goo>();

            // Iterate over all branches in buildingData
            foreach (GH_Path path in buildingData.Paths)
            {
                // Get the list of items in this branch
                var items = buildingData.get_Branch(path);

                // Place material information in tree branches
                if (items[0] is GH_String firstItem && items[1] is GH_String secondItem)
                {
                    // Create a simplified path with a single index {B}
                    GH_Path buildingMaterialsPath = new GH_Path(path.Indices[0]);

                    // Initialize the branch if it doesn't exist
                    if (!buildingMaterials.PathExists(buildingMaterialsPath))
                    {
                        buildingMaterials.AppendRange(new IGH_Goo[4], buildingMaterialsPath); // Create a branch with 4 placeholders
                    }

                    // Place the item at the correct index in the branch
                    var branch = buildingMaterials.get_Branch(buildingMaterialsPath);
                    if (firstItem.Value == "NL/SfB Code")
                    {
                        branch[0] = secondItem; // Index 0 for "NL/SfB Code"
                    }
                    else if (firstItem.Value == "NL/SfB Name")
                    {
                        branch[1] = secondItem; // Index 1 for "NL/SfB Name"
                    }
                    else if (firstItem.Value == "Name")
                    {
                        branch[2] = secondItem; // Index 2 for "Name"
                    }
                    else if (firstItem.Value == "Product Description")
                    {
                        branch[3] = secondItem; // Index 3 for "Product Description"
                    }
                }
            }


//////////////////////////////////////////////////////////////////////
// --- Retrieve building geometry and place in a tree --
//////////////////////////////////////////////////////////////////////


            // Create buildingGeometry tree to include the geometry per building element
            GH_Structure<IGH_Goo> buildingGeometry = new GH_Structure<IGH_Goo>();

            // Iterate through all branches in buildingData
            foreach (GH_Path path in buildingData.Paths)
            {
                // Get the list of items in the current branch
                var items = buildingData.get_Branch(path);

                // Create a simplified path with a single index {B}
                GH_Path buildingGeometryPath = new GH_Path(path.Indices[0]);

                // Ensure the branch has at least one item
                if (items.Count > 0 && items[0] is GH_String firstItem && firstItem.Value == "Geometry")
                {
                    // Convert items to IEnumerable<IGH_Goo>
                    var geometryItems = items.Cast<IGH_Goo>().Skip(1).Where(item => item is GH_Mesh).ToList();

                    // Append the geometry items to the new tree
                    buildingGeometry.AppendRange(geometryItems, buildingGeometryPath);
                }
            }


//////////////////////////////////////////////////////////////////////
// --- Calculate replacements and new MKI B4 and GWP B4 ---
//////////////////////////////////////////////////////////////////////


            // Group all branches from buildingData by their main index (B) in a dictionary. So group the data for each building element
            var buildingElementGroups = buildingData.Paths.GroupBy(path => path.Indices[0]).ToDictionary(group => group.Key, group => group.ToList());

            // Iterate over all main buildingElementGroups so each calculation is performed for each individual building element
            foreach (var group in buildingElementGroups)
            {
                // Set main and secondary branch indices
                int groupIndexB = group.Key;
                List<GH_Path> groupIndexA = group.Value;

                // Set variable for number of replacements of a building element
                double nReplacements = 0.0;

                // Set variables for calculating new MKI B4 and GWP B4 that include impact caused by replacements
                // Set calculation result variables for building elements
                double iTotalMKIElement = 0.0; // Initial total MKI of a building element
                double iTotalGWPElement = 0.0; // Initial total GWP of a building element

                double newMKIB4 = 0.0; // MKI B4 including replacements
                double newGWPB4 = 0.0; // MK B4 including replacements

                // Calculate initial total MKI and GWP for each building element
                foreach (GH_Path subPath in groupIndexA)
                {
                    var branch = buildingData.get_Branch(subPath);

                    if (branch.Count == 2 && branch[0] is GH_String ghString && branch[1] is GH_Number ghNumber)
                    {
                        string description = ghString.Value;
                        double value = ghNumber.Value;

                        if (description.StartsWith("MKI"))
                        {
                            iTotalMKIElement += value;
                        }
                        else if (description.StartsWith("GWP"))
                        {
                            iTotalGWPElement += value;
                        }
                    }
                }

                // Calculate number of replacements of each building element and add this impact to MKI B4 and GWP B4
                foreach (GH_Path subPath in groupIndexA)
                {
                    var branch = buildingData.get_Branch(subPath);
                    if (branch.Count == 2 && branch[0] is GH_String ghString && branch[1] is GH_Number ghNumber)
                    {
                        string description = ghString.Value;
                        double value = ghNumber.Value;

                        if (description == "Technical Lifespan [years]" && value > 0)
                        {
                            nReplacements = Math.Max(0, functionalLifespan / value - 1);
                        }

                        if (description == "MKI B4 [€]")
                        {
                            newMKIB4 = value + nReplacements * iTotalMKIElement;
                            branch[1] = new GH_Number(newMKIB4);
                        }
                        else if (description == "GWP B4 [kg CO2 eq.]")
                        {
                            newGWPB4 = value + nReplacements * iTotalGWPElement;
                            branch[1] = new GH_Number(newGWPB4);
                        }
                    }
                }
            }


//////////////////////////////////////////////////////////////////////
// --- Add results to output parameters ---
//////////////////////////////////////////////////////////////////////


            // Create new buildingData tree to include general building information
            GH_Structure<IGH_Goo> newBuildingData = new GH_Structure<IGH_Goo>();

            // Create buildingInformation tree to include general building information
            GH_Structure<IGH_Goo> buildingInformation = new GH_Structure<IGH_Goo>();

            // If "Name" parameter input is empty, set the default value to "My Building"
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "My Building";
            }

            // Add building name to buildingInformation
            buildingInformation.Append(new GH_String("Building Name"), new GH_Path(0, 0));
            buildingInformation.Append(new GH_String(name), new GH_Path(0, 0));

            // Add functional lifespan to buildingInformation
            buildingInformation.Append(new GH_String("Functional Lifespan [years]"), new GH_Path(0,1));
            buildingInformation.Append(new GH_Number(functionalLifespan), new GH_Path(0,1));

            // Add functional lifespan to newBuildingData
            newBuildingData.Append(new GH_String("Functional Lifespan [years]"), new GH_Path(0, 0));
            newBuildingData.Append(new GH_Number(functionalLifespan), new GH_Path(0, 0));


            // Add building function to buildingInformation
            Dictionary<int, string> buildingFunctionMapping = new Dictionary<int, string> // Create dictionary for mapping building function integers to string values
            {
                {0, "Residential function - ground based"},
                {1, "Residential function - stacked"},
                {2, "Office function"},
                {3, "Educational function"},
                {4, "Healthcare function"}
            };
            string buildingFunctionName = buildingFunctionMapping.ContainsKey(buildingFunction)
                ? buildingFunctionMapping[buildingFunction]
                : "Unknown function";
            buildingInformation.Append(new GH_String("Building Function"), new GH_Path(0, 2));
            buildingInformation.Append(new GH_String(buildingFunctionName), new GH_Path(0, 2));

            // Add GFA to buildingInformation
            buildingInformation.Append(new GH_String("GFA [m2]"), new GH_Path(0, 3));
            buildingInformation.Append(new GH_Number(gfa), new GH_Path(0, 3));

            // Add GFA to newBuildingData
            newBuildingData.Append(new GH_String("GFA [m2]"), new GH_Path(0, 1));
            newBuildingData.Append(new GH_Number(gfa), new GH_Path(0, 1));

            // Shift all paths {B} in {B;A} so general building information can be placed in branch {0;A}
            foreach (GH_Path oldPath in buildingData.Paths)
            {
                // Shift paths
                int newIndex = oldPath.Indices[0] + 1;
                GH_Path newPath = new GH_Path(newIndex, oldPath.Indices[1]);
                // Place old buildingData in newBuildingData
                newBuildingData.AppendRange(buildingData.get_Branch(oldPath).Cast<IGH_Goo>(), newPath);
            }


//////////////////////////////////////////////////////////////////////
// --- Set ouptut parameters ---
//////////////////////////////////////////////////////////////////////


            DA.SetDataTree(0, newBuildingData);
            DA.SetDataTree(1, buildingInformation);
            DA.SetDataTree(2, buildingMaterials);
            DA.SetDataTree(3, buildingGeometry);
        }

        /// <summary>
        /// The Exposure property controls where in the panel a component icon 
        /// will appear. There are seven possible locations (primary to septenary), 
        /// each of which can be combined with the GH_Exposure.obscure flag, which 
        /// ensures the component will only be visible on panel dropdowns.
        /// </summary>
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // Load the embedded icon
                using (var stream = typeof(CreateBuildingElementComponent).Assembly.GetManifestResourceStream("ReLifeCycleGHPlugin.Resources.CreateBuildingIcon.png"))
                {
                    if (stream != null)
                    {
                        return new Bitmap(stream);
                    }
                }

                // Fallback if resource is not found
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("EA05FF42-C476-4A11-B175-4CC9686B7BF8"); }
        }
    }
}