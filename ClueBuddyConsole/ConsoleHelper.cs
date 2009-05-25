using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ClueBuddyConsole {
	internal static class ConsoleHelper {
		public static ConsoleColor QuestionColor = ConsoleColor.Yellow;

		public static T Choose<T>(string prompt, bool includeSkip, Func<T, string> toString, params T[] options) {
			var dict = new Dictionary<char, T>();
			for (int i = 0; i < options.Length; i++) {
				dict.Add((char)('A' + i), options[i]);
			}
			if (includeSkip) {
				dict.Add((char)('A' + options.Length), default(T));
			}
			return Choose(prompt, dict, c => (c == null) ? "Skip" : toString(c)).Value;
		}

		public static KeyValuePair<char, T> Choose<T>(string prompt, Dictionary<char, T> options, Func<T, string> toString) {
			WriteColor(QuestionColor, prompt);
			foreach (KeyValuePair<char, T> pair in options) {
				Debug.Assert(pair.Key == pair.Key.ToString().ToUpper()[0]);
				Console.WriteLine("{0}. {1}", pair.Key.ToString().ToUpper(), toString(pair.Value));
			}
			Console.Write("Selection: ");
			char keyPressed = ' ';
			while (!options.ContainsKey(keyPressed))
				keyPressed = Console.ReadKey(true).KeyChar.ToString().ToUpper()[0];
			Console.WriteLine(keyPressed);
			return new KeyValuePair<char, T>(keyPressed, options[keyPressed]);
		}

		public static int Choose(string prompt, bool includeSkip, string[] options) {
			string result = Choose<string>(prompt, includeSkip, s => s, options);
			int indexOfSelection = Array.IndexOf(options, result);
			return indexOfSelection;
		}

		public static bool? AskYesOrNo(string prompt, bool includeSkip) {
			int result = Choose(prompt, includeSkip, new[] { "Yes", "No" });
			switch (result) {
				case 0:
					return true;
				case 1:
					return false;
				default:
					return null;
			}
		}

		public static string AskString(string prompt) {
			Console.Write(prompt + " ");
			return Console.ReadLine();
		}

		public static int AskNumber(string prompt) {
			Console.Write(prompt + " ");
			int result;
			while (!int.TryParse(Console.ReadLine(), out result)) {
				Console.Error.WriteLine("Invalid input.");
				Console.Write(prompt + " ");
			}
			return result;
		}

		public static void WriteColor(ConsoleColor color, string prompt, params object[] args) {
			ConsoleColor oldColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.WriteLine(prompt, args);
			Console.ForegroundColor = oldColor;
		}

		public static string CenterString(string value, int maxLength, int spacing) {
			Debug.Assert(maxLength <= spacing);
			int actualCharactersFromValueToDisplay = Math.Min(maxLength, value.Length);
			StringBuilder result = new StringBuilder(spacing);

			int prefixCharacters, suffixCharacters;
			prefixCharacters = (spacing - actualCharactersFromValueToDisplay) / 2;
			suffixCharacters = (spacing - actualCharactersFromValueToDisplay) - prefixCharacters;

			result.Append(' ', prefixCharacters);
			result.Append(value, 0, actualCharactersFromValueToDisplay);
			result.Append(' ', suffixCharacters);
			return result.ToString();
		}

	}
}
