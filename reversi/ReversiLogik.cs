using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace reversi
{
    public struct Vektor 
    { 
        public int X; 
        public int Y; 

        public Vektor(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public static Vektor operator +(Vektor a, Vektor b)
        {
            return new Vektor(a.X + b.X, a.Y + b.Y);
        }

        public static Vektor operator -(Vektor a, Vektor b)
        {
            return new Vektor(a.X - b.X, a.Y - b.Y);
        }
    }

    public static class ReversiLogik
    {

        public const int LeeresFeld = 0;

        public static int AndererSpieler(int aktuellerSpieler)
        {
            return 3 - aktuellerSpieler;
        }
		
		public static int[] Statistik(int[,] feld)
		{
			int[] counter = new int[3];
			
            // Felder auszählen
            for (int y = 0; y < 6; y++)
                for (int x = 0; x < 6; x++)
                    counter[feld[x, y]]++;
            return counter;
		}

        /// <summary>
        /// Gibt eine Liste von Richtungen zurück, in denen der Spieler an diesem Feld etwas gewinnen könnte.
        /// </summary>
        /// <param name="Feld"></param>
        /// <param name="Ziel"></param>
        /// <param name="Spieler"></param>
        /// <returns></returns>
        static List<Vektor> EffektiveRichtungen(int[,] Feld, Vektor Ziel, int Spieler)
        {
            List<Vektor> möglicheRichtungen = new List<Vektor>();
            
            int otherPlayer = AndererSpieler(Spieler);
            List<Vektor> vektoren = new List<Vektor>();
            // nach benachbarten Gegnersteinen suchen
            for (int y = Ziel.Y - 1; y < Ziel.Y + 2 && y < 6; y++)
            {
                if (y < 0) continue;
                for (int x = Ziel.X - 1; x < Ziel.X + 2 && x < 6; x++)
                {
                    if (x < 0) continue;
                    if ((y == Ziel.Y) && (x == Ziel.X)) continue;
                    else if (Feld[x, y] == otherPlayer)
                        vektoren.Add(new Vektor(x - Ziel.X, y - Ziel.Y));
                }
            }

            if (vektoren.Count > 0)
            {
                foreach (Vektor v in vektoren)
                {
                    // v ist eine Richtung zum Traversieren
                    for (Vektor probe = Ziel + v;
                        probe.X >= 0 && probe.X < 6 && probe.Y >= 0 && probe.Y < 6 && Feld[probe.X, probe.Y] != LeeresFeld;
                        probe += v)
                    {
                        if (Feld[probe.X, probe.Y] == Spieler)
                        {
                            // in dieser Richtung gibt es einen gegenüberliegenden eigenen Stein
                            möglicheRichtungen.Add(v);
                            break;
                        }
                    }
                }
            }

            return möglicheRichtungen;
        }

        public static bool ZugMöglich(int[,] Feld, int Spieler)
        {
            for (int x = 0; x < 6; x++)
                for (int y = 0; y < 6; y++)
                {
                    if (Feld[x, y] == LeeresFeld)
                    {
                        if (EffektiveRichtungen(Feld, new Vektor(x, y), Spieler).Count > 0)
                            return true;
                    }
                }
            return false;
        }

        public static bool ZugGültig(int[,] Feld, Vektor Ziel, int Spieler)
        {
            return Feld[Ziel.X, Ziel.Y] == LeeresFeld 
                && EffektiveRichtungen(Feld, Ziel, Spieler).Count > 0;
        }

        public static List<Vektor> GewonneneFelder(int[,] Feld, Vektor Ziel, int Spieler)
        {
            List<Vektor> felder = new List<Vektor>();

            List<Vektor> richtungen = EffektiveRichtungen(Feld, Ziel, Spieler);
			
            foreach (Vektor v in richtungen)
            {
                // Sammle fremde Felder in der Richtung ein
                for (Vektor pos = Ziel; Feld[pos.X, pos.Y] != Spieler; pos += v)
                    // mehr leere Felder als das angeklickte können nicht eingesammelt werden, 
                    // da die Richtung sonst gar nicht in Betracht gekommen wäre
                    felder.Add(pos);
            }
            
            return felder;
        }


      internal struct MinimaxTreeNode {
	public Vektor GesetztesFeld;
	public int Ergebnis;
	public MinimaxTreeNode[] children;

	public MinimaxTreeNode(Vektor ziel, int ergebnis, MinimaxTreeNode[] children) {
	  this.GesetztesFeld = ziel;
	  this.Ergebnis = ergebnis;
	  this.children = children;
	}
      }						    
		    
      internal static MinimaxTreeNode Minimax(int[,] feld, Vektor ziel, int spieler, int amZug, bool letzterÜbersprungen)
      {
	Vektor dx = new Vektor(1, 0);
	Vektor dy = new Vektor(0, 1);

	List<Vektor> ziele = new List<Vektor>();
	#region Suche mögliche Felder zum Setzen zusammen
	for (Vektor xv = new Vektor(0, 0); xv.X < 6; xv += dx) {
	  for (Vektor yv = new Vektor(0, 0); yv.Y < 6; yv += dy) {
	    if (feld[xv.X, yv.Y] == LeeresFeld)
	      if (EffektiveRichtungen(feld, xv + yv, amZug).Count > 0)
		ziele.Add(xv + yv);
	  }
	}
	#endregion

	if (ziele.Count > 0) {
	  MinimaxTreeNode[] children = new MinimaxTreeNode[ziele.Count];
#region Simuliere nächste Züge und sammle die Knoten in children
	  int i = 0;

	  foreach (Vektor v in ziele) {
	    // Zug ausführen und dann weiterrechnen
	    int old = feld[v.X, v.Y];
	    feld[v.X, v.Y] = amZug;
	    try {
	      children[i++] = Minimax(feld, v, spieler, AndererSpieler(amZug), false);
	    }
	    finally {
	      // feld zurücksetzen
	      feld[v.X, v.Y] = old;
	    }
	  }
#endregion

	  int ergebnis = -2;
#region Finde das beste zu erreichende Ergebnis
	  foreach (MinimaxTreeNode n in children) {
	    if (n.Ergebnis == 1) {
	      ergebnis = 1;
	      break;
	    }
	    else if (n.Ergebnis > ergebnis) {
	      ergebnis = n.Ergebnis;
	    }
	  }
#endregion
	
	  return new MinimaxTreeNode(ziel, ergebnis, children);
	} // if (ziele.Count > 0)
	else {
	  if (letzterÜbersprungen) {
	    // Spiel vorbei, auszählen und schauen, ob gewonnen oder verloren
	    // und dann mach ein Blatt drauß
	    int[] stat = Statistik(feld);
	    if (stat[spieler] > stat[AndererSpieler(spieler)])
	      return new MinimaxTreeNode(new Vektor(-1, -1), 1, null);
	    else if (stat[spieler] < stat[AndererSpieler(spieler)])
	      return new MinimaxTreeNode(new Vektor(-1, -1), -1, null);
	    else
	      return new MinimaxTreeNode(new Vektor(-1, -1), 0, null); // Patt
	  }
	  else {
	    // überspringen
	    return Minimax(feld, new Vektor(-1, -1), spieler, AndererSpieler(amZug), true);
	  }
	}
      }
    }
}
