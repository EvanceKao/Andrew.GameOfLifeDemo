﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GameHost1
{
    public class Program
    {
        static void Main(string[] args)
        {
            bool[,] matrix = new bool[50, 20];
            int[,] frames = new int[50, 20];
            Init(matrix, frames, 10, 20);

            var world = new World(matrix, frames, 10);

            int count = 0;
            foreach(var frame in world.Running())
            {
                count++;
                int live_count = 0;
                Console.SetCursorPosition(0, 0);

                var current_matrix = frame.matrix;
                var time = frame.time;

                for (int y = 0; y < current_matrix.GetLength(1); y++)
                {
                    for (int x = 0; x < current_matrix.GetLength(0); x++)
                    {
                        var c = current_matrix[x, y];
                        live_count += (c ? 1 : 0);
                        Console.Write(c ? '★' : '☆');
                    }
                    Console.WriteLine();
                }

                Thread.Sleep(200);
                Console.WriteLine($"total lives: {live_count}, time frame: {time} / 5000...");
            }



            /*
            for (int count = 0; count < 5000; count++)
            {
                int live_count = 0;

                Thread.Sleep(200);
                Console.SetCursorPosition(0, 0);

                for(int y = 0; y < matrix.GetLength(1); y++)
                {
                    for (int x = 0; x < matrix.GetLength(0); x++)
                    {
                        var c = matrix[x, y];
                        live_count += (c ? 1 : 0);
                        Console.Write(c ? '★' : '☆');
                    }
                    Console.WriteLine();
                }

                //matrix = GetNextGenMatrix(matrix);
                //god.TimePass();

                world.StartRunning();

                matrix = god.SeeWholeWorld();

                Console.WriteLine($"total lives: {live_count}, round: {count} / 5000...");
            }
            */
        }


        public static bool[,] GetNextGenMatrix(bool[,] matrix)
        {
            int[,] frames = new int[matrix.GetLength(0), matrix.GetLength(1)];

            for (int y = 0; y < matrix.GetLength(1); y++) for (int x = 0; x < matrix.GetLength(0); x++) frames[x, y] = 10;

            var world = new World(matrix, frames, 10);

            return world.Running().First().matrix;
        }

        private static void Init(bool[,] matrix, int[,] frames, int cell_frame = 10, int rate = 20)
        {
            Random rnd = new Random();
            for (int y = 0; y < matrix.GetLength(1); y++)
            {
                for (int x = 0; x < matrix.GetLength(0); x++)
                {
                    matrix[x, y] = (rnd.Next(100) < rate);
                    frames[x, y] = cell_frame;
                }
            }
        }



    }
}
