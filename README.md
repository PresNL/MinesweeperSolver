# MinesweeperSolver
A C# program that attempts to solve MinesweeperX (http://www.curtisbright.com/msx/) in the most efficient way.

![MinesweeperSolver solving expert mode in 2 seconds](https://raw.githubusercontent.com/PresNL/MinesweeperSolver/master/examples/minesweeper.gif)

# Requirements
- .net 4.7+ for Tuples

# Setup
- Open the project in visual studio
- Set the w,h and bombs variables in program.cs to the width, height and amount of bombs of the board you are currenly trying to solve
- Start the program
- Press enter
- If it is making obviously wrong moves/flagging wrong bombs double check your w and h variable
- If it crashes when reading the board check if MinesweeperX is open and double check your w and h variables

# Adding support for a different minesweeper
- Create a new class that inherits from Board
- Override the methods in board to do what their name says (GrabBoard, Open, Flag and Retry)
- Initialize ai with the new class
- Run the AI

# Known problems/improvements
- Currently only uses the flag count to check for the win condition, it could also be used to solve certain scenarios or to pick a random tile instead of a known tile in case that boasts a lower probability of hitting a bomb
- No proper error handling in the XBoard.cs class
- XBoard.cs in general is pretty rough and could do with a code cleanup/added comments
- Only the windows xp skin is currently supported
