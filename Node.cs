namespace pathfinder
{
    public class Node
    {
        public Node(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Node Parent { get; set; }

        public int X { get; }

        public int Y { get; }

        public int F { get; set; }

        public int G { get; set; }

        public int H { get; set; }

        public bool IsEqual(Node node)
        {
            return X == node.X && Y == node.Y;
        }
    }
}