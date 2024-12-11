# BRAND.CSharp

## Overview
`BRAND.CSharp` is a general purpose BRAND client for .NET languages. It allows applications developed in under any .NET frameworks (e.g., C# and Unity) to be seamlessly connected to the BRAND architecture.

This library uses `StackExchange.Redis` and `Newtonsoft.Json` for handling communication with the BRAND graph architecture.

## Setup
### For .NET Applications
#### 1. Install `BRAND.CSharp.dll`

<details>
<summary>Manually</summary>

Anywhere in your project folder, download the `BRAND.CSharp.dll` found [here](https://api.github.com/repos/brandbci/BRAND.CSharp/releases).

</details>

<details>
<summary>Command line</summary>
Run the following command in your terminal

```
curl -s https://api.github.com/repos/brandbci/BRAND.CSharp/releases/latest \
| grep "browser_download_url.*BRAND\.CSharp\.dll" \
| cut -d : -f 2,3 \
| tr -d \" \
| wget -qi - -P YOUR_FOLDER_PATH
```

Note: Replace `YOUR_FOLDER_PATH` with where you want the `dll` to be downloaded.
</details>

#### 2. Add a Project Reference
<details>
<summary>Visual Studio</summary>

If you are using Visual Studio, add a reference to `BRAND.CSharp.dll` by navigating to `Project` > `Add Project Reference` > `.NET Assembly`. Then browse for the location of `BRAND.CSharp.dll`

</details>

<details>
<summary>Visual Studio Code (Manual)</summary>
Add the following code snippet into your `.csproj` file.

```
<ItemGroup>
  <Reference Include="BRAND.CSharp">
    <HintPath>.\[RELATIVE_PATH]\BRAND.CSharp.dll</HintPath>
  </Reference>
</ItemGroup>
```
</details>


### For Unity Applications
Visit [BRAND-Unity project template]() for more instructions

## Documentation
- [Basic Usage](brand-unity-library-basic-setup)
- [How `BRANDNode` Works]()
- [Building and Running]()

## Contribute
We welcome contributions from the community! Please review the following options to see how you can get started.

1. Open a new issue. Our team will try our best to review them.
2. Fork the project and open a new PR. There should be no special set up needed!




