using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using System.Drawing;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System.Linq;

namespace ReLifeCycleGHPlugin
{
    //  ███████╗██╗███╗   ██╗ █████╗ ███╗   ██╗ ██████╗██╗ █████╗ ██╗                         
    //  ██╔════╝██║████╗  ██║██╔══██╗████╗  ██║██╔════╝██║██╔══██╗██║                         
    //  █████╗  ██║██╔██╗ ██║███████║██╔██╗ ██║██║     ██║███████║██║                         
    //  ██╔══╝  ██║██║╚██╗██║██╔══██║██║╚██╗██║██║     ██║██╔══██║██║                         
    //  ██║     ██║██║ ╚████║██║  ██║██║ ╚████║╚██████╗██║██║  ██║███████╗                    
    //  ╚═╝     ╚═╝╚═╝  ╚═══╝╚═╝  ╚═╝╚═╝  ╚═══╝ ╚═════╝╚═╝╚═╝  ╚═╝╚══════╝                    
    //                                                                                        
    //  ██╗███╗   ███╗██████╗  █████╗  ██████╗████████╗                                       
    //  ██║████╗ ████║██╔══██╗██╔══██╗██╔════╝╚══██╔══╝                                       
    //  ██║██╔████╔██║██████╔╝███████║██║        ██║                                          
    //  ██║██║╚██╔╝██║██╔═══╝ ██╔══██║██║        ██║                                          
    //  ██║██║ ╚═╝ ██║██║     ██║  ██║╚██████╗   ██║                                          
    //  ╚═╝╚═╝     ╚═╝╚═╝     ╚═╝  ╚═╝ ╚═════╝   ╚═╝                                          
    //                                                                                        
    //   █████╗ ███████╗███████╗███████╗███████╗███████╗███╗   ███╗███████╗███╗   ██╗████████╗
    //  ██╔══██╗██╔════╝██╔════╝██╔════╝██╔════╝██╔════╝████╗ ████║██╔════╝████╗  ██║╚══██╔══╝
    //  ███████║███████╗███████╗█████╗  ███████╗███████╗██╔████╔██║█████╗  ██╔██╗ ██║   ██║   
    //  ██╔══██║╚════██║╚════██║██╔══╝  ╚════██║╚════██║██║╚██╔╝██║██╔══╝  ██║╚██╗██║   ██║   
    //  ██║  ██║███████║███████║███████╗███████║███████║██║ ╚═╝ ██║███████╗██║ ╚████║   ██║   
    //  ╚═╝  ╚═╝╚══════╝╚══════╝╚══════╝╚══════╝╚══════╝╚═╝     ╚═╝╚══════╝╚═╝  ╚═══╝   ╚═╝   
    //                                                                                        

    public class FinancialImpactAssessmentComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FinancialImpactAssessmentComponent class.
        /// </summary>
        public FinancialImpactAssessmentComponent()
          : base("Financial Impact Assessment", "F-Assessment",
              "Assess the financial impact of a building",
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

            // Add financial impact output parameters
            pManager.AddGenericParameter("Direct costs [€]", "€D", "Direct costs (material + labour costs) in Euros (€)", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Material costs [€]", "€M", "Material costs in Euros (€)", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Labour costs [€]", "€L", "Labour costs in Euros (€)", GH_ParamAccess.tree);
            pManager.AddGenericParameter("True price [€]", "€T", "True price (direct costs + total MKI in Euros (€)", GH_ParamAccess.tree);
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

                // Check if the first item in the branch is a string with value "Functional Lifespan [years]" and add value to the functionalLifespan variable
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

                // Check if the first item in the branch is a string with value "GFA [m2]" and add value to the gfa variable
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
// --- Detailed financial impact calculations for each building element ---
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

            // Iterate over all main buildingElementGroups so each calculation is performed for each individual building element
            foreach (var group in buildingElementGroups)
            {
                // Set main and secondary branch indices
                int groupIndexB = group.Key;
                List<GH_Path> groupIndexA = group.Value;

                // Set calculation result variables for building elements
                double directCostsElement = 0.0; // Direct costs of building element
                double materialCostsElement = 0.0; //  Material costs of building element
                double labourCostsElement = 0.0; //  Labour costs of building element
                double truePriceElement = 0.0; // True price of building element

                // Set intermediate calculation results for building elements
                double totalMKIElement = 0.0; // Total MKI of a building element including replacements


                // Calculate total MKI for each building element including replacements
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
                    }
                }

