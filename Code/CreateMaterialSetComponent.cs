using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using MySql.Data.MySqlClient;
using Grasshopper;
using Grasshopper.Kernel.Data;
using System.Drawing;
using System.Linq;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;
using Grasshopper.Kernel.Attributes;

namespace ReLifeCycleGHPlugin
{
    //   ██████╗██████╗ ███████╗ █████╗ ████████╗███████╗                                           
    //  ██╔════╝██╔══██╗██╔════╝██╔══██╗╚══██╔══╝██╔════╝                                           
    //  ██║     ██████╔╝█████╗  ███████║   ██║   █████╗                                             
    //  ██║     ██╔══██╗██╔══╝  ██╔══██║   ██║   ██╔══╝                                             
    //  ╚██████╗██║  ██║███████╗██║  ██║   ██║   ███████╗                                           
    //   ╚═════╝╚═╝  ╚═╝╚══════╝╚═╝  ╚═╝   ╚═╝   ╚══════╝                                           
    //                                                                                              
    //  ███╗   ███╗ █████╗ ████████╗███████╗██████╗ ██╗ █████╗ ██╗         ███████╗███████╗████████╗
    //  ████╗ ████║██╔══██╗╚══██╔══╝██╔════╝██╔══██╗██║██╔══██╗██║         ██╔════╝██╔════╝╚══██╔══╝
    //  ██╔████╔██║███████║   ██║   █████╗  ██████╔╝██║███████║██║         ███████╗█████╗     ██║   
    //  ██║╚██╔╝██║██╔══██║   ██║   ██╔══╝  ██╔══██╗██║██╔══██║██║         ╚════██║██╔══╝     ██║   
    //  ██║ ╚═╝ ██║██║  ██║   ██║   ███████╗██║  ██║██║██║  ██║███████╗    ███████║███████╗   ██║   
    //  ╚═╝     ╚═╝╚═╝  ╚═╝   ╚═╝   ╚══════╝╚═╝  ╚═╝╚═╝╚═╝  ╚═╝╚══════╝    ╚══════╝╚══════╝   ╚═╝   
    //                                                                                              

    public class CreateMaterialSetComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreateMaterialSetComponent class.
        /// </summary>

        // Creates MySQL connection string. CHANGE THE PLACEHOLDER X'S WITH YOUR OWN DATABASE SERVER INFORMATION
        private string connectionString = "server=XXX.X.X.X;user=XX;database=relifecycle_db;port=XX;password=XX";



