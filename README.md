# ReLifeCycle
## Grasshopper Plugin for Responsible Material Use Assessment


Welcome to the ReLifeCycle repository!


## Overview
ReLifeCycle is developed as a Grasshopper plugin for responsible material use assessment (environmental impact, circularity and financial impact) as part of my Master Graduation Project at Eindhoven University of Technology. This repository contains all relevant code, schemas, and example files for the project. If there are any questions, feel free to contact me at pvanrijsbergen@gmail.com.

Below you will find instructions for setting up the RelifeCycle plugin.

⚠ IMPORTANT: The original material data cannot be published. To use the plugin, you must populate the ReLifeCycle database with your own material data.

## Contents

**In the repository**
- **Code:** Folder containing all source code for the plugin.
- **Database:** Folder containing a MySQL database schema for the material database.
- **Example files:** Folder containing Grasshopper example files demonstrating the workings of ReLifeCycle, how to connect the user interface and how to integrate ReLifeCycle with the Galapagos and Wallacei third-party optimisation plugins.

**Additional resources**
- **Thesis:** Link to thesis will be placed here after publication.
- **Demo videos:**
     - ReLifeCycle Demo 1: Workflow and Interface: https://www.youtube.com/watch?v=AK7u2ayUnnw&t
     - ReLifeCycle Demo 2: Integration with Wallacei Multi-Objective Optimisation Plugin: https://www.youtube.com/watch?v=xFewBShcyd0&t


## Set up Instructions

**0. Requirements**

Before setting up the ReLifeCycle Grasshopper plugin, ensure you have the following software installed:
- Visual Studio (highly recommended): https://visualstudio.microsoft.com/downloads/
- MySQL Workbench: https://dev.mysql.com/downloads/workbench/
- Grasshopper for Rhino 8 (earlier versions may work, but have not been tested): https://www.rhino3d.com/download/

     
**1. Setting up MySQL database**
1. Open MySQL Workbench.
2. Create a new MySQL server if you don't have one already (Go to MySQL Connections > click the + icon).
3. Open your server and open the "relifecycle_db_schema.sql" file from the "Database" folder in this repository.
4. Execute the SQL script by clicking the lightning bolt icon. The relifecycle_db should now appear in the Navigator.
5. ⚠ IMPORTANT: Do not modfiy the database schema structure. Keep all table, view and column names as they are.
6. Populate the database with your own material data. The schema serves as a template and contains one example material. Below are the table descriptions for clarification:

- **nmd_db:** Table for environmental data. Within this table you can map materials from the "nibe_db" and "archicalc_db" tables by inserting their corresponding "nibe_id" and "archicalc_id" unique identifiers.
- **nibe_db:** Table for circularity data.
- **archicalc_db:** Table for financial data.
- **classification_table:** Table with building element classifications. This is already filled with data, so you can keep this as it is.
- **classification_junction_table:** Table for categorising materials from the "nmd_db" into a specific building element class. Check the "classification_table" to determine under which class your material entry falls. You can add a material to multiple classifications.


**2. Setting up Visual Studio**
1. Ensure the following frameworks and packages are installed in Visual Studio:
     - .NET framework (latest release for Rhino 8, .NET 4.8 for earlier versions of Rhino): https://dotnet.microsoft.com/en-us/download/dotnet-framework
     - Grasshopper SDK package: Install via Project > manage NuGet Packages
     - MySql.Data package: Install via Project > manage NuGet Packages

   
**3. Setting up ReLifeCycle Grasshopper plugin**
1. Clone this repository locally.
2. Open Visual Studio.
3. Navigate to the "Code" folder in this repository and open the "ReLifeCycleGHPlugin.sln" file.
4. Navigate to line 38 of the "CreateMaterialSetComponent.cs" script and change the connectionString to your MySQL server by filling in the placeholders:
   ("server=XXX.X.X.X;user=XX;database=relifecycle_db;port=XX;password=XX).
5. Repeat step 4 for line 32 of the "NLSfBComponent.cs" script.
6. Select "Release" from the dropdown menu in the top bar of Visual Studio and click Build > Build Solution.
7. In file explorer, navigate to the local ReLifeCycle repository folder. Go to bin > Release.
8. Open the folder corresponding to your Rhino version (net7.0 for Rhino 8, net48 for earlier versions of Rhino).
9. Copy all files from this folder and paste them into a new folder in the Grasshopper Libraries folder (located at C:\Users\...\AppData\Roaming\Grasshopper\Libraries).
10. Open Rhino and Grasshopper and the ReLifeCycle plugin should be there!


**4. OPTIONAL: Setting up ReLifeCycle interface**
1. Ensure the following Grasshopper plugins are downloaded:
     - Human UI: https://www.food4rhino.com/en/app/human-ui
     - Metahopper: https://www.food4rhino.com/en/app/metahopper
2. Follow the steps in the "relifecycle_example_script_interface.gh" example file


**5. OPTIONAL: Setting up ReLifeCycle Wallacei Multi-Objective Optimisation integration**
1. Ensure the following Grasshopper plugins are downloaded:
     - Wallacei: https://www.food4rhino.com/en/app/wallacei
     - Human UI: https://www.food4rhino.com/en/app/human-ui
     - Metahopper: https://www.food4rhino.com/en/app/metahopper
2. Follow the steps in the "relifecycle_example_script_wallacei_integration.gh" example file
