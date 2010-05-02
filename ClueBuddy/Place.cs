//-----------------------------------------------------------------------
// <copyright file="Place.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ClueBuddy
{
	using System;

	/// <summary>
	/// A card representing a room or place.
	/// </summary>
	[Serializable]
	public class Place : Card {
		#region Constructors and Destructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Place"/> class.
		/// </summary>
		public Place() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="Place"/> class.
		/// </summary>
		/// <param name="name">
		/// The name of the place.
		/// </param>
		public Place(string name) : base(name) { }

		#endregion
	}
}