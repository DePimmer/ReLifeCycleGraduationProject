using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using System.Drawing;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System.Linq;
namespace ReLifeCycleGHPlugin
{
    //   ██████╗██╗██████╗  ██████╗██╗   ██╗██╗      █████╗ ██████╗ ██╗████████╗██╗   ██╗     
    //  ██╔════╝██║██╔══██╗██╔════╝██║   ██║██║     ██╔══██╗██╔══██╗██║╚══██╔══╝╚██╗ ██╔╝     
    //  ██║     ██║██████╔╝██║     ██║   ██║██║     ███████║██████╔╝██║   ██║    ╚████╔╝      
    //  ██║     ██║██╔══██╗██║     ██║   ██║██║     ██╔══██║██╔══██╗██║   ██║     ╚██╔╝       
    //  ╚██████╗██║██║  ██║╚██████╗╚██████╔╝███████╗██║  ██║██║  ██║██║   ██║      ██║        
    //   ╚═════╝╚═╝╚═╝  ╚═╝ ╚═════╝ ╚═════╝ ╚══════╝╚═╝  ╚═╝╚═╝  ╚═╝╚═╝   ╚═╝      ╚═╝        
    //                                                                                        
    //   █████╗ ███████╗███████╗███████╗███████╗███████╗███╗   ███╗███████╗███╗   ██╗████████╗
    //  ██╔══██╗██╔════╝██╔════╝██╔════╝██╔════╝██╔════╝████╗ ████║██╔════╝████╗  ██║╚══██╔══╝
    //  ███████║███████╗███████╗█████╗  ███████╗███████╗██╔████╔██║█████╗  ██╔██╗ ██║   ██║   
    //  ██╔══██║╚════██║╚════██║██╔══╝  ╚════██║╚════██║██║╚██╔╝██║██╔══╝  ██║╚██╗██║   ██║   
    //  ██║  ██║███████║███████║███████╗███████║███████║██║ ╚═╝ ██║███████╗██║ ╚████║   ██║   
    //  ╚═╝  ╚═╝╚══════╝╚══════╝╚══════╝╚══════╝╚══════╝╚═╝     ╚═╝╚══════╝╚═╝  ╚═══╝   ╚═╝   
    //                                                                                        

