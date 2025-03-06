using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System.Drawing;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel.Attributes;

namespace ReLifeCycleGHPlugin
{
    //  ███████╗███████╗██╗     ███████╗ ██████╗████████╗    ███╗   ███╗ █████╗ ████████╗███████╗██████╗ ██╗ █████╗ ██╗     
    //  ██╔════╝██╔════╝██║     ██╔════╝██╔════╝╚══██╔══╝    ████╗ ████║██╔══██╗╚══██╔══╝██╔════╝██╔══██╗██║██╔══██╗██║     
    //  ███████╗█████╗  ██║     █████╗  ██║        ██║       ██╔████╔██║███████║   ██║   █████╗  ██████╔╝██║███████║██║     
    //  ╚════██║██╔══╝  ██║     ██╔══╝  ██║        ██║       ██║╚██╔╝██║██╔══██║   ██║   ██╔══╝  ██╔══██╗██║██╔══██║██║     
    //  ███████║███████╗███████╗███████╗╚██████╗   ██║       ██║ ╚═╝ ██║██║  ██║   ██║   ███████╗██║  ██║██║██║  ██║███████╗
    //  ╚══════╝╚══════╝╚══════╝╚══════╝ ╚═════╝   ╚═╝       ╚═╝     ╚═╝╚═╝  ╚═╝   ╚═╝   ╚══════╝╚═╝  ╚═╝╚═╝╚═╝  ╚═╝╚══════╝
    //

    public class SelectMaterialComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SelectMaterialComponent class.
        /// </summary>=

