using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace ClueBuddy {
	[XmlType(Namespace = TypeNamespace)]
	public class GameVariety {
		/// <summary>
		/// The default file extension for a saved game variety file.
		/// Does not include the period.
		/// </summary>
		public const string DefaultFileExtension = "clueVariety";
		const string TypeNamespace = "http://www.nerdbank.net/clue/variety";
		static XmlSerializer getSerializer() {
			XmlSerializer serializer = new XmlSerializer(typeof(GameVariety), TypeNamespace);
			return serializer;
		}
		public static GameVariety LoadFrom(Stream stream) {
			return (GameVariety)getSerializer().Deserialize(stream);
		}
		public void Save(Stream stream) {
			getSerializer().Serialize(stream, this);
		}

		/// <summary>
		/// Constructs a <see cref="Game"/> based on the variety described by this
		/// <see cref="GameVariety"/> instance.
		/// </summary>
		public Game Initialize() {
			return new Game(Name, Rules, Cards);
		}

		GameRules rules;
		/// <summary>
		/// The set of rules this game is playing by.
		/// </summary>
		public GameRules Rules {
			get { return rules; }
			set { rules = value; }
		}

		List<Weapon> weapons = new List<Weapon>();
		/// <summary>
		/// The weapon cards in the game.
		/// </summary>
		public List<Weapon> Weapons {
			get { return weapons; }
			set { weapons = value; }
		}

		List<Suspect> suspects = new List<Suspect>();
		/// <summary>
		/// The suspect cards in the game.
		/// </summary>
		public List<Suspect> Suspects {
			get { return suspects; }
			set { suspects = value; }
		}

		List<Place> places = new List<Place>();
		/// <summary>
		/// The place cards in the game.
		/// </summary>
		public List<Place> Places {
			get { return places; }
			set { places = value; }
		}

		[XmlIgnore]
		public IEnumerable<Card> Cards {
			get {
				foreach (var card in Suspects) {
					yield return card;
				}
				foreach (var card in Weapons) {
					yield return card;
				}
				foreach (var card in Places) {
					yield return card;
				}
			}
		}

		string name;
		[XmlAttribute]
		public string Name {
			get { return name; }
			set { name = value; }
		}
	}
}
