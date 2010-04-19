
using System;
using System.Collections.Generic;

namespace reversi
{
	public class Reversi
	{
		int spieler;
        int[,] feld;
        bool gameOver = false;
		Random random = new Random();

        ReversiLogik.MinimaxTreeNode[] minimax = new ReversiLogik.MinimaxTreeNode[3];

        public Dictionary<Vektor, int> Bewertungen = new Dictionary<Vektor, int>();
        
		public Reversi()
		{
		}
		
		internal int[,] Feld
		{
			get { return feld; }
		}
		
		public bool GameOver
		{
			get { return gameOver; }
		}
		
		public int AktuellerSpieler
		{
			get { return spieler; }
		}
		
		public void NeuesSpiel ()
		{
            gameOver = false;
            feld = new int[6, 6];
            feld[2, 2] = 1;
            feld[3, 2] = 2;
            feld[2, 3] = 1;
            feld[3, 3] = 2;
            spieler = 1;
            minimax[1] = new ReversiLogik.MinimaxTreeNode();
            minimax[2] = new ReversiLogik.MinimaxTreeNode();
		}
		
		public bool SpielerWechsel()
		{
			spieler = ReversiLogik.AndererSpieler(spieler);
			
			// Ende?
			if (!ReversiLogik.ZugMöglich(feld, spieler))
			{
				spieler = ReversiLogik.AndererSpieler(spieler);
				if (!ReversiLogik.ZugMöglich(feld, spieler))
					BeendeSpiel();
				return false;
			}
			else
				return true;
		}
		
		private void BeendeSpiel()
		{
			gameOver = true;
		}
		
		/// <summary>
		/// Führt den Zug aus, sofern er gültig ist, und wechselt den Spieler.
		/// </summary>
		/// <param name="ziel">
		/// A <see cref="Vektor"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>, true, wenn der Zug ausgeführt wurde.
		/// </returns>
		public bool Zug(Vektor ziel)
		{
			if (ReversiLogik.ZugGültig(feld, ziel, spieler))
			{
				List<Vektor> gewonneneFelder = ReversiLogik.GewonneneFelder(feld, ziel, spieler);
				foreach (Vektor p in gewonneneFelder)
					feld[p.X, p.Y] = spieler;
				SpielerWechsel();
                // Minimax updaten
                Bewertungen.Clear();
                //for (int i = 1; i < minimax.Length; i++)
                //    if (minimax[i] != null)
                //    {
                //        ReversiLogik.MinimaxTreeNode node = new List<ReversiLogik.MinimaxTreeNode>(minimax[i].children).Find(n => n.GesetztesFeld.Equals(ziel));
                //        if (node != null)
                //            minimax[i] = node;
                //        else
                //            minimax[i] = new ReversiLogik.MinimaxTreeNode();
                //        break;
                //    }
                minimax[1] = new ReversiLogik.MinimaxTreeNode();
                minimax[2] = new ReversiLogik.MinimaxTreeNode();
				return true;
			}
			else
				return false;
		}

        public Dictionary<Vektor, int> BerechneBewertungen(int tiefe)
        {
            // neu berechnen
            if (minimax[AktuellerSpieler] == null)
                minimax[AktuellerSpieler] = new ReversiLogik.MinimaxTreeNode();
            if (minimax[AktuellerSpieler].SpielerAmZug == 0)
                minimax[AktuellerSpieler].SpielerAmZug = AktuellerSpieler;
            ReversiLogik.Minimax(minimax[AktuellerSpieler], this.Feld, this.AktuellerSpieler, false, // TODO: das false muss nicht immer sein!
                tiefe, int.MinValue, int.MaxValue);
            this.Bewertungen.Clear();
            foreach (ReversiLogik.MinimaxTreeNode n in minimax[AktuellerSpieler].children)
            {
                this.Bewertungen.Add(n.GesetztesFeld, n.Bewertung);
            }
            return this.Bewertungen;
        }
	}
}