                // Calculate material costs for each building element
                foreach (GH_Path subPath in groupIndexA)
                {
                    var branch = buildingData.get_Branch(subPath);

                    if (branch.Count == 2 && branch[0] is GH_String ghString && branch[1] is GH_Number ghNumber)
                    {
                        string description = ghString.Value;
                        double value = ghNumber.Value;

                        if (description == "Material Costs [€]")
                        {
                            materialCostsElement = value;
                        }
                    }
                }

                // Calculate labour costs for each building element
                foreach (GH_Path subPath in groupIndexA)
                {
                    var branch = buildingData.get_Branch(subPath);

                    if (branch.Count == 2 && branch[0] is GH_String ghString && branch[1] is GH_Number ghNumber)
                    {
                        string description = ghString.Value;
                        double value = ghNumber.Value;

                        if (description == "Labour Costs [€]")
                        {
                            labourCostsElement = value;
                        }
                    }
                }

                // Calculate direct costs for each building element
                directCostsElement = materialCostsElement + labourCostsElement;

                // Calculate true price for each building element
                truePriceElement = directCostsElement + totalMKIElement;


//////////////////////////////////////////////////////////////////////
// --- Add building element financial impact results to detailedResults tree ---
//////////////////////////////////////////////////////////////////////


                // Fill detailedResults tree with data from buildingData tree
                foreach (GH_Path subPath in groupIndexA)
                {
                    if (subPath.Indices[1] >= 0 && subPath.Indices[1] <= 13)
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

                // Add material costs for building element as an extra branch
                int materialCostsElementIndexA;
                materialCostsElementIndexA = existingPaths.Max(path => path.Indices[1]) + 1;
                GH_Path materialCostsElementPath = new GH_Path(groupIndexB, materialCostsElementIndexA); // Create the new path for the results

                List<IGH_Goo> materialCostsElementData = new List<IGH_Goo>
                {
                    new GH_String("Material Costs Building Element [€]"),
                    new GH_Number(materialCostsElement)
                };

                detailedResults.AppendRange(materialCostsElementData, materialCostsElementPath);

                // Add labour costs for building element as an extra branch
                int labourCostsElementIndexA;
                labourCostsElementIndexA = existingPaths.Max(path => path.Indices[1]) + 1;
                GH_Path labourCostsElementPath = new GH_Path(groupIndexB, labourCostsElementIndexA); // Create the new path for the results

                List<IGH_Goo> labourCostsElementData = new List<IGH_Goo>
                {
                    new GH_String("Labour Costs Building Element [€]"),
                    new GH_Number(labourCostsElement)
                };

                detailedResults.AppendRange(labourCostsElementData, labourCostsElementPath);

                // Add direct costs for building element as an extra branch
                int directCostsElementIndexA;
                directCostsElementIndexA = existingPaths.Max(path => path.Indices[1]) + 1;
                GH_Path directCostsElementPath = new GH_Path(groupIndexB, directCostsElementIndexA); // Create the new path for the results

                List<IGH_Goo> directCostsElementData = new List<IGH_Goo>
                {
                    new GH_String("Direct Costs Building Element [€]"),
                    new GH_Number(directCostsElement)
                };

                detailedResults.AppendRange(directCostsElementData, directCostsElementPath);

                // Add true price for building element as an extra branch
                int truePriceElementIndexA;
                truePriceElementIndexA = existingPaths.Max(path => path.Indices[1]) + 1;
                GH_Path truePriceElementPath = new GH_Path(groupIndexB, truePriceElementIndexA); // Create the new path for the results

                List<IGH_Goo> truePriceElementData = new List<IGH_Goo>
                {
                    new GH_String("True Price Building Element [€]"),
                    new GH_Number(truePriceElement)
                };

                detailedResults.AppendRange(truePriceElementData, truePriceElementPath);
            }


//////////////////////////////////////////////////////////////////////
// --- Financial impact calculations for the complete building ---
//////////////////////////////////////////////////////////////////////


