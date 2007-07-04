using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NerdBank.Algorithms.NodeConstraintSelection;

namespace ClueBuddy {
	[Serializable]
	public partial class Game : INotifyPropertyChanged {
		/// <summary>
		/// Builds up a game based on a given set of cards.
		/// </summary>
		/// <param name="rules">The set of rules the game will play by.</param>
		/// <param name="cards">The cards in this game.</param>
		internal Game(string varietyName, GameRules rules, IEnumerable<Card> cards) {
			if (string.IsNullOrEmpty(varietyName)) throw new ArgumentNullException("varietyName");
			if (cards == null) throw new ArgumentNullException("cards");
			if (cards.Count() == 0)
				throw new ArgumentException(Strings.ListCannotBeEmpty, "cards");
			this.varietyName = varietyName;
			this.rules = rules;
			this.cards = cards.ToList();
		}

		string varietyName;
		/// <summary>
		/// The variety of the game.
		/// </summary>
		public string VarietyName {
			get { return varietyName; }
		}

		GameRules rules;
		/// <summary>
		/// The set of rules this game is playing by.
		/// </summary>
		public GameRules Rules {
			get { return rules; }
		}

		CaseFile caseFile;
		/// <summary>
		/// The orange envelope that has the three cards that solve the game.
		/// </summary>
		public CaseFile CaseFile {
			get { return caseFile; }
			set { caseFile = value; }
		}

		List<Player> players = new List<Player>();
		/// <summary>
		/// The players in the game.
		/// </summary>
		public IList<Player> Players {
			get { return IsStarted ? (IList<Player>)players.AsReadOnly() : players; }
		}
		public IEnumerable<Player> PlayersInOrderAfter(Player player) {
			int initialPlayerIndex = Players.IndexOf(player);
			for (int i = (initialPlayerIndex + 1) % Players.Count; i != initialPlayerIndex; i = (i + 1) % Players.Count) {
				yield return Players[i];
			}
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

		List<Card> cards;
		/// <summary>
		/// The cards in the game.
		/// </summary>
		public IEnumerable<Card> Cards {
			get { return from c in cards orderby c.Name select c; }
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
		/// The places in the game.
		/// </summary>
		public IEnumerable<Place> Places {
			get { return getCardsOfType<Place>(); }
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

		internal const bool AutoConstraintRegenerationDefault = true;
		bool autoConstraintRegeneration = AutoConstraintRegenerationDefault;
		/// <summary>
		/// Whether the game will automatically regenerate all constraints when an invasive
		/// change occurs to an existing clue.
		/// </summary>
		public bool AutoConstraintRegeneration {
			get { return autoConstraintRegeneration; }
			set { autoConstraintRegeneration = value; }
		}

		internal const bool AutoAnalysisDefault = true;
		bool autoAnalysis = AutoAnalysisDefault;
		/// <summary>
		/// Whether the game will automatically perform analysis on the game state
		/// with each change in the set of clues.
		/// </summary>
		public bool AutoAnalysis {
			get { return autoAnalysis; }
			set {
				autoAnalysis = value;
				if (AutoAnalysis && analysisPending) {
					Analyze();
				}
			}
		}
		/// <summary>
		/// Whether an analysis needs to be performed.  
		/// Only applies when <see cref="AutoAnalysis"/> is false.
		/// </summary>
		bool analysisPending;

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
		public bool CardAssignmentsAcceptable {
			get { return Players.Sum(p => p.CardsHeldCount) == Cards.Count() - 3; }
		}
		public void Start() {
			if (Players.Count == 0)
				throw new InvalidOperationException(Strings.PlayersRequired);
			// Verify that the right number of cards are distributed.
			if (!CardAssignmentsAcceptable)
				throw new InvalidOperationException(Strings.CardsToPlayersDistributionError);
			foreach (Player p in Players)
				p.Game = this;

			// Create the CaseFile
			CaseFile = new CaseFile(this);

			createNodes();

			// Create the initial set of constraints...
			constraints = new List<IConstraint>();
			addPredefinedConstraints();

			clues = new ObservableCollection<Clue>();
			clues.CollectionChanged += new NotifyCollectionChangedEventHandler(clues_CollectionChanged);
		}

		void createNodes() {
			// Create all the nodes
			nodes = (from h in cardHolders
					 from c in Cards
					 select new Node(h, c)).ToList();
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
				foreach (Clue clue in e.NewItems) {
					clue.PropertyChanged += new PropertyChangedEventHandler(clue_PropertyChanged);
				}
			if (e.OldItems != null)
				foreach (Clue clue in e.OldItems)
					clue.PropertyChanged -= new PropertyChangedEventHandler(clue_PropertyChanged);
			switch (e.Action) {
				case NotifyCollectionChangedAction.Add:
					foreach (Clue clue in e.NewItems) {
						if (clue == null) continue; // skip over any null clues.
						constraints.AddRange(clue.GetConstraints(Nodes));
					}
					resolvePartially();
					autoAnalyze();
					break;
				default: // any other change is potentially devastating to current state, so recalculate everything.
					if (AutoConstraintRegeneration) {
						RegenerateConstraints();
					}
					break;
			}
		}

		/// <summary>
		/// Either schedules or calls the Analyze method, depending on the configuration.
		/// </summary>
		void autoAnalyze() {
			if (AutoAnalysis) {
				Analyze();
			} else {
				analysisPending = true;
			}
		}

		void clue_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			if (AutoConstraintRegeneration) {
				RegenerateConstraints(); // any internal clue change is potentially devastating to current state, so recalculate everything.
			}
		}

		public void Analyze() {
			resolvePartially();
			// Perform some deep analysis for new opportunities to resolve nodes.
			var deducedConstraints = ConstraintGenerator.GenerateDeducedConstraints(constraints, true, true).ToArray();
			if (deducedConstraints.Length > 0) {
				//foreach (var c in deducedConstraints) {
				//    Debug.WriteLine("Adding deduced constraint: " + c.ToString());
				//}
				constraints.AddRange(deducedConstraints);
				// Once again, settle any that can be.
				new CompositeConstraint(constraints).ResolvePartially();
			}
			analysisPending = false;
		}

		void resolvePartially() {
			// Settle any nodes that can be
			new CompositeConstraint(constraints).ResolvePartially();
		}

		public void RegenerateConstraints() {
			constraints.Clear();
			foreach (Node n in Nodes)
				n.Reset();
			addPredefinedConstraints();
			foreach (Clue clue in Clues) {
				if (clue == null) continue; // skip over any null clues.
				constraints.AddRange(clue.GetConstraints(Nodes));
			}
			resolvePartially();
			autoAnalyze();
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
