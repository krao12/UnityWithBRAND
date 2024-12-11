# BRAND-Unity Project Template
## What is included in this template?
- Unity 3D project already configured with the correct BRAND settings
- A setup script (`setup.sh`) if you already have an existing Unity project
- A `Makefile` to build the Unity project automatically

You can choose to only download `setup.sh` and `Makefile` if you would love to create your own Unity project or already have an existing project.

## Set up
1. Clone this repo locally
2. Run `setup.sh` in your terminal
3. Open the project in Unity and wait for the project to reload. If you encounter the following screen when opening up the project, choose `Ignore`.

## Build the Project
1. Locate the `Unity.app` on your local machine.
2. In the `Makefile`, replace [] with the path to XXX
3. Run `make` in the terminal as you would with any BRAND node!

## Troubleshooting
If you encounter any of the following errors:
<details>
<summary>Missing compiler required member `Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo.Create`</summary>

Go to `File` > `Build Settings` > `Player Settings` > `Other Settings` > Change `API Compatibility Level` to either .NET 4x or .NET Framework
</details>


