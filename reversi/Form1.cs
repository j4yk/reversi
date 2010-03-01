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

        /// <summary>
        /// Spieler an der Reihe
        /// </summary>
        int Spieler = 1;

        int[,] Feld;
        bool gameOver = false;
        Color[] Farben = new Color[3] { SystemColors.Control, Color.Red, Color.Blue };

        int fieldw, fieldh = 10;

        void NeuesSpiel()
        {
            Feld = new int[6, 6];
            Feld[2, 2] = 1;
            Feld[3, 2] = 2;
            Feld[2, 3] = 1;
            Feld[3, 3] = 2;
            SpielfeldAktualisieren();
        }

        private void boardPanel_Paint(object sender, PaintEventArgs e)
        {
            // Spielsteine
            for (int y = 0; y < 6; y++)
            {
                for (int x = 0; x < 6; x++)
                {
                    e.Graphics.FillRectangle(new SolidBrush(Farben[Feld[x, y]]),
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
        /// Wechselt den Spieler und überprüft, ob der einen Zug ausführen kann.
        /// Kann er dies nicht, ist der ursprüngliche Spieler wieder dran.
        /// Kann der auch nichts machen, wird das Spiel beendet, indem <see cref="BeendeSpiel"/> aufgerufen wird.
        /// </summary>
        void SpielerWechsel()
        {
            Spieler = ReversiLogik.AndererSpieler(Spieler);

            // Ende?
            if (!ReversiLogik.ZugMöglich(Feld, Spieler))
            {
                Spieler = ReversiLogik.AndererSpieler(Spieler);
                if (!ReversiLogik.ZugMöglich(Feld, Spieler))
                    // der andere kann auch nichts machen, Ende
                    BeendeSpiel();
                else
                    // der Spieler muss passen
                    MessageBox.Show(string.Format("Spieler {0} kann keinen gültigen Zug vornehmen!\nSpieler {1} ist dran.",
                        Spieler, ReversiLogik.AndererSpieler(Spieler)));
            }

            // Anzeige aktualisieren
            spielerDispPanel.Refresh();
        }

        /// <summary>
        /// Beendet das Spiel, indem <see cref="gameOver"/> auf true gesetzt wird.
        /// Eine MessageBox macht auf das Spielergebnis aufmerksam.
        /// </summary>
        private void BeendeSpiel()
        {
            gameOver = true;
            int[] statistik = FelderStatistik();
            int winner = statistik[1] > statistik[2] ? 1 : statistik[2] > statistik[1] ? 2 : 0;
            MessageBox.Show("Spiel vorbei! " + (winner == 0 ? "Unentschieden" : string.Format("Spieler {0} gewinnt.", winner)), 
                "Spielende");
        }

        /// <summary>
        /// Zählt die Felder aus.
        /// </summary>
        /// <returns>Ein int-Array mit folgenden Posten: { leere Felder, Felder von Spieler 1, Felder von Spieler 2 }</returns>
        int[] FelderStatistik()
        {
            int[] counter = new int[3];
            // Felder auszählen
            for (int y = 0; y < 6; y++)
                for (int x = 0; x < 6; x++)
                    counter[Feld[x, y]]++;
            return counter;
        }

        /// <summary>
        /// Versucht einen Zug auf das angegebene Feld <paramref name="Ziel"/> vorzunehmen.
        /// </summary>
        /// <param name="Ziel">Ortsvektor zum Feld, auf das gezogen werden soll.</param>
        void VersucheZug(Vektor Ziel)
        {
            if (ReversiLogik.ZugGültig(Feld, Ziel, Spieler))
            {
                List<Vektor> gewonneneFelder = ReversiLogik.GewonneneFelder(Feld, Ziel, Spieler);
                // Ändere Felder
                foreach (Vektor p in gewonneneFelder)
                    Feld[p.X, p.Y] = Spieler;

                int[] counter = FelderStatistik();
                score1.Text = counter[1].ToString();
                score2.Text = counter[2].ToString();

                // neu zeichnen
                SpielfeldAktualisieren();

                SpielerWechsel();
            }
        }

        private void SpielfeldAktualisieren()
        {
            boardPanel.Refresh();
        }

        private void boardPanel_MouseClick(object sender, MouseEventArgs e)
        {
            if (!gameOver)
            {
                int x = e.X - boardPanel.ClientRectangle.Left;
                int y = e.Y - boardPanel.ClientRectangle.Top;
                Vektor Ziel = ToBoardCoordinates(e.X, e.Y);
                if (Ziel.X < 6 && Ziel.Y < 6)
                {
                    VersucheZug(Ziel);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            score1.BackColor = Farben[1];
            score2.BackColor = Farben[2];
            NeuesSpiel();
            Spieler = new Random().Next(1, 3);
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

        private void boardPanel_MouseMove(object sender, MouseEventArgs e)
        {
        }

        private void spielerDispPanel_Paint(object sender, PaintEventArgs e)
        {
            // in der Spielerfarbe einfärben
            e.Graphics.FillRectangle(new SolidBrush(Farben[Spieler]), e.ClipRectangle);
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
