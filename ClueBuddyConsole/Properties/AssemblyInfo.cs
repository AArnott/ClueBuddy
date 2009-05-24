//#define PARTIAL_TRUST

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("ClueBuddyConsole")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("NerdBank")]
[assembly: AssemblyProduct("ClueBuddyConsole")]
[assembly: AssemblyCopyright("Copyright ©  2007")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("23353307-3129-456b-ac73-7da1abc05510")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

#if PARTIAL_TRUST
/* Minimum permissions required for game to run */
[assembly: UIPermission(SecurityAction.RequestMinimum, Unrestricted = true)]
// Required for opening a game variety to play
[assembly: FileDialogPermission(SecurityAction.RequestMinimum, Open = true, Save=true)]

/* Optional permissions that if granted will enhance the user experience */
// Required for loading/saving games
[assembly: SecurityPermission(SecurityAction.RequestOptional, SerializationFormatter = true)]
// Required for loading/saving games
[assembly: ReflectionPermission(SecurityAction.RequestOptional, MemberAccess = true)]
// Required for setting the FileDialog.Title, Filename properties
//[assembly: FileIOPermission(SecurityAction.RequestOptional, Unrestricted = true)]

/* Permissions we absolutely don't need */
#endif
