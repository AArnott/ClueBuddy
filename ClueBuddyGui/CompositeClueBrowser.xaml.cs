using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClueBuddy;

namespace ClueBuddyGui {
	/// <summary>
	/// Interaction logic for CompositeClueBrowser.xaml
	/// </summary>
	public partial class CompositeClueBrowser : System.Windows.Controls.UserControl {
		public CompositeClueBrowser() {
			InitializeComponent();
			//ComboBox cb = (ComboBox)cc.FindName("suggester");
			//cb.ItemsSource = ((Game)DataContext).Players;
			//cb.ItemsSource = Game.GreatDetective.Players;
		}

		void previousClueButton_Click(object sender, EventArgs e) {
		}
	}
}
