//-----------------------------------------------------------------------
// <copyright file="Suspect.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ClueBuddy
{
	using System;
	using System.Xml.Serialization;

	/// <summary>
	/// The suspect.
	/// </summary>
	[Serializable]
	public class Suspect : Card {
		#region Constants and Fields

		/// <summary>
		/// The gender.
		/// </summary>
		private SuspectGender gender = SuspectGender.Undetermined;

		/// <summary>
		/// The well known suspect.
		/// </summary>
		private StandardSuspect wellKnownSuspect = StandardSuspect.Other;

		#endregion

		#region Constructors and Destructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Suspect"/> class.
		/// </summary>
		public Suspect() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="Suspect"/> class.
		/// </summary>
		/// <param name="name">
		/// The name of the suspect.
		/// </param>
		public Suspect(string name) : base(name) { }

		#endregion

		#region Enums

		/// <summary>
		/// The standard suspects.
		/// </summary>
		public enum StandardSuspect {
			/// <summary>
			/// A non-standard suspect.
			/// </summary>
			Other,

			/// <summary>
			/// Ms. Peach
			/// </summary>
			Peach,

			/// <summary>
			/// Madame Rose
			/// </summary>
			Rose,

			/// <summary>
			/// Ms. Scarlet
			/// </summary>
			Scarlet,

			/// <summary>
			/// Mrs. White
			/// </summary>
			White,

			/// <summary>
			/// Mrs. Peacock
			/// </summary>
			Peacock,

			/// <summary>
			/// Mr. Green
			/// </summary>
			Green,

			/// <summary>
			/// Professor Plum
			/// </summary>
			Plum,

			/// <summary>
			/// Colonel Mustard
			/// </summary>
			Mustard,

			/// <summary>
			/// Monsieur Brunette
			/// </summary>
			Brunette,

			/// <summary>
			/// Sergeant Grey
			/// </summary>
			Grey,
		}

		/// <summary>
		/// The suspect gender.
		/// </summary>
		public enum SuspectGender {
			/// <summary>
			/// The suspect's gender is not known.
			/// </summary>
			Undetermined,

			/// <summary>
			/// The suspect is male.
			/// </summary>
			Male,

			/// <summary>
			/// The suspect is female.
			/// </summary>
			Female
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the gender.
		/// </summary>
		/// <value>The gender.</value>
		[XmlAttribute]
		public SuspectGender Gender {
			get {
				if (this.gender == SuspectGender.Undetermined) {
					switch (this.WellKnownSuspect) {
						case StandardSuspect.Brunette:
						case StandardSuspect.Green:
						case StandardSuspect.Grey:
						case StandardSuspect.Mustard:
						case StandardSuspect.Plum:
							this.gender = SuspectGender.Male;
							break;
						case StandardSuspect.Peach:
						case StandardSuspect.Peacock:
						case StandardSuspect.Rose:
						case StandardSuspect.Scarlet:
						case StandardSuspect.White:
							this.gender = SuspectGender.Female;
							break;
					}
				}
				return this.gender;
			}

			set {
				this.gender = value;
			}
		}

		/// <summary>
		/// Gets or sets the well known suspect.
		/// </summary>
		/// <value>The well known suspect.</value>
		[XmlAttribute]
		public StandardSuspect WellKnownSuspect {
			get {
				if (this.wellKnownSuspect == StandardSuspect.Other && this.Name != null) {
					foreach (string standardSuspect in Enum.GetNames(typeof(StandardSuspect))) {
						if (this.Name.IndexOf(standardSuspect) >= 0) {
							this.wellKnownSuspect = (StandardSuspect)Enum.Parse(typeof(StandardSuspect), standardSuspect);
							break;
						}
					}
				}
				return this.wellKnownSuspect;
			}

			set {
				this.wellKnownSuspect = value;
			}
		}

		#endregion
	}
}