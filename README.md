# Sudoku-Solver

   	*	offer two algorithms to solve sudoku puzzles
  
 	*	Rule based Algorithm 3RA, uses three logic rules to solve puzzle and is the fastest algorithm.
 		The rules are: Cell singles, Cluster singles, Cluster cells traverse exclusions
		Will not be able to solve puzzles with multiple solutions as it uses logic rules
 	 	
 	*	BackTrack Algorithm (enhanched), utilizes the rule base algorithm as a starting point when using switch -b.
 		There after it will test all qualified possible numbers and will solve any valid puzzle.
		To run backtrack only, use switch -bo instead.
		
	*	Performance test solving: 4000-puzzles.txt (located in binaries folder)
 		14 seconds using logic only to solve all 4000 puzzles
 		20 seconds using backtrack only (-bo) to solve all 4000 puzzles
	
 