        public SelectMaterialComponent()
          : base("Select Material", "SelectMat",
              "Select a material from the material set",
              "ReLifeCycle", "    Material")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Material Set Data", "MS", "Data from the material set", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Index", "i", "Index for selecting a material from the material set. A slider will automatically pop up when the Material Set Data input is connected! The slider maximum updates based on the number of materials in the material set."
                + "You can use this slider as input for optimisation variables of third-party Grasshopper optimisation plugins.", GH_ParamAccess.item, 0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Material Data", "M", "Data for the material", GH_ParamAccess.tree);
        }

        /// <summary>
        // Create two custom display screens inside the component's UI: one for material name and one for unit and geometry type
        /// </summary>

        public string materialNameDisplay { get; private set; } = "No Material Selected";
        public string unitGeometryDisplay { get; private set; } = "Unknown Geometry Type";

        public override void CreateAttributes()
        {
            m_attributes = new MaterialNameDisplay(this);
        }

        public class MaterialNameDisplay : GH_ComponentAttributes
        {
            // Create two rectangles
            private RectangleF _materialRect;
            private RectangleF _unitRect;

            public MaterialNameDisplay(SelectMaterialComponent owner) : base(owner)
            {
                _materialRect = new RectangleF(0, 0, 0, 0);
                _unitRect = new RectangleF(0, 0, 0, 0);
            }

            // Set display screens layout
            protected override void Layout()
            {
                base.Layout();

                // Set rectangle dimensions
                int rectHeight = 20;
                float padding = 5;
                float spacing = 5;

                using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    Font customFont = new Font("Segoe UI", 6, FontStyle.Regular);
                    Font paramFont = GH_FontServer.Standard;

                    string materialName = (Owner as SelectMaterialComponent)?.materialNameDisplay ?? "No Material Selected";
                    string unitGeometry = (Owner as SelectMaterialComponent)?.unitGeometryDisplay ?? "Unknown Geometry Type";

                    float materialTextWidth = g.MeasureString(materialName, customFont).Width;
                    float unitTextWidth = g.MeasureString(unitGeometry, customFont).Width;

                    // Get longest output parameter name
                    float maxOutputWidth = 0;
                    foreach (IGH_Param output in Owner.Params.Output)
                    {
                        float outputTextWidth = g.MeasureString(output.NickName, paramFont).Width;
                        maxOutputWidth = Math.Max(maxOutputWidth, outputTextWidth);
                    }

                    // Determine required additional width
                    float requiredTextWidth = Math.Max(materialTextWidth, unitTextWidth);
                    float requiredWidth = Math.Max(requiredTextWidth, maxOutputWidth) + 2 * padding;

                    // Calculate additional width needed
                    float extraWidth = Math.Max(0, requiredWidth - Bounds.Width);

                    // Update component bounds by increasing the width
                    Bounds = new RectangleF(
                        Bounds.X,
                        Bounds.Y,
                        Bounds.Width + extraWidth, // Preserve original width, just add extra if needed
                        Bounds.Height + (2 * rectHeight) + spacing + (2 * padding)
                    );
                }

                // Update rectangle positions
                float pivotX = Bounds.Left + padding;
                float pivotY = Bounds.Bottom - ((2 * rectHeight) + spacing + padding);

                _materialRect = new RectangleF(pivotX, pivotY, Bounds.Width - 2 * padding, rectHeight);
                _unitRect = new RectangleF(pivotX, _materialRect.Bottom + spacing, Bounds.Width - 2 * padding, rectHeight);
            }



            // Set display screen render settings
            protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
            {
                base.Render(canvas, graphics, channel);

                if (channel == GH_CanvasChannel.Objects)
                {
                    // Define colors
                    Color fillColor = Color.LightGray;
                    Color borderColor = Color.Black;
                    Color textColor = Color.Black;

                    // Render Material Name Rectangle
                    Brush fillBrush = new SolidBrush(fillColor);
                    Pen borderPen = new Pen(borderColor);
                    graphics.FillRectangle(fillBrush, _materialRect);
                    graphics.DrawRectangle(borderPen, _materialRect.X, _materialRect.Y, _materialRect.Width, _materialRect.Height);

                    // Render Unit Geometry Rectangle
                    graphics.FillRectangle(fillBrush, _unitRect);
                    graphics.DrawRectangle(borderPen, _unitRect.X, _unitRect.Y, _unitRect.Width, _unitRect.Height);

                    // Set text font and alignment
                    Font customFont = new Font("Segoe UI", 6, FontStyle.Regular);
                    StringFormat format = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };

                    // Draw Material Name text
                    string materialName = (Owner as SelectMaterialComponent)?.materialNameDisplay ?? "No Material Selected";
                    graphics.DrawString(materialName, customFont, Brushes.Black, _materialRect, format);

                    // Draw Unit Geometry text
                    string unitGeometry = (Owner as SelectMaterialComponent)?.unitGeometryDisplay ?? "Unknown Geometry Type";
                    graphics.DrawString(unitGeometry, customFont, Brushes.Black, _unitRect, format);
                }
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>

        // Add a pop-up slider when the "Select Material" component is placed on the canvas.
        // This slider is used to select a material from the material set. 
        // The sliders maximum index is updated automatically based on the number of materials in a material set.


        // Boolean flag to track if the material slider is already added
        private bool sliderExists = false;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Create input variables
            GH_Structure<IGH_Goo> materialSet = new GH_Structure<IGH_Goo>();
            int sliderIndex = 0;

            // Retrieve data from input parameters
            if (!DA.GetDataTree(0, out materialSet)) return;
            if (!DA.GetData(1, ref sliderIndex)) return; // Retrieve the slider value (Index) to filter the tree

            // Get the document
            var document = OnPingDocument();

            // Check if the "Material Set Data" input parameter is connected
            var materialSetInput = Params.Input[0];
            var indexInput = Params.Input[1];

            if (materialSet.IsEmpty)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Material Set is empty.");
            }

            // Find the maximum value of the first path index
            int maxFirstIndex = 0;
            foreach (GH_Path path in materialSet.Paths)
            {
                if (path.Indices.Length > 0)
                {
                    maxFirstIndex = Math.Max(maxFirstIndex, path[0]);
                }
            }

            // Detect if a slider is already connected (ensures detection even after copy-paste)
            var existingSlider = indexInput.SourceCount > 0 ? indexInput.Sources[0] as GH_NumberSlider : null;
            sliderExists = existingSlider != null;

            // Add or update the slider if Material Set Data is connected and maxFirstIndex > 0
            if (materialSetInput.SourceCount > 0 && maxFirstIndex > 0)
            {
                // Check if a slider is already connected
                if (!sliderExists)
                {
                    document.ScheduleSolution(5, doc =>
                    {
                        if (indexInput.SourceCount > 0) return; // Prevent adding a new slider if one is now connected

                        GH_NumberSlider newSlider = new GH_NumberSlider();

                        // Set slider attributes
                        newSlider.CreateAttributes();
                        newSlider.NickName = "Material Index";
                        newSlider.Slider.Type = Grasshopper.GUI.Base.GH_SliderAccuracy.Integer;
                        newSlider.Slider.Minimum = 0;
                        newSlider.Slider.Maximum = Math.Max(1, maxFirstIndex); // This value will be updated dynamically
                        newSlider.Slider.Value = 0;

                        // Add the slider to the document and position it near the component
                        doc.AddObject(newSlider, false);
                        newSlider.Attributes.Pivot = new System.Drawing.PointF(this.Attributes.Pivot.X - 350, this.Attributes.Pivot.Y + 100);

                        // Connect the new slider to the "Index" input of the component (second input)
                        indexInput.AddSource(newSlider);

                        // Update flag
                        sliderExists = true;
                    });
                }
                // Update existing slider if the maxFirstIndex has changed
                else
                {
                    if (existingSlider != null && existingSlider.Slider.Maximum != maxFirstIndex)
                    {
                        existingSlider.Slider.Maximum = maxFirstIndex;
                        existingSlider.ExpireSolution(true);
                    }
                }
            }
            // Remove the slider if maxFirstIndex == 0 
            else
            {
                if (sliderExists)
                {
                    if (existingSlider != null)
                    {
                        document.ScheduleSolution(5, doc =>
                        {
                            // Check if the maxFirstIndex is 0
                            if (maxFirstIndex == 0)
                            {
                                doc.RemoveObject(existingSlider, false);

                                // Reset flag
                                sliderExists = false;
                            }
                        });
                    }
                }
            }

            // Filter the branches in the data tree based on the slider index to retrieve data for one material
            GH_Structure<IGH_Goo> materialData = new GH_Structure<IGH_Goo>();
            foreach (GH_Path path in materialSet.Paths)
            {
                // Match branches with the slider index as the first path index
                if (path.Indices.Length > 0 && path[0] == sliderIndex)
                {
                    // Cast the branch to IEnumerable<IGH_Goo> before adding it
                    var branch = materialSet.get_Branch(path) as IEnumerable<IGH_Goo>;
                    if (branch != null)
                    {
                        materialData.AppendRange(branch, path);
                    }
                }
            }

            // Create variable "materialName" that stores the name of the selected material
            string materialName = "";

            foreach (GH_Path path in materialData.Paths)
            {
                var items = materialData.get_Branch(path);

                // Check if the first item in the branch is a string with value "Product Description" and assign that value to the materialName variable
                if (items[0] is IGH_Goo firstItemGoo && firstItemGoo.CastTo(out string firstItem) && firstItem == "Product Description")
                {
                    // Retrieve the second item
                    if (items[1] is IGH_Goo secondItemGoo && secondItemGoo.CastTo(out string secondItem))
                    {
                        materialName = secondItem;
                        break;
                    }
                }
            }

            // Create variable "unit" that returns the unit of the selected material
            string unit = "";
            string unitGeometry = ""; // Interpolated string that combines the unit with its required geometry type

            // Retrieve second item of the data tree branch with as first item value a string "Unit"
            foreach (GH_Path path in materialData.Paths)
            {
                var items = materialData.get_Branch(path);

                // Check if the first item in the branch is a string with value "Unit". The IGH_Goo object is unwrapped first to retrieve the string value
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

            // Displays which geometry input is necessary for which unit
            if (unit.Equals("m2"))
            {
                unitGeometry = $"{unit} (use surfaces)";
            }

            else if (unit.Equals("m3"))
            {
                unitGeometry = $"{unit} (use solids)";
            }

            else
            {
                unitGeometry = $"{unit} (unknown geometry type)";
            }

            // Update materialNameDisplay and unitGeometry display properties for the display screen
            materialNameDisplay = materialName;
            unitGeometryDisplay = $"Unit: {unitGeometry}";

            // Set output parameter data
            materialData.Simplify(GH_SimplificationMode.CollapseAllOverlaps); // simplify material data tree
            DA.SetDataTree(0, materialData); // output material data tree
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
                using (var stream = typeof(SelectMaterialComponent).Assembly.GetManifestResourceStream("ReLifeCycleGHPlugin.Resources.SelectMaterialIcon.png"))
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
            get { return new Guid("8216C1B0-25AD-4986-B1AB-79583B28A210"); }
        }
    }
}