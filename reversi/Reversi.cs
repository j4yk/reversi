
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
            feld = new int[6, 6];
            feld[2, 2] = 1;
            feld[3, 2] = 2;
            feld[2, 3] = 1;
            feld[3, 3] = 2;
            spieler = random.Next(1, 3);
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
				return true;
			}
			else
				return false;
		}
				
	}
}
