Welcome to the ReLifeCycle repository!


**General Info**
- ReLifeCycle is developed as a Grasshopper plugin developed for responsible material use assessment (environmental impact, circularity and financial impact) as part of my Master Graduation Project at Eindhoven University of Technology.
- This repository contains all relevant code, schemas, and example files for the project.
- To install and set up the ReLifeCycle plugin, follow the instructions below.
- ⚠ IMPORTANT: The original material data cannot be published. To use the plugin, you must populate the ReLifeCycle database with your own material data.


**Report**
- Link to thesis will be placed here after publication


**Demo videos**
- Links to demo videos will be placed here soon


**Example Grasshopper files**
You can find multiple example files under the "Example Files" folder of this repository. The example files demonstrate the workings of ReLifeCycle, how to connect a user interface and how to integrate it with the Galapagos and Wallacei third-party optimisation plugins.


**Requirements for setting up ReLifeCycle Grasshopper plugin**
- Visual Studio (recommended): https://visualstudio.microsoft.com/downloads/
- MySQL Workbench: https://dev.mysql.com/downloads/workbench/
- Grasshopper for Rhino: https://www.rhino3d.com/download/

     
**1. Setting up MySQL database**
1. Open MySQL Workbench
2. Create a new MySQL server if you don't already have one (MySQL Connections > +)
3. Open your server and open the "relifecycle_db_schema.sql" file under the "Database" folder of this repository
4. Run the SQL script by clicking the lightning bolt and the relifecycle_db should appear in the Navigator
5. ⚠ IMPORTANT: Don't change anything of the database schema structure. Keep all table, view and column names as they are.
6. Fill the database with your own material data. The schema serves as a template and contains one example material. Read the table descriptions below for clarification:

- nmd_db: Table for environmental data. Within this table you can map materials from the "nibe_db" and "archicalc_db" tables by inserting their corresponding "nibe_id" and "archicalc_id" unique identifiers.
- nibe_db: Table for circularity data.
- archicalc_db: Table for financial data.
- classification_table: Table with building element classifications. This is already filled with data, so you can keep this as it is.
- classification_junction_table: Table for categorising materials from the "nmd_db" into a specific building element class. Check the "classification_table" to determine under which class your material entry falls. You can add a material to multiple classifications.


**2. Setting up Visual Studio**
Make sure you download the following frameworks and packages:
- .NET framework (latest release for Rhino 8, .NET 4.8 for earlier versions of Rhino): https://dotnet.microsoft.com/en-us/download/dotnet-framework
- Grasshopper SDK package (Project > Manage NuGet Packages...)
- MySql.Data package (Project > Manage NuGet Packages...)

   
**3. Setting up ReLifeCycle Grasshopper plugin**
1. Clone this repository locally
2. Open Visual Studio
3. Navigate to the "Code" folder from this repository and open the "ReLifeCycleGHPlugin.sln" file
4. Navigate to line 38 of the "CreateMaterialSetComponent.cs" script and change the connectionString to your MySQL server by filling in the placeholder X's:
   ("server=XXX.X.X.X;user=XX;database=relifecycle_db;port=XX;password=XX)
5. Repeat step 3 for line 32 of the "NLSfBComponent.cs" script
6. Select "Release" from the dropdown menu in the top bar of Visual Studio and click Build > Build Solution
7. In file explorer, navigate to the local ReLifeCycle repository to bin > Release
8. Open the folder that corresponds to your Rhino version (net7.0 for Rhino 8, net48 for earlier versions of Rhino)
9. Copy all the files in the folder and paste in a new folder in the Grasshopper Libraries folder (C:\Users\...\AppData\Roaming\Grasshopper\Libraries)
10. Open Rhino and Grasshopper and the ReLifeCycle plugin should be there!


**4. Setting up ReLifeCycle interface**
1. Make sure you download the following Grasshopper plugins:
- Human UI: https://www.food4rhino.com/en/app/human-ui
- Metahopper: https://www.food4rhino.com/en/app/metahopper
2. Follow the steps in the "relifecycle_example_script_interface.gh" example file


**4. Setting up ReLifeCycle Wallacei Multi-Objective Optimisation integration**
1. Make sure you download the following Grasshopper plugins:
- Wallacei: https://www.food4rhino.com/en/app/wallacei
- Human UI: https://www.food4rhino.com/en/app/human-ui
- Metahopper: https://www.food4rhino.com/en/app/metahopper
2. Follow the steps in the "relifecycle_example_script_wallacei_integration.gh" example file