        public CreateMaterialSetComponent()
          : base("Create Material Set", "CreateMatSet",
              "Create a set of materials for a building element class",
              "ReLifeCycle", "    Material")
        {
            // Check if the connection string contains the placeholder "XXX"
            if (connectionString.Contains("XXX"))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Please change the SQL string in the source code to connect to your database server.");
            }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("NL/SfB Class", "C", "NL/SfB class of building element", GH_ParamAccess.item, "11");
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Material Set Data", "MS", "Data for the material set", GH_ParamAccess.tree);
            pManager.AddTextParameter("Material Names", "N", "Names of the materials in the material set", GH_ParamAccess.item);
        }

        /// <summary>
        /// Create a custom "Load NL/SfB" classes button inside the GUI of the CreateMaterialSet component
        /// </summary>
        public override void CreateAttributes()
        {
            m_attributes = new CustomButton(this);
        }

        public class CustomButton : GH_ComponentAttributes
        {
            private RectangleF _buttonBounds;

            // Set initial button hover and click states
            private bool _isHovered = false;
            private bool _isClicked = false;

            public CustomButton(GH_Component owner) : base(owner)
            {
                // Set dynamic button rectangle bounds
                _buttonBounds = new RectangleF(0, 0, 0, 0);
            }

            // Set button layout
            protected override void Layout()
            {
                base.Layout();

                // Set button dimensions
                int buttonHeight = 20;
                float buttonWidth = Bounds.Width - 10; // Match component width with padding

                // Position the button
                float pivotX = Bounds.Left + 5; // Add padding from the left
                float pivotY = Bounds.Bottom + 5; // Add padding below the component

                _buttonBounds = new RectangleF(pivotX, pivotY, buttonWidth, buttonHeight);

                // Extend the component bounds to fit the button inside the component
                Bounds = new RectangleF(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height + buttonHeight + 10);
            }

            // Set button render settings
            protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
            {
                base.Render(canvas, graphics, channel);

                if (channel == GH_CanvasChannel.Objects)
                {
                    // Adjust button appearance based on hover and click states
                    Color backColor;
                    Color borderColor;
                    Color textColor = Color.White; // Keep text white

                    if (_isClicked)
                    {
                        backColor = Color.FromArgb(10, 10, 10); // Darker black for clicked state
                        borderColor = Color.Black; // Solid black border
                    }
                    else if (_isHovered)
                    {
                        backColor = Color.FromArgb(50, 50, 50); // Slightly darker gray for hover
                        borderColor = Color.Black;
                    }
                    else
                    {
                        backColor = Color.FromArgb(30, 30, 30); // Default colour
                        borderColor = Color.Black;
                    }

                    // Create a custom style using the calculated colors
                    GH_PaletteStyle buttonStyle = new GH_PaletteStyle(backColor, borderColor, textColor);
                    GH_Capsule button = GH_Capsule.CreateTextCapsule(_buttonBounds, _buttonBounds, GH_Palette.Transparent, "Load NL/SfB Classes", 2, 0);

                    // Apply custom style
                    button.Render(graphics, buttonStyle);
                    button.Dispose();
                }
            }

            // Set button behaviour when hovered
            public override GH_ObjectResponse RespondToMouseMove(GH_Canvas sender, GH_CanvasMouseEvent e)
            {
                bool isHovering = _buttonBounds.Contains(e.CanvasLocation);

                if (isHovering != _isHovered)
                {
                    _isHovered = isHovering;
                    sender.Refresh(); // Refreshes the button based on the hover state
                }

                return base.RespondToMouseMove(sender, e);
            }

            // Set button behaviour when clicked. This will pop-up and connect an NL/SfB Classes value list
            public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
            {
                if (_buttonBounds.Contains(e.CanvasLocation))
                {
                    // Button was clicked
                    _isClicked = true;
                    sender.Refresh(); // Refreshes the button based on the hover state
                    CreateValueList();
                    return GH_ObjectResponse.Handled;
                }

                return base.RespondToMouseDown(sender, e);
            }

            // Set button behaviour when released
            public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
            {
                if (_isClicked)
                {
                    _isClicked = false; // Reset clicked state
                    sender.Refresh(); // Update rendering
                }

                return base.RespondToMouseUp(sender, e);
            }

            /// <summary>
            /// Add a pop-up Value List with NL/SfB classifciations when the "Load NL/SfB Classes" button is clicked
            /// </summary>
            private void CreateValueList()
            {
                var owner = (CreateMaterialSetComponent)Owner;
                var document = owner.OnPingDocument();

                if (document == null)
                    return;

                // Check if a value list is already connected
                bool valueListExists = owner.Params.Input[0]
                    .Sources
                    .OfType<GH_ValueList>()
                    .Any();

                if (valueListExists) return;

                // Create a new GH_ValueList and populate it with NL/SfB classifications
                var nlsfbValueList = new GH_ValueList
                {
                    // Set value list name and mode
                    NickName = "NL/SfB Class",
                    ListMode = GH_ValueListMode.DropDown
                };

                // Clear any default items
                nlsfbValueList.ListItems.Clear();

                // Retrieve NL/SfB classes from MySQL ReLifeCycle database
                var nlsfbClasses = new List<(string code, string name)>();

                using (var ReLifeCycleDB = new MySqlConnection(owner.connectionString))
                {
                    try
                    {
                        ReLifeCycleDB.Open();

                        string query = $@"
                        SELECT 
                            nl_sfb_code,
                            nl_sfb_name_en
                        FROM relifecycle_db.classification_table";

                        using (var command = new MySqlCommand(query, ReLifeCycleDB))
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string code = reader["nl_sfb_code"].ToString();
                                string name = reader["nl_sfb_name_en"].ToString();
                                nlsfbClasses.Add((code, name));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Display error message when connection with the ReLifeCycle database failed
                        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Database error: A problem occured while trying to connect the ReLifeCycle database");
                    }
                }

                // Add the retrieved NL/SfB classes to the value list
                foreach (var (code, name) in nlsfbClasses)
                {
                    nlsfbValueList.ListItems.Add(new GH_ValueListItem($"{code} {name}", $"\"{code}\""));
                }

                // Add valuelist to document
                document.AddObject(nlsfbValueList, false);

                // Position the value list near the component
                var componentPivot = owner.Attributes.Pivot;
                nlsfbValueList.Attributes.Pivot = new PointF(componentPivot.X - 450, componentPivot.Y -11);

                // Connect the material slider to the "Index" input (second input)
                owner.Params.Input[0].AddSource(nlsfbValueList);
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Create NL/SfB class input variable
            string nLsfbClass = "";

            // Retrieve selected NL/SfB class from input parameter
            if (!DA.GetData(0, ref nLsfbClass)) return;

            // Creates datatree for material set data output 
            DataTree<object> materialSet = new DataTree<object>();

            // Creates column descriptions for each data column
            var columnDescriptions = new List<string>
            {
                "NL/SfB Code", 
                "NL/SfB Name", 
                "NMD ID", 
                "NIBE ID", 
                "ArchiCalc ID", 
                "NMD Category", 
                "Product Description", 
                "Unit", 
                "Thickness [m]",
                "Technical Lifespan [years]", 
                "GWP A1-A2-A3 [kg CO2 eq.]",
                "GWP A4 [kg CO2 eq.]",
                "GWP A5 [kg CO2 eq.]",
                "GWP B1 [kg CO2 eq.]",
                "GWP B2 [kg CO2 eq.]",
                "GWP B3 [kg CO2 eq.]",
                "GWP B4 [kg CO2 eq.]",
                "GWP B5 [kg CO2 eq.]",
                "GWP C1 [kg CO2 eq.]",
                "GWP C2 [kg CO2 eq.]",
                "GWP C3 [kg CO2 eq.]",
                "GWP C4 [kg CO2 eq.]",
                "GWP D [kg CO2 eq.]", 
                "MKI A1-A2-A3 [€]",
                "MKI A4 [€]",
                "MKI A5 [€]",
                "MKI B1 [€]",
                "MKI B2 [€]",
                "MKI B3 [€]",
                "MKI B4 [€]",
                "MKI B5 [€]",
                "MKI C1 [€]",
                "MKI C2 [€]",
                "MKI C3 [€]",
                "MKI C4 [€]",
                "MKI D [€]",
                "Mass per Unit [kg]", 
                "CSC [kg CO2 eq.]", 
                "% New", 
                "% Biobased", 
                "% Recycled", 
                "% Reused", 
                "% Landfill", 
                "% Burning", 
                "% Recycling", 
                "% Reusing", 
                "DP [%]",
                "MCI [%]",
                "PCI [%]", 
                "Material Costs [€]", 
                "Labour Costs [€]"
            };

            // Retrieve material data from MySQL ReLifeCycle database for selected NL/SfB class
            using (var ReLifeCycleDB = new MySqlConnection(connectionString))
            {
                try
                {
                    ReLifeCycleDB.Open();

                    string query = $@"
                    SELECT 
                        nl_sfb_code, 
                        nl_sfb_name_en, 
                        nmd_id, 
                        nibe_id, 
                        archicalc_id, 
                        nmd_category, 
                        product_description_en, 
                        unit, 
                        thickness,
                        technical_lifespan, 
                        gwp_a1_a2_a3, 
                        gwp_a4, 
                        gwp_a5, 
                        gwp_b1, 
                        gwp_b2, 
                        gwp_b3, 
                        gwp_b4, 
                        gwp_b5, 
                        gwp_c1, 
                        gwp_c2, 
                        gwp_c3, 
                        gwp_c4, 
                        gwp_d, 
                        mki_a1_a2_a3, 
                        mki_a4, 
                        mki_a5, 
                        mki_b1, 
                        mki_b2, 
                        mki_b3, 
                        mki_b4, 
                        mki_b5, 
                        mki_c1, 
                        mki_c2, 
                        mki_c3, 
                        mki_c4, 
                        mki_d, 
                        mass_per_unit, 
                        csc, 
                        `%_new`, 
                        `%_biobased`, 
                        `%_recycled`, 
                        `%_reused`, 
                        `%_landfill`, 
                        `%_burning`, 
                        `%_recycling`, 
                        `%_reusing`, 
                        dp, 
                        mci, 
                        pci, 
                        material_costs_per_unit, 
                        labour_costs_per_unit
                    FROM relifecycle_db.relifecycle_joined_db
                    WHERE nl_sfb_code = @nLsfbClass";

                    using (var command = new MySqlCommand(query, ReLifeCycleDB))
                    {
                        command.Parameters.AddWithValue("@nLsfbClass", nLsfbClass);

                        using (var reader = command.ExecuteReader())
                        {
                            int rowIndex = 0;

                            while (reader.Read())
                            {
                                // Loop through each column for the current row
                                for (int colIndex = 0; colIndex < reader.FieldCount; colIndex++)
                                {
                                    // Assign a column description to each data value
                                    string description = columnDescriptions[colIndex];
                                    object value = reader.GetValue(colIndex);

                                    // Create a path: {rowIndex; colIndex}
                                    var path = new GH_Path(rowIndex, colIndex);

                                    // Add the description and value separately
                                    materialSet.Add(description, path);  // First item (description)
                                    materialSet.Add(value, path);        // Second item (value)
                                }

                                rowIndex++;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Display error message when connection with the ReLifeCycle database failed
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Database error: A problem occured while trying to connect the ReLifeCycle database");
                }

            }

            // Create variable "materialNames" that returns a list of names of all materials in the material set
            List<string> materialNames = new List<string>();

            // Iterate over all branches in materialSet
            foreach (GH_Path path in materialSet.Paths)
            {
                // Get the list of items in this branch
                var items = materialSet.Branch(path);

                // Check if the first item of a data tree branch has the string value "Product Description"
                if (items[0] is string firstItem && firstItem == "Product Description")
                {
                    // Add the second item of the data tree branch with first item "Product Description" to the list of material names
                    if (items[1] is string secondItem)
                    {
                        materialNames.Add(secondItem);
                    }
                }
            }

            // Set outputs
            DA.SetDataTree(0, materialSet);     // Set the Material Set data tree
            DA.SetDataList(1, materialNames);   // Set the Material Names list

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
                using (var stream = typeof(CreateMaterialSetComponent).Assembly.GetManifestResourceStream("ReLifeCycleGHPlugin.Resources.CreateMaterialSetIcon.png"))
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
            get { return new Guid("2F30B691-1075-4CEE-9EF6-73F00811D06B"); }
        }
    }
}
