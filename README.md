# Overview 

This repository demonstrates how to build a VSIX to include a template engine template pack for use in Visual Studio 16.8 and later.

# To build this repository:

This which includes building the template package, the stub vstemplate file and the VSIX

```msbuild.exe TemplateSample.sln /t:Restore;Build```


# Walkthrough of creating the installable VSIX with template

The folllowing sections describe the steps needed to create an installable VSIX for installation into Visual Studio 2019 16.3 or later. 

## Create template and template package

The first step is to create the .NET Core Template Engine template and package it into a nupkg for deployment. You can read more details about template creation [here](https://docs.microsoft.com/en-us/dotnet/core/tools/custom-templates). 

Notable pieces of information in the [sample template package](https://github.com/phenning/templateSamples/tree/master/templatepackages/TemplatePackage) which will be needed for later steps are the ```groupIdentity``` and the ```language``` and ```type``` tags:

```json
  "tags": {
    "language": "C#",
    "type": "project"
  }, 
  "groupIdentity": "MyCompany.Common.Console",
```
With the generated nupkg, you could now install this template pack into the command line .NET Core CLI via ```dotnet new -i template.nupkg``` and invoke it via ```dotnet new MyCompany.Common.Console```.

Another thing to note is that the nupkg file structure is important. You can have one or more templates in a template pack, but they need to be in indidual of a folder named "content" at the package root.

## Create Vsix for vstemplate installation

The last step is to create the [VSIX installer]((https://github.com/phenning/templateSamples/tree/master/vsix))

In this sample, we add a project reference to the vstemplate project previously created. Note, that in newer versions (2022 and later) of Visual Studio, you can skip creating the vstemplate and adding it to the VSIX, as Visual Studio will automatically discover templates packs installed. However, if you wish to continue to invoke through a vstemplate or use a custom wizard, you need to add the same metadata to an ide.host.json as shown [here](https://github.com/phenning/templateSamples/tree/master/templatepackages/templatepackages/TemplatePackage/content/MyCompany.Common.Console.CSharp/.template.config/ide.host.json). This will cause Visual Studio to still install the template so it can be invoked from a vstemplate file, but won't list the template itself - only the vstemplate will be shown.

```json
{
   "unsupportedHosts": [ 
       { "id": "vs"  } 
   ]
}
```

We need to add the generated nupkg to the VSIX. In order to do this, we need to update the VSIX project file and manually add the following [target](https://github.com/phenning/templateSamples/blob/3638ff51d04ae637591508e7c2848cbdb988e2e8/vsix/TemplateVsix.csproj#L73). Note that this step is needed even if the vstemplate(s) are removed from the project. This will cause the nupkg to be pckaged in a ProjectTemplates subfolder under our package installation path.

```xml
  <Target Name="PreCreateVsixContainer" BeforeTargets="GetVsixSourceItems">
    <ItemGroup>
      <_TemplatePackage Include="..\templatepackages\TemplatePackage\bin\$(Configuration)\MyCompany.SampleTemplates.*.nupkg" />
    </ItemGroup>
    <Error Text="No template files found." Condition="@(_TemplatePackage-&gt;Count()) == 0" />
    <ItemGroup>
      <VSIXSourceItem Include="@(_TemplatePackage)">
        <VSIXSubPath>ProjectTemplates\</VSIXSubPath>
      </VSIXSourceItem>
    </ItemGroup>
  </Target>
```

Additionally, we need to add the following [pkgdef](https://github.com/phenning/templateSamples/blob/master/vsix/Templates.pkgdef) to the VSIX which tells the template discovery where to find the template nupkg.

```pkgdef
[$RootKey$\TemplateEngine\Templates\MyCompany.SampleTemplates\1.0.0]
"InstalledPath"="$PackageFolder$\ProjectTemplates"
```

Also add the following line to the source.extensions.vsixmanifest:
```xml
<Asset Type="Microsoft.VisualStudio.VsPackage" d:Source="File" Path="Templates.pkgdef" />
```

