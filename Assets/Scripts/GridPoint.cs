using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;

namespace KaimiraGames.GameJam
{
    public struct GridPoint : IEquatable<GridPoint>
    {
        public readonly int X { get; }

        public readonly int Y { get; }

        public static readonly GridPoint Empty = new GridPoint(-1, -1);

        public GridPoint(int x, int y) { X = x; Y = y; }

        public GridPoint(GridPoint GP) { X = GP.X; Y = GP.Y; }

        public bool Equals(GridPoint p) => X == p.X && Y == p.Y;

        public override bool Equals(object obj) => obj is GridPoint other && this.Equals(other);

        public override int GetHashCode() => (X, Y).GetHashCode();

        public static bool operator ==(GridPoint lhs, GridPoint rhs) => lhs.Equals(rhs);

        public static bool operator !=(GridPoint lhs, GridPoint rhs) => !(lhs == rhs);

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(7);
            sb.Append('(');
            sb.Append(X);
            sb.Append(",");
            sb.Append(Y);
            sb.Append(')');
            return sb.ToString();
        }
    }

    public static class GridPointExtensions
    {
        public static GridPoint Up(this GridPoint gp) => new GridPoint(gp.X, gp.Y + 1);
        public static GridPoint Right(this GridPoint gp) => new GridPoint(gp.X + 1, gp.Y);
        public static GridPoint Left(this GridPoint gp) => new GridPoint(gp.X - 1, gp.Y);
        public static GridPoint Down(this GridPoint gp) => new GridPoint(gp.X, gp.Y - 1);
        public static string GetString(this List<GridPoint> gridPoints)
        {
            bool foundOne = false;
            StringBuilder sb = new();
            foreach (GridPoint gp in gridPoints)
            {
                if (foundOne) sb.Append(", ");
                foundOne = true;
                sb.Append(gp.ToString());
            }
            if (!foundOne) sb.Append("None");
            return sb.ToString();
        }
    }

}
