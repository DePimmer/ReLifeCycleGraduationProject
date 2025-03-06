using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using System.Drawing;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System.Linq;

namespace ReLifeCycleGHPlugin
{
    //  ███████╗███╗   ██╗██╗   ██╗██╗██████╗  ██████╗ ███╗   ██╗███╗   ███╗███████╗███╗   ██╗████████╗ █████╗ ██╗     
    //  ██╔════╝████╗  ██║██║   ██║██║██╔══██╗██╔═══██╗████╗  ██║████╗ ████║██╔════╝████╗  ██║╚══██╔══╝██╔══██╗██║     
    //  █████╗  ██╔██╗ ██║██║   ██║██║██████╔╝██║   ██║██╔██╗ ██║██╔████╔██║█████╗  ██╔██╗ ██║   ██║   ███████║██║     
    //  ██╔══╝  ██║╚██╗██║╚██╗ ██╔╝██║██╔══██╗██║   ██║██║╚██╗██║██║╚██╔╝██║██╔══╝  ██║╚██╗██║   ██║   ██╔══██║██║     
    //  ███████╗██║ ╚████║ ╚████╔╝ ██║██║  ██║╚██████╔╝██║ ╚████║██║ ╚═╝ ██║███████╗██║ ╚████║   ██║   ██║  ██║███████╗
    //  ╚══════╝╚═╝  ╚═══╝  ╚═══╝  ╚═╝╚═╝  ╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚═╝     ╚═╝╚══════╝╚═╝  ╚═══╝   ╚═╝   ╚═╝  ╚═╝╚══════╝
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

    public class EnvironmentalImpactAssessmentComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EnvironmentalImpactAssessmentComponent class.
        /// </summary>
        public EnvironmentalImpactAssessmentComponent()
          : base("Environmental Impact Assessment", "E-Assessment",
              "Assess the environmental impact of a building",
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

            // Add environmental impact output parameters
            pManager.AddGenericParameter("MPG [€ / m2 GFA / year]", "MPG", "MilieuPrestatie Gebouw score in € / m2 GFA / year", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Paris Proof [kg CO2 eq. / m2 GFA]", "PP", "Paris Proof Indicator in  kg CO2 eq. / m2 GFA", GH_ParamAccess.tree);
            pManager.AddGenericParameter("CSC [kg CO2 eq.]", "CSC", "Construction Stored Carbon in kg CO2 eq.", GH_ParamAccess.tree);
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
// --- Detailed evironmental impact calculations for each building element ---
//////////////////////////////////////////////////////////////////////


            // Remove branches from buildingData where the first path index (B) is 0 (these contain general building information and are not required for this calculation)
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
                double totalMKIElement = 0.0; // Total MKI of a building element including replacements
                double totalGWPElement = 0.0; // Total GWP of a building element including replacements

                double mpgElement = 0.0;
                double parisProofElement = 0.0;

                // Set intermediate calculation results for building elements
                double GWPA1A5 = 0.0; // GWP for life cycle stages A1-A5 for a building element

                // Calculate total MKI and GWP for each building element including replacements
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
                        else if (description.StartsWith("GWP"))
                        {
                            totalGWPElement += value;
                        }
                    }
                }

                // Calculate Paris Proof Indicator for each building element
                foreach (GH_Path subPath in groupIndexA)
                {
                    var branch = buildingData.get_Branch(subPath);

                    if (branch.Count == 2 && branch[0] is GH_String ghString && branch[1] is GH_Number ghNumber)
                    {
                        string description = ghString.Value;
                        double value = ghNumber.Value;

                        if (description == "GWP A1-A2-A3 [kg CO2 eq.]"
                         || description == "GWP A4 [kg CO2 eq.]"
                         || description == "GWP A5 [kg CO2 eq.]")
                        {
                            GWPA1A5 += value;
                            parisProofElement = GWPA1A5 / gfa;
                        }
                    }
                }

                // Calculate MPG for each building element
                mpgElement = totalMKIElement / (gfa * functionalLifespan);


//////////////////////////////////////////////////////////////////////
// --- Add building element environmental impact results to detailedResults tree ---
//////////////////////////////////////////////////////////////////////


                // Fill detailedResults tree with data from buildingData tree
                foreach (GH_Path subPath in groupIndexA)
                {
                    // Retrieve and append branches {B;0} to {B;40} (the branches with environmental impact data}
                    if (subPath.Indices[1] >= 0 && subPath.Indices[1] <= 40)
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

                // Add total GWP for building element as an extra branch
                int totalGWPElementIndexA;
                totalGWPElementIndexA = existingPaths.Max(path => path.Indices[1]) + 1;
                GH_Path totalGWPElementPath = new GH_Path(groupIndexB, totalGWPElementIndexA); // Create the new path for the results

                List<IGH_Goo> totalGWPElementData = new List<IGH_Goo>
                {
                    new GH_String("Total GWP Building Element [kg CO2 eq.]"),
                    new GH_Number(totalGWPElement)
                };

                detailedResults.AppendRange(totalGWPElementData, totalGWPElementPath);

                // Add Paris Proof Indicator for building element as an extra branch
                int parisProofElementIndexA;
                parisProofElementIndexA = existingPaths.Max(path => path.Indices[1]) + 1;
                GH_Path parisProofElementPath = new GH_Path(groupIndexB, parisProofElementIndexA); // Create the new path for the results

                List<IGH_Goo> parisProofElementData = new List<IGH_Goo>
                {
                    new GH_String("Paris Proof Indicator Building Element [kg CO2 eq. / m2 GFA"),
                    new GH_Number(parisProofElement)
                };

                detailedResults.AppendRange(parisProofElementData, parisProofElementPath);

                // Add MPG for building element as an extra branch
                int mpgElementIndexA;
                mpgElementIndexA = existingPaths.Max(path => path.Indices[1]) + 1;
                GH_Path mpgElementPath = new GH_Path(groupIndexB, mpgElementIndexA); // Create the new path for the results

                List<IGH_Goo> mpgElementData = new List<IGH_Goo>
                {
                    new GH_String("MPG Building Element [€ / m2 GFA / year]"),
                    new GH_Number(mpgElement)
                };

                detailedResults.AppendRange(mpgElementData, mpgElementPath);
            }


//////////////////////////////////////////////////////////////////////
// --- Environmental impact calculations for the complete building ---
//////////////////////////////////////////////////////////////////////


            // Create dictionary to store GWP and MKI per life cycle stage for the complete building
            Dictionary<string, double> resultsPerLCS = new Dictionary<string, double>();

            // Set total calculation results for building
            double totalMKI = 0.0; // Total MKI of the building
            double totalMPG = 0.0; // Total MPG of the building
            double totalParisProof = 0.0; // Total Paris Proof Indicator of the building
            double totalCSC = 0.0; // Total Construction Stored Carbon of the building

            // Set intermediate calculation results for building
            double totalGWPA1A5 = 0.0;

