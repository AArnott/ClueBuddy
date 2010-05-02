using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClueBuddy {
	using System.Diagnostics.Contracts;

	/// <summary>
	/// The "Case File" envelope in which the murderer, murder weapon, and location
	/// are all kept.
	/// </summary>
	[Serializable]
	public class CaseFile : ICardHolder {
		/// <summary>
		/// Gets the number of cards that are in a case file at any time.  (Not just the known cards).
		/// </summary>
		public const int CardsInCaseFile = 3;

		/// <summary>
		/// Initializes a new instance of the <see cref="CaseFile"/> class.
		/// </summary>
		/// <param name="game">The game being constructed.</param>
		public CaseFile(Game game) {
			Contract.Requires<ArgumentNullException>(game != null);
			this.Game = game;
		}

		/// <summary>
		/// Gets the game this case file belongs to.
		/// </summary>
		/// <value></value>
		public Game Game { get; private set; }

		/// <summary>
		/// Gets the location where the murder took place.
		/// </summary>
		public Place Place {
			get { return this.GetCardInCaseFile<Place>(); }
		}

		/// <summary>
		/// Gets the tool used to commit the murder.
		/// </summary>
		public Weapon Weapon {
			get { return this.GetCardInCaseFile<Weapon>(); }
		}

		/// <summary>
		/// Gets the murderer.
		/// </summary>
		public Suspect Suspect {
			get { return this.GetCardInCaseFile<Suspect>(); }
		}

		/// <summary>
		/// Gets the number of cards in the player's hand or in the Case File.
		/// </summary>
		public int CardsHeldCount {
			get
			{
				Contract.Ensures(Contract.Result<int>() == 3);
				return CardsInCaseFile;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this case is solved.
		/// </summary>
		/// <value><c>true</c> if this case is solved; otherwise, <c>false</c>.</value>
		public bool IsSolved {
			get {
				return this.Game.Nodes.Count(n => n.CardHolder == this.Game.CaseFile && n.IsSelected.HasValue && n.IsSelected.Value) == 3;
			}
		}

		/// <summary>
		/// Determines whether the specified card is in the case file.
		/// </summary>
		/// <param name="card">The card in question.</param>
		/// <returns>True if the case file contains the card, false if it doesn't, and null if it cannot yet be determined.</returns>
		public bool? HasCard(Card card) {
			return this.Game.Nodes.First(n => n.CardHolder == this && n.Card == card).IsSelected;
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString() {
			return typeof(CaseFile).Name;
		}

		/// <summary>
		/// Gets the card in the case file of the given type.
		/// </summary>
		/// <typeparam name="T">The type of card being sought.</typeparam>
		/// <returns>The card if found.  <c>null</c> if the card for the given type is not yet known.</returns>
		private T GetCardInCaseFile<T>() where T : Card {
			return (from n in this.Game.Nodes
					where n.CardHolder == this && n.IsSelected.HasValue && n.IsSelected.Value
					let card = n.Card as T
					where card != null
					select card).SingleOrDefault();
		}

		[ContractInvariantMethod]
		private void ObjectInvariant() {
			Contract.Invariant(this.Game != null);
		}
	}
}
