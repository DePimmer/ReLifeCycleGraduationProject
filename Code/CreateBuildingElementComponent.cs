using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
using System.Drawing;
using Grasshopper.Kernel.Types;
using System.Linq;
using System.Threading.Tasks;

namespace ReLifeCycleGHPlugin
{
    //   ██████╗██████╗ ███████╗ █████╗ ████████╗███████╗                                                                           
    //  ██╔════╝██╔══██╗██╔════╝██╔══██╗╚══██╔══╝██╔════╝                                                                           
    //  ██║     ██████╔╝█████╗  ███████║   ██║   █████╗                                                                             
    //  ██║     ██╔══██╗██╔══╝  ██╔══██║   ██║   ██╔══╝                                                                             
    //  ╚██████╗██║  ██║███████╗██║  ██║   ██║   ███████╗                                                                           
    //   ╚═════╝╚═╝  ╚═╝╚══════╝╚═╝  ╚═╝   ╚═╝   ╚══════╝                                                                           
    //                                                                                                                              
    //  ██████╗ ██╗   ██╗██╗██╗     ██████╗ ██╗███╗   ██╗ ██████╗     ███████╗██╗     ███████╗███╗   ███╗███████╗███╗   ██╗████████╗
    //  ██╔══██╗██║   ██║██║██║     ██╔══██╗██║████╗  ██║██╔════╝     ██╔════╝██║     ██╔════╝████╗ ████║██╔════╝████╗  ██║╚══██╔══╝
    //  ██████╔╝██║   ██║██║██║     ██║  ██║██║██╔██╗ ██║██║  ███╗    █████╗  ██║     █████╗  ██╔████╔██║█████╗  ██╔██╗ ██║   ██║   
    //  ██╔══██╗██║   ██║██║██║     ██║  ██║██║██║╚██╗██║██║   ██║    ██╔══╝  ██║     ██╔══╝  ██║╚██╔╝██║██╔══╝  ██║╚██╗██║   ██║   
    //  ██████╔╝╚██████╔╝██║███████╗██████╔╝██║██║ ╚████║╚██████╔╝    ███████╗███████╗███████╗██║ ╚═╝ ██║███████╗██║ ╚████║   ██║   
    //  ╚═════╝  ╚═════╝ ╚═╝╚══════╝╚═════╝ ╚═╝╚═╝  ╚═══╝ ╚═════╝     ╚══════╝╚══════╝╚══════╝╚═╝     ╚═╝╚══════╝╚═╝  ╚═══╝   ╚═╝   
    //     

    public class CreateBuildingElementComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreateBuildingElement class.
        /// </summary>
        
        public CreateBuildingElementComponent()
          : base("Create Building Element", "CreateBE",
              "Create a building element from Grasshopper geometry",
              "ReLifeCycle", "   Building")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Custom name for the building element", GH_ParamAccess.item, "");
            pManager.AddTextParameter("NL/SfB Class", "C", "NL/SfB class of building element (you can use the same value list from the Create Material Set component as input)", GH_ParamAccess.item);
            pManager.AddGenericParameter("Material Data", "M", "Data for the material", GH_ParamAccess.tree);
            pManager.AddMeshParameter("Geometry", "G", "Mesh geometry of the building element. Check the Unit of the Select Material component for the required geometry type input. The geometry input is flattened internally.", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Building Element Data", "BED", "Data for the building element", GH_ParamAccess.tree);
            pManager.AddTextParameter("Area [m2]", "A", "Total area of the building element if it's a surface", GH_ParamAccess.item);
            pManager.AddTextParameter("Volume [m3]", "V", "Total volume of the building element if it's a solid", GH_ParamAccess.item);
            pManager.AddNumberParameter("Mass [kg]", "M", "Total mass of the building element in kilogram", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Create input variables
            string name = "";
            string nLsfbClass = "";
            GH_Structure<IGH_Goo> materialData = new GH_Structure<IGH_Goo>();
            GH_Structure<GH_Mesh> geometryTree = new GH_Structure<GH_Mesh>();
            List<Mesh> geometry = new List<Mesh>();

            // Retrieve data from input parameters
            if (!DA.GetData(0, ref name)) return;
            if (!DA.GetData(1, ref nLsfbClass)) return;
            if (!DA.GetDataTree(2, out materialData)) return;
            if (!DA.GetDataTree(3, out geometryTree)) return;

            // Flatten the geometry input parameter
            geometryTree.Flatten();
            foreach (GH_Mesh ghMesh in geometryTree)
            {
                if (ghMesh != null && ghMesh.Value != null)
                    geometry.Add(ghMesh.Value);
            }


//////////////////////////////////////////////////////////////////////
// --- Validate input and display runtime messages if necessary ---
//////////////////////////////////////////////////////////////////////


            // Check if materialdata is provided and display a remark in case no data is provided
            if (materialData.IsEmpty)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "No material data is provided. Material data is needed for responsible material assessment");
                return;
            }

