//-----------------------------------------------------------------------
// <copyright file="Weapon.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ClueBuddy
{
	using System;

	/// <summary>
	/// A card representing a weapon.
	/// </summary>
	[Serializable]
	public class Weapon : Card {
		#region Constructors and Destructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Weapon"/> class.
		/// </summary>
		public Weapon() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="Weapon"/> class.
		/// </summary>
		/// <param name="name">
		/// The name of the weapon.
		/// </param>
		public Weapon(string name) : base(name) { }

		#endregion
	}
}