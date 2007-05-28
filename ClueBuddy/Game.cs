using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ClueBuddy {
	public partial class Game : INotifyPropertyChanged {
		/// <summary>
		/// Builds up a game based on a given set of cards.
		/// </summary>
		/// <param name="rules">The set of rules the game will play by.</param>
		/// <param name="cards">The cards in this game.</param>
		Game(string varietyName, GameRules rules, IEnumerable<Card> cards) {
			if (string.IsNullOrEmpty(varietyName)) throw new ArgumentNullException("varietyName");
			if (cards == null) throw new ArgumentNullException("cards");
			if (cards.Count() == 0)
				throw new ArgumentException(Strings.ListCannotBeEmpty, "cards");
			this.varietyName = varietyName;
			this.rules = rules;
			this.cards = cards.ToList(); // Cache the enumeration to a list to avoid recreating cards each time we enumerate
		}

		private string varietyName;
		/// <summary>
		/// The variety of the game.
		/// </summary>
		public string VarietyName {
			get { return varietyName; }
		}

		private GameRules rules;
		/// <summary>
		/// The set of rules this game is playing by.
		/// </summary>
		public GameRules Rules {
			get { return rules; }
		}

		private CaseFile caseFile;
		/// <summary>
		/// The orange envelope that has the three cards that solve the game.
		/// </summary>
		public CaseFile CaseFile {
			get { return caseFile; }
			set { caseFile = value; }
		}

		private List<Player> players = new List<Player>();
		/// <summary>
		/// The players in the game.
		/// </summary>
		public IList<Player> Players {
			get { return IsStarted ? (IList<Player>)players.AsReadOnly() : players; }
		}

		/// <summary>
		/// Whether the game is in a playing state.
		/// </summary>
		/// <remarks>
		/// Calling the <see cref="Start"/> method causes this to return true, and
		/// calling the <see cref="Reset"/> method causes it to return false.
		/// </remarks>
		public bool IsStarted {
			get {
				return CaseFile != null;
			}
		}

		IEnumerable<ICardHolder> cardHolders {
			get {
				foreach (Player p in Players)
					yield return p;
				yield return CaseFile;
			}
		}

		List<Node> nodes;
		internal IEnumerable<Node> Nodes {
			get { return nodes; }
		}

		private List<Card> cards;
		/// <summary>
		/// The cards in the game.
		/// </summary>
		public IEnumerable<Card> Cards {
			get { return cards; }
		}

		IEnumerable<T> getCardsOfType<T>() where T : Card {
			return from c in Cards where c is T select (T)c;
		}
		/// <summary>
		/// The weapons in the game.
		/// </summary>
		public IEnumerable<Weapon> Weapons {
			get { return getCardsOfType<Weapon>(); }
		}
		/// <summary>
		/// The locations in the game.
		/// </summary>
		public IEnumerable<Location> Locations {
			get { return getCardsOfType<Location>(); }
		}
		/// <summary>
		/// The suspects in the game.
		/// </summary>
		public IEnumerable<Suspect> Suspects {
			get { return getCardsOfType<Suspect>(); }
		}

		List<IConstraint> constraints;
		/// <summary>
		/// The list of internal constraints derived from the list of <see cref="Clues"/>.
		/// </summary>
		internal List<IConstraint> Constraints { get { return constraints; } }

		ObservableCollection<Clue> clues;
		/// <summary>
		/// The list of clues.
		/// </summary>
		public ObservableCollection<Clue> Clues {
			get { return clues; }
		}

		internal const int AnalysisDepthDefault = 3;
		private int analysisDepth;
		/// <summary>
		/// How deep to analyze possible solutions in search of more nodes to resolve.
		/// </summary>
		/// <remarks>
		/// Infinity would be the ideal answer, but the possibilities to explore are 
		/// 2^Nodes.Count, which is on the order of 1x10^39.  
		/// The best realistic number is probably the maximum number of cards in any 
		/// player's hand, since the most obvious deductions can take up to that deep
		/// of an analysis to discover.  The time for 6-8 levels deep is very acceptable too.
		/// </remarks>
		public int AnalysisDepth {
			get { return analysisDepth; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException("AnalysisDepth", value, Strings.NonNegativeRequired);
				if (analysisDepth == value) return;
				analysisDepth = value;
				OnPropertyChanged("AnalysisDepth");
			}
		}

		/// <summary>
		/// Estimates the number of cards each player will have in his/her hand.
		/// </summary>
		public void AssignApproximatePlayerHandSizes() {
			if (Players.Count == 0)
				throw new InvalidOperationException(Strings.PlayersRequired);
			for (int i = 0; i < Players.Count; i++) {
				int handSize = (Cards.Count() - CaseFile.CardsInCaseFile) / Players.Count;
				if (i < (Cards.Count() - CaseFile.CardsInCaseFile) % Players.Count)
					handSize++;
				Players[i].CardsHeldCount = handSize;
			}
		}
		public void Start() {
			if (Players.Count == 0)
				throw new InvalidOperationException(Strings.PlayersRequired);
			// Verify that the right number of cards are distributed.
			if (Players.Sum(p => p.CardsHeldCount) != Cards.Count() - 3)
				throw new InvalidOperationException(Strings.CardsToPlayersDistributionError);
			foreach (Player p in Players)
				p.Game = this;

			// Create the CaseFile
			CaseFile = new CaseFile(this);

			// Create all the nodes
			nodes = (from h in cardHolders
					 from c in Cards
					 select new Node(h, c)).ToList();

			// Create the initial set of constraints...
			constraints = new List<IConstraint>();
			addPredefinedConstraints();

			clues = new ObservableCollection<Clue>();
			clues.CollectionChanged += new NotifyCollectionChangedEventHandler(clues_CollectionChanged);
		}

		void addPredefinedConstraints() {
			Debug.Assert(constraints != null && constraints.Count == 0);
			// Each card can only be held once.
			constraints.AddRange((from n in Nodes
								  group n by n.Card into cardGroup
								  select SelectionCountConstraint.ExactSelected(1, cardGroup.OfType<INode>())).
								  OfType<IConstraint>());
			// Each player has exactly CardsHeldCount cards in their hands
			constraints.AddRange((from n in Nodes
								  group n by n.CardHolder into handGroup
								  select SelectionCountConstraint.ExactSelected(handGroup.Key.CardsHeldCount, handGroup.OfType<INode>())).
								  OfType<IConstraint>());
			// The CaseFile has exactly one of each type of cards.
			var tc = (from n in Nodes
					  where n.CardHolder == CaseFile
					  group n by n.Card.GetType() into typeGroup
					  select SelectionCountConstraint.ExactSelected(1, typeGroup.OfType<INode>())).
								  OfType<IConstraint>().ToArray();
			constraints.AddRange(tc);
			Debug.Assert(constraints.All(c => c.IsSatisfiable), "Initial constraints are already failing.");
		}

		public void Reset() {
			foreach (Player player in players) {
				player.Game = null;
			}
			players.Clear();
			caseFile = null;
			nodes = null;
			constraints = null;
			clues = null;
		}

		void clues_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			if (e.NewItems != null)
				foreach (Clue clue in e.NewItems)
					clue.PropertyChanged += new PropertyChangedEventHandler(clue_PropertyChanged);
			if (e.OldItems != null)
				foreach (Clue clue in e.OldItems)
					clue.PropertyChanged -= new PropertyChangedEventHandler(clue_PropertyChanged);
			switch (e.Action) {
				case NotifyCollectionChangedAction.Add:
					foreach (Clue clue in e.NewItems) {
						if (clue == null) continue; // skip over any null clues.
						constraints.AddRange(clue.GetConstraints(Nodes));
					}
					break;
				default: // any other change is potentially devastating to current state, so recalculate everything.
					refigureAllClues();
					break;
			}
			Analyze();
		}

		void clue_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			refigureAllClues(); // any internal clue change is potentially devastating to current state, so recalculate everything.
		}

		public void Analyze() {
			// Settle any nodes that can be
			new CompositeConstraint(constraints).ResolvePartially();
			if (AnalysisDepth > 0) {
				// Perform some deep analysis for new opportunities to resolve nodes.
				var deducedConstraints = ConstraintGenerator.AnalyzeConstraints(constraints, AnalysisDepth - 1).ToArray();
				if (deducedConstraints.Length > 0) {
					foreach (var c in deducedConstraints) {
						Debug.WriteLine("Adding deduced constraint: " + c.ToString());
					}
					constraints.AddRange(deducedConstraints);
					// Once again, settle any that can be.
					new CompositeConstraint(constraints).ResolvePartially();
				}
			}
		}

		void refigureAllClues() {
			constraints.Clear();
			foreach (Node n in Nodes)
				n.Reset();
			addPredefinedConstraints();
			foreach (Clue clue in Clues) {
				if (clue == null) continue; // skip over any null clues.
				constraints.AddRange(clue.GetConstraints(Nodes));
			}
			Analyze();
		}

		public bool? IsCardHeld(ICardHolder playerOrCaseFile, Card card) {
			return Nodes.Where(n => n.CardHolder == playerOrCaseFile && n.Card == card).First().IsSelected;
		}

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChangedEventHandler propertyChanged = PropertyChanged;
			if (propertyChanged != null) {
				propertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion
	}
}
