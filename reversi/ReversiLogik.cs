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

        public override string ToString()
        {
            return string.Format("X={0}, Y={1}", X, Y);
        }

        public static Vektor operator +(Vektor a, Vektor b)
        {
            return new Vektor(a.X + b.X, a.Y + b.Y);
        }

        public static Vektor operator -(Vektor a, Vektor b)
        {
            return new Vektor(a.X - b.X, a.Y - b.Y);
        }

        public bool  Equals(Vektor b)
        {
            return X == b.X && Y == b.Y;
        }
    }

    public class SpecialVectors
    {
        public static readonly Vektor n = new Vektor(0, -1);
        public static readonly Vektor nw = new Vektor(-1, -1);
        public static readonly Vektor ne = new Vektor(1, -1);
        public static readonly Vektor s = new Vektor(0, 1);
        public static readonly Vektor sw = new Vektor(-1, 1);
        public static readonly Vektor se = new Vektor(1, 1);
        public static readonly Vektor w = new Vektor(-1, 0);
        public static readonly Vektor e = new Vektor(1, 0);
        public static readonly Vektor[] AlleRichtungen = new Vektor[8] { n, ne, e, se, s, sw, w, nw };
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

            felder.Add(Ziel);
            foreach (Vektor v in richtungen)
            {
                // Sammle fremde Felder in der Richtung ein
                for (Vektor pos = Ziel + v; Feld[pos.X, pos.Y] != Spieler; pos += v)
                    // mehr leere Felder als das angeklickte können nicht eingesammelt werden, 
                    // da die Richtung sonst gar nicht in Betracht gekommen wäre
                    felder.Add(pos);
            }

            return felder;
        }

        /// <summary>
        /// Bewertet das Feld aus der Sicht des angegebenen Spielers.
        /// </summary>
        /// <param name="feld"></param>
        /// <param name="spieler"></param>
        /// <returns></returns>
        public static int Bewertung(int[,] feld, int spieler, int spielerAmZug, List<Vektor> bekanntStabil)
        {
            int[] stat = Statistik(feld);
            int result = stat[spieler];
            #region stabile Steine
            for (int x = 0; x < 6; x++)
            {
                for (int y = 0; y < 6; y++)
                {
                    if (!bekanntStabil.Contains(new Vektor(x, y)) && SteinStabil(feld, new Vektor(x, y)))
                        bekanntStabil.Add(new Vektor(x, y));
                }
            }
            result += bekanntStabil.Count;
            #endregion
            if (spieler == spielerAmZug)
            {
            }
            else
            {
                int maxLoss = 0;
                foreach (Vektor zug in MöglicheZüge(feld, spielerAmZug))
                {
                    int n = GewonneneFelder(feld, zug, spielerAmZug).Count;
                    maxLoss = n > maxLoss ? n : maxLoss;
                }
                result -= maxLoss;
            }
            return result;
        }

        #region Stabilität
        static bool Stabil_RichtungKlar(int[,] feld, int farbe, Vektor position, Vektor d)
        {
            Vektor p = position;
            Vektor n = p + d;
            while (n.X >= 0 && n.X < 6 && n.Y >= 0 && n.Y < 6 && feld[n.X, n.Y] == farbe)
            {
                p = n;
                n = p + d;
            }
            if (feld[p.X, p.Y] != LeeresFeld)
                return true;
            else
                return false;
        }

        static bool Stabil_RichtungspaarKlar(int[,] feld, int farbe, Vektor position, Vektor d1, Vektor d2)
        {
            return Stabil_RichtungKlar(feld, farbe, position, d1) && Stabil_RichtungKlar(feld, farbe, position, d2);
        }

        public static bool SteinStabil(int[,] feld, Vektor position)
        {
            int farbe = feld[position.X, position.Y];
            if (farbe == LeeresFeld) return false;
            if (!Stabil_RichtungspaarKlar(feld, farbe, position, SpecialVectors.n, SpecialVectors.s)) return false;
            if (!Stabil_RichtungspaarKlar(feld, farbe, position, SpecialVectors.w, SpecialVectors.e)) return false;
            if (!Stabil_RichtungspaarKlar(feld, farbe, position, SpecialVectors.ne, SpecialVectors.sw)) return false;
            if (!Stabil_RichtungspaarKlar(feld, farbe, position, SpecialVectors.nw, SpecialVectors.se)) return false;
            return true;
        }
        #endregion // Stabilität

        internal static List<Vektor> MöglicheZüge(int[,] feld, int spieler)
        {
            List<Vektor> result = new List<Vektor>();
            for (int x = 0; x < 6; x++)
                for (int y = 0; y < 6; y++)
                {
                    if (feld[x, y] == LeeresFeld && EffektiveRichtungen(feld, new Vektor(x, y), spieler).Count > 0)
                        result.Add(new Vektor(x, y));
                }
            return result;
        }

        internal static List<Vektor> MöglicheZüge(int[,] feld, int spieler, IEnumerable<Vektor> grenzfelder)
        {
            List<Vektor> result = new List<Vektor>();
            foreach (Vektor p in grenzfelder)
                if (EffektiveRichtungen(feld, p, spieler).Count > 0)
                    result.Add(p);
            return result;
        }

        static IEnumerable<Vektor> nachbarfelder(Vektor f)
        {
            return SpecialVectors.AlleRichtungen.Select(v => f + v).Where(v => v.X >= 0 && v.X < 6 && v.Y >= 0 && v.Y < 6);
        }


        internal class MinimaxTreeNode
        {
            public Vektor GesetztesFeld;
            public List<Vektor> GewonneneFelder = new List<Vektor>();
            public List<Vektor> Grenzfelder;
            public int Bewertung;
            public List<MinimaxTreeNode> children = new List<MinimaxTreeNode>();
            public List<Vektor> MöglicheZüge;
            public Dictionary<Vektor, List<Vektor>> EffektiveRichtungen;
            public int SpielerAmZug;
            public List<Vektor> BekannteStabilePositionen = new List<Vektor>();
        }

        [global::System.Serializable]
        public class MinimaxTreeNodeNotInitedException : Exception
        {
            //
            // For guidelines regarding the creation of new exception types, see
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
            // and
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
            //

            public MinimaxTreeNodeNotInitedException() { }
            public MinimaxTreeNodeNotInitedException(string message) : base(message) { }
            public MinimaxTreeNodeNotInitedException(string message, Exception inner) : base(message, inner) { }
            protected MinimaxTreeNodeNotInitedException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
        }

        internal static void MaybeInitMinimaxTreeNode(MinimaxTreeNode node, int[,] feld)
        {
            if (node.SpielerAmZug < 1)
                throw new MinimaxTreeNodeNotInitedException("SpielerAmZug not set!");
            if (node.MöglicheZüge == null)
                node.MöglicheZüge = MöglicheZüge(feld, node.SpielerAmZug);
            if (node.Grenzfelder == null)
            {
                node.Grenzfelder = new List<Vektor>();
                for (int x = 0; x < 6; x++)
                {
                    for (int y = 0; y < 6; y++)
                    {
                        Vektor v = new Vektor(x, y);
                        if (feld[v.X, v.Y] == LeeresFeld && 
                            SpecialVectors.AlleRichtungen.Any(d => (d + v).X > 0 && (d + v).X < 6 && (d + v).Y > 0 && (d + v).Y < 6 &&
                                feld[(d + v).X, (d + v).Y] != LeeresFeld) && 
                            !node.Grenzfelder.Contains(v))
                            node.Grenzfelder.Add(v);
                    }
                }
            }
        }

        internal static MinimaxTreeNode Minimax(MinimaxTreeNode existingNode, int[,] feld,
            int maximierenderSpieler, bool letzterÜbersprungen, int tiefe, int alphaMax, int betaMin)
        {
            MinimaxTreeNode thisNode = existingNode;
            MaybeInitMinimaxTreeNode(existingNode, feld);
            List<Vektor> ziele = new List<Vektor>(existingNode.MöglicheZüge);
            if (tiefe == 0)
            {
                thisNode.Bewertung = Bewertung(feld, maximierenderSpieler, existingNode.SpielerAmZug, existingNode.BekannteStabilePositionen);
            }
            else if (ziele.Count > 0)
            {
                bool cutoff = false;
                foreach (MinimaxTreeNode child in existingNode.children)
                {
                    if (child.GewonneneFelder.Count > 0)
                    {
                        // child wurde schon mal durchgerechnet, die gewonnenen Felder sind klar
                        ziele.Remove(child.GesetztesFeld);  // erledigt
                        bool übersprungen = child.SpielerAmZug == existingNode.SpielerAmZug;
                        Dictionary<Vektor, int> alteFelder = new Dictionary<Vektor, int>(child.GewonneneFelder.Count);
                        foreach (Vektor f in child.GewonneneFelder)
                        {
                            // geändertes speichern
                            alteFelder.Add(f, feld[f.X, f.Y]);
                            // Felder setzen
                            feld[f.X, f.Y] = child.SpielerAmZug;
                        }
                        try
                        {
                            Minimax(child, feld, maximierenderSpieler, übersprungen,
                                tiefe - 1, alphaMax, betaMin);
                            // Alpha-Beta-Cutoffs:
                            if (maximierenderSpieler == existingNode.SpielerAmZug)
                            {
                                // maximierender Spieler, child für den minimierenden
                                if (child.Bewertung >= betaMin)
                                {
                                    // Beta-Cutoff
                                    existingNode.Bewertung = betaMin;
                                    cutoff = true;
                                    break;
                                }
                                if (child.Bewertung > alphaMax)
                                    alphaMax = child.Bewertung;
                            }
                            else
                            {
                                // minimierender Spieler, child für den maximierenden
                                if (child.Bewertung <= alphaMax)
                                {
                                    // Alpha-Cutoff
                                    existingNode.Bewertung = alphaMax;
                                    cutoff = true;
                                    break;
                                }
                                if (child.Bewertung < betaMin)
                                    betaMin = child.Bewertung;
                            }
                        }
                        finally
                        {
                            foreach (Vektor f in alteFelder.Keys)
                                feld[f.X, f.Y] = alteFelder[f];
                        }
                    }
                }
                if (!cutoff)
                    // restliche, vorher vllt. nicht bearbeitete Knoten
                    BewerteZüge(feld, existingNode.Grenzfelder, maximierenderSpieler, existingNode.SpielerAmZug, existingNode.BekannteStabilePositionen, tiefe,
                        ref alphaMax, ref betaMin, ziele, existingNode.children);
                if (maximierenderSpieler == existingNode.SpielerAmZug)
                    existingNode.Bewertung = alphaMax;
                else
                    existingNode.Bewertung = betaMin;
            }
            else
            {
                // keine möglichen Züge, tritt eigentlich nur auf, wenn das Spiel sowieso vorbei ist, 
                // da der Knoten ja durch den des nächsten Zuges ersetzt worden wäre
                if (letzterÜbersprungen)
                {
                    existingNode.Bewertung = Endbewertung(feld, maximierenderSpieler);
                }
                else
                {
                    // ...hierher darf man eigentlich nicht kommen...
                    // überspringen
                    return Minimax(existingNode, feld, maximierenderSpieler, true, tiefe - 1, alphaMax, betaMin);
                }
            }
            return thisNode;
        }

        /// <summary>
        /// Erstellt einen neuen Knoten für diesen Spielzustand.
        /// Nebeneffekte:  bei tiefe != 0 wird BewerteZüge aufgerufen.
        /// </summary>
        internal static MinimaxTreeNode Minimax(int[,] feld, List<Vektor> grenzfelder, Vektor zuletztGezogen, List<Vektor> gewonneneFelder,
            int spieler, int amZug, bool letzterÜbersprungen, List<Vektor> bekanntStabil, int tiefe, int alphaMax, int betaMin)
        {
            // bis zu einer angegebenen Tiefe soll rekursiv durchgerechnet werden
            // und der Baum aufgebaut werden
            // bei tiefe == 0 wird die Bewertungsfunktion aufgerufen
            // per Alpha-Beta-Suche sollen anhand der Bewertungen in dieser Tiefe 
            // "beste" Entscheidungen getroffen werden

            // Idee: erst ab einer bestimmten Anzahl belegter Felder wird so gesucht
            //   sonst werden "leise Züge" gesucht, wo sich möglichst wenige Steine ändern
            //   spart vllt. Zeit am Anfang

            // Idee: noch ein Parameter "grenzfelder", eine Liste der freien Felder, die
            //   belegte Felder als Nachbarn haben
            //   Das spart die Überprüfung von Feldern außer jeder Reichweite.
            //   Bei jedem Zug muss die Liste angepasst werden: das bezogene Feld kommt raus
            //   und seine freien Nachbarn hinein.  In konstanter Zeit möglich.

            MinimaxTreeNode thisNode = new MinimaxTreeNode();

            List<Vektor> möglicheZüge = new List<Vektor>();
            #region Suche mögliche Felder zum Setzen zusammen
            Dictionary<Vektor, List<Vektor>> richtungen = new Dictionary<Vektor, List<Vektor>>();
            foreach (Vektor f in grenzfelder)
            {
                richtungen.Add(f, EffektiveRichtungen(feld, f, amZug));
                if (richtungen[f].Count > 0)
                    möglicheZüge.Add(f);
            }
            thisNode.EffektiveRichtungen = richtungen;
            #endregion

            int bewertung;
            List<MinimaxTreeNode> children = new List<MinimaxTreeNode>(möglicheZüge.Count);
            if (tiefe == 0)
            {
                bewertung = Bewertung(feld, spieler, amZug, bekanntStabil);
                // kopiere das für später, damit man da gleich ansetzen kann
                thisNode.Grenzfelder = new List<Vektor>(grenzfelder);
            }
            else if (möglicheZüge.Count > 0)
            {
                #region Simuliere nächste Züge und sammle die Knoten in children, Alpha- und Beta-Cutoffs
                BewerteZüge(feld, grenzfelder, spieler, amZug, bekanntStabil, tiefe, ref alphaMax, ref betaMin, möglicheZüge, children);
                if (spieler == amZug)
                    bewertung = children.Max(n => n.Bewertung);
                else
                    bewertung = children.Min(n => n.Bewertung);
                #endregion
            } // if (ziele.Count > 0)
            else
            {
                if (letzterÜbersprungen)
                {
                    bewertung = Endbewertung(feld, spieler);
                }
                else
                {
                    // aussetzen (dafür nicht extra einen Knoten einhängen)
                    return Minimax(feld, grenzfelder, zuletztGezogen, gewonneneFelder, spieler, AndererSpieler(amZug), true, bekanntStabil, tiefe - 1, alphaMax, betaMin);
                }
            }

            thisNode.Bewertung = bewertung;
            thisNode.children = children;
            thisNode.GesetztesFeld = zuletztGezogen;
            thisNode.GewonneneFelder = gewonneneFelder;
            thisNode.MöglicheZüge = möglicheZüge;
            thisNode.SpielerAmZug = amZug;
            return thisNode;
        }

        private static int Endbewertung(int[,] feld, int spieler)
        {
            int bewertung;

            // Spiel vorbei, auszählen und schauen, ob gewonnen oder verloren
            // und dann mach einen Blattknoten drauß
            int[] stat = Statistik(feld);
            if (stat[spieler] > stat[AndererSpieler(spieler)])
                bewertung = int.MaxValue;
            else if (stat[spieler] < stat[AndererSpieler(spieler)])
                bewertung = int.MinValue;
            else
                bewertung = 0; // Patt
            return bewertung;
        }

        /// <summary>
        /// Nebeneffekte:  Knoten werden zu children hinzugefügt, Nebeneffekte von Funktion Minimax (ohne Knoten im Aufruf),
        /// ändert alphaMax und betaMin, wenn neue Rekordwerte erzielt werden
        /// </summary>
        private static void BewerteZüge(int[,] feld, List<Vektor> grenzfelder, int spieler, int amZug, List<Vektor> bekanntStabil,
            int tiefe, ref int alphaMax, ref int betaMin, List<Vektor> züge, List<MinimaxTreeNode> children/*, ref int childrenIndex*/)
        {
            foreach (Vektor zug in züge)
            {
                // Zug ausführen und dann weiterrechnen
                List<Vektor> gewonneneFelder = GewonneneFelder(feld, zug, amZug);
                Dictionary<Vektor, int> alteFelder = new Dictionary<Vektor, int>(gewonneneFelder.Count);
                foreach (Vektor f in gewonneneFelder)
                {
                    alteFelder.Add(f, feld[f.X, f.Y]);
                    feld[f.X, f.Y] = amZug;
                }
                #region Grenzfelder aktualisieren
                grenzfelder.Remove(zug);  // nun belegt
                IEnumerable<Vektor> nachbarn = nachbarfelder(zug);
                foreach (Vektor nachbar in nachbarn)
                {
                    // neue angrenzende Felder hinzufügen
                    if (feld[nachbar.X, nachbar.Y] == LeeresFeld && !grenzfelder.Contains(nachbar))
                        grenzfelder.Add(nachbar);
                }
                #endregion
                try
                {
                    MinimaxTreeNode child = Minimax(feld, grenzfelder, zug, gewonneneFelder, 
                        spieler, AndererSpieler(amZug), false,
                        new List<Vektor>(bekanntStabil), tiefe - 1, alphaMax, betaMin);
                    // speichern
                    children.Add(child);

                    if (spieler == amZug) // maximierender Spieler
                    {
                        // der Kindknoten ist beim minimierenden Spieler
                        if (child.Bewertung >= betaMin)
                        {
                            // Beta-Cutoff
                            //bewertung = betaMin;
                            break;
                        }
                        if (child.Bewertung > alphaMax)
                            // neuer Maximumrekord, Alpha anpassen
                            alphaMax = child.Bewertung;
                    }
                    else  // minimierender Spieler
                    {
                        if (child.Bewertung <= alphaMax)
                        {
                            // Alpha-Cutoff
                            //bewertung = alphaMax;
                            break;
                        }
                        if (child.Bewertung < betaMin)
                            // neuer Minimumrekord, Beta anpassen
                            betaMin = child.Bewertung;
                    }
                }
                finally
                {
                    // feld zurücksetzen
                    foreach (Vektor f in gewonneneFelder)
                        feld[f.X, f.Y] = alteFelder[f];
                    // grenzfelder zurücksetzen
                    grenzfelder.Add(zug);
                    foreach (Vektor nachbar in nachbarn)
                        grenzfelder.Remove(nachbar);
                }
            }
        }
    }
}
