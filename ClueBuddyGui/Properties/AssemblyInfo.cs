//-----------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("ClueBuddyGui")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("ClueBuddyGui")]
[assembly: AssemblyCopyright("Copyright @  2007")]
[assembly: AssemblyTrademark("")]
[assembly: ComVisible(false)]

// In order to begin building localizable applications, set 
// <UICulture>CultureYouAreCodingWith</UICulture> in your .csproj file
// inside a <PropertyGroup>.  For example, if you are using US english
// in your source files, set the <UICulture> to en-US.  Then uncomment
// the NeutralResourceLanguage attribute below.  Update the "en-US" in
// the line below to match the UICulture setting in the project file.

// [assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]


[assembly: ThemeInfo(
	ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
	//(used if a resource is not found in the page, 
	// or application resource dictionaries)
	ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
	//(used if a resource is not found in the page, 
	// app, or any theme specific resource dictionaries)
)]


// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

///* Minimum permissions required for game to run */
//[assembly: UIPermission(SecurityAction.RequestMinimum, Unrestricted = true/* Window = UIPermissionWindow.SafeSubWindows*/)]
//// Required for opening a game variety to play
//[assembly: FileDialogPermission(SecurityAction.RequestMinimum, Open = true, Save = true)]

///* Optional permissions that if granted will enhance the user experience */
//// Required for loading/saving games
//[assembly: SecurityPermission(SecurityAction.RequestOptional, SerializationFormatter = true)]
//// Required for loading/saving games
//[assembly: ReflectionPermission(SecurityAction.RequestOptional, MemberAccess = true)]
//// Required for setting the FileDialog.Title, Filename properties
////[assembly: FileIOPermission(SecurityAction.RequestOptional, Unrestricted = true)]

///* Permissions we absolutely don't need */
