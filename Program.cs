using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace pathfinder
{
    internal class Program
    {
        private const int DefaultWidth = 5;
        private const int DefaultHeight = 5;
        private const double ObstacleRatio = 0.6;

        private static readonly Stopwatch Stopwatch = new Stopwatch();
        private static readonly ConcurrentBag<Task<bool>> Grids = new ConcurrentBag<Task<bool>>();
        private static bool _resultFound;

        private static void Main(string[] args)
        {
            var i = 0;
            Stopwatch.Start();
            while (Task.WhenAll(Grids).Result.All(x => !x))
            {
                Grids.Clear();

                var creationTasks = new ConcurrentBag<Task>();
                Parallel.For(0, 100,
                    i1 => { creationTasks.Add(Task.Run(() => Grids.Add(CreateGridTask(CreateRandomGrid())))); });

                Task.WaitAll(creationTasks.ToArray());
                i++;
                Console.WriteLine($"Iteration {i} done. {Stopwatch.ElapsedMilliseconds}ms elapsed.");
            }

            Console.ReadKey();
        }

        private static void OnPathFound(Grid grid)
        {
            Stopwatch.Stop();
            
            // TODO: Cancel all running tasks

            Console.WriteLine("Printing result...");
            Thread.Sleep(2000);
            grid.PrintPath();
            Console.WriteLine($"Done. Total time elapsed: {Stopwatch.ElapsedMilliseconds}ms");
        }

        private static Task<bool> CreateGridTask(Grid grid)
        {
            var task = Task.Run(grid.FindPath);
            task.ContinueWith(result =>
            {
                if (!result.Result || _resultFound) return;
                _resultFound = true;
                OnPathFound(grid);
            });

            return task;
        }

        private static Grid CreateRandomGrid()
        {
            var grid = new Grid(DefaultWidth, DefaultHeight)
            {
                AllowDiagonalMovement = true,
                StartNode = new Node(0, 0),
                EndNode = new Node(DefaultWidth - 1, DefaultHeight - 1)
            };
            grid.Obstacles.AddRange(CreateRandomObstacles(DefaultWidth, DefaultHeight,
                new[] {grid.StartNode, grid.EndNode}));

            return grid;
        }

        private static IEnumerable<Node> CreateRandomObstacles(int width, int height,
            IReadOnlyCollection<Node> blockedNodes)
        {
            var random = new Random();
            var obstacles = new List<Node>();
            for (var i = 0; i <= DefaultWidth * DefaultHeight * ObstacleRatio; i++)
            {
                var randomNode = new Node(random.Next(0, width), random.Next(0, height));
                if (blockedNodes.Any(x => x.IsEqual(randomNode)) || obstacles.Any(x => x.IsEqual(randomNode)))
                    i--;
                else
                    obstacles.Add(randomNode);
            }

            return obstacles;
        }
    }
}