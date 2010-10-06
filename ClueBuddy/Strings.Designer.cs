﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ClueBuddy {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ClueBuddy.Strings", typeof(Strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} must be greater than {1}..
        /// </summary>
        internal static string Arg1GreaterThanArg2Required {
            get {
                return ResourceManager.GetString("Arg1GreaterThanArg2Required", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} cannot be greater than the number of elements in {1}..
        /// </summary>
        internal static string Arg1NoGreaterThanElementsInArg2Required {
            get {
                return ResourceManager.GetString("Arg1NoGreaterThanElementsInArg2Required", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to At least one of the arguments {0}, {1} must be set to {2}..
        /// </summary>
        internal static string AtLeastOneOfTwoArgumentsMustBeSet {
            get {
                return ResourceManager.GetString("AtLeastOneOfTwoArgumentsMustBeSet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This constraint is broken..
        /// </summary>
        internal static string BrokenConstraint {
            get {
                return ResourceManager.GetString("BrokenConstraint", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The cards in the game have not been properly distributed to the players..
        /// </summary>
        internal static string CardsToPlayersDistributionError {
            get {
                return ResourceManager.GetString("CardsToPlayersDistributionError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The value of the {0} argument must be one of the cards in the {1} argument..
        /// </summary>
        internal static string DisprovingCardNotInSuspicion {
            get {
                return ResourceManager.GetString("DisprovingCardNotInSuspicion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You cannot do this once the game has started..
        /// </summary>
        internal static string IllegalAfterGameIsStarted {
            get {
                return ResourceManager.GetString("IllegalAfterGameIsStarted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The list of nodes provided does not include all nodes necessary to construct the required constraint(s) or contains duplicate nodes..
        /// </summary>
        internal static string IncompleteNodesList {
            get {
                return ResourceManager.GetString("IncompleteNodesList", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A non-empty list is required..
        /// </summary>
        internal static string ListCannotBeEmpty {
            get {
                return ResourceManager.GetString("ListCannotBeEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A non-negative value is required..
        /// </summary>
        internal static string NonNegativeRequired {
            get {
                return ResourceManager.GetString("NonNegativeRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This node is not in simulation mode..
        /// </summary>
        internal static string NotSimulating {
            get {
                return ResourceManager.GetString("NotSimulating", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Players must be added to the game first..
        /// </summary>
        internal static string PlayersRequired {
            get {
                return ResourceManager.GetString("PlayersRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This property cannot be changed from {0} to {1}..
        /// </summary>
        internal static string PropertyChangeFromToError {
            get {
                return ResourceManager.GetString("PropertyChangeFromToError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} property must be set first..
        /// </summary>
        internal static string PropertyMustBeSetFirst {
            get {
                return ResourceManager.GetString("PropertyMustBeSetFirst", resourceCulture);
            }
        }
    }
}
