using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperSolver
{
  class AI
  {
    struct Tile
    {
      public bool flag;
      public bool open;
    }
    Tile[,] tiles;
    Board board;

    int w;
    int h;
    int flag_count;
    bool flag_tiles;
    public AI(int w, int h, int flag_count, Board board, bool flag_tiles = false)
    {
      this.w = w;
      this.h = h;
      this.board = board;
      this.flag_count = flag_count;
      this.flag_tiles = flag_tiles;
      tiles = new Tile[w, h];
      for (int x = 0; x < w; x++)
      {
        for (int y = 0; y < h; y++)
        {
          Tile tile;
          tile.flag = false;
          tile.open = false;
          tiles[x, y] = tile;
        }
      }
    }

    private void Open(int x, int y)
    {
      tiles[x, y].open = true;
      this.board.Open(x, y);
    }
    private void Flag(int x, int y)
    {
      tiles[x, y].flag = true;
      if (flag_tiles)
        this.board.Flag(x, y);
    }
    
    // Apply all certain moves for the current board state
    // if no certain moves are found it will perform
    // the move with the lowest chance of hitting a bomb
    public bool NextMove()
    {
      // Get the current state of the board
      var b = this.board.GrabBoard();
      var board = b.board;

      // Check if we hit a mine last step
      if (b.dead)
        return false;

      // Check if there are any open tiles, if not open 0,0
      if (GetOpenTiles(ref board).Count == 0)
      {
        Open(0, 0);
        return true;
      }

      bool changed;
      bool step_performed = false;
      do
      {
        changed = false;

        // check if we opened all tiles
        // if we did return that the game has ended
        if (GetClosedCount(ref board) == 0)
          return false;

        // check if we found all flags
        // if we did open all remaining closed tiles and return 
        // that the game has ended
        if (GetFlagCount(ref board) == this.flag_count)
        {
          var closed = GetClosedTiles(ref board);
          PerformCertainSteps(closed.Select(x => (0.0, x.x, x.y)).ToList());
          return false;
        }

        // Calculate probabilities for all open tiles
        var single_probabilities = CalculateSingles(ref board);
        // flag all tiles with probability 1 and open all tiles with probability 0
        changed = PerformCertainSteps(single_probabilities);

        // check all tiles in pairs, open the tiles with probability 0
        // and flag all tiles with probability 1
        // (calculate link only returns tiles with probability 1 or 0
        // so no need to save the list)
        if (PerformCertainSteps(CalculateLinked(ref board)))
          changed = true;

        // If we did not perform any steps this entire function
        if (changed == false && step_performed == false)
        {
          // Check if we know any probabilities
          if (single_probabilities.Count > 0)
          {
            // Open the tile with the lowest probability of being a bomb
            var ordered_probabilities = single_probabilities.OrderBy(x => x.p);
            Open(ordered_probabilities.First().x, ordered_probabilities.First().y);

            step_performed = true;

            // step out of the loop so we can get the new board
            break;
          }
          else
          {
            // Perform a random step since we know no probabilities
            PerformRandomStep(ref board);
            step_performed = true;
            break;
          }
        }
        else
        {
          step_performed = true;
        }
      // loop untill we could not perform an action with 100% certainty
      // this is because it takes a long (relatively) time to grab a new board state
      } while (changed);

      // check if we opened all tiles
      // if we did return that the game has ended
      if (GetClosedCount(ref board) == 0)
        return false;

      // check if we found all flags
      // if we did open all remaining closed tiles and return 
      // that the game has ended
      if (GetFlagCount(ref board) == this.flag_count)
      {
        var closed = GetClosedTiles(ref board);
        PerformCertainSteps(closed.Select(x => (0.0, x.x, x.y)).ToList());
        return false;
      }

      // return if we performed an action
      // if we did not it means the game has ended
      return step_performed;
    }

    // Flag all tiles with probability 1 and
    // open all tiles with probability 0
    private bool PerformCertainSteps(List<(double p, int x, int y)> probabilities)
    {
      bool changed = false;

      var bombs = probabilities.Where(x => x.p == 1.0);
      foreach (var bomb in bombs)
      {
        if (!tiles[bomb.x, bomb.y].flag)
        {
          Flag(bomb.x, bomb.y);
          changed = true;
        }
      }

      var no_bombs = probabilities.Where(x => x.p == 0);
      foreach (var tile in no_bombs)
      {
        Open(tile.x, tile.y);
        changed = true;
      }

      return changed;
    }
    // Perform random step close to a current open tile (this is to make the most use out of already
    // uncovered information) and if possible next to the border
    private void PerformRandomStep(ref Board.Tiles[,] board)
    {
      // Get all open and closed tiles
      var closed = GetClosedTiles(ref board);
      var open = GetOpenTiles(ref board);

      List<(int d, int x, int y)> closed_distance = new List<(int d, int x, int y)>();

      // Calculate lowest distance to an open tile for each
      // closed tile
      for (int i = 0; i < closed.Count; i++)
      {
        int x = closed[i].x;
        int y = closed[i].y;
        closed_distance.Add((int.MaxValue, x, y));
        foreach (var open_tile in open)
        {
          int distance = GetDistance(x, y, open_tile.x, open_tile.y);
          if (distance < closed_distance[i].d)
            closed_distance[i] = (distance, x, y);
        }
      }

      // Remove all tiles with a distance higher then the lowest possible
      var closest_tiles = closed_distance.Where(i => i.d == closed_distance.OrderBy(x => x.d).First().d);

      // If a close tile is in a corner open that one
      var corners = closest_tiles.Where(x => (x.x == 0 || x.x == w - 1) && (x.y == 0 || x.y == h - 1));
      if (corners.Count() > 0)
      {
        Open(corners.First().x, corners.First().y);
        return;
      }

      // If a closed tile is next to the border open that one
      var borders = closest_tiles.Where(x => x.x == 0 || x.x == w - 1 || x.y == 0 || x.y == h - 1);
      if (borders.Count() > 0)
      {
        Open(borders.First().x, borders.First().y);
        return;
      }

      // Open the first closed tile in the list
      Open(closest_tiles.First().x, closest_tiles.First().y);
    }

    //  Loop over all opened tiles (that are not zero) and 
    // calculate the probability that their closed neighbours are a bomb
    private List<(double p, int x, int y)> CalculateSingles(ref Board.Tiles[,] board)
    {
      List<(double p, int x, int y)> probabilities = new List<(double p, int x, int y)>();
      List<(int x, int y)> open_tiles = GetOpenTiles(ref board);
      foreach (var coords in open_tiles)
      {
        int x = coords.x;
        int y = coords.y;
        if (board[x, y] > 0)
        {
          List<(int x, int y)> closed = GetClosedNear(x, y, ref board);
          int flags = GetFlagCountNear(x, y);

          if (flags == (int)board[x, y])
          {
            foreach (var tile in closed)
            {
              probabilities.Add((0, tile.x, tile.y));
            }
          }
          else
          {
            foreach (var tile in closed)
            {
              var existing = probabilities.FindIndex(t => t.x == tile.x && t.y == tile.y);
              (double p, int x, int y) p = (((double)board[x, y] - flags) / closed.Count, tile.x, tile.y);
              if (existing != -1)
              {
                if (
                    p.p > probabilities[existing].p &&
                    probabilities[existing].p > 0 &&
                    probabilities[existing].p < 1)
                  probabilities[existing] = p;
              }
              else
                probabilities.Add(p);
            }
          }
        }
      }
      return probabilities;
    }
    // Loop over all opened tiles (that are not zero) and 
    // check if we can resolve them taking into account their neighbours
    private List<(double p, int x, int y)> CalculateLinked(ref Board.Tiles[,] board)
    {
      List<(double p, int x, int y)> predictions = new List<(double p, int x, int y)>();
      List<(int x, int y)> open_tiles = GetOpenTiles(ref board);
      foreach (var coords in open_tiles)
      {
        int x = coords.x;
        int y = coords.y;
        var tile = board[x, y];

        if (tile > 0)
        {

          int flags = GetFlagCountNear(x, y);
          if (tile - flags == 0)
            continue;

          List<(int x, int y)> neighbours = GetNeighbours(x, y, ref board);
          List<(int x, int y)> closed = GetClosedNear(x, y, ref board);
          int mines = (int)tile - flags;

          foreach (var neighbour in neighbours)
          {
            int neighbourMines = (int)board[neighbour.x, neighbour.y] - GetFlagCountNear(neighbour.x, neighbour.y);
            List<(int x, int y)> closedNeighbour = GetClosedNear(neighbour.x, neighbour.y, ref board);
            List<(int x, int y)> Uncommon = MatchAllClosedNeighbours(x, y, neighbour.x, neighbour.y, ref board);

            if (Uncommon == null)
              continue;

            if (neighbourMines == mines)
            {
              foreach (var open in Uncommon)
              {
                predictions.Add((0, open.x, open.y));
              }
            }
            else if (neighbourMines - mines == closedNeighbour.Count - closed.Count)
            {
              foreach (var open in Uncommon)
              {
                predictions.Add((1, open.x, open.y));
              }
            }
          }
        }

      }
      return predictions;
    }


    private int GetClosedCount(ref Board.Tiles[,] board)
    {
      int closed = 0;
      for (int xi = 0; xi < w; xi++)
      {
        for (int yi = 0; yi < h; yi++)
        {
          if ((int)board[xi, yi] == -1 && tiles[xi, yi].flag == false && tiles[xi, yi].open == false)
            closed++;
        }
      }
      return closed;
    }
    private int GetFlagCount(ref Board.Tiles[,] board)
    {
      int flags = 0;
      for (int xi = 0; xi < w; xi++)
      {
        for (int yi = 0; yi < h; yi++)
        {
          if (tiles[xi, yi].flag)
            flags++;
        }
      }
      return flags;
    }
    private int GetFlagCountNear(int x, int y)
    {
      int flags = 0;

      for (int xi = x - 1; xi <= x + 1; xi++)
      {
        for (int yi = y - 1; yi <= y + 1; yi++)
        {
          if ((xi == x && yi == y) || (xi < 0) || (yi < 0) || (xi >= w) || (yi >= h))
            continue;

          if (tiles[xi, yi].flag)
            flags++;
        }
      }
      return flags;
    }

    private List<(int, int)> GetClosedNear(int x, int y, ref Board.Tiles[,] board)
    {
      List<(int, int)> closed = new List<(int, int)>();

      for (int xi = x - 1; xi <= x + 1; xi++)
      {
        for (int yi = y - 1; yi <= y + 1; yi++)
        {
          if ((xi < 0) || (yi < 0) || (xi >= w) || (yi >= h))
            continue;

          if ((int)board[xi, yi] == -1 && tiles[xi, yi].flag == false && tiles[xi, yi].open == false)
            closed.Add((xi, yi));
        }
      }
      return closed;
    }
    private List<(int x, int y)> GetClosedTiles(ref Board.Tiles[,] board)
    {
      List<(int x, int y)> closed = new List<(int, int)>();

      for (int xi = 0; xi < w; xi++)
      {
        for (int yi = 0; yi < h; yi++)
        {
          if ((int)board[xi, yi] == -1 && tiles[xi, yi].flag == false && tiles[xi, yi].open == false)
            closed.Add((xi, yi));
        }
      }

      return closed;
    }
    private List<(int x, int y)> GetOpenTiles(ref Board.Tiles[,] board)
    {
      List<(int x, int y)> open = new List<(int, int)>();

      for (int xi = 0; xi < w; xi++)
      {
        for (int yi = 0; yi < h; yi++)
        {
          if (board[xi, yi] > 0 || tiles[xi, yi].open)
            open.Add((xi, yi));
        }
      }

      return open;
    }
    private List<(int x, int y)> GetNeighbours(int x, int y, ref Board.Tiles[,] board)
    {
      List<(int x, int y)> neighbours = new List<(int x, int y)>();
      for (int xi = x - 1; xi <= x + 1; xi++)
      {
        for (int yi = y - 1; yi <= y + 1; yi++)
        {
          if ((xi == x && yi == y) || (xi < 0) || (yi < 0) || (xi >= w) || (yi >= h))
            continue;

          if (board[xi, yi] > 0)
            neighbours.Add((xi, yi));
        }
      }
      return neighbours;
    }

    private bool IsNeighbour(int x, int y, int nx, int ny)
    {
      return Math.Max(Math.Abs(x - nx), Math.Abs(y - ny)) <= 1;
    }
    private int GetDistance(int x, int y, int nx, int ny)
    {
      // Return positive distance between x,y and nx, ny
      return Math.Abs(x - nx) + Math.Abs(y - ny);
    }

    // Check if all neighbours of x,y also neighbour nx,ny
    // returns all neighbours of nx,ny that don't neighbour x,y if true
    // returns null if false
    public List<(int x, int y)> MatchAllClosedNeighbours(int x, int y, int nx, int ny, ref Board.Tiles[,] board)
    {
      List<(int x, int y)> closed_x_y = GetClosedNear(x, y, ref board);

      // not all tiles neighbouring x,y also neighbour nx,ny
      if (closed_x_y.Count(o => !IsNeighbour(o.x, o.y, nx, ny)) > 0)
        return null;

      List<(int x, int y)> closed_nx_ny = GetClosedNear(nx, ny, ref board);

      // return all tiles that neighbour nx, ny but not x,y
      return closed_nx_ny.Where(o => !closed_x_y.Contains((o.x, o.y))).ToList();
    }


  }
}
