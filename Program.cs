using System;

using Sudoku.Puzzle;

namespace Sudoku.Solver
{
	class Program
	{
		static void Main(string[] args)
		{
			var Solver = new SudokuSolver();
			Solver.Run(args);
		}

	}
}
