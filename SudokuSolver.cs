using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Syslib;
using Sudoku.Puzzle;

/*
 * Solving performance test	solving: 4000-puzzles.txt (located in binaries folder)
 *			14 seconds using logic only to solve all 4000 puzzles
 *			20 seconds using backtrack only (-bo) to solve all 4000 puzzles
 * 
 * 
 *		Version		Description
 *		0.09		Added creation of puzzles (switch -cp)
 *					Added option to use backtrack only (switch -bo)
 *		0.08		Added creation of random solved sudoku
 *					Changed more situations to show puzzle as grid
 *		0.07		Removed limitation of max 81 sudoko lines read from file and increased file buffer
 *					Removed Numpass algorithm
 *		0.06		Extracted some code to its own method
 *					Re-arranged how to show help
 *					Added option to show mask and puzzle as grid
 *					Added feature to read multiple puzzles from one file
 *					BugFix - when reading puzzle from file, the opened file was never closed
 *		0.05		Fixed problem with demo puzzles introduced in version 0.04
 *		0.04		Added feature to read sudoku puzzle from file
 *		0.03		Moved SudokuPuzzle to its own assembly
 *		0.02		End application after showing help if help switch is provided
 *		0.01		Initial
 *					
 */

namespace Sudoku.Solver
{
	public class SudokuSolver
	{
		string MSGVersion = "Sudoku-Solver 0.09  (-? for help)";
		string[] MSGHelp = {
			"usage: sudoku-solver  {-?} {-r} {-b} {-c {count}} {-cp {count}} {-m} {-1} {-2} {-3} {puzzlestring}",
			"      -? show this help",
			"      -f read puzzle from file",
			"      -fr read puzzles from file, each line should be a puzzle",
			"      -r show progress solving 3R algorithm ",
			"      -b extend solving using backtrack algorithm",
			"      -bo use backtrack algorithm only",
			"      -m show possible numbers mask if not able to solve puzzle",
			"      -v show puzzle and number mask as a grid",
			"      -c {count} Create Sudoku solution, count = number of sudokus to create",
			"      -cp {count} Create Sudoku puzzle, count = number of puzzles to create",
			"      -1 solve demo puzzle 1",
			"      -2 solve demo puzzle 2",
			"      -3 solve demo puzzle 3",
			"   puzzlestring a string representing the sudokupuzzle containing 81 characters of numbers 1-9 and",
			"   '.' '0' 'x' for undefined cells. If not provided or string is shorter than 81 characters the",
			"   remaining cells will be set as undefined",
			"\n   Examples:",
			"   sudoku-solver -1 -r              sudoku-solver -m \"123456789\"     sudoku-solver -cp 10",
			"   sudoku-solver -f puzzle.txt      sudoku-solver -b \"123456789\"     sudoku-solver -cp -v",
			"   sudoku-solver -f -m puzzle.txt   sudoku-solver -f -b puzzle.txt   sudoku-solver -fr puzzles.txt",
		};

		public void ShowHelp() {
			Console.WriteLine(MSGVersion);
			foreach (var s in MSGHelp) Console.WriteLine(s);
		}

		public void Run(string[] args) {
			int count = 0;
			string str;
			string puzzlefile = "";
			this.Puzzle = new SudokuPuzzle();
			while (count < args.Length) {
				str = args[count].ToLower();
				if (str.StartsWith('-'))
				{
					// expectd to be an option
					if (str.StartsWith("-r")) { OptRule = true; }
					else if (str.StartsWith("-bo")) { OptBackTrack = true; OptBackTrackOnly = true; }
					else if (str.StartsWith("-b")) { OptBackTrack = true; }
					else if (str.StartsWith("-m")) { OptShowMask = true; }
					else if (str.StartsWith("-fr")) { OptReadFileMultipleRow = true; }
					else if (str.StartsWith("-f")) { OptReadFile = true; }
					else if (str.StartsWith("-v")) { OptShowVertical = true; }
					else if (str.StartsWith("-cp")) { OptCreateSudoku = true; OptCreateSudokuPuzzle = true; }
					else if (str.StartsWith("-c")) { OptCreateSudoku = true; }
					else if (str.StartsWith("-1")) { puzzlefile = demopuzzle1; }
					else if (str.StartsWith("-2")) { puzzlefile = demopuzzle2; }
					else if (str.StartsWith("-3")) { puzzlefile = demopuzzle3; }
					else if ((str.StartsWith("-h")) || (str.StartsWith("-?"))) { ShowHelp(); return; }
					else Console.WriteLine($"Unknow switch {str}");
				}
				else {
					// expected to be the puzzle string OR filename if readfile is set
					if (str.StartsWith("?")) { ShowHelp(); return; }
					else puzzlefile = str;
				}
				count++;
			}

			if (count == 0) Console.WriteLine(MSGVersion);

			if (OptCreateSudoku) {
				count = 1;
				if (puzzlefile.Length > 0) { if (!Int32.TryParse(puzzlefile, out count)) count = 1; }
				while (count-- > 0) {
					CreateSudoku();
					ShowPuzzle($"New Random sudoku {this.Puzzle.GetNumberCount()}");
				}
				return;
			}

			if (OptReadFileMultipleRow)	{
				CList<CStr> puzzlelist = GetMultiplePuzzles(puzzlefile);
				if (puzzlelist != null) {
					foreach (var p in puzzlelist) {
						this.Puzzle.SetPuzzle(p.ToString());
						SolvePuzzle();
					}
				}
			}
			else {
				// read a single puzzle from file or commandline input
				if (!SetPuzzle(puzzlefile)) return;
				SolvePuzzle();
			}

		}

		SudokuPuzzle CreateSudoku() {
			if (this.OptCreateSudokuPuzzle) this.Puzzle = new CreateSudoku().GetSudokuPuzzle();
			else this.Puzzle = new CreateSudoku().GetNewSudoku();
			return this.Puzzle;
		}

		CList<CStr> GetMultiplePuzzles(string puzzlefile) {
			var puzzlelist = new CList<CStr>();
			if ((puzzlefile == null) || (puzzlefile.Length == 0)) { Console.WriteLine("No file name is provided"); return null; }
			if ((puzzlefile.Contains('?')) || (puzzlefile.Contains('*'))) { Console.WriteLine("Multiple file selections is not supported"); return null; }
			CStream fstream = new CStream();
			if (!fstream.isFileExist(puzzlefile)) { Console.WriteLine($"File '{puzzlefile}' not found"); return null; }
			if (fstream.OpenStream(CStream.StreamMode.ReadFile, filename: puzzlefile) != 0) { Console.WriteLine($"Could not Read file '{puzzlefile}'"); return null; }
			int counter = 0;
			CStr puzzle;
			while (fstream.BytesToRead() > 0)
			{
				counter++;
				puzzle = fstream.GetLine();
				if (puzzle.Length() > 300) { Console.WriteLine($"Puzzle length > 300 characters. at line {counter}"); fstream.CloseStream(); return null; }
				puzzlelist.Add(puzzle);
			}
			fstream.CloseStream();
			return puzzlelist;
		}



		// return false if failed to set puzzle file
		bool SetPuzzle(string puzzlefile) {
			// check if puzzle is provided as argument or it should be read from file
			if (OptReadFile)
			{
				if ((puzzlefile == null) || (puzzlefile.Length == 0)) { Console.WriteLine("No file name is provided"); return false; }
				if ((puzzlefile.Contains('?')) || (puzzlefile.Contains('*'))) { Console.WriteLine("Multiple file selections is not supported"); return false; }
				CStream fstream = new CStream();
				if (!fstream.isFileExist(puzzlefile)) { Console.WriteLine($"File '{puzzlefile}' not found"); return false; }
				if (fstream.OpenStream(CStream.StreamMode.ReadFile, filename: puzzlefile, buffersize: 500) != 0) { Console.WriteLine($"Could not Read file '{puzzlefile}'"); return false; }
				int counter = 0;
				CStr puzzle = new CStr(82);
				byte ch;
				while ((fstream.BytesToRead() > 0) && (counter < 81))
				{
					ch = fstream.GetChar();
					if ((ch >= '0') && (ch <= '9') || (ch == '.') || (ch == 'x') || (ch == 'X'))
					{
						puzzle.Append(ch);
						counter++;
					}
				}
				fstream.CloseStream();
				this.Puzzle.SetPuzzle(puzzle.ToString());
			}
			else this.Puzzle.SetPuzzle(puzzlefile);
			return true;
		}

		SudokuPuzzle SolvePuzzle() {

			// check if its valid and not solved allready
			if (!this.Puzzle.IsValid()) {
				ShowPuzzle("Invalid or unsolvable puzzle");
				return this.Puzzle;
			}
			else if (this.Puzzle.IsSolved()) {
				ShowPuzzle("Puzzle alredy solved");
				return this.Puzzle;
			}

			// Always start with rule based algorithm, if option rule is set then display progress
			if (!OptBackTrackOnly) {
				Console.WriteLine($"{this.Puzzle.GetPuzzle()}   - Starting 3R algorithm ({this.Puzzle.GetNumberCount()})");
				if (OptRule) {
					while (Puzzle.ResolveRules(1) > 0) Console.WriteLine($"{this.Puzzle.GetPuzzle()}   - Resolved number");
				}
				else Puzzle.ResolveRules();
				if (this.Puzzle.IsSolved())	{
					ShowPuzzle("Puzzle solved");
					return this.Puzzle;
				}
			}


			// Check if BackTrack should be used
			if (OptBackTrack) {
				Console.WriteLine($"{this.Puzzle.GetPuzzle()}   - Starting backtrack ({this.Puzzle.GetNumberCount()})");
				Puzzle.ResolveBacktrack();
				if (this.Puzzle.IsSolved()) {
					ShowPuzzle("Puzzle solved");
					return this.Puzzle;
				}
			}

			// Puzzle not solved
			if (!Puzzle.IsValid()) {
				ShowPuzzle("Invalid or unsolvable puzzle");
				return this.Puzzle;
			}


			// Check if mask should be shown
			if (OptShowMask) ShowMask();
			ShowPuzzle("Could not solve puzzle");
			return this.Puzzle;

		}

		// Show puzzle as grid
		void ShowPuzzle(string msg) {
			int count = 0, spacer = 0;
			string puzzle = this.Puzzle.GetPuzzle();

			if (!OptShowVertical)
			{
				if ((msg != null) && (msg.Length > 0)) Console.WriteLine($"{puzzle}   - {msg}");
				else Console.WriteLine($"{puzzle}");
				return;
			}
			Console.Write("\n");
			if ((msg != null) && (msg.Length > 0)) { Console.WriteLine(msg); }
			foreach (var ch in puzzle) {
				Console.Write(ch);
				count++;
				if ((count == 3) || (count == 6)) { Console.Write(" "); }
				if (count == 9) { Console.Write("\n"); count = 0; spacer++; }
				if (spacer == 3) { Console.Write("\n"); spacer = 0; }
			}
		}

		// show mask as grid
		void ShowMask() {
			string[] mask = new string[9];
			int count = 0, spacer;

			if (!OptShowVertical)
			{
				for (int num = 1; num <= 9; num++) Console.WriteLine($"{Puzzle.GetPossible(num)}   - Possible mask");
				return;
			}

			Console.WriteLine("Possible number mask");
			for (int num = 0; num < 9; num++) mask[num] = Puzzle.GetPossible(num + 1);
			while (count < 81) {
				for (int row = 0; row < 9; row++)
				{
					spacer = 0;
					for (int col = 0; col < 9; col++)
					{
						Console.Write(mask[row][count + col]);
						spacer++;
						if ((spacer == 3) || (spacer == 6)) Console.Write(" ");
					}
					Console.Write("  ");
				}
				count += 9;
				if ((count == 27) || (count == 54)) Console.Write("\n\n");
				else Console.Write("\n");
			}

		}

		string demopuzzle1 = "38........5...2..6...14....1...8..3...9.3.8.4..2.........62.7....6...........1.8.";
		string demopuzzle2 = "..4..7...6......5........9...195....29....7..8...1...3.....32.8.5..........12...4";
		string demopuzzle3 = "6....1.7......75..3.....9...4..9.3.........8....5.4.2..7.6.8....93...7....6.2..1.";
		SudokuPuzzle Puzzle;
		bool OptRule = false;
		bool OptBackTrack = false;
		bool OptBackTrackOnly = false;
		bool OptShowMask = false;
		bool OptReadFile = false;
		bool OptReadFileMultipleRow = false;
		bool OptShowVertical = false;
		bool OptCreateSudoku = false;
		bool OptCreateSudokuPuzzle = false;
	}
}
