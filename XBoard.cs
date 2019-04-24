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
  class XBoard : Board
  {
    readonly int[] tile = { 16, 16 };
    readonly int[] offset = { 15, 101 };
    readonly int[] number_offset = { 9, 4 };
    Dictionary<Color, Tiles> tile_values = new Dictionary<Color, Tiles>();

    private int w;
    private int h;
    private IntPtr minesweeper_window;
    public XBoard(int w, int h)
    {
      var proc = Process.GetProcessesByName("Minesweeper X")[0];
      minesweeper_window = FindWindow(proc.Id);
      this.w = w;
      this.h = h;

      tile_values.Add(Color.FromArgb(255, 192, 192, 192), Tiles.Zero);
      tile_values.Add(Color.FromArgb(255, 0, 0, 255), Tiles.One);
      tile_values.Add(Color.FromArgb(255, 0, 128, 0), Tiles.Two);
      tile_values.Add(Color.FromArgb(255, 255, 0, 0), Tiles.Three);
      tile_values.Add(Color.FromArgb(255, 0, 0, 128), Tiles.Four);
      tile_values.Add(Color.FromArgb(255, 128, 0, 0), Tiles.Five);
      tile_values.Add(Color.FromArgb(255, 0, 128, 128), Tiles.Six);
      tile_values.Add(Color.FromArgb(255, 0, 0, 0), Tiles.Seven);
      tile_values.Add(Color.FromArgb(255, 128, 128, 128), Tiles.Eight);
    }

    private IntPtr FindWindow(int processid)
    {
      IntPtr found = IntPtr.Zero;

      IntPtr window = IntPtr.Zero;
      User32.EnumWindows(delegate (IntPtr wnd, IntPtr param)
      {
        int process_id;
        uint identifier = User32.GetWindowThreadProcessId(wnd, out process_id);
        if (process_id == processid)
        {
          window = wnd;
          return false;
        }

        // but return true here so that we iterate all windows
        return true;
      }, IntPtr.Zero);

      return window;
    }

    public override (Tiles[,] board, bool dead) GrabBoard()
    {
      User32.SetForegroundWindow(minesweeper_window);
      var rect = new User32.Rect();
      User32.GetWindowRect(minesweeper_window, ref rect);
      
      int width = rect.right - rect.left;
      int height = rect.bottom - rect.top;

      var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
      Graphics graphics = Graphics.FromImage(bmp);
      graphics.CopyFromScreen(rect.left, rect.top, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);

      Tiles[,] board = new Tiles[w, h];

      int yi = 0;
      int xi = 0;
      for (int y = offset[1]; y < tile[1] * h + offset[1]; y += tile[1])
      {
        xi = 0;
        for (int x = offset[0]; x < tile[0] * w + offset[0]; x += tile[0])
        {
          Color first_pixel = bmp.GetPixel(x + 1, y + 1);
          if (first_pixel == Color.FromArgb(255, 255, 255, 255))
          {
            board[xi, yi] = Tiles.Closed;
            xi++;
            continue;
          }
          if (first_pixel == Color.FromArgb(255, 255, 0, 0))
          {
            return (new Tiles[w, h], true);
          }

          Color color = bmp.GetPixel(x + number_offset[0], y + number_offset[1]);
          bmp.SetPixel(x + number_offset[0], y + number_offset[1], Color.Fuchsia);
          if (!tile_values.ContainsKey(color))
          {
            return (new Tiles[w, h], true);
          }
          board[xi, yi] = tile_values[color];
          xi++;
        }
        yi++;
      }
      bmp.Save("test2.png");
      return (board, false);
    }

    public override void Open(int x, int y)
    {
      User32.SetForegroundWindow(minesweeper_window);
      var rect = new User32.Rect();
      User32.GetWindowRect(minesweeper_window, ref rect);

      int xpos = ((x * tile[0]) + offset[0] + number_offset[0]) + rect.left;
      int ypos = ((y * tile[1]) + offset[1] + number_offset[1]) + rect.top;

      User32.SetCursorPos(xpos, ypos);
      User32.mouse_event(User32.MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
      User32.mouse_event(User32.MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
    }

    public override void Flag(int x, int y)
    {
      User32.SetForegroundWindow(minesweeper_window);
      var rect = new User32.Rect();
      User32.GetWindowRect(minesweeper_window, ref rect);

      int xpos = ((x * tile[0]) + offset[0] + number_offset[0]) + rect.left;
      int ypos = ((y * tile[1]) + offset[1] + number_offset[1]) + rect.top;

      User32.SetCursorPos(xpos, ypos);
      User32.mouse_event(User32.MOUSEEVENTF_RIGHTDOWN, xpos, ypos, 0, 0);
      User32.mouse_event(User32.MOUSEEVENTF_RIGHTUP, xpos, ypos, 0, 0);
    }

    public override void Retry()
    {
      User32.SetForegroundWindow(minesweeper_window);
      var rect = new User32.Rect();
      User32.GetWindowRect(minesweeper_window, ref rect);
      int width = rect.right - rect.left;
      int xpos = (width / 2) + rect.left;
      int ypos = 75 + rect.top;

      User32.SetCursorPos(xpos, ypos);
      User32.mouse_event(User32.MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
      User32.mouse_event(User32.MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
    }
  }
}

public class User32
{
  [StructLayout(LayoutKind.Sequential)]
  public struct Rect
  {
    public int left;
    public int top;
    public int right;
    public int bottom;
  }

  [DllImport("user32.dll", SetLastError = true)]
  public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

  public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

  [DllImport("user32.dll")]
  public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

  [DllImport("user32.dll")]
  public static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

  [DllImport("User32.dll")]
  public static extern bool SetForegroundWindow(IntPtr hWnd);

  [System.Runtime.InteropServices.DllImport("user32.dll")]
  public static extern bool SetCursorPos(int x, int y);

  [System.Runtime.InteropServices.DllImport("user32.dll")]
  public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

  public const int MOUSEEVENTF_LEFTDOWN = 0x02;
  public const int MOUSEEVENTF_LEFTUP = 0x04;

  public const int MOUSEEVENTF_RIGHTDOWN = 0x08;
  public const int MOUSEEVENTF_RIGHTUP = 0x10;
}