            // Set total calculation results for building
            double totalMKI = 0.0; // Total MKI of the building
            double totalMaterialCosts = 0.0; // Total material costs of the building
            double totalLabourCosts = 0.0; // Total labour costs of the buildingb
            double totalDirectCosts = 0.0; // Total direct costs of the building
            double totalTruePrice = 0.0; // Total True Price of the building

            // Perform calculations for building
            foreach (GH_Path path in buildingData.Paths)
            {
                var branch = buildingData.get_Branch(path);

                if (branch.Count == 2 && branch[0] is GH_String ghString && branch[1] is GH_Number ghNumber)
                {
                    // Set description and value variables to retrieve the required data
                    string description = ghString.Value;
                    double value = ghNumber.Value;

                    // Calculate the total MKI and MPG for the building
                    if (description.StartsWith("MKI"))
                    {
                        totalMKI += value;
                    }

                    // Calculate total material costs for building
                    if (description == "Material Costs [€]")
                    {
                        totalMaterialCosts += value;
                    }

                    // Calculate total labour costs for building
                    if (description == "Labour Costs [€]")
                    {
                       totalLabourCosts += value;
                    }
                }
            }

            // Perform final calculations
            totalDirectCosts = totalMaterialCosts + totalLabourCosts;
            totalTruePrice = totalDirectCosts + totalMKI;


//////////////////////////////////////////////////////////////////////
// --- Add results to totalResults tree ---
//////////////////////////////////////////////////////////////////////


            // Add direct costs
            totalResults.Append(new GH_String("Direct Costs [€]"), new GH_Path(0));
            totalResults.Append(new GH_Number(totalDirectCosts), new GH_Path(0));

            // Add material costs
            totalResults.Append(new GH_String("Material Costs [€]"), new GH_Path(1));
            totalResults.Append(new GH_Number(totalMaterialCosts), new GH_Path(1));

            // Add labour costs
            totalResults.Append(new GH_String("Labour Costs [€]"), new GH_Path(2));
            totalResults.Append(new GH_Number(totalLabourCosts), new GH_Path(2));

            // Add true price
            totalResults.Append(new GH_String("True Price [€]"), new GH_Path(3));
            totalResults.Append(new GH_Number(totalTruePrice), new GH_Path(3));

            // Add MKI
            totalResults.Append(new GH_String("Total MKI [€]"), new GH_Path(4));
            totalResults.Append(new GH_Number(totalMKI), new GH_Path(4));


//////////////////////////////////////////////////////////////////////
// --- Set ouptut parameters ---
//////////////////////////////////////////////////////////////////////


            DA.SetDataTree(0, totalResults);
            DA.SetDataTree(1, detailedResults);
            DA.SetData(2, totalDirectCosts);
            DA.SetData(3, totalMaterialCosts);
            DA.SetData(4, totalLabourCosts);
            DA.SetData(5, totalTruePrice);
        }

        /// <summary>
        /// The Exposure property controls where in the panel a component icon 
        /// will appear. There are seven possible locations (primary to septenary), 
        /// each of which can be combined with the GH_Exposure.obscure flag, which 
        /// ensures the component will only be visible on panel dropdowns.
        /// </summary>
        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // Load the embedded icon
                using (var stream = typeof(CreateBuildingElementComponent).Assembly.GetManifestResourceStream("ReLifeCycleGHPlugin.Resources.FinancialImpactAssessmentIcon.png"))
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
            get { return new Guid("0A37DD0E-4CB5-4987-BB0A-868CCA48AAD7"); }
        }
    }
}