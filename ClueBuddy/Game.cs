//-----------------------------------------------------------------------
// <copyright file="Game.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ClueBuddy {
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Collections.Specialized;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Diagnostics.Contracts;
	using System.Linq;

	using NerdBank.Algorithms.NodeConstraintSelection;

	/// <summary>
	/// The game.
	/// </summary>
	[Serializable]
	public class Game : INotifyPropertyChanged {
		#region Constants and Fields

		/// <summary>
		/// The auto analysis default.
		/// </summary>
		internal const bool AutoAnalysisDefault = true;

		/// <summary>
		/// The auto constraint regeneration default.
		/// </summary>
		internal const bool AutoConstraintRegenerationDefault = true;

		/// <summary>
		/// The cards.
		/// </summary>
		private readonly List<Card> cards;

		/// <summary>
		/// The players.
		/// </summary>
		private readonly List<Player> players = new List<Player>();

		/// <summary>
		/// Whether an analysis needs to be performed.  
		/// Only applies when <see cref="AutoAnalysis"/> is false.
		/// </summary>
		private bool analysisPending;

		/// <summary>
		/// The auto analysis.
		/// </summary>
		private bool autoAnalysis = AutoAnalysisDefault;

		/// <summary>
		/// The suspend clue change handler.
		/// </summary>
		private bool suspendClueChangeHandler;

		#endregion

		#region Constructors and Destructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Game"/> class. 
		/// Builds up a game based on a given set of cards.
		/// </summary>
		/// <param name="varietyName">
		/// The variety Name.
		/// </param>
		/// <param name="rules">
		/// The set of rules the game will play by.
		/// </param>
		/// <param name="cards">
		/// The cards in this game.
		/// </param>
		internal Game(string varietyName, GameRules rules, IEnumerable<Card> cards) {
			Contract.Requires<ArgumentNullException>(cards != null);
			Contract.Requires<ArgumentException>(cards.Count() > 0, Strings.ListCannotBeEmpty);
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(varietyName));

			this.AutoConstraintRegeneration = AutoConstraintRegenerationDefault;
			this.VarietyName = varietyName;
			this.Rules = rules;
			this.cards = cards.ToList();

			this.Clues = new ObservableCollection<Clue>();
			this.Nodes = new List<Node>();
			this.Constraints = new List<IConstraint>();
		}

		#endregion

		#region Events

		/// <summary>
		/// The bad clue detected.
		/// </summary>
		public event EventHandler<BadClueEventArgs> BadClueDetected;

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region Properties

		/// <summary>
		/// Gets a value indicating whether AreCluesConflicted.
		/// </summary>
		public bool AreCluesConflicted { get; private set; }

		/// <summary>
		/// Whether the game will automatically perform analysis on the game state
		/// with each change in the set of clues.
		/// </summary>
		public bool AutoAnalysis {
			get {
				return this.autoAnalysis;
			}

			set {
				this.autoAnalysis = value;
				if (this.AutoAnalysis && this.analysisPending) {
					this.Analyze();
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the game will automatically regenerate all constraints when an invasive
		/// change occurs to an existing clue.
		/// </summary>
		public bool AutoConstraintRegeneration { get; set; }

		/// <summary>
		/// Gets a value indicating whether CardAssignmentsAcceptable.
		/// </summary>
		public bool CardAssignmentsAcceptable {
			get { return this.Players.Sum(p => p.CardsHeldCount) == this.Cards.Count() - 3; }
		}

		/// <summary>
		/// Gets the cards in the game.
		/// </summary>
		public IEnumerable<Card> Cards {
			get { return from c in this.cards orderby c.Name select c; }
		}

		/// <summary>
		/// Gets or sets the orange envelope that has the three cards that solve the game.
		/// </summary>
		public CaseFile CaseFile { get; set; }

		/// <summary>
		/// Gets the list of clues.
		/// </summary>
		public ObservableCollection<Clue> Clues { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the game is in a playing state.
		/// </summary>
		/// <remarks>
		/// Calling the <see cref="Start"/> method causes this to return true, and
		/// calling the <see cref="Reset"/> method causes it to return false.
		/// </remarks>
		public bool IsStarted {
			get { return this.CaseFile != null; }
		}

		/// <summary>
		/// Gets the places in the game.
		/// </summary>
		public IEnumerable<Place> Places {
			get {
				Contract.Ensures(Contract.Result<IEnumerable<Place>>() != null);
				return this.Cards.OfType<Place>();
			}
		}

		/// <summary>
		/// Gets the players in the game.
		/// </summary>
		public IList<Player> Players {
			get {
				Contract.Ensures(Contract.Result<IList<Player>>() != null);
				Contract.Ensures(Contract.Result<IList<Player>>().All(p => p != null));

				return this.IsStarted ? (IList<Player>)this.players.AsReadOnly() : this.players;
			}
		}

		/// <summary>
		/// Gets the set of rules this game is playing by.
		/// </summary>
		public GameRules Rules { get; private set; }

		/// <summary>
		/// Gets the suspects in the game.
		/// </summary>
		public IEnumerable<Suspect> Suspects {
			get {
				Contract.Ensures(Contract.Result<IEnumerable<Suspect>>() != null);
				return this.Cards.OfType<Suspect>();
			}
		}

		/// <summary>
		/// Gets the variety of the game.
		/// </summary>
		public string VarietyName { get; private set; }

		/// <summary>
		/// Gets the weapons in the game.
		/// </summary>
		public IEnumerable<Weapon> Weapons {
			get {
				Contract.Ensures(Contract.Result<IEnumerable<Weapon>>() != null);
				return this.Cards.OfType<Weapon>();
			}
		}

		/// <summary>
		/// Gets the list of internal constraints derived from the list of <see cref="Clues"/>.
		/// </summary>
		internal List<IConstraint> Constraints { get; private set; }

		/// <summary>
		/// Gets the nodes.
		/// </summary>
		/// <value>The nodes.</value>
		internal List<Node> Nodes { get; private set; }

		/// <summary>
		/// Gets the card holders.
		/// </summary>
		/// <value>The card holders.</value>
		private IEnumerable<ICardHolder> CardHolders {
			get {
				Contract.Ensures(Contract.Result<IEnumerable<ICardHolder>>() != null);
				foreach (var p in this.Players) {
					yield return p;
				}

				yield return this.CaseFile;
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Analyzes the board and all the clues collected thus far.
		/// </summary>
		public void Analyze() {
			this.ResolvePartially();

			// Perform some deep analysis for new opportunities to resolve nodes.
			var deducedConstraints = ConstraintGenerator.GenerateDeducedConstraints(this.Constraints, true, true).ToArray();
			if (deducedConstraints.Length > 0) {
				// foreach (var c in deducedConstraints) {
				// Debug.WriteLine("Adding deduced constraint: " + c.ToString());
				// }
				this.Constraints.AddRange(deducedConstraints);

				// Once again, settle any that can be.
				new CompositeConstraint(this.Constraints).ResolvePartially();
			}
			this.analysisPending = false;
		}

		/// <summary>
		/// Estimates the number of cards each player will have in his/her hand.
		/// </summary>
		public void AssignApproximatePlayerHandSizes() {
			Contract.Requires<InvalidOperationException>(this.Players.Count > 0, Strings.PlayersRequired);
			Contract.Requires<InvalidOperationException>(this.Players.All(p => p.Game != null));
			Contract.Ensures(this.Players.All(p => p.CardsHeldCount > 0));

			for (int i = 0; i < this.Players.Count; i++) {
				int handSize = (this.Cards.Count() - CaseFile.CardsInCaseFile) / this.Players.Count;
				if (i < (this.Cards.Count() - CaseFile.CardsInCaseFile) % this.Players.Count) {
					handSize++;
				}
				this.Players[i].CardsHeldCount = handSize;
			}
		}

		/// <summary>
		/// Finds contradicting clues.
		/// </summary>
		/// <returns>A sequence of clues.</returns>
		public IEnumerable<Clue> FindContradictingClues() {
			Contract.Ensures(Contract.Result<IEnumerable<Clue>>() != null);

			var originalClues = this.Clues.ToList();
			var suspectClues = new List<Clue>();

			this.suspendClueChangeHandler = true;
			try {
				for (int i = 0; i < this.Clues.Count; i++) {
					this.Clues.RemoveAt(i);

					try {
						this.RegenerateConstraintsCore();

						// Removing this clue solved the conflict.
						suspectClues.Add(originalClues[i]);
					} catch (BrokenConstraintException) {
						// The conflict still exists, so the clue we just removed
						// isn't likely to blame.
					}

					this.Clues.Insert(i, originalClues[i]);
				}
			} finally {
				// Restore original list of clues.
				this.Clues.Clear();
				originalClues.ForEach(clue => this.Clues.Add(clue));
				this.suspendClueChangeHandler = false;
			}

			return suspectClues;
		}

		/// <summary>
		/// Determines whether a card is held by the specified player or case file.
		/// </summary>
		/// <param name="playerOrCaseFile">The player or case file.</param>
		/// <param name="card">The card in question.</param>
		/// <returns>True if the card is held, false if it is not, and null if the answer is unknown.</returns>
		public bool? IsCardHeld(ICardHolder playerOrCaseFile, Card card) {
			return this.Nodes.Where(n => n.CardHolder == playerOrCaseFile && n.Card == card).First().IsSelected;
		}

		/// <summary>
		/// Gets the sequence of play after the given player
		/// </summary>
		/// <param name="player">The player.</param>
		/// <returns>A sequence of players.</returns>
		public IEnumerable<Player> PlayersInOrderAfter(Player player) {
			Contract.Ensures(Contract.Result<IEnumerable<Player>>() != null);

			int initialPlayerIndex = this.Players.IndexOf(player);
			for (int i = (initialPlayerIndex + 1) % this.Players.Count; i != initialPlayerIndex; i = (i + 1) % this.Players.Count) {
				yield return this.Players[i];
			}
		}

		/// <summary>
		/// Regenerates the constraints based on the set of clues collected thus far.
		/// </summary>
		public void RegenerateConstraints() {
			bool resolvedBadClues;
			do {
				resolvedBadClues = false;
				try {
					this.RegenerateConstraintsCore();
					this.AreCluesConflicted = false;
				} catch (BrokenConstraintException ex) {
					this.AreCluesConflicted = true;
					resolvedBadClues = this.OnBadClueDetected(ex);
				}
			} while (resolvedBadClues);
		}

		/// <summary>
		/// Resets the game for another play.
		/// </summary>
		public void Reset() {
			this.players.ForEach(player => player.Game = null);
			this.players.Clear();
			this.CaseFile = null;
			this.Nodes.Clear();
			this.Constraints.Clear();
			this.Clues.Clear();
		}

		/// <summary>
		/// The resume from load.
		/// </summary>
		public void ResumeFromLoad() {
			this.PrepareFromStartOrLoad();

			// Just in case the intelligence of this program has improved since this game was saved,
			// recalculate everything.
			this.RegenerateConstraints();
		}

		/// <summary>
		/// The start.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// </exception>
		public void Start() {
			Contract.Requires<InvalidOperationException>(this.Players.Count > 0, Strings.PlayersRequired);
			Contract.Requires<InvalidOperationException>(this.CardAssignmentsAcceptable, Strings.CardsToPlayersDistributionError);

			foreach (Player p in this.Players) {
				p.Game = this;
			}

			// Create the CaseFile
			this.CaseFile = new CaseFile(this);

			this.CreateNodes();

			// Create the initial set of constraints...
			this.Constraints.Clear();
			this.AddPredefinedConstraints();

			this.PrepareFromStartOrLoad();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Fires the <see cref="BadClueDetected"/> event.
		/// </summary>
		/// <param name="ex">The exception.</param>
		/// <returns>
		/// 	<c>true</c> if an event handler claims to have resolved the problem; <c>false otherwise</c>.
		/// </returns>
		protected virtual bool OnBadClueDetected(BrokenConstraintException ex) {
			var badClueDetected = this.BadClueDetected;
			if (badClueDetected != null) {
				var args = new BadClueEventArgs(this, ex);
				badClueDetected(this, args);
				return args.Handled;
			}

			return false;
		}

		/// <summary>
		/// Fires the <see cref="PropertyChanged"/> event.
		/// </summary>
		/// <param name="propertyName">
		/// Name of the property.
		/// </param>
		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
			if (propertyChanged != null) {
				propertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		/// <summary>
		/// The add predefined constraints.
		/// </summary>
		private void AddPredefinedConstraints() {
			Contract.Requires(this.Constraints.Count == 0);
			Contract.Ensures(this.Constraints.All(c => c.IsSatisfiable), "Initial constraints are already failing.");

			// Each card can only be held once.
			this.Constraints.AddRange(
				(from n in this.Nodes
				 group n by n.Card
					 into cardGroup
					 select SelectionCountConstraint.ExactSelected(1, cardGroup.OfType<INode>())).OfType<IConstraint>());

			// Each player has exactly CardsHeldCount cards in their hands
			this.Constraints.AddRange(
				(from n in this.Nodes
				 group n by n.CardHolder
					 into handGroup
					 select SelectionCountConstraint.ExactSelected(handGroup.Key.CardsHeldCount, handGroup.OfType<INode>())).OfType<IConstraint>());

			// The CaseFile has exactly one of each type of cards.
			var tc = (from n in this.Nodes
					  where n.CardHolder == this.CaseFile
					  group n by n.Card.GetType()
						  into typeGroup
						  select SelectionCountConstraint.ExactSelected(1, typeGroup.OfType<INode>())).OfType<IConstraint>().ToArray();
			this.Constraints.AddRange(tc);
		}

		/// <summary>
		/// The create nodes.
		/// </summary>
		private void CreateNodes() {
			// Create all the nodes
			this.Nodes.AddRange(from h in this.CardHolders
								from c in this.Cards
								select new Node(h, c));
		}

		/// <summary>
		/// The prepare from start or load.
		/// </summary>
		private void PrepareFromStartOrLoad() {
			this.Clues.CollectionChanged += this.CluesCollectionChanged;
		}

		/// <summary>
		/// The resolve partially.
		/// </summary>
		private void ResolvePartially() {
			// Settle any nodes that can be
			new CompositeConstraint(this.Constraints).ResolvePartially();
		}

		/// <summary>
		/// Either schedules or calls the Analyze method, depending on the configuration.
		/// </summary>
		private void AutoAnalyze() {
			if (this.AutoAnalysis) {
				this.Analyze();
			} else {
				this.analysisPending = true;
			}
		}

		/// <summary>
		/// Clues the property changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
		private void CluePropertyChanged(object sender, PropertyChangedEventArgs e) {
			if (this.AutoConstraintRegeneration) {
				this.RegenerateConstraints(); // any internal clue change is potentially devastating to current state, so recalculate everything.
			}
		}

		/// <summary>
		/// Fired when the Clues collection changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
		private void CluesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			if (this.suspendClueChangeHandler) {
				return;
			}

			if (e.NewItems != null) {
				foreach (Clue clue in e.NewItems) {
					clue.PropertyChanged += this.CluePropertyChanged;
				}
			}
			if (e.OldItems != null) {
				foreach (Clue clue in e.OldItems) {
					clue.PropertyChanged -= this.CluePropertyChanged;
				}
			}

			switch (e.Action) {
				case NotifyCollectionChangedAction.Add:
					Contract.Assume(e.NewItems != null);
					foreach (Clue clue in e.NewItems) {
						if (clue == null) {
							continue; // skip over any null clues.
						}
						this.Constraints.AddRange(clue.GetConstraints(this.Nodes));
					}
					try {
						this.ResolvePartially();
						this.AutoAnalyze();
					} catch (BrokenConstraintException) {
						this.RegenerateConstraints();
					}
					break;
				default: // any other change is potentially devastating to current state, so recalculate everything.
					if (this.AutoConstraintRegeneration) {
						this.RegenerateConstraints();
					}
					break;
			}
		}

		/// <summary>
		/// The regenerate constraints core.
		/// </summary>
		private void RegenerateConstraintsCore() {
			Contract.Requires(this.Nodes != null);
			Contract.Requires(this.Clues != null);

			this.Constraints.Clear();
			foreach (Node n in this.Nodes) {
				n.Reset();
			}
			this.AddPredefinedConstraints();
			foreach (Clue clue in this.Clues) {
				this.Constraints.AddRange(clue.GetConstraints(this.Nodes));
			}
			this.ResolvePartially();
			this.AutoAnalyze();
		}

		[ContractInvariantMethod]
		private void ObjectInvariant() {
			Contract.Invariant(this.Nodes != null && this.Nodes.All(n => n != null));
			Contract.Invariant(this.Clues != null && this.Clues.All(c => c != null));
			Contract.Invariant(this.cards != null);
			Contract.Invariant(this.players.All(p => p != null));
			Contract.Invariant(this.Constraints != null);
		}

		#endregion
	}
}
