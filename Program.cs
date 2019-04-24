using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MinesweeperSolver
{
  class Program
  {
    static void Main(string[] args)
    {
      int w = 16;
      int h = 16;
      int bombs = 40;
      Console.WriteLine("Ready?");
      Console.ReadKey();
      Board board = new XBoard(w, h);
      AI ai = new AI(w, h, bombs, board, true);
      bool c = false;

      do
      {
        while (ai.NextMove())
        {
        }
        Console.WriteLine("AI is done, if you want to run again press y");
        c = Console.ReadKey().Key == ConsoleKey.Y;
        if (c)
        {
          ai = new AI(w, h, bombs, board, true);
          board.Retry();
        }
      } while (c);
    }
  }
}