    public class CircularityAssessmentComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CircularityAssessmentComponent class.
        /// </summary>
        public CircularityAssessmentComponent()
          : base("Circularity Assessment", "C-Assessment",
              "Assess the circularity of a building",
              "ReLifeCycle", "  Assessment")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Building Data", "BD", "Data for the building. Drag the Building Data output from the Create Building component to this input", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // Add combined results output parameter
            pManager.AddGenericParameter("Total Results", "TR", "Total results for the complete building", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Detailed Results", "DR", "Detailed results on the building element level", GH_ParamAccess.tree);

            // Add circularity output parameters
            pManager.AddGenericParameter("BCI [%]", "BCI", "Building Circularity Index as percentage (%)", GH_ParamAccess.tree);
            pManager.AddGenericParameter("DP [%]", "DP", "Disassembly Potential as percentage (%)", GH_ParamAccess.tree);
            pManager.AddGenericParameter("% New input", "%NEW_IN", "Percentage of new material input (%)", GH_ParamAccess.tree);
            pManager.AddGenericParameter("% Biobased input", "%BIO_IN", "Percentage of biobased material input (%)", GH_ParamAccess.tree);
            pManager.AddGenericParameter("% Recycled input", "%REC_IN", "Percentage of recycled material input (%)", GH_ParamAccess.tree);
            pManager.AddGenericParameter("% Reused input", "%REU_IN", "Percentage of reused material input (%)", GH_ParamAccess.tree);
            pManager.AddGenericParameter("% Landfill output", "%LAN_OUT", "Percentage of material sent to landfill output (%)", GH_ParamAccess.tree);
            pManager.AddGenericParameter("% Burning output", "%BUR_OUT", "Percentage of material sent to burning output (%)", GH_ParamAccess.tree);
            pManager.AddGenericParameter("% Recycling output", "%REC_OUT", "Percentage of material sent to recycling output (%)", GH_ParamAccess.tree);
            pManager.AddGenericParameter("% Reusing output", "%REU_OUT", "Percentage of material sent to reusing output (%)", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

//////////////////////////////////////////////////////////////////////
// --- Retrieve data and create variables for data storage --- 
//////////////////////////////////////////////////////////////////////


            // Create input variables
            GH_Structure<IGH_Goo> buildingData = new GH_Structure<IGH_Goo>();
            double functionalLifespan = 0.0;
            double gfa = 0.0;

            // Retrieve data from input parameters
            if (!DA.GetDataTree(0, out buildingData)) return;

            // Create new tree "detailedResults" that will contain all detailed results on the building element level
            GH_Structure<IGH_Goo> detailedResults = new GH_Structure<IGH_Goo>();

            // Create new tree "totalResults" that will contain the total results for the complete building
            GH_Structure<IGH_Goo> totalResults = new GH_Structure<IGH_Goo>();


//////////////////////////////////////////////////////////////////////
// --- Retrieve general building information ---
//////////////////////////////////////////////////////////////////////


            // Retrieve functional lifespan
            foreach (GH_Path path in buildingData.Paths)
            {
                var items = buildingData.get_Branch(path);

                // Check if the first item in the branch is a string with value "Functional Lifespan [years]" and add this value to the functionalLifespan variable
                if (items[0] is IGH_Goo firstItemGoo && firstItemGoo.CastTo(out string firstItem) && firstItem == "Functional Lifespan [years]")
                {
                    // Retrieve the second item
                    if (items[1] is IGH_Goo secondItemGoo && secondItemGoo.CastTo(out double secondItem))
                    {
                        functionalLifespan = secondItem;
                        break;
                    }
                }
            }

            // Retrieve GFA
            foreach (GH_Path path in buildingData.Paths)
            {
                var items = buildingData.get_Branch(path);

                // Check if the first item in the branch is a string with value "GFA [m2]" and add this value to the gfa variable
                if (items[0] is IGH_Goo firstItemGoo && firstItemGoo.CastTo(out string firstItem) && firstItem == "GFA [m2]")
                {
                    // Retrieve the second item
                    if (items[1] is IGH_Goo secondItemGoo && secondItemGoo.CastTo(out double secondItem))
                    {
                        gfa = secondItem;
                        break;
                    }
                }
            }


//////////////////////////////////////////////////////////////////////
// --- Detailed circularity calculations for each building element ---
//////////////////////////////////////////////////////////////////////


            // Remove branches from buildingData where the first path index (B) is 0
            var filteredPaths = buildingData.Paths.Where(path => path.Indices[0] > 0).ToList();
            var filteredBuildingData = new GH_Structure<IGH_Goo>();

            // Iterate over the filtered paths and add the branches to the filtered structure
            foreach (var path in filteredPaths)
            {
                var branch = buildingData.get_Branch(path);
                filteredBuildingData.AppendRange(branch.Cast<IGH_Goo>(), path);
            }

            // Reset first path index {B} in {B;A} to 0
            var shiftedBuildingData = new GH_Structure<IGH_Goo>();

            // Iterate over all paths in the original buildingData
            foreach (var path in filteredBuildingData.Paths)
            {
                // Get the original branch content
                var branch = buildingData.get_Branch(path);

                // Create a new path with the first index (B) shifted back by 1
                int shiftedB = path.Indices[0] - 1;
                int A = path.Indices[1]; // Keep the second index (A) unchanged
                var newPath = new GH_Path(shiftedB, A);

                // Append the branch to the new path in the updated tree
                shiftedBuildingData.AppendRange(branch.Cast<IGH_Goo>(), newPath);
            }

            // Replace buildingData with the filtered tree
            buildingData = shiftedBuildingData;


            // Group all branches from buildingData by their main index (B) in a dictionary. So group the data for each building element
            var buildingElementGroups = buildingData.Paths.GroupBy(path => path.Indices[0]).ToDictionary(group => group.Key, group => group.ToList());

            // Set calculation results for building elements
            double totalMKI = 0.0; // Total MKI of the building
            double totalMass = 0.0; // Total mass of the building
            double totalWeightedPCI = 0.0; // Total PCI score of the builidng elements, weighted with their MKI
            double totalWeightedDP = 0.0; // Total DP score of the building elements, weighted with its MKI

            double totalWeightedNewIn = 0.0; // Total percentage of new material input of the building elements, weighted with their mass
            double totalWeightedBioIn = 0.0; // Total percentage of biobased material input of the building elements, weighted with their mass
            double totalWeightedRecIn = 0.0; // Total percentage of recycled material input of the building elements, weighted with their mass
            double totalWeightedReuIn = 0.0; // Total percentage of reused material input of the building elements, weighted with their mass

            double totalWeightedLanOut = 0.0; // Total percentage of material sent to landfill output of the building elements, weighted with their mass
            double totalWeightedBurOut = 0.0; // Total percentage of material sent to burning output of the building elements, weighted with their mass
            double totalWeightedRecOut = 0.0; // Total percentage of material sent to recycling output of the building elements, weighted with their mass
            double totalWeightedReuOut = 0.0; // Total percentage of material sent to reusing output of the building elements, weighted with their mass

            // Perform circularity calculations for building elements
            foreach (var group in buildingElementGroups)
            {
                // Set main and secondary branch indices
                int groupIndexB = group.Key;
                List<GH_Path> groupIndexA = group.Value;

                // Set intermediate calculation results for building elements
                double totalMKIElement = 0.0; // Total MKI of a building element including replacements
                double totalMassElement = 0.0; // Total mass of a building element

                double weightedPCIElement = 0.0; // PCI score of a building element, weighted with its MKI
                double weightedDPElement = 0.0; // DP score of a building element, weighted with its MKI

                double weightedNewInElement = 0.0; // Percentage of new material input of a building element, weighted with its mass
                double weightedBioInElement = 0.0; // Percentage of biobased material input of a building element, weighted with its mass
                double weightedRecInElement = 0.0; // Percentage of recycled material input of a building element, weighted with its mass
                double weightedReuInElement = 0.0; // Percentage of reused material input of a building element, weighted with its mass

                double weightedLanOutElement = 0.0; // Percentage of material sent to landfill output of a building element, weighted with its mass
                double weightedBurOutElement = 0.0; // Percentage of material sent to burning output of a building element, weighted with its mass
                double weightedRecOutElement = 0.0; // Percentage of material sent to recycling output of a building element, weighted with its mass
                double weightedReuOutElement = 0.0; // Percentage of material sent to reusing output of a building element, weighted with its mass

                // Calculate total MKI of each building element
                foreach (GH_Path subPath in groupIndexA)
                {
                    var branch = buildingData.get_Branch(subPath);

                    if (branch.Count == 2 && branch[0] is GH_String ghString && branch[1] is GH_Number ghNumber)
                    {
                        string description = ghString.Value;
                        double value = ghNumber.Value;

                        if (description.StartsWith("MKI"))
                        {
                            totalMKIElement += value;
                        }
                        if (description.StartsWith("Total Mass"))
                        {
                            totalMassElement = value;
                        }
                    }
                }

                // Calculate weighted circularity indicators for each building element
                foreach (GH_Path subPath in groupIndexA)
                {
                    var branch = buildingData.get_Branch(subPath);

                    if (branch.Count == 2 && branch[0] is GH_String ghString && branch[1] is GH_Integer ghInteger)
                    {
                        string description = ghString.Value;
                        double value = ghInteger.Value;

                        if (description == "PCI [%]")
                        {
                            weightedPCIElement = totalMKIElement * value;
                        }
                        if (description == "DP [%]")
                        {
                            weightedDPElement = totalMKIElement * value;
                        }
                        if (description == "% New")
                        {
                            weightedNewInElement = totalMassElement * value;
                        }
                        if (description == "% Biobased")
                        {
                            weightedBioInElement = totalMassElement * value;
                        }
                        if (description == "% Recycled")
                        {
                            weightedRecInElement = totalMassElement * value;
                        }
                        if (description == "% Reused")
                        {
                            weightedReuInElement = totalMassElement * value;
                        }
                        if (description == "% Landfill")
                        {
                            weightedLanOutElement = totalMassElement * value;
                        }
                        if (description == "% Burning")
                        {
                            weightedBurOutElement = totalMassElement * value;
                        }
                        if (description == "% Recycling")
                        {
                            weightedRecOutElement = totalMassElement * value;
                        }
                        if (description == "% Reusing")
                        {
                            weightedReuOutElement = totalMassElement * value;
                        }
                    }
                }

                // Calculate total weighted building element circularity results
                totalMKI += totalMKIElement;
                totalMass += totalMassElement;
                totalWeightedPCI += weightedPCIElement;
                totalWeightedDP += weightedDPElement;

                totalWeightedNewIn += weightedNewInElement;
                totalWeightedBioIn += weightedBioInElement;
                totalWeightedRecIn += weightedRecInElement;
                totalWeightedReuIn += weightedReuInElement;

                totalWeightedLanOut += weightedLanOutElement;
                totalWeightedBurOut += weightedBurOutElement;
                totalWeightedRecOut += weightedRecOutElement;
                totalWeightedReuOut += weightedReuOutElement;


//////////////////////////////////////////////////////////////////////
// --- Add building element circularity results to detailedResults tree ---
//////////////////////////////////////////////////////////////////////


                // Fill detailedResults tree with data from buildingData tree
                foreach (GH_Path subPath in groupIndexA)
                {
                    // Add general building element information
                    if (subPath.Indices[1] >= 0 && subPath.Indices[1] <= 13)
                    {
                        var branch = buildingData.get_Branch(subPath);
                        detailedResults.AppendRange(branch.Cast<IGH_Goo>(), subPath);
                    }

                    // Add building element ciruclarity results
                    if (subPath.Indices[1] >= 41 && subPath.Indices[1] <= 51)
                    {
                        var branch = buildingData.get_Branch(subPath);
                        detailedResults.AppendRange(branch.Cast<IGH_Goo>(), subPath);
                    }
                }

                // Get the existing paths from the detailedResults tree
                var existingPaths = detailedResults.Paths.Where(path => path.Indices[0] == groupIndexB);

                // Add total MKI for building element as an extra branch
                int totalMKIElementIndexA;
                totalMKIElementIndexA = existingPaths.Max(path => path.Indices[1]) + 1; // Find maximum path index and add 1 so a new branch is added to the bottom of each main data branch
                GH_Path totalMKIElementPath = new GH_Path(groupIndexB, totalMKIElementIndexA); // Create the new path for the results

                List<IGH_Goo> totalMKIElementData = new List<IGH_Goo>
                {
                    new GH_String("Total MKI Building Element [€]"),
                    new GH_Number(totalMKIElement)
                };

                detailedResults.AppendRange(totalMKIElementData, totalMKIElementPath);
            }


//////////////////////////////////////////////////////////////////////
// --- Circularity calculations for the complete building ---
//////////////////////////////////////////////////////////////////////


            // Calculate total circularity indicator scores for the complete building
            double totalBCI = totalWeightedPCI / totalMKI; ; // Total BCI score of the building
            double totalDP = totalWeightedDP / totalMKI ; // Total DP score of the building

            double totalNewIn = totalWeightedNewIn / totalMass; // Total percentage of new material input
            double totalBioIn = totalWeightedBioIn / totalMass; // Total percentage of biobased material input
            double totalRecIn = totalWeightedRecIn / totalMass; // Total percentage of recycled material input
            double totalReuIn = totalWeightedReuIn / totalMass; // Total percentage of reused material input

            double totalLanOut = totalWeightedLanOut / totalMass; // Total percentage of material sent to landfill output
            double totalBurOut = totalWeightedBurOut / totalMass; // Total percentage of material sent to burning output
            double totalRecOut = totalWeightedRecOut / totalMass; // Total percentage of material sent to recycling output
            double totalReuOut = totalWeightedReuOut / totalMass; // Total percentage of material sent to reusing output


//////////////////////////////////////////////////////////////////////
// --- Add results to totalResults tree ---
//////////////////////////////////////////////////////////////////////


            // Add BCI
            totalResults.Append(new GH_String("BCI [%]"), new GH_Path(0));
            totalResults.Append(new GH_Number(totalBCI), new GH_Path(0));

            // Add DP
            totalResults.Append(new GH_String("DP [%]"), new GH_Path(1));
            totalResults.Append(new GH_Number(totalDP), new GH_Path(1));

            // Add % New Input
            totalResults.Append(new GH_String("New Input [%]"), new GH_Path(2));
            totalResults.Append(new GH_Number(totalNewIn), new GH_Path(2));

            // Add % Biobased Input
            totalResults.Append(new GH_String("Biobased Input [%]"), new GH_Path(3));
            totalResults.Append(new GH_Number(totalBioIn), new GH_Path(3));

            // Add % Recycled Input
            totalResults.Append(new GH_String("Recycled Input [%]"), new GH_Path(4));
            totalResults.Append(new GH_Number(totalRecIn), new GH_Path(4));

            // Add % Reused Input
            totalResults.Append(new GH_String("Reused Input [%]"), new GH_Path(5));
            totalResults.Append(new GH_Number(totalReuIn), new GH_Path(5));

            // Add % Landfill Output
            totalResults.Append(new GH_String("Landfill Output [%]"), new GH_Path(6));
            totalResults.Append(new GH_Number(totalLanOut), new GH_Path(6));

            // Add % Burning Output
            totalResults.Append(new GH_String("Burning Output [%]"), new GH_Path(7));
            totalResults.Append(new GH_Number(totalBurOut), new GH_Path(7));

            // Add % Recycling Output
            totalResults.Append(new GH_String("Recycling Output [%]"), new GH_Path(8));
            totalResults.Append(new GH_Number(totalRecOut), new GH_Path(8));

            // Add % Reusing Output
            totalResults.Append(new GH_String("Reusing Output [%]"), new GH_Path(9));
            totalResults.Append(new GH_Number(totalReuOut), new GH_Path(9));


//////////////////////////////////////////////////////////////////////
// --- Set ouptut parameters ---
//////////////////////////////////////////////////////////////////////


            DA.SetDataTree(0, totalResults);
            DA.SetDataTree(1, detailedResults);
            DA.SetData(2, totalBCI);
            DA.SetData(3, totalDP);

            DA.SetData(4, totalNewIn);
            DA.SetData(5, totalBioIn);
            DA.SetData(6, totalRecIn);
            DA.SetData(7, totalReuIn);

            DA.SetData(8, totalLanOut);
            DA.SetData(9, totalBurOut);
            DA.SetData(10, totalRecOut);
            DA.SetData(11, totalReuOut);
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
                using (var stream = typeof(CreateBuildingElementComponent).Assembly.GetManifestResourceStream("ReLifeCycleGHPlugin.Resources.CircularityAssessmentIcon.png"))
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
            get { return new Guid("C821CF5D-6CC6-4602-BA7B-8A5BD387CD08"); }
        }
    }
}