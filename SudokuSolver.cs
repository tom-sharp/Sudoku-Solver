using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sudoku.Puzzle;


/*
 *		Version		Description
 *		0.01		Initial
 *		0.02		End application after showing help if help switch is provided
 *		0.03		Moved SudokuPuzzle to its own assembly
 */

namespace Sudoku.Solver
{
	public class SudokuSolver
	{
		string MSGVersion = "Sudoku-Solver 0.03  (-? for help)";
		string demopuzzle1 = "38........5...2..6...14....1...8..3...9.3.8.4..2.........62.7....6...........1.8.";
		string demopuzzle2 = "..4..7...6......5........9...195....29....7..8...1...3.....32.8.5..........12...4";
		string demopuzzle3 = "6....1.7......75..3.....9...4..9.3.........8....5.4.2..7.6.8....93...7....6.2..1.";
		public void ShowHelp() {
			Console.WriteLine("Sudoku-Solver");
			Console.WriteLine("usage: sudoku-solver  {-?} {-r} {-n} {-b} {-m} {-1} {-2} {-3} {puzzlestring}");
			Console.WriteLine("   By default 3R Algorithm is used (not showing progress)");
			Console.WriteLine("      -? show this help");
			Console.WriteLine("      -r show progress solving 3R algorithm ");
			Console.WriteLine("      -n extend solving using numpass algorithm");
			Console.WriteLine("      -b extend solving using backtrack algorithm");
			Console.WriteLine("      -m show possible numbers mask if not able to solve puzzle");
			Console.WriteLine("      -1 solve demo puzzle 1");
			Console.WriteLine("      -2 solve demo puzzle 2");
			Console.WriteLine("      -3 solve demo puzzle 3");
			Console.WriteLine("   puzzlestring a string representing the sudokupuzzle containing 81 characters");
			Console.WriteLine("   of numbers 1-9 and '.' '0' 'x' for undefined cells. If not provided or");
			Console.WriteLine("   string is shorter than 81 characters the remaining cells will be set as");
			Console.WriteLine("   undefined");
		}
		
		public void Run(string[] args) {
			int count = 0;
			string str;
			this.Puzzle = new SudokuPuzzle();
			while (count < args.Length) {
				str = args[count].ToLower();
				if (str.StartsWith('-'))
				{
					// expectd to be an option
					if (str.StartsWith("-r")) { OptRule = true; }
					else if (str.StartsWith("-n")) { OptNumpass = true; }
					else if (str.StartsWith("-b")) { OptBackTrack = true; }
					else if (str.StartsWith("-m")) { OptShowMask = true; }
					else if ((str.StartsWith("-h")) || (str.StartsWith("-?"))) { ShowHelp(); return; }
					else if (str.StartsWith("-1")) { this.Puzzle.SetPuzzle(demopuzzle1); }
					else if (str.StartsWith("-2")) { this.Puzzle.SetPuzzle(demopuzzle2); }
					else if (str.StartsWith("-3")) { this.Puzzle.SetPuzzle(demopuzzle3); }
					else Console.WriteLine($"Unknow switch {str}");
				}
				else {
					// expected to be the puzzle string
					if (str.StartsWith("?")) { ShowHelp(); }
					else this.Puzzle.SetPuzzle(str);
				}
				count++;
			}
			if (count == 0) Console.WriteLine(MSGVersion);

			// check if its valid and ot solved allready
			Console.WriteLine($"{this.Puzzle.GetPuzzle()}   - Starting 3R algorithm");
			if (!this.Puzzle.IsValid())
			{
				Console.WriteLine("Invalid or unsolvable puzzle");
				return;
			}
			else if (this.Puzzle.IsSolved()) {
				Console.WriteLine("Puzzle alredy solved");
				return;
			}

			this.SolvePuzzle();
		}
		
		void SolvePuzzle() {

			// Always start with rule based algorithm, if option rule is set then display progress
			if (OptRule) {
				while (Puzzle.ResolveMask(1) > 0) Console.WriteLine($"{this.Puzzle.GetPuzzle()}   - Resolved number");
			} 
			else Puzzle.ResolveMask();
			if (this.Puzzle.IsSolved()) {
				Console.WriteLine($"{this.Puzzle.GetPuzzle()}   - Puzzle solved");
				return;
			}

			// Check if Numpass algorithm should be used
			if (OptNumpass) {
				Console.WriteLine($"{this.Puzzle.GetPuzzle()}   - Starting numpass");
				Puzzle.ResolveNumPass();
				if (this.Puzzle.IsSolved())
				{
					Console.WriteLine($"{this.Puzzle.GetPuzzle()}   - Puzzle solved");
					return;
				}
			}

			// Check if BackTrack should be used
			if (OptBackTrack) {
				Console.WriteLine($"{this.Puzzle.GetPuzzle()}   - Starting backtrack");
				Puzzle.ResolveBacktrack();
				if (this.Puzzle.IsSolved())
				{
					Console.WriteLine($"{this.Puzzle.GetPuzzle()}   - Puzzle solved");
					return;
				}
			}

			// Puzzle not solved
			if (!Puzzle.IsValid()) {
				Console.WriteLine($"{this.Puzzle.GetPuzzle()}   - Invalid puzzle");
				Console.WriteLine("Invalid or unsolvable puzzle");
				return;
			}


			// Check if mask should be shown
			if (OptShowMask) { 
				for (int num = 1; num <= 9; num++) Console.WriteLine($"{Puzzle.GetPossible(num)}   - Possible mask");
			}
			Console.WriteLine($"{this.Puzzle.GetPuzzle()}   - Could not solve puzzle");

		}

		SudokuPuzzle Puzzle;
		bool OptRule = false;
		bool OptNumpass = false;
		bool OptBackTrack = false;
		bool OptShowMask = false;
	}
}
