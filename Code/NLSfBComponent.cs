using System;
using System.Drawing;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using MySql.Data.MySqlClient;

namespace ReLifeCycleGHPlugin
{
    //  ███╗   ██╗██╗         ██╗███████╗███████╗██████╗                                  
    //  ████╗  ██║██║        ██╔╝██╔════╝██╔════╝██╔══██╗                                 
    //  ██╔██╗ ██║██║       ██╔╝ ███████╗█████╗  ██████╔╝                                 
    //  ██║╚██╗██║██║      ██╔╝  ╚════██║██╔══╝  ██╔══██╗                                 
    //  ██║ ╚████║███████╗██╔╝   ███████║██║     ██████╔╝                                 
    //  ╚═╝  ╚═══╝╚══════╝╚═╝    ╚══════╝╚═╝     ╚═════╝                                  
    //                                                                                    
    //   ██████╗ ██████╗ ███╗   ███╗██████╗  ██████╗ ███╗   ██╗███████╗███╗   ██╗████████╗
    //  ██╔════╝██╔═══██╗████╗ ████║██╔══██╗██╔═══██╗████╗  ██║██╔════╝████╗  ██║╚══██╔══╝
    //  ██║     ██║   ██║██╔████╔██║██████╔╝██║   ██║██╔██╗ ██║█████╗  ██╔██╗ ██║   ██║   
    //  ██║     ██║   ██║██║╚██╔╝██║██╔═══╝ ██║   ██║██║╚██╗██║██╔══╝  ██║╚██╗██║   ██║   
    //  ╚██████╗╚██████╔╝██║ ╚═╝ ██║██║     ╚██████╔╝██║ ╚████║███████╗██║ ╚████║   ██║   
    //   ╚═════╝ ╚═════╝ ╚═╝     ╚═╝╚═╝      ╚═════╝ ╚═╝  ╚═══╝╚══════╝╚═╝  ╚═══╝   ╚═╝   
    //                                                                                    

    public class NLSfBComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the NLSfBComponent class.
        /// </summary>

        // Creates MySQL connection string. CHANGE THE PLACEHOLDER X'S WITH YOUR OWN DATABASE SERVER INFORMATION
        private string connectionString = "server=XXX.X.X.X;user=XX;database=relifecycle_db;port=XX;password=XX";

        public NLSfBComponent()
          : base("NL/SfB Classes", "NL/SfB",
              "Value list of all NL/SfB classes",
              "ReLifeCycle", " Utility")
        {
            // Check if the connection string contains the placeholder "XXX"
            if (connectionString.Contains("XXX"))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Please change the SQL string in the source code to connect to your database server.");
            }
        }

        /// <summary>
        /// Override the AddedToDocument method to add a pop-up Value List with NL/SfB classifciations when the "Create Material Set" component is placed on the canvas.
        /// </summary>
        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);

            // Create a new GH_ValueList and populate it with NL/SfB classifications
            var nlSfbValueList = new GH_ValueList
            {
                // Set value list name and mode
                NickName = "NL/SfB Class",
                ListMode = GH_ValueListMode.DropDown
            };

            // Clear any default items
            nlSfbValueList.ListItems.Clear();

            // Retrieve NL/SfB classes from MySQL ReLifeCycle database
            var nlsfbClasses = new List<(string code, string name)>();

            using (var ReLifeCycleDB = new MySqlConnection(connectionString))
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
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Database error: A problem occured while trying to connect the ReLifeCycle database");
                }
            }

            // Add the retrieved NL/SfB classes to the value list
            foreach (var (code, name) in nlsfbClasses)
            {
                nlSfbValueList.ListItems.Add(new GH_ValueListItem($"{code} {name}", $"\"{code}\""));
            }

            // Add the value list to the document
            document.AddObject(nlSfbValueList, false);
            nlSfbValueList.Attributes.Pivot = new System.Drawing.PointF(this.Attributes.Pivot.X, this.Attributes.Pivot.Y);

            // Remove empty component from canvas so only the NL/SfB value list remains
            document.ScheduleSolution(5, doc => doc.RemoveObject(this, false));
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // Load the embedded icon
                using (var stream = typeof(NLSfBComponent).Assembly.GetManifestResourceStream("ReLifeCycleGHPlugin.Resources.NLSfBIcon.png"))
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
            get { return new Guid("7C7AD56B-3B37-4F99-B729-FFBC1B51C9DB"); }
        }
    }
}