//-----------------------------------------------------------------------
// <copyright file="ICardHolder.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ClueBuddy {
	using System.Diagnostics.Contracts;

	/// <summary>
	/// An entity in the game that can hold cards, such as the case file or a player.
	/// </summary>
	[ContractClass(typeof(ICardHolderContract))]
	public interface ICardHolder {
		/// <summary>
		/// Gets the number of cards in the player's hand or in the Case File.
		/// </summary>
		int CardsHeldCount { get; }

		/// <summary>
		/// Gets the game this player belongs to.
		/// </summary>
		Game Game { get; }

		/// <summary>
		/// Determines whether this player holds a given card.
		/// </summary>
		/// <param name="card">The card in question.</param>
		/// <returns>True if the player holds the card, false if not, or null if it is unknown.</returns>
		bool? HasCard(Card card);
	}
}
