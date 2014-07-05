using UnityEngine;
using UnityEditor;

using System.IO;
using System.Collections.Generic;

public static class ExportSourceForLibrary   {

	private static string SourceDialogTitle = "Choose Root Folder of Export Sources";

	private static string ExportDialogTitle = "Export Library";
	private static string ExportFolderName = "ExportedLibrary";

	private static string SolutionName = "solution";
	private static string ProjectName = "project";
	private static string dllName = "exportLib";

	[MenuItem("Extra/Library/Export Sources for Library")]
	public static void ProcExportSourceForLibrary(){
		var srcPath = EditorUtility.OpenFolderPanel(SourceDialogTitle,"","");
		if(string.IsNullOrEmpty(srcPath)){ return; }
		var dstPath = EditorUtility.SaveFilePanel(ExportDialogTitle,"",ExportFolderName,"");
		if(string.IsNullOrEmpty(dstPath)){ return; }

		string managedPath = "Frameworks/Managed";
#if UNITY_EDITOR_WIN
		managedPath = "Managed";
#endif


		// copy framework dlls for refer
		copy(Path.Combine(EditorApplication.applicationContentsPath,managedPath), Path.Combine( dstPath,"dlls") ,"*.dll",null);
		var copiedSources = new List<string>();
		var baseSourcePath = srcPath;//Path.Combine(Application.dataPath,SourceFilesFromAssets);
		copy(baseSourcePath, Path.Combine( dstPath,"Sources") ,"*.cs",copiedSources);

		string guidForSol = System.Guid.NewGuid().ToString().ToUpper();
		string guidForProj = System.Guid.NewGuid().ToString().ToUpper();

		File.WriteAllText( Path.Combine( dstPath, string.Format("{0}.sln",SolutionName)),getSlnFileContents(guidForSol,guidForProj,SolutionName,ProjectName));
		File.WriteAllText( Path.Combine( dstPath, string.Format("{0}.csproj",ProjectName)),getProjFileContents(guidForProj,dllName,baseSourcePath,copiedSources));
	} 

	private static void copy(string sourceRoot,string dstRoot,string filePattern,List<string> copied){
		if(string.IsNullOrEmpty(dstRoot)){ return;}

		// check dstFolder
		if(!Directory.Exists(dstRoot)){
			Directory.CreateDirectory(dstRoot);
		}

		DirectoryInfo di = new DirectoryInfo(sourceRoot);
		foreach(var v in di.GetFiles(filePattern)){
			File.Copy( Path.Combine( sourceRoot, v.Name), Path.Combine( dstRoot,v.Name),true);
			if(copied!=null){
				copied.Add(Path.Combine( sourceRoot, v.Name));
			}
		}
		foreach(var v in di.GetDirectories()){
			copy (Path.Combine(sourceRoot,v.Name),Path.Combine(dstRoot,v.Name),filePattern,copied);
		}
	}

	private static string getSlnFileContents(string guidForSolution,string guidForProject,string solutionName,string projectName){
		return string.Format(slnFileTemplate,
		                     guidForSolution,guidForProject,solutionName,projectName);
	}

	private static string slnFileTemplate = @"
Microsoft Visual Studio Solution File, Format Version 10.00
# Visual Studio 2008
Project(""{{{0}}}"") = ""{2}"", ""{3}.csproj"", ""{{{1}}}""
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{{{1}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{{{1}}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{{{1}}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{{{1}}}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
	GlobalSection(MonoDevelopProperties) = preSolution
		StartupItem = {3}.csproj
	EndGlobalSection
EndGlobal
";

	private static string  getProjFileContents(string guidForProject,string dllName,string baseSourcePath,List<string> entires){
		string entry = @"<Compile Include=""{0}"" />";
		var b = new System.Text.StringBuilder();
		foreach(var e in entires){
			string line =  string.Format(entry, Path.Combine( "Sources/",e.Substring(baseSourcePath.Length+1) ));
			b.Append(line).AppendLine();
		}
		return string.Format(csprojFileTemplate,guidForProject,dllName,b.ToString());
	}

	private static string csprojFileTemplate = 
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project DefaultTargets=""Build"" ToolsVersion=""3.5"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
		<PropertyGroup>
			<Configuration Condition="" '$(Configuration)' == '' "">Debug</Configuration>
			<Platform Condition="" '$(Platform)' == '' "">AnyCPU</Platform>
			<ProductVersion>9.0.21022</ProductVersion>
			<SchemaVersion>2.0</SchemaVersion>
<ProjectGuid>{{{0}}}</ProjectGuid>
		<OutputType>Library</OutputType>
			<RootNamespace>{1}</RootNamespace>
			<AssemblyName>{1}</AssemblyName>
			<TargetFrameworkVersion>v3.5</TargetFrameworkVersion>
			</PropertyGroup>
			<PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' "">
			<DebugSymbols>True</DebugSymbols>
			<DebugType>full</DebugType>
			<Optimize>False</Optimize>
			<OutputPath>bin\Debug</OutputPath>
			<DefineConstants>DEBUG;</DefineConstants>
		<ErrorReport>prompt</ErrorReport>
			<WarningLevel>4</WarningLevel>
			<ConsolePause>False</ConsolePause>
			</PropertyGroup>
			<PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' "">
			<DebugType>none</DebugType>
			<Optimize>True</Optimize>
			<OutputPath>bin\Release</OutputPath>
			<ErrorReport>prompt</ErrorReport>
			<WarningLevel>4</WarningLevel>
			<ConsolePause>False</ConsolePause>
			</PropertyGroup>
			<ItemGroup>
			<Reference Include=""System"" />
			<Reference Include=""UnityEditor"">
			<HintPath>dlls\UnityEditor.dll</HintPath>
			</Reference>
			<Reference Include=""UnityEngine"">
			<HintPath>dlls\UnityEngine.dll</HintPath>
			</Reference>
			</ItemGroup>
			<ItemGroup>
			{2}
			</ItemGroup>
			<Import Project=""$(MSBuildBinPath)\Microsoft.CSharp.targets"" />
			<ItemGroup>
			<Folder Include=""Sources\"" />
  </ItemGroup>
</Project>";
}
