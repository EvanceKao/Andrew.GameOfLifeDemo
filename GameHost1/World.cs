﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace GameHost1
{
    public class World
    {
        public readonly (int width, int depth) Dimation;

        //private WorldContext[,] _matrix;

        // 目前 life 存在的地圖
        private Life[,] _maps_current;
        private int _frame;

        // 上一個 frame 的地圖快照。所有 life / god 的視覺都會觀察到這個 frame 的景物
        private Life[,] _maps_snapshot;
        private Dictionary<Life, (int x, int y)> _links = new Dictionary<Life, (int x, int y)>();

        //public World(int width, int depth)
        //{
        //    this.Dimation = (width, depth);
        //    this._maps_current = new Life[width, depth];
        //    this._maps_snapshot = new Life[width, depth];

        //    this.RefreshFrame();
        //}

        public World(bool[,] init_matrix, int[,] init_cell_frame, int world_frame)
        {
            this.Dimation = (init_matrix.GetLength(0), init_matrix.GetLength(1));
            this._maps_current = new Life[this.Dimation.width, this.Dimation.depth];
            this._maps_snapshot = new Life[this.Dimation.width, this.Dimation.depth];
            this._frame = world_frame;

            for (int y = 0; y < this.Dimation.depth; y++)
            {
                for (int x = 0; x < this.Dimation.width; x++)
                {
                    (int x, int y) cell_pos = (x, y);
                    this.Born(
                        new Life(new LifeSensibility(this, cell_pos), init_matrix[cell_pos.x, cell_pos.y], init_cell_frame[cell_pos.x, cell_pos.y]),
                        cell_pos);
                }
            }

            this.RefreshFrame();
        }


        // only God (world) can do this
        private void Born(Life cell, (int x, int y) position)
        {
            if (this._maps_current[position.x, position.y] != null)
            {
                throw new ArgumentOutOfRangeException();
            }

            this._maps_current[position.x, position.y] = cell;
            this._links.Add(cell, position);
        }

        // only God (world) can do this
        private int TimePass()
        {
            this.RefreshFrame();
            return this._frame;
        }

        public IEnumerable<(int time, bool[,] matrix)> Running()
        {
            SortedList<ToDoItem, object> todos = new SortedList<ToDoItem, object>(new ToDoItemComparer());
            for (int y = 0; y < this.Dimation.depth; y++)
            {
                for (int x = 0; x < this.Dimation.width; x++)
                {
                    //this._maps_current[x, y].TimePass();
                    var life = this._maps_current[x, y];
                    todos.Add(new ToDoItem()
                    {
                        ID = life.ID,
                        IsWorld = false,
                        TimePass = life.TimePass,
                        NextTimeFrame = life.TimePass()
                    }, null);
                }
            }
            todos.Add(new ToDoItem()
            {
                ID = -1,
                IsWorld = true,
                TimePass = this.TimePass,
                NextTimeFrame = this.TimePass()
            }, null);

            do
            {
                var item = todos.First().Key;

                todos.Remove(item);
                todos.Add(new ToDoItem()
                {
                    ID = item.ID,
                    IsWorld = item.IsWorld,
                    TimePass = item.TimePass,
                    NextTimeFrame = item.NextTimeFrame + item.TimePass()
                }, null);
                if (item.IsWorld) yield return (item.NextTimeFrame, this.GodVision());
            } while (true);
        }

        private class ToDoItem
        {
            public int ID;
            public bool IsWorld;
            public Func<int> TimePass;
            public int NextTimeFrame;
        }

        private class ToDoItemComparer : IComparer<ToDoItem>
        {
            public int Compare([AllowNull] ToDoItem x, [AllowNull] ToDoItem y)
            {
                if (x.NextTimeFrame == y.NextTimeFrame) return x.ID - y.ID;
                return x.NextTimeFrame - y.NextTimeFrame;
            }
        }





        private void RefreshFrame()
        {
            for (int y = 0; y < this.Dimation.depth; y++)
            {
                for (int x = 0; x < this.Dimation.width; x++)
                {
                    this._maps_snapshot[x, y] = this._maps_current[x, y].Snapshot;
                }
            }
        }

        // only God (world) can do this
        private bool[,] GodVision()
        {
            bool[,] matrix = new bool[this.Dimation.width, this.Dimation.depth];

            for (int y = 0; y < this.Dimation.depth; y++)
            {
                for (int x = 0; x < this.Dimation.width; x++)
                {
                    matrix[x, y] = (this._maps_snapshot[x, y] != null && this._maps_snapshot[x, y].IsAlive);
                }
            }

            return matrix;
        }

        // only life itself can do this
        private Life[,] SeeAround((int x, int y) pos)
        {
            Life[,] result = new Life[3, 3];

            result[0, 0] = this.SeePosition(pos.x - 1, pos.y - 1);
            result[1, 0] = this.SeePosition(pos.x   ,  pos.y - 1);
            result[2, 0] = this.SeePosition(pos.x + 1, pos.y - 1);

            result[0, 1] = this.SeePosition(pos.x - 1, pos.y   );
            //result[1, 1] = this.SeePosition(pos.x    , pos.y   );
            result[2, 1] = this.SeePosition(pos.x + 1, pos.y   );

            result[0, 2] = this.SeePosition(pos.x - 1, pos.y + 1);
            result[1, 2] = this.SeePosition(pos.x    , pos.y + 1);
            result[2, 2] = this.SeePosition(pos.x + 1, pos.y + 1);

            return result;
        }

        private Life SeePosition(int x, int y)
        {
            if (x < 0) return null;
            if (x >= this.Dimation.width) return null;
            if (y < 0) return null;
            if (y >= this.Dimation.depth) return null;
            return this._maps_snapshot[x, y];
        }


        public class LifeSensibility
        {
            private World _reality;
            private (int x, int y) _position;

            public LifeSensibility(World reality, (int x, int y) pos)
            {
                this._reality = reality;
                this._position = pos;
            }

            public Life[,] SeeAround()
            {
                return this._reality.SeeAround(this._position);
            }
        }
    }
}
