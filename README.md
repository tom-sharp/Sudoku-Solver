# Sudoku-Solver

   	*	offer two algorithms to solve sudoku puzzles
  
 	*	Rule based Algorithm 3RA, uses three logic rules to solve puzzle and is the fastest algorithm.
 		The rules are: Cell singles, Cluster singles, Cluster cells traverse exclusions
		Will not be able to solve puzzles with multiple solutions as it uses logic rules
 	 	
 	*	NumPass Algorithm (default), is a tweek of backtrack and utilizes the rule base algorithm to
		increase performance. It will guess only qualified numbers and try to solve puzzle after
		each number guess.
		
	*	Performance test solving: 4000-puzzles.txt (located in binaries folder)
 		13 seconds using logic only to solve all 4000 puzzles
 		13 seconds using numpass algorithm to solve all 4000 puzzles (as all puzzles resolves on logic, numpass never kick-in)

	*	Now NumPass is the default algorithm (use -l or -lr) to explicit use logic only
	
 