            // Create variable "unit" that returns the unit of the selected material
            string unit = "";
   
            // Retrieve second item of the data tree branch with as first item value a string "Unit"
            foreach (GH_Path path in materialData.Paths)
            {
                var items = materialData.get_Branch(path);

                // Displays error message if one or more meshes are null
                if (geometry.Any(mesh => mesh == null))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "One or more geometry items as null");
                    return;
                }

                // Check if the first item in the branch is a string with value "Unit" and assign that value to the unit variable
                if (items[0] is IGH_Goo firstItemGoo && firstItemGoo.CastTo(out string firstItem) && firstItem == "Unit")
                {
                    // Retrieve the second item
                    if (items[1] is IGH_Goo secondItemGoo && secondItemGoo.CastTo(out string secondItem))
                    {
                        unit = secondItem;
                        break;
                    }
                }
            }

            // Validate if geometry type input is in line with the corresponiding unit (m3 = solid, m2 = surface)
            if (unit == "m3")
            {
                if (geometry.Any(mesh => !mesh.IsSolid))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Wrong geometry type. Geometry input can only contain solids because the unit is 'm3'.");
                    return;
                }
            }
            else if (unit == "m2")
            {
                if (geometry.Any(mesh => mesh.IsSolid))
                {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Wrong geometry type. Geometry input can only contain surfaces because the unit is 'm2'.");
                return;
                }
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Unsupported unit '{unit}' in material data. Accepted units are 'm3' or 'm2'.");
                return;
            }

            // Create booleans to check which geometry type is used in the input
            bool containsSolid = false;
            bool containsSurface = false;

            // Display error message and stop processing if both geometry types are detected
            foreach (Mesh mesh in geometry)
            {
                // Displays error message if one or more meshes are null
                if (mesh == null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "One or more geometry items as null");
                    return;
                }

                // Set boolean variables for when the mesh is a solid
                if (mesh.IsSolid)
                {
                    containsSolid = true;
                }

                // Set boolean variables for when the mesh is a surface
                else
                {
                    containsSurface = true;
                }
                // If both types are detected, stop processing
                if (containsSolid && containsSurface)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Geometry input contains both solids and surfaces. Please provide only one geometry type.");
                    return;
                }
            }


//////////////////////////////////////////////////////////////////////
// --- Calculate total area or volume ---
//////////////////////////////////////////////////////////////////////


            // Create variable that stores either the total area or the total volume of a building element based on its geometry type (surface or solid)
            double totalAreaOrVolume = 0.0;

            // Calculate total volume or area of building element based on geometry type
            // Use Parallel.ForEach for performance improvement
            Parallel.ForEach(geometry, mesh =>
            {
                // If the mesh is a solid, calculate the volume and assign it to the totalAreaOrVolume variable
                if (mesh.IsSolid)
                {
                    var meshVolume = VolumeMassProperties.Compute(mesh);
                    if (meshVolume != null)
                    {
                        double volume = Math.Round(meshVolume.Volume, 6);
                        lock (geometry)
                        {
                            totalAreaOrVolume += volume;
                        }
                    }
                }
                // If the mesh is not a solid, treat it as a surface, calculate the area and assign it to the totalAreaOrVolume variable
                else
                {
                    var meshArea = AreaMassProperties.Compute(mesh);
                    if (meshArea != null)
                    {
                        double area = Math.Round(meshArea.Area, 6);
                        lock (geometry)
                        {
                            totalAreaOrVolume += area;
                        }
                    }
                }
            });

            // Place the final result in the appropriate output parameter
            if (geometry.Any(mesh => mesh.IsSolid))
            {
                DA.SetData(2, totalAreaOrVolume);
            }
            else
            {
                DA.SetData(1, totalAreaOrVolume);
            }


//////////////////////////////////////////////////////////////////////
// --- Calculate total mass ---
//////////////////////////////////////////////////////////////////////


            // Create variable "massPerUnit" that stores the mass per unit of the material
            double massPerUnit = 0;

            // Calculate total mass of building element
            foreach (GH_Path path in materialData.Paths)
            {
                var items = materialData.get_Branch(path);

                // Check if the first item in the branch is a string with value "Mass per Unit [kg]" and assign that value to the massPerUnit variable
                if (items[0] is IGH_Goo firstItemGoo && firstItemGoo.CastTo(out string firstItem) && firstItem == "Mass per Unit [kg]")
                {
                    // Retrieve the second item
                    if (items[1] is IGH_Goo secondItemGoo && secondItemGoo.CastTo(out double secondItem))
                    {
                        massPerUnit = secondItem;
                        break;
                    }
                }
            }

            // Create variable "totalMass" that stores the total mass of the building element
            double totalMass = massPerUnit * totalAreaOrVolume;


//////////////////////////////////////////////////////////////////////
// --- Retrieve NL/SfB name ---
//////////////////////////////////////////////////////////////////////


            // Create variable "nlsfbName" to store the value (NL/SfB name) of the NL/SfB value list connected to the NL/SfB class input
            string nlsfbCodeAndName = "";
            string nlsfbName = "";

            // Assume the input is connected to a Value List, then retrieve the value of the corresponding key of the valuelist
            IGH_Param inputParam = Params.Input[1].Sources.FirstOrDefault();

            if (inputParam is GH_ValueList nlsfbValueList)
            {
                // Access all value list items
                var items = nlsfbValueList.ListItems;

                // Find the corresponding value
                var matchedItem = items.FirstOrDefault(item =>
                {
                    if (item.Value is IGH_Goo gooValue && gooValue.CastTo(out string value))
                    {
                        return value == nLsfbClass;
                    }
                    return false;
                });
                nlsfbCodeAndName = matchedItem.Name; // This is the name of the NL/SfB class based on the value list key
                int spaceIndex = nlsfbCodeAndName.IndexOf(' '); // Remove the NL/SfB code and extract only the NL/SfB name by retrieving the index of the space and retrieve all successive characters
                nlsfbName = spaceIndex >= 0 ? nlsfbCodeAndName.Substring(spaceIndex + 1) : nlsfbCodeAndName;
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "NL/SfB class input should be a value list. You can use the NLSfB Classes component for this");
            }


