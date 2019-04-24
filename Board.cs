using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace MinesweeperSolver
{
  abstract class Board
  {
    public enum Tiles
    {
      Closed = -1,
      Zero = 0,
      One = 1,
      Two = 2,
      Three = 3,
      Four = 4,
      Five = 5,
      Six = 6,
      Seven = 7,
      Eight = 8
    }

    // Get the current playing board and check if we hit a mine
    public abstract (Tiles[,] board, bool dead) GrabBoard() ;

    // Open tile at x,y
    public abstract void Open(int x, int y);

    // Flag the tile at x,y
    public abstract void Flag(int x, int y);

    // Restart the game
    public abstract void Retry();
  }
}