using System.Collections.Generic;

namespace Patate.Combat
{
    /// <summary>
    /// Rectangle d'une définition, en unités monde entières.
    /// Convention commune à toutes les données :
    /// origine = pivot du combattant (au sol, entre les pieds) ; x vers l'avant
    /// (le personnage est toujours décrit regardant à droite) ; y vers le haut,
    /// 0 = sol ; (X, Y) = coin bas-gauche.
    /// Les entiers rendent la simulation identique d'une machine à l'autre
    /// (replays, synchronisation réseau, rollback).
    /// </summary>
    public readonly struct Box
    {
        public readonly int X, Y, W, H;

        public Box(int x, int y, int w, int h)
        {
            X = x; Y = y; W = w; H = h;
        }

        public override string ToString() => $"({X}, {Y}, {W}×{H})";
    }

    public readonly struct PointI
    {
        public readonly int X, Y;

        public PointI(int x, int y)
        {
            X = x; Y = y;
        }
    }

    /// <summary>Rectangle placé dans le monde, bornes absolues.</summary>
    public readonly struct WorldBox
    {
        public readonly int Left, Right, Bottom, Top;

        public WorldBox(int left, int right, int bottom, int top)
        {
            Left = left; Right = right; Bottom = bottom; Top = top;
        }

        /// <summary>Recouvrement strict : des rectangles qui se touchent par un bord ne se recouvrent pas.</summary>
        public bool Overlaps(WorldBox other) =>
            Left < other.Right && other.Left < Right && Bottom < other.Top && other.Bottom < Top;
    }

    public static class Geometry
    {
        /// <param name="facing">1 = regarde à droite, -1 = à gauche (la boîte est retournée).</param>
        public static WorldBox ToWorld(Box box, int originX, int originY, int facing)
        {
            int left = facing == 1 ? originX + box.X : originX - box.X - box.W;
            int bottom = originY + box.Y;
            return new WorldBox(left, left + box.W, bottom, bottom + box.H);
        }

        public static bool FirstOverlap(IReadOnlyList<WorldBox> a, IReadOnlyList<WorldBox> b, out WorldBox hitA, out WorldBox hitB)
        {
            foreach (var boxA in a)
            {
                foreach (var boxB in b)
                {
                    if (!boxA.Overlaps(boxB)) continue;
                    hitA = boxA;
                    hitB = boxB;
                    return true;
                }
            }
            hitA = default;
            hitB = default;
            return false;
        }
    }
}