            // Perform calculations for building
            foreach (GH_Path path in buildingData.Paths)
            {
                var branch = buildingData.get_Branch(path);

                if (branch.Count == 2 && branch[0] is GH_String ghString && branch[1] is GH_Number ghNumber)
                {
                    // Set description and value variables to retrieve the required data
                    string description = ghString.Value;
                    double value = ghNumber.Value;

                    // Sum GWP and MKI per life cycle stage across all branches
                    if (description == "GWP A1-A2-A3 [kg CO2 eq.]"
                     || description == "GWP A4 [kg CO2 eq.]"
                     || description == "GWP A5 [kg CO2 eq.]"
                     || description == "GWP B1 [kg CO2 eq.]"
                     || description == "GWP B2 [kg CO2 eq.]"
                     || description == "GWP B3 [kg CO2 eq.]"
                     || description == "GWP B4 [kg CO2 eq.]"
                     || description == "GWP B5 [kg CO2 eq.]"
                     || description == "GWP C1 [kg CO2 eq.]"
                     || description == "GWP C2 [kg CO2 eq.]"
                     || description == "GWP C3 [kg CO2 eq.]"
                     || description == "GWP C4 [kg CO2 eq.]"
                     || description == "GWP D [kg CO2 eq.]"
                     || description == "MKI A1-A2-A3 [€]"
                     || description == "MKI A4 [€]"
                     || description == "MKI A5 [€]"
                     || description == "MKI B1 [€]"
                     || description == "MKI B2 [€]"
                     || description == "MKI B3 [€]"
                     || description == "MKI B4 [€]"
                     || description == "MKI B5 [€]"
                     || description == "MKI C1 [€]"
                     || description == "MKI C2 [€]"
                     || description == "MKI C3 [€]"
                     || description == "MKI C4 [€]"
                     || description == "MKI D [€]")
                    {
                        if (resultsPerLCS.ContainsKey(description))
                        {
                            resultsPerLCS[description] += value; // Add value if description exists
                        }
                        else
                        {
                            resultsPerLCS[description] = value; // Initialize value if description doesn't exist
                        }
                    }

                    // Calculate the total MKI and MPG for the building
                    if (description.StartsWith("MKI"))
                    {
                        totalMKI += value;
                    }

                    // Calculate total Paris Proof Indicator for building
                    if (description == "GWP A1-A2-A3 [kg CO2 eq.]"
                     || description == "GWP A4 [kg CO2 eq.]"
                     || description == "GWP A5 [kg CO2 eq.]")
                    {
                        totalGWPA1A5 += value;
                    }

                    // Calculate total Construction Stored Carbon for building
                    if (description == "CSC [kg CO2 eq.]")
                    {
                        totalCSC += value;
                    }
                }
            }

            // Perform final calculations
            totalMPG = totalMKI / (gfa * functionalLifespan);
            totalParisProof = totalGWPA1A5 / gfa;


//////////////////////////////////////////////////////////////////////
// --- Add results to totalResults tree ---
//////////////////////////////////////////////////////////////////////


            // Add MPG
            totalResults.Append(new GH_String("MPG [€ / m2 GFA / year]"), new GH_Path(0));
            totalResults.Append(new GH_Number(totalMPG), new GH_Path(0));

            // Add Paris Proof Indicator
            totalResults.Append(new GH_String("Paris Proof Indicator [kg CO2 eq. / m2 GFA"), new GH_Path(1));
            totalResults.Append(new GH_Number(totalParisProof), new GH_Path(1));

            // Add CSC
            totalResults.Append(new GH_String("CSC [kg CO2 eq.]"), new GH_Path(2));
            totalResults.Append(new GH_Number(totalCSC), new GH_Path(2));

            // Add MKI
            totalResults.Append(new GH_String("Total MKI [€]"), new GH_Path(3));
            totalResults.Append(new GH_Number(totalMKI), new GH_Path(3));

            // Add GWP and MKI results per life cycle stage
            int totalResultsIndex = 4;
            foreach (var branch in resultsPerLCS)
            {
                string description = branch.Key;
                double totalValue = branch.Value;

                // Create new path
                GH_Path bCalculationVariablePath = new GH_Path(totalResultsIndex);

                // Append description and value to the buildingData tree
                totalResults.Append(new GH_String(description), bCalculationVariablePath); // Add description
                totalResults.Append(new GH_Number(totalValue), bCalculationVariablePath); // Add value

                totalResultsIndex++;
            }


//////////////////////////////////////////////////////////////////////
// --- Set ouptut parameters ---
//////////////////////////////////////////////////////////////////////


            DA.SetDataTree(0, totalResults);
            DA.SetDataTree(1, detailedResults);
            DA.SetData(2, totalMPG);
            DA.SetData(3, totalParisProof);
            DA.SetData(4, totalCSC);
        }

        /// <summary>
        /// The Exposure property controls where in the panel a component icon 
        /// will appear. There are seven possible locations (primary to septenary), 
        /// each of which can be combined with the GH_Exposure.obscure flag, which 
        /// ensures the component will only be visible on panel dropdowns.
        /// </summary>
        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // Load the embedded icon
                using (var stream = typeof(CreateBuildingElementComponent).Assembly.GetManifestResourceStream("ReLifeCycleGHPlugin.Resources.EnvironmentalImpactAssessmentIcon.png"))
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
            get { return new Guid("8C915B1E-B5F6-4A64-9023-3B684ED66A81"); }
        }
    }

}