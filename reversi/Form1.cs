using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace reversi
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Reversi game = new Reversi();

        Color[] Farben = new Color[3] { SystemColors.Control, Color.Red, Color.Blue };

        int fieldw, fieldh = 10;
        
        void NeuesSpiel()
        {
        	game.NeuesSpiel();
            SpielfeldAktualisieren();
        }

        private void boardPanel_Paint(object sender, PaintEventArgs e)
        {
            // Spielsteine
            for (int y = 0; y < 6; y++)
            {
                for (int x = 0; x < 6; x++)
                {
                    e.Graphics.FillRectangle(new SolidBrush(Farben[game.Feld[x, y]]),
                        boardPanel.ClientRectangle.Left + x * fieldw,
                        boardPanel.ClientRectangle.Top + y * fieldh,
                        fieldw, fieldh);
                }
            }
            // Linien
            for (int i = 1; i <= 6; i++)
            {
                e.Graphics.DrawLine(SystemPens.ControlDark,
                    boardPanel.ClientRectangle.Left + i * fieldw,
                    boardPanel.ClientRectangle.Top,
                    boardPanel.ClientRectangle.Left + i * fieldw,
                    boardPanel.ClientRectangle.Top + 6 * fieldh);
            }
            for (int i = 0; i <= 6; i++)
            {
                e.Graphics.DrawLine(SystemPens.ControlDark,
                    boardPanel.ClientRectangle.Left,
                    boardPanel.ClientRectangle.Top + i * fieldh,
                    boardPanel.ClientRectangle.Left + 6 * fieldw,
                    boardPanel.ClientRectangle.Top + i * fieldh);
            }
        }

        /// <summary>
        /// Aktualisiert die Feldergröße
        /// </summary>
        void UpdateFieldSize()
        {
            int width = boardPanel.ClientRectangle.Width;
            int height = boardPanel.ClientRectangle.Height;
            fieldw = width / 6;
            fieldh = height / 6;
            // Quadrate bitte
            if (fieldw < fieldh) fieldh = fieldw;
            else fieldw = fieldh;
        }

        private void boardPanel_Resize(object sender, EventArgs e)
        {
            UpdateFieldSize();
            (sender as Panel).Refresh();
        }

        /// <summary>
        /// Beendet das Spiel, indem <see cref="gameOver"/> auf true gesetzt wird.
        /// Eine MessageBox macht auf das Spielergebnis aufmerksam.
        /// </summary>
        private void SpielBeendet()
        {
            int[] statistik = ReversiLogik.Statistik(game.Feld);
            int winner = statistik[1] > statistik[2] ? 1 : statistik[2] > statistik[1] ? 2 : 0;
            MessageBox.Show("Spiel vorbei! " + (winner == 0 ? "Unentschieden" : string.Format("Spieler {0} gewinnt.", winner)), 
                "Spielende");
        }

		private void StatistikAktualisieren()
		{
			int[] statistik = ReversiLogik.Statistik(game.Feld);
			score1.Text = statistik[1].ToString();
			score2.Text = statistik[2].ToString();
		}

        private void SpielfeldAktualisieren()
        {
            boardPanel.Refresh();
        }

        /// <summary>
        /// Konvertiert Fensterkoordinaten in Brettkoordinaten
        /// </summary>
        /// <param name="x">Fensterkoordinate X-Komponente</param>
        /// <param name="y">Fensterkoordinate Y-Komponente</param>
        /// <returns>Einen <see cref="Vektor"/>, für eine Zelle im Spielfeld.</returns>
        Vektor ToBoardCoordinates(int x, int y)
        {
            int col = (x - boardPanel.ClientRectangle.Left) / fieldw;
            int row = (y - boardPanel.ClientRectangle.Top) / fieldh;
            return new Vektor(col, row);
        }

        private void boardPanel_MouseClick(object sender, MouseEventArgs e)
        {
            if (!game.GameOver)
            {
                Vektor Ziel = ToBoardCoordinates(e.X, e.Y);
                if (Ziel.X < 6 && Ziel.Y < 6)
                {
					int spielerVorher = game.AktuellerSpieler;
					if (game.Zug(Ziel))
					{
						// Zug erfolgreich
						StatistikAktualisieren();
						SpielfeldAktualisieren();
						if (game.GameOver)
							SpielBeendet();
						else if (spielerVorher == game.AktuellerSpieler)
							MessageBox.Show(string.Format("Spieler {0} kann keinen gültigen Zug vornehmen!\nSpieler {1} ist dran.", 
							                              game.AktuellerSpieler, ReversiLogik.AndererSpieler(game.AktuellerSpieler)));
						else
							// Spielerfarbe aktualisieren
							spielerDispPanel.Refresh();
					}
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            score1.BackColor = Farben[1];
            score2.BackColor = Farben[2];
            NeuesSpiel();
        }

        private void boardPanel_MouseMove(object sender, MouseEventArgs e)
        {
        }

        private void spielerDispPanel_Paint(object sender, PaintEventArgs e)
        {
            // in der Spielerfarbe einfärben
            e.Graphics.FillRectangle(new SolidBrush(Farben[game.AktuellerSpieler]), e.ClipRectangle);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            UpdateFieldSize();
            boardPanel.Refresh();
        }

        private void beendenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void neuesSpielToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NeuesSpiel();
        }
    }
}
