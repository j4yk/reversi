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
            int andererSpieler = AndererSpieler(Spieler);

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
    }
}
