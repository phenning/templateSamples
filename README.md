# Create installable VSIX with template

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

## Create vstemplate “breadcrumb”

After creating the template and template pack, the next step is to create a [vstemplate “breadcrumb” template](https://github.com/phenning/templateSamples/tree/master/vstemplates/MyConsoleTemplate) to invoke the wizard which implements the logic to create .NET Core Template Engine templates. Start with creating a C# Project Template from within Visual Studio, and update the generated vstemplate file with the following, updating the template name and description and specifying the proper values for the language and groupid in the CustomParameters. A critical piece of information to note, the Type of the VSTemplate should be updated from the default ```Project``` to ```ProjectGroup```.

```xml
<?xml version="1.0" encoding="utf-8"?>
<VSTemplate Version="3.0.0" Type="ProjectGroup"  
     xmlns="http://schemas.microsoft.com/developer/vstemplate/2005"    
     xmlns:sdk="http://schemas.microsoft.com/developer/vstemplate-sdkextension/2010">
  <TemplateData>
    <Name>My Console Template</Name>
    <Description>My Console Template for .NET Core</Description>
    <Icon>MyConsoleTemplate.ico</Icon>
    <ProjectType>CSharp</ProjectType>
    <SortOrder>1000</SortOrder>
    <TemplateID>917b2ee8-7bcf-4062-afef-0106c4a27848</TemplateID>
    <CreateNewFolder>true</CreateNewFolder>
    <DefaultName>MyConsoleTemplate</DefaultName>
    <ProvideDefaultName>true</ProvideDefaultName>
    <LanguageTag>csharp</LanguageTag>
    <PlatformTag>linux</PlatformTag>
    <PlatformTag>macos</PlatformTag>
    <PlatformTag>windows</PlatformTag>
  </TemplateData>
  <TemplateContent>
    <ProjectCollection/>
    <CustomParameters>
      <CustomParameter Name="$language$" Value="CSharp" />
      <CustomParameter Name="$groupid$" Value="MyCompany.Common.Console"/>
    </CustomParameters>
  </TemplateContent>
  <WizardExtension>
    <Assembly>Microsoft.VisualStudio.TemplateEngine.Wizard, Version=1.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a</Assembly>
    <FullClassName>Microsoft.VisualStudio.TemplateEngine.Wizard.TemplateEngineWizard</FullClassName>
  </WizardExtension>
</VSTemplate>
```

## Create Vsix for vstemplate installation

The last step is to create the [VSIX installer]((https://github.com/phenning/templateSamples/tree/master/vsix))

In this sample, we add a project reference to the vstemplate project previously created.

We need to add the generated nupkg to the VSIX. In order to do this, we need to update the VSIX project file and manually add the following [target](https://github.com/phenning/templateSamples/blob/3638ff51d04ae637591508e7c2848cbdb988e2e8/vsix/TemplateVsix.csproj#L73).

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