//////////////////////////////////////////////////////////////////////
// --- Construct Building Element data tree ---
//////////////////////////////////////////////////////////////////////


            // Create building element data tree
            GH_Structure<IGH_Goo> buildingElementData = new GH_Structure<IGH_Goo>();

            // If "Name" parameter input is empty, set the default value to "My Building Element"
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "My Building Element";
            }

            // Add "Name" to 1st branch of building element data tree
            List<IGH_Goo> nameList = new List<IGH_Goo>
            {
                new GH_String("Name"),
                new GH_String(name)
            };

            buildingElementData.AppendRange(nameList, new GH_Path(0));

            // Add (updated) "NL/SfB Code" to 2nd branch of building element data tree
            List<IGH_Goo> nlsfbCodeList = new List<IGH_Goo>
            {
                new GH_String("NL/SfB Code"),
                new GH_String(nLsfbClass)
            };

            buildingElementData.AppendRange(nlsfbCodeList, new GH_Path(1));

            // Add (updated) "NL/SfB Name" to 3rd branch of building element data tree
            List<IGH_Goo> nlsfbNameList = new List<IGH_Goo>
            {
                new GH_String("NL/SfB Name"),
                new GH_String(nlsfbName)
            };

            buildingElementData.AppendRange(nlsfbNameList, new GH_Path(2));

            // Add "Geometry" to 4th branch of building element data tree
            List<IGH_Goo> geometryList = new List<IGH_Goo>
            {
                new GH_String("Geometry"),
            };

            foreach (Mesh mesh in geometry)
            {
                geometryList.Add(new GH_Mesh(mesh));
            }

            buildingElementData.AppendRange(geometryList, new GH_Path(3));

            // Add material data multiplied by total area or volume to 5th branch of building element data tree
            string volumeOrArea = "";

            // Change list title to "Total volume [m3]" if the meshes are a solid and to "Total area [m2]" if the meshes are a surface
            if (containsSolid)
            {
                volumeOrArea = "Total volume [m3]";
            }
            else if (containsSurface)
            {
                volumeOrArea = "Total area [m2]";
            }

            List<IGH_Goo> areaOrVolumeList = new List<IGH_Goo>
            {
                new GH_String(volumeOrArea),
                new GH_Number(totalAreaOrVolume)
            };

            buildingElementData.AppendRange(areaOrVolumeList, new GH_Path(4));

            // Add total mass to 6th branch of building element data tree
            List<IGH_Goo> massList = new List<IGH_Goo>
            {
                new GH_String("Total Mass [kg]"),
                new GH_Number(totalMass)
            };
            
            buildingElementData.AppendRange(massList, new GH_Path(5));


            // Add material data tree to remaining branches of building element data tree
            // Set target branches from materialData that should be added to buildingElementData
            var targetBranches = new HashSet<string>
            {
                "NMD ID", "NIBE ID", "ArchiCalc ID", "NMD Category", "Product Description", "Unit", "Thickness [m]",
                "Technical Lifespan [years]", "GWP A1-A2-A3 [kg CO2 eq.]", "GWP A4 [kg CO2 eq.]", "GWP A5 [kg CO2 eq.]", "GWP B1 [kg CO2 eq.]", "GWP B2 [kg CO2 eq.]", "GWP B3 [kg CO2 eq.]",
                "GWP B4 [kg CO2 eq.]", "GWP B5 [kg CO2 eq.]", "GWP C1 [kg CO2 eq.]", "GWP C2 [kg CO2 eq.]", "GWP C3 [kg CO2 eq.]", "GWP C4 [kg CO2 eq.]", "GWP D [kg CO2 eq.]", 
                "MKI A1-A2-A3 [€]", "MKI A4 [€]", "MKI A5 [€]", "MKI B1 [€]", "MKI B2 [€]", "MKI B3 [€]", "MKI B4 [€]",
                "MKI B5 [€]", "MKI C1 [€]", "MKI C2 [€]", "MKI C3 [€]", "MKI C4 [€]", "MKI D [€]", "CSC [kg CO2 eq.]", 
                "% New", "% Biobased", "% Recycled", "% Reused", "% Landfill", "% Burning", 
                "% Recycling", "% Reusing", "DP [%]", "MCI [%]", "PCI [%]", "Material Costs [€]",
                "Labour Costs [€]"
            };

            // Set selection of branches that should be multiplied with the total area or volume of the building element
            var branchesToMultiply = new HashSet<string>
            {
                "GWP A1-A2-A3 [kg CO2 eq.]", "GWP A4 [kg CO2 eq.]", "GWP A5 [kg CO2 eq.]", "GWP B1 [kg CO2 eq.]", "GWP B2 [kg CO2 eq.]", "GWP B3 [kg CO2 eq.]",
                "GWP B4 [kg CO2 eq.]", "GWP B5 [kg CO2 eq.]", "GWP C1 [kg CO2 eq.]", "GWP C2 [kg CO2 eq.]", "GWP C3 [kg CO2 eq.]", "GWP C4 [kg CO2 eq.]", "GWP D [kg CO2 eq.]",
                "MKI A1-A2-A3 [€]", "MKI A4 [€]", "MKI A5 [€]", "MKI B1 [€]", "MKI B2 [€]", "MKI B3 [€]", "MKI B4 [€]",
                "MKI B5 [€]", "MKI C1 [€]", "MKI C2 [€]", "MKI C3 [€]", "MKI C4 [€]", "MKI D [€]", "CSC [kg CO2 eq.]",
                "Material Costs [€]", "Labour Costs [€]"
            };

            // Set the branch start index to 6 so all branches are added at the end of the buildingElementData tree
            int startIndex = 6;

            // Retrieve all target branches from materialData tree, multiply only the specific branches from "branchesToMultiply" with the total volume or area and add the updated data to the buildingElement tree
            foreach (GH_Path path in materialData.Paths)
            {
                var items = materialData.get_Branch(path);

                // Check if the first item in a branch matches any of the target branches values
                if (items[0] is IGH_Goo firstItemGoo && firstItemGoo.CastTo(out string firstItem))
                {
                    // Target all target branches from the materialData tree
                    if (targetBranches.Contains(firstItem))
                    {
                        // Target all branches to multiply from the materialData tree
                        if (branchesToMultiply.Contains(firstItem))
                        {
                            // Check if a second item exists and is a double
                            if (items[1] is IGH_Goo secondItemGoo && secondItemGoo.CastTo(out double secondItemValue))
                            {
                                // Multiply the second item with the total area or volume depending on geometry type input
                                secondItemValue *= totalAreaOrVolume;

                                // Create a new branch with updated data
                                var multipliedValues = new List<IGH_Goo>
                                {
                                    new GH_String(firstItem), // First item remains the same
                                    new GH_Number(secondItemValue) // Updated second item
                                };

                                // Add the updated branch to the buildingElementData tree
                                buildingElementData.AppendRange(multipliedValues, new GH_Path(startIndex));
                            }
                            else
                            {
                                // Append original branch without modifications
                                buildingElementData.AppendRange(items.Cast<IGH_Goo>(), new GH_Path(startIndex));
                            }
                        }
                        else
                        {
                            // Append original branch without modifications
                            buildingElementData.AppendRange(items.Cast<IGH_Goo>(), new GH_Path(startIndex));
                        }

                        startIndex++; // Repeat for the next branch
                    }
                }
            }


//////////////////////////////////////////////////////////////////////
// --- Set ouptut parameters ---
//////////////////////////////////////////////////////////////////////


            DA.SetDataTree(0, buildingElementData);
            DA.SetData(3, totalMass);
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
                using (var stream = typeof(CreateBuildingElementComponent).Assembly.GetManifestResourceStream("ReLifeCycleGHPlugin.Resources.CreateBuildingElementIcon.png"))
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
            get { return new Guid("A6592DF5-9175-4AD6-BC47-8F17A881C440"); }
        }
    }
}
