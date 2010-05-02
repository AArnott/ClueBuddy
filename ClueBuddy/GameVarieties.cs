//-----------------------------------------------------------------------
// <copyright file="GameVarieties.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ClueBuddy {
	using System.Collections.Generic;
	using System.IO;
	using System.Xml.Serialization;

	[XmlType(Namespace = TypeNamespace)]
	public class GameVariety {
		/// <summary>
		/// The default file extension for a saved game variety file.
		/// Does not include the period.
		/// </summary>
		public const string DefaultFileExtension = "clueVariety";

		/// <summary>
		/// The namespace to use when serializing the game.
		/// </summary>
		private const string TypeNamespace = "http://www.nerdbank.net/clue/variety";

		/// <summary>
		/// Initializes a new instance of the <see cref="GameVariety"/> class.
		/// </summary>
		public GameVariety() {
			this.Places = new List<Place>();
			this.Suspects = new List<Suspect>();
			this.Weapons = new List<Weapon>();
		}

		/// <summary>
		/// Gets or sets the set of rules this game is playing by.
		/// </summary>
		public GameRules Rules { get; set; }

		/// <summary>
		/// Gets or sets the weapon cards in the game.
		/// </summary>
		public List<Weapon> Weapons { get; set; }

		/// <summary>
		/// Gets or sets the suspect cards in the game.
		/// </summary>
		public List<Suspect> Suspects { get; set; }

		/// <summary>
		/// Gets or sets the place cards in the game.
		/// </summary>
		public List<Place> Places { get; set; }

		/// <summary>
		/// Gets an enumeration of all the cards included in this game.
		/// </summary>
		/// <value>The cards.</value>
		[XmlIgnore]
		public IEnumerable<Card> Cards {
			get {
				foreach (var card in this.Suspects) {
					yield return card;
				}
				foreach (var card in this.Weapons) {
					yield return card;
				}
				foreach (var card in this.Places) {
					yield return card;
				}
			}
		}

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		[XmlAttribute]
		public string Name { get; set; }

		/// <summary>
		/// Loads a game variety.
		/// </summary>
		/// <param name="stream">The stream to load the game variety from.</param>
		/// <returns>The game variety instance.</returns>
		public static GameVariety LoadFrom(Stream stream) {
			return (GameVariety)GetSerializer().Deserialize(stream);
		}

		/// <summary>
		/// Constructs a <see cref="Game"/> based on the variety described by this
		/// <see cref="GameVariety"/> instance.
		/// </summary>
		/// <returns>A new game instance.</returns>
		public Game Initialize() {
			return new Game(this.Name, this.Rules, this.Cards);
		}

		/// <summary>
		/// Saves the game..
		/// </summary>
		/// <param name="stream">The stream to save the game to.</param>
		public void Save(Stream stream) {
			GetSerializer().Serialize(stream, this);
		}

		/// <summary>
		/// Gets the serializer to use for game varieties.
		/// </summary>
		/// <returns>An XmlSerializer instance.</returns>
		private static XmlSerializer GetSerializer() {
			var serializer = new XmlSerializer(typeof(GameVariety), TypeNamespace);
			return serializer;
		}
	}
}
