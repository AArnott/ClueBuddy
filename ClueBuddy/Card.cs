//-----------------------------------------------------------------------
// <copyright file="Card.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ClueBuddy {
	using System;
	using System.Diagnostics.Contracts;
	using System.Xml.Serialization;

	/// <summary>
	/// A playing card.
	/// </summary>
	[Serializable]
	public class Card {
		#region Constructors and Destructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Card"/> class.
		/// </summary>
		public Card() {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Card"/> class.
		/// </summary>
		/// <param name="name">The character, place or weapon on the card.</param>
		protected Card(string name) {
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(name));
			this.Name = name;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the name of the weapon, place or suspect.
		/// </summary>
		[XmlAttribute]
		public string Name { get; set; }

		#endregion

		#region Public Methods

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString() {
			return this.Name ?? string.Empty;
		}

		#endregion
	}
}
