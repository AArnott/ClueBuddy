using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClueBuddy {
	/// <summary>
	/// The "Case File" envelope in which the murderer, murder weapon, and location
	/// are all kept.
	/// </summary>
	[Serializable]
	public class CaseFile : ICardHolder {
		public CaseFile(Game game) {
			this.game = game;
		}

		Game game;
		public Game Game { get { return game; } }

		T getCardInCaseFile<T>() where T : Card {
			return (from n in game.Nodes
					where n.CardHolder == this && n.Card is T && n.IsSelected.HasValue && n.IsSelected.Value
					select (T)n.Card).FirstOrDefault();
		}

		/// <summary>
		/// Where the murder took place.
		/// </summary>
		public Place Place {
			get { return getCardInCaseFile<Place>(); }
		}

		/// <summary>
		/// The tool used to commit the murder.
		/// </summary>
		public Weapon Weapon {
			get { return getCardInCaseFile<Weapon>(); }
		}

		/// <summary>
		/// The murderer.
		/// </summary>
		public Suspect Suspect {
			get { return getCardInCaseFile<Suspect>(); }
		}

		public int CardsHeldCount {
			get { return 3; }
		}

		/// <summary>
		/// The number of cards that are in a case file at any time.  (Not just the known cards).
		/// </summary>
		public const int CardsInCaseFile = 3;

		public bool IsSolved {
			get {
				return game.Nodes.Count(n => n.CardHolder == game.CaseFile && n.IsSelected.HasValue && n.IsSelected.Value) == 3;
			}
		}

		public bool? HasCard(Card card) {
			return Game.Nodes.First(n => n.CardHolder == this && n.Card == card).IsSelected;
		}

		public override string ToString() {
			return typeof(CaseFile).Name;
		}
	}
}
