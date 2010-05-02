//-----------------------------------------------------------------------
// <copyright file="ICardHolderContract.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ClueBuddy {
	using System;
	using System.Diagnostics.Contracts;

	/// <summary>
	/// Contract class for the <see cref="ICardHolder"/> interface.
	/// </summary>
	[ContractClassFor(typeof(ICardHolder))]
	internal abstract class ICardHolderContract : ICardHolder {
		#region ICardHolder Members

		/// <summary>
		/// Gets the number of cards in the player's hand or in the Case File.
		/// </summary>
		int ICardHolder.CardsHeldCount {
			get { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Gets the game this player belongs to.
		/// </summary>
		Game ICardHolder.Game {
			get { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Determines whether this player holds a given card.
		/// </summary>
		/// <param name="card">The card in question.</param>
		/// <returns>
		/// True if the player holds the card, false if not, or null if it is unknown.
		/// </returns>
		bool? ICardHolder.HasCard(Card card) {
			Contract.Requires<InvalidOperationException>(((ICardHolder)this).Game != null);
			Contract.Requires<ArgumentNullException>(card != null, "card");
			throw new NotImplementedException();
		}

		#endregion
	}
}