//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ClueBuddyConsole {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Runtime.Serialization;
	using System.Runtime.Serialization.Formatters.Binary;
	using System.Security;
	using System.Security.Permissions;

	using ClueBuddy;

	using Microsoft.Win32;

	using NerdBank.Algorithms.NodeConstraintSelection;

	/// <summary>
	/// The program.
	/// </summary>
	internal class Program {
		#region Constants and Fields

		/// <summary>
		/// The open game dialog.
		/// </summary>
		private readonly OpenFileDialog openGameDialog = new OpenFileDialog();

		/// <summary>
		/// The open variety dialog.
		/// </summary>
		private readonly OpenFileDialog openVarietyDialog = new OpenFileDialog();

		/// <summary>
		/// The save game dialog.
		/// </summary>
		private readonly SaveFileDialog saveGameDialog = new SaveFileDialog();

		/// <summary>
		/// The game.
		/// </summary>
		private Game game;

		/// <summary>
		/// The player who is using this program.
		/// </summary>
		private Player interactivePlayer;

		/// <summary>
		/// The solution found already.
		/// </summary>
		private bool solutionFoundAlready;

		/// <summary>
		/// The player whose turn it is.
		/// </summary>
		private Player suggestingPlayer;

		#endregion

		#region Constructors and Destructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Program"/> class.
		/// </summary>
		private Program() {
			SetupGameFileDialog(this.openGameDialog);
			SetupGameFileDialog(this.saveGameDialog);

			this.openVarietyDialog.DefaultExt = GameVariety.DefaultFileExtension;
			this.openVarietyDialog.Filter = string.Format("Clue Varieties (*.{0})|*.{0}|All files (*.*)|*.*", GameVariety.DefaultFileExtension);
			this.openVarietyDialog.FilterIndex = 0;
			try {
				this.openVarietyDialog.Title = "Choose which game variety to load";
			} catch (SecurityException) {
				// a small convenience we'll leverage when we can	
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the width of the player column.
		/// </summary>
		/// <value>The width of the player column.</value>
		int playerColumnWidth {
			get { return Math.Max(this.game.Players.Max(p => p.Name.Length), "Case File".Length); }
		}

		#endregion

		#region Methods

		/// <summary>
		/// Program entrypoint.
		/// </summary>
		/// <param name="args">Command line arguments.</param>
		[STAThread]
		private static void Main(string[] args) {
			new Program().main();
		}

		/// <summary>
		/// The clues_ collection changed.
		/// </summary>
		/// <param name="sender">
		/// The sender.
		/// </param>
		/// <param name="e">
		/// The e.
		/// </param>
		private void Clues_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			// Has the solution been found?
			if (!this.solutionFoundAlready && this.game.CaseFile.IsSolved) {
				// we just found the solution
				ConsoleHelper.WriteColor(ConsoleColor.Red, "Solved!  {0}, {1}, {2}", this.game.CaseFile.Place, this.game.CaseFile.Suspect, this.game.CaseFile.Weapon);
				this.solutionFoundAlready = true;
			}
		}

		/// <summary>
		/// Makes an accusation.
		/// </summary>
		private void Accusation() {
			var suggestion = new Suspicion();
			suggestion.Place = ConsoleHelper.Choose(
				"Where?", true, p => this.GetCardSuggestionStrength(p), this.game.Places.ToArray());
			if (suggestion.Place == null) {
				return;
			}
			suggestion.Suspect = ConsoleHelper.Choose(
				"Who?", true, s => this.GetCardSuggestionStrength(s), this.game.Suspects.ToArray());
			if (suggestion.Suspect == null) {
				return;
			}
			suggestion.Weapon = ConsoleHelper.Choose(
				"How?", true, w => this.GetCardSuggestionStrength(w), this.game.Weapons.ToArray());
			if (suggestion.Weapon == null) {
				return;
			}
			switch (
				ConsoleHelper.Choose(string.Format("Was the accusation correct ({0})?", suggestion), false, new[] { "Yes", "No", "Abort accusation" })) {
				case 0: // Correct
					return; // game over
				case 1: // Incorrect
					// Add a clue that this solution is impossible
					this.game.Clues.Add(new BadAccusation(suggestion, this.game.CaseFile));
					break;
				case 2: // Abort
					break;
			}
		}

		/// <summary>
		/// Conducts the user through choosing which game to play.
		/// </summary>
		private void ChooseGame() {
			bool? result = this.openVarietyDialog.ShowDialog();
			if (!result.HasValue || !result.Value) {
				return;
			}
			using (Stream s = this.openVarietyDialog.OpenFile()) {
				this.game = GameVariety.LoadFrom(s).Initialize();
			}
			Console.WriteLine("Starting {0}...", this.game.VarietyName);
		}

		/// <summary>
		/// Has the user choose a player.
		/// </summary>
		/// <param name="prompt">The text to prompt the user with.</param>
		/// <param name="includeSkip">if set to <c>true</c> the user may choose to skip this step.</param>
		/// <param name="includeInteractivePlayer">if set to <c>true</c> the interactive player himself will be in the list to choose from.</param>
		/// <returns>The player the user chose, or <c>null</c> if the user skipped.</returns>
		private Player ChoosePlayer(string prompt, bool includeSkip, bool includeInteractivePlayer) {
			return ConsoleHelper.Choose(prompt, includeSkip, p => p.Name,
				this.game.Players.Where(p => includeInteractivePlayer || p != this.interactivePlayer).ToArray());
		}

		/// <summary>
		/// Asks the user to details about a clue.
		/// </summary>
		private void ForceClue() {
			Player player = this.ChoosePlayer("Who do you have information about?", true, false);
			if (player == null) {
				return;
			}
			int choice =
				ConsoleHelper.Choose(
					string.Format(CultureInfo.CurrentCulture, "What do you know about {0}?", player.Name),
				true,
				new[] { "Has card", "Could not disprove", "Disproved" });
			switch (choice) {
				case 0: // has card
					Card card = ConsoleHelper.Choose("Which card?", true, c => c.Name, this.game.Cards.ToArray());
					if (card != null) {
						this.game.Clues.Add(new SpyCard(player, card));
					}
					break;
				case 1: // could not disprove
					var suggestion = this.GetSuggestion();
					if (suggestion != null) {
						this.game.Clues.Add(new CannotDisprove(player, suggestion));
					}
					break;
				case 2: // disproved
					suggestion = this.GetSuggestion();
					if (suggestion != null) {
						this.game.Clues.Add(new Disproved(player, suggestion));
					}
					break;
				default:
					return;
			}
		}

		/// <summary>
		/// Handles the BadClueDetected event of the game control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="ClueBuddy.BadClueEventArgs"/> instance containing the event data.</param>
		private void game_BadClueDetected(object sender, BadClueEventArgs e) {
			ConsoleHelper.WriteColor(ConsoleColor.Red, "Conflicting clues exist!");
		}

		private string GetCardSuggestionStrength(Card card) {
			int strength = 0;

			// Consider the card worthwhile if its existence in the Case File is not known.
			if (!this.game.CaseFile.HasCard(card).HasValue) {
				// Its strength is based on how much we know about it so far, which is based
				// on how many constraints we already have on it.
				int constraintWithCardCount =
					this.game.Constraints.Count(
						c => c is ConstraintBase && ((ConstraintBase)c).Nodes.Any(cn => ((Node)cn).Card == card));
				strength = 50 - constraintWithCardCount;
			}
			return string.Format("{0,-20}", card.Name) + string.Format("{0,4}", strength);
		}

		/// <summary>
		/// Gets a suggestion from the user.
		/// </summary>
		/// <returns>The suggestion.</returns>
		private Suspicion GetSuggestion() {
			Suspicion suggestion = new Suspicion();
			suggestion.Place = ConsoleHelper.Choose(
				"Where?", true, this.GetCardSuggestionStrength, this.game.Places.ToArray());
			if (suggestion.Place == null)
			{
				return null;
			}
			suggestion.Suspect = ConsoleHelper.Choose(
				"Who?", true, this.GetCardSuggestionStrength, this.game.Suspects.ToArray());
			if (suggestion.Suspect == null)
			{
				return null;
			}
			suggestion.Weapon = ConsoleHelper.Choose(
				"How?", true, this.GetCardSuggestionStrength, this.game.Weapons.ToArray());
			if (suggestion.Weapon == null)
			{
				return null;
			}
			return suggestion;
		}

		/// <summary>
		/// Learns what cards the interactive player is holding.
		/// </summary>
		private void LearnOwnHand() {
			List<Card> cardsInHand = new List<Card>(this.interactivePlayer.CardsHeldCount);
			for (int i = 0; i < this.interactivePlayer.CardsHeldCount; i++) {
				Card card = ConsoleHelper.Choose("Indicate a card that you hold in your hand:", false, c => c.Name, this.game.Cards.Where(c2 => !cardsInHand.Contains(c2)).ToArray());
				cardsInHand.Add(card);
			}
			foreach (Card card in cardsInHand) {
				this.game.Clues.Add(new SpyCard(this.interactivePlayer, card));
			}
		}

		/// <summary>
		/// Lists the clues the game has collected so far.
		/// </summary>
		private void ListClues() {
			foreach (Clue c in this.game.Clues) {
				Console.WriteLine(c);
			}
		}

		/// <summary>
		/// The load game.
		/// </summary>
		/// <returns>
		/// </returns>
		private bool? LoadGame() {
			bool? result = this.openGameDialog.ShowDialog();
			if (result.HasValue && result.Value) {
				IFormatter formatter = new BinaryFormatter();
				using (Stream s = this.openGameDialog.OpenFile()) {
					this.game = (Game)formatter.Deserialize(s);
					this.interactivePlayer = this.game.Players.First(p => p.Name.Equals(formatter.Deserialize(s)));

					this.prepareNewOrLoadedGameState();
					this.game.ResumeFromLoad();
				}
				try {
					this.saveGameDialog.FileName = this.openGameDialog.FileName;
				} catch (SecurityException) { } // just a convenience that we'll ignore if we can't do it.
			}
			this.prepareNewOrLoadedGameState();
			return result;
		}

		/// <summary>
		/// The main.
		/// </summary>
		private void main() {
			Console.WriteLine("=======ClueBuddy=======");
			try {
				var savingPermissions = new PermissionSet(null);
				savingPermissions.AddPermission(new FileDialogPermission(FileDialogPermissionAccess.Save));
				savingPermissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.SerializationFormatter));
				savingPermissions.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.MemberAccess));

				// briefly demand these permissions to detect whether we'll succeed later
				savingPermissions.Demand();
			} catch (SecurityException) {
				ConsoleHelper.WriteColor(ConsoleColor.Red, "WARNING: insufficient permissions to save games.");
			}
			while (true) {
				try {
					if (this.game != null && this.game.AreCluesConflicted) {
						this.ResolveConflicts();
					}

					var mainMenu = new Dictionary<char, string>();
					mainMenu.Add('L', "Load game");
					mainMenu.Add('N', "New game");
					if (this.game != null) {
						mainMenu.Add('S', "Save game");
						mainMenu.Add('T', "Play a Turn");
						mainMenu.Add('G', "See Grid");
						mainMenu.Add('C', "List Clues");
						mainMenu.Add('F', "Force enter a clue");
					}
					mainMenu.Add('Q', "Quit");
					switch (ConsoleHelper.Choose("Main menu:", mainMenu, s => s).Key) {
						case 'L':
							this.LoadGame();
							break;
						case 'N':
							this.ChooseGame();
							if (this.game == null) break;
							this.SetupPlayers();
							this.game.Start();
							this.prepareNewOrLoadedGameState();
							this.LearnOwnHand();
							break;
						case 'S':
							this.SaveGame();
							break;
						case 'T':
							this.TakeTurn();
							break;
						case 'F':
							this.ForceClue();
							break;
						case 'G':
							this.PrintGrid();
							break;
						case 'C':
							this.ListClues();
							break;
						case 'Q':
							return;
					}
				} catch (SecurityException ex) {
					Console.Error.WriteLine("Insufficient permissions: {0}.", ex.Demanded);
				} catch (Exception e) {
					if (e is OutOfMemoryException || e is StackOverflowException) throw;
					if (ConsoleHelper.Choose(string.Format("An {0} exception was thrown: {1}.{2}Do you want to try to continue the game?",
						e.GetType().Name, e.Message, Environment.NewLine), false, new[] { "Yes", "No" }) == 1)
						throw;
				}
			}
		}

		/// <summary>
		/// The prepare new or loaded game state.
		/// </summary>
		void prepareNewOrLoadedGameState() {
			this.solutionFoundAlready = false;

			// Hook into the CaseFile changed events so we are notified when the solution
			// has been found.
			this.game.Clues.CollectionChanged += this.Clues_CollectionChanged;
			this.game.BadClueDetected += this.game_BadClueDetected;
		}

		/// <summary>
		/// Prints the grid.
		/// </summary>
		private void PrintGrid() {
			this.PrintGridHeader(this.game.Suspects.OfType<Card>());
			this.PrintGridBody(this.game.Suspects.OfType<Card>());
			this.PrintGridHeader(this.game.Weapons.OfType<Card>());
			this.PrintGridBody(this.game.Weapons.OfType<Card>());
			this.PrintGridHeader(this.game.Places.OfType<Card>());
			this.PrintGridBody(this.game.Places.OfType<Card>());
		}

		/// <summary>
		/// Prints the grid body.
		/// </summary>
		/// <param name="cards">The cards.</param>
		private void PrintGridBody(IEnumerable<Card> cards) {
			foreach (Player player in this.game.Players) {
				this.PrintGridRow(player, cards);
			}
			this.PrintGridRow(this.game.CaseFile, cards);
		}

		/// <summary>
		/// Prints the grid header.
		/// </summary>
		/// <param name="cards">The cards.</param>
		private void PrintGridHeader(IEnumerable<Card> cards) {
			// Print header row with card names
			Console.Write(new string(' ', this.playerColumnWidth + 1));
			foreach (Card card in cards) {
				Console.Write("{0} ", card.Name);
			}
			Console.WriteLine();
		}

		/// <summary>
		/// Prints an individual grid row.
		/// </summary>
		/// <param name="player">The player.</param>
		/// <param name="cards">The cards.</param>
		private void PrintGridRow(ICardHolder player, IEnumerable<Card> cards) {
			string name = (player is Player) ? (player as Player).Name : "Case File";
			Console.Write("{0,-" + this.playerColumnWidth + "} ", name);
			foreach (Card card in cards) {
				bool? isSelected = player.HasCard(card);
				string value = "?";
				if (isSelected.HasValue) {
					value = isSelected.Value ? "1" : "0";
				}
				Console.Write(ConsoleHelper.CenterString(value, card.Name.Length, card.Name.Length + 1));
			}
			Console.WriteLine();
		}

		/// <summary>
		/// Resolves conflicting clues.
		/// </summary>
		private void ResolveConflicts() {
			var conflictingClues = this.game.FindContradictingClues().ToArray();
			Clue badClue = ConsoleHelper.Choose("Which of these clues are incorrect?", true, clue => clue.ToString(), conflictingClues);
			if (badClue != null) {
				this.game.Clues.Remove(badClue);
			}
		}

		/// <summary>
		/// Saves the game.
		/// </summary>
		/// <returns>A value indicating whether the game was actually saved. False if the user canceled the Save dialog.</returns>
		private bool? SaveGame() {
			bool? result = this.saveGameDialog.ShowDialog();
			if (result.HasValue && result.Value) {
				try {
					// We remove the event handlers hooking to this console app so that
					// we don't serialize an extra instance of it.
					this.game.Clues.CollectionChanged -= this.Clues_CollectionChanged;
					this.game.BadClueDetected -= this.game_BadClueDetected;

					// Serialize the game state.
					IFormatter formatter = new BinaryFormatter();

					// Save to a memory stream first, to make sure that serialization will complete
					// successfully before we knock out an existing file.
					MemoryStream ms = new MemoryStream();
					formatter.Serialize(ms, this.game);
					formatter.Serialize(ms, this.interactivePlayer.Name);

					// Now that we've gotten this far, go ahead and write to the actual file.
					using (Stream s = this.saveGameDialog.OpenFile()) {
						ms.WriteTo(s);
					}
				} finally {
					// Restore the event handlers.
					this.game.Clues.CollectionChanged += this.Clues_CollectionChanged;
					this.game.BadClueDetected += this.game_BadClueDetected;
				}
			}
			return result;
		}

		private static void SetupGameFileDialog(FileDialog dlg) {
			dlg.DefaultExt = "clueBuddy";
			dlg.Filter = "ClueBuddy games (*.clueBuddy)|*.clueBuddy|All Files|*.*";
			dlg.FilterIndex = 0;
		}

		/// <summary>
		/// Sets up the players in the game.
		/// </summary>
		private void SetupPlayers() {
			int players = ConsoleHelper.AskNumber("How many players?");
			Console.WriteLine("In clockwise order...");
			for (int i = 0; i < players; i++) {
				string name;
				do {
					name = ConsoleHelper.AskString(string.Format("Player {0} name:", i + 1));
				}
				while (string.IsNullOrEmpty(name));

				this.game.Players.Add(new Player(name));
			}
			while (!this.game.CardAssignmentsAcceptable) {
				for (int i = 0; i < players; i++) {
					this.game.Players[i].CardsHeldCount = ConsoleHelper.AskNumber(string.Format("How many cards is {0} holding?", this.game.Players[i].Name));
				}
				if (!this.game.CardAssignmentsAcceptable) {
					Console.Error.WriteLine("ERROR: Those cards do not add up to {0}.", this.game.Cards.Count() - 3);
				}
			}
			this.interactivePlayer = this.ChoosePlayer("Which player are you?", false, true);
		}

		/// <summary>
		/// The spy.
		/// </summary>
		private void Spy() {
			// Display a hint to the user about which player might be the most beneficial to spy on.
			// Factors to consider include: 
			// * what % of cards held by a player are still unknown? (odds of benefit)
			// * what % of the unknown cards are also unknown to the case file? (size of benefit assuming any)
			// TODO: an enhancement would be to simulate each of the unknown nodes for this player and measure
			// the size of any cascading effect.
			var stats = from p in this.game.Players
						where p != this.interactivePlayer
						let handSize = p.CardsHeldCount
						let knownCardsInHand = this.game.Nodes.Count(n => n.CardHolder == p && n.IsSelected.HasValue && n.IsSelected.Value)
						let unknownCardsInHand = handSize - knownCardsInHand
						let possiblyRevealingCards = this.game.Nodes.Count(n => n.CardHolder == p && !n.IsSelected.HasValue)
						let possibleCardsThatMayBeInCaseFile = this.game.Nodes.Count(n => n.CardHolder == p && !n.IsSelected.HasValue && !this.game.Nodes.First(cn => cn.CardHolder == this.game.CaseFile && cn.Card == n.Card).IsSelected.HasValue)
						let oddsOfAnyBenefit = (float)unknownCardsInHand / p.CardsHeldCount
						let oddsOfBenefitBeingSubstantial = possiblyRevealingCards > 0 ? ((float)possibleCardsThatMayBeInCaseFile / possiblyRevealingCards) : 0
						let choiceStrength = oddsOfAnyBenefit * oddsOfBenefitBeingSubstantial
						orderby choiceStrength descending
						select new {
							p.Name,
							OddsOfAnyBenefit = oddsOfAnyBenefit,
							OddsOfBenefitBeingSubstantial = oddsOfBenefitBeingSubstantial,
							ChoiceStrength = choiceStrength
						};

			Console.Write("{0,-" + this.playerColumnWidth + "} ", "Player");
			Console.Write("{0,20} ", "Odds of any benefit");
			Console.Write("{0,36} ", "Odds of an unknown card being useful");
			Console.Write("{0,20} ", "Strength of choice");
			Console.WriteLine();
			foreach (var stat in stats) {
				Console.Write("{0,-" + this.playerColumnWidth + "} ", stat.Name);
				Console.Write("{0,20:0}% ", stat.OddsOfAnyBenefit * 100);
				Console.Write("{0,36:0}% ", stat.OddsOfBenefitBeingSubstantial * 100);
				Console.Write("{0,20:0}%", stat.ChoiceStrength * 100);
				Console.WriteLine();
			}

			// Begin spying
			SpyCard clue = new SpyCard();
			clue.Player = this.ChoosePlayer("Spy on which player?", true, false);
			if (clue.Player == null) return;
			clue.Card = ConsoleHelper.Choose("Which card did you see?", true, c => c.Name, clue.PossiblySeenCards.ToArray());
			if (clue.Card == null) return;
			this.game.Clues.Add(clue);
		}

		/// <summary>
		/// Conducts the user through entering a suggestion.
		/// </summary>
		private void Suggestion() {
			Suspicion suggestion = this.GetSuggestion();
			if (suggestion == null) return;
			List<Clue> deducedClues = new List<Clue>();
			foreach (Player opponent in this.game.PlayersInOrderAfter(this.suggestingPlayer)) {
				bool explicitAnswer = false;
				bool? disproved;

				// Do we already know whether this player could disprove it?
				if ((disproved = opponent.HasAtLeastOneOf(suggestion.Cards)).HasValue) {
					ConsoleHelper.WriteColor(
						ConsoleHelper.QuestionColor,
						"{0} {1} disprove {2}.",
						opponent.Name,
						disproved.Value ? "CAN" : "CANNOT",
						suggestion);
				} else {
					// Ask the gamer if the opponent did.
					switch (
						ConsoleHelper.Choose(
							string.Format("Could {0} disprove {1}?", opponent.Name, suggestion),
							false,
							new[] { "Yes", "No", "Skip player", "Abort suggestion" })) {
						case 0:
							disproved = true;
							explicitAnswer = true;
							break;
						case 1:
							disproved = false;
							explicitAnswer = true;
							break;
						case 2:
							continue;
						case 3:
							return;
					}
				}
				Card alabi = null;
				if (this.suggestingPlayer == this.interactivePlayer && disproved.HasValue && disproved.Value) {
					IEnumerable<Card> possiblyShownCards = opponent.PossiblyHeldCards.Where(c => suggestion.Cards.Contains(c));
					if (possiblyShownCards.Count() == 1) {
						ConsoleHelper.WriteColor(
							ConsoleHelper.QuestionColor, "{0} must have shown you {1}.", opponent, alabi = possiblyShownCards.First());
					} else {
						alabi = ConsoleHelper.Choose(
							string.Format("Which card did {0} show you?", opponent), true, c => c.Name, possiblyShownCards.ToArray());
					}
				}
				if (disproved.HasValue) {
					Clue clue;
					if (disproved.Value) {
						clue = new Disproved(opponent, suggestion, alabi);
					} else {
						clue = new CannotDisprove(opponent, suggestion);
					}
					if (!explicitAnswer) {
						deducedClues.Add(clue);
					}
					this.game.Clues.Add(clue);
				}

				if (disproved.HasValue && disproved.Value && this.game.Rules.DisprovalEndsTurn) {
					break;
				}
			}

			// We added clues that we could predict as explicit clues.
			// But if one of them were wrong (due to a mistaken answer by another player or data entry error)
			// we are increasing the strength of the mistake here by making a new clue based on our deductions.
			// So remove the deduced clues if the operator found a problem.
			if (deducedClues.Count > 0) {
				if (!ConsoleHelper.AskYesOrNo("Were the deduced answers correct?", false).Value) {
					deducedClues.ForEach(badClue => this.game.Clues.Remove(badClue));
					Console.WriteLine("Deduced answers were removed from the set of clues.  Use Force to add the correct answer and resolve conflicts.");
				}
			}
		}

		/// <summary>
		/// The take turn.
		/// </summary>
		private void TakeTurn() {
			this.suggestingPlayer = this.ChoosePlayer("Whose turn is it?", true, true);
			if (this.suggestingPlayer == null) return;
			try {
				if (this.suggestingPlayer == this.interactivePlayer && this.game.Rules.HasSpyglass) {
					while (true) {
						var turnMenu = new Dictionary<char, string>();
						turnMenu.Add('S', "Make a suggestion");
						turnMenu.Add('L', "Look at someone else's card");
						turnMenu.Add('E', "End turn");
						switch (ConsoleHelper.Choose("What do you want to do?", turnMenu, s => s).Key) {
							case 'S':
								this.Suggestion();
								break;
							case 'L':
								this.Spy();
								break;
							case 'E':
								return;
						}
					}
				} else if (this.suggestingPlayer == this.interactivePlayer) {
					this.Suggestion();
				} else {
					var turnMenu = new Dictionary<char, string>();
					turnMenu.Add('S', "Make a suggestion");
					turnMenu.Add('A', "Make an accusation");
					turnMenu.Add('E', "End turn");
					switch (ConsoleHelper.Choose("What do you want to do?", turnMenu, s => s).Key) {
						case 'S':
							this.Suggestion();
							break;
						case 'A':
							this.Accusation();
							break;
						case 'E':
							return;
					}
				}
			} finally {
				this.suggestingPlayer = null;
			}
		}

		#endregion
	}
}
