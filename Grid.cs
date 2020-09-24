using System;
using System.Collections.Generic;
using System.Linq;

namespace pathfinder
{
    public class Grid
    {
        private readonly List<Node> _closedList = new List<Node>();
        private readonly List<Node> _openList = new List<Node>();
        private Node _startNode;

        public Grid(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public List<Node> Obstacles { get; } = new List<Node>();

        public int Width { get; }

        public int Height { get; }

        public List<Node> Path { get; } = new List<Node>();

        public bool AllowDiagonalMovement { get; set; }

        public Node StartNode
        {
            get => _startNode;
            set
            {
                _startNode = value;
                _openList.Clear();
                _openList.Add(value);
            }
        }

        public Node EndNode { get; set; }

        public bool FindPath()
        {
            while (_openList.Count > 0)
            {
                var q = _openList.Aggregate((node1, node2) => node1.F < node2.F ? node1 : node2);
                _openList.Remove(q);

                var successors = GetPossibleSuccessors(q).ToList();
                foreach (var successor in successors)
                {
                    successor.Parent = q;
                    if (successor.IsEqual(EndNode))
                    {
                        Console.WriteLine("Path found");
                        EndNode = successor;
                        CreatePathList();
                        return true;
                    }

                    successor.G = successor.Parent.G + 1;
                    successor.H = Math.Abs(successor.X - EndNode.X) + Math.Abs(successor.Y - EndNode.Y);

                    successor.F = successor.G + successor.H;
                    //Console.WriteLine($"current node - f-cost: {successor.F}");

                    if (_openList.Any(openNode => openNode.IsEqual(successor) && openNode.F < successor.F)) continue;

                    if (_closedList.Any(closedNode => closedNode.IsEqual(successor) && closedNode.F < successor.F))
                        continue;

                    _openList.Add(successor);
                }

                _closedList.Add(q);
            }

            Console.WriteLine("No path found");
            return false;
        }

        public void ResetPath()
        {
            Path.Clear();
            _closedList.Clear();
            _openList.Clear();
            StartNode = _startNode;
        }

        private IEnumerable<Node> GetPossibleSuccessors(Node node)
        {
            var successors = new List<Node>();
            if (AllowDiagonalMovement)
            {
                successors.Add(new Node(node.X + 1, node.Y + 1));
                successors.Add(new Node(node.X - 1, node.Y - 1));
                successors.Add(new Node(node.X - 1, node.Y + 1));
                successors.Add(new Node(node.X + 1, node.Y - 1));
            }

            successors.Add(new Node(node.X + 1, node.Y));
            successors.Add(new Node(node.X, node.Y + 1));
            successors.Add(new Node(node.X - 1, node.Y));
            successors.Add(new Node(node.X, node.Y - 1));

            return successors
                .Where(node =>
                    node.X >= 0 && node.Y >= 0 && node.X <= Width - 1 &&
                    node.Y <= Height - 1) // check for grid boundaries
                //.Select(GetGridNode).ToList();
                .Where(x => !Obstacles.Any(obstacle => obstacle.IsEqual(x)));
        }

        private void CreatePathList()
        {
            var parentNode = EndNode;

            while (parentNode.Parent != null)
            {
                Path.Add(parentNode);
                parentNode = parentNode.Parent;
            }
        }

        public void PrintPath()
        {
            // Console.Clear();
            if (Path.Count == 0)
            {
                Console.WriteLine("No path found");
                return;
            }

            Console.WriteLine($"Count of nodes on path: {Path.Count}");
            Console.WriteLine("------");
            for (var i = 0; i <= EndNode.Y; i++)
            {
                var rowString = "";
                for (var j = 0; j <= EndNode.X; j++)
                {
                    var refNode = new Node(j, i);
                    if (refNode.IsEqual(StartNode) || refNode.IsEqual(EndNode))
                        rowString += " 0";
                    else if (Path.Any(x => x.IsEqual(refNode)))
                        rowString += " ,";
                    else if (Obstacles.Any(x => x.IsEqual(refNode)))
                        rowString += " X";
                    else
                        rowString += " *";
                }

                Console.WriteLine(rowString);
            }

            Console.WriteLine("------");
        }
    }
}