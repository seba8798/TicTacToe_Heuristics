using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace TicTacToe_Heuristics
{
    class Program
    {
        /*
        ================================================================================
        GAME STATE VARIABLES
        ================================================================================
        */
        static char[] arr = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        static char[] Permanent = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        static int player = 1;
        static int flag = 0; // 1 = someone won, -1 = draw, 0 = game running

        /*
        ================================================================================
        HEURISTIC LEARNING AI VARIABLES
        ================================================================================
        */
        static Dictionary<string, List<Move>> MovesAtBoardState = new Dictionary<string, List<Move>>();
        static List<(string boardState, int moveIndex)> player2MoveHistory = new List<(string, int)>();

        /*
        ================================================================================
        STATISTICS TRACKING
        ================================================================================
        */
        static int timesPlayed = 0;
        static int timesDrawn = 0;
        static int timesPlayer1Won = 0;
        static int timesPlayer2Won = 0;


        static void Main(string[] args)
        {
            /*
            =========================================================================
            MAIN GAME LOOP - TRAINING GAMES WITH PROGRESS TRACKING
            =========================================================================
            */
            
            // Track results for different phases
            var phaseResults = new List<(string phaseName, int totalGames, int player1Wins, int player2Wins, int draws)>();
            
            // Training phases configuration
            var trainingPhases = new[]
            {
                ("Initial Learning", 5000),
                ("2nd Learning", 5000), 
                ("3rd Learning", 5000),
                ("4th Learning", 5000),
                ("Final Phase", 5000)

            };
            
            int totalGamesPlayed = 0;
            
            foreach (var (phaseName, gameCount) in trainingPhases)
            {
                // Reset counters for this phase
                timesPlayed = 0;
                timesDrawn = 0;
                timesPlayer1Won = 0;
                timesPlayer2Won = 0;
                
                // Run games for this phase
                for (int i = 0; i < gameCount; i++)
                {
                    // Reset game state for new game
                    arr = (char[])Permanent.Clone();
                    player = 1;
                    flag = 0;
                    timesPlayed++;
                    totalGamesPlayed++;
                    player2MoveHistory.Clear();

                    /*
                    ========================================================================
                    SINGLE GAME LOOP
                    ========================================================================
                    */
                    do
                    {
                        if (player % 2 == 0)
                        {
                            /*
                            ================================================================
                            PLAYER 2 TURN - HEURISTIC LEARNING AI (O)
                            ================================================================
                            */
                            string boardState = GetBoardStateString();

                            if (!MovesAtBoardState.ContainsKey(boardState))
                            {
                                List<char> availableMoves = arr.Where(x => x != 'X' && x != 'O' && x != '0').ToList();
                                MovesAtBoardState[boardState] = availableMoves.Select(x => new Move(int.Parse(x.ToString()), 1.0f)).ToList();
                            }

                            var moveList = MovesAtBoardState[boardState];
                            if (moveList.Count > 0)
                            {
                                int moveIndex = SelectBestHeuristicMove(moveList);
                                int boardPos = moveList[moveIndex].MovesMade;
                                arr[boardPos] = 'O';
                                
                                // Track move history for heuristic updates
                                player2MoveHistory.Add((boardState, moveIndex));
                            }
                            else
                            {
                                Console.WriteLine("Learning AI has no available moves!");
                            }
                            player++;
                        }
                        else
                        {
                            /*
                            ================================================================
                            PLAYER 1 TURN - RANDOM AI (X)
                            ================================================================
                            */
                            List<char> moves = arr.Where(x => x != 'X' && x != 'O' && x != '0').ToList();
                            if (moves.Count > 0)
                            {
                                int choice = RND.Range(0, moves.Count);
                                int ting = int.Parse(moves[choice].ToString());
                                arr[ting] = 'X';
                            }
                            player++;
                        }

                        flag = CheckWin();

                        /*
                        ====================================================================
                        PRE-POPULATE BOARD STATES FOR HEURISTIC AI
                        ====================================================================
                        */
                        if (flag == 0 && player % 2 == 0)
                        {
                            string boardState = GetBoardStateString();
                            if (!MovesAtBoardState.ContainsKey(boardState))
                            {
                                List<char> availableMoves = arr.Where(x => x != 'X' && x != 'O' && x != '0').ToList();
                                MovesAtBoardState[boardState] = availableMoves.Select(x => new Move(int.Parse(x.ToString()), 1.0f)).ToList();
                            }
                        }
                    }
                    while (flag != 1 && flag != -1);

                    /*
                    ========================================================================
                    GAME RESULT PROCESSING & HEURISTIC LEARNING
                    ========================================================================
                    */
                    if (flag == 1)
                    {
                        int winner = (player % 2) + 1;

                        if (winner == 1) // Player 2 (Learning AI) lost
                        {
                            timesPlayer1Won++;
                            ApplyLosingHeuristics();
                        }
                        else if (winner == 2) // Player 2 (Learning AI) won
                        {
                            timesPlayer2Won++;
                            ApplyWinningHeuristics();
                        }
                    }
                    else if (flag == -1)
                    {
                        timesDrawn++;
                    }
                }
                
                // Store results for this phase
                phaseResults.Add((phaseName, timesPlayed, timesPlayer1Won, timesPlayer2Won, timesDrawn));
            }

            /*
            =========================================================================
            COMPREHENSIVE RESULTS DISPLAY
            =========================================================================
            */
            Console.WriteLine("\n" + new string('=', 80));
            Console.WriteLine("TRAINING COMPLETE");
            Console.WriteLine(new string('=', 80));
            
            // Display results for each phase
            foreach (var (phaseName, totalGames, player1Wins, player2Wins, draws) in phaseResults)
            {
                Console.WriteLine($"\n{phaseName} ({totalGames} games):");
                Console.WriteLine($"  Player 1 (Random AI) won: {player1Wins} times ({(double)player1Wins / totalGames * 100:F2}%)");
                Console.WriteLine($"  Player 2 (Heuristic AI) won: {player2Wins} times ({(double)player2Wins / totalGames * 100:F2}%)");
                Console.WriteLine($"  Draws: {draws} times ({(double)draws / totalGames * 100:F2}%)");
            }
            
            // Display overall summary
            int totalPlayer1Wins = phaseResults.Sum(r => r.player1Wins);
            int totalPlayer2Wins = phaseResults.Sum(r => r.player2Wins);
            int totalDraws = phaseResults.Sum(r => r.draws);
            
            Console.WriteLine($"\n{new string('-', 60)}");
            Console.WriteLine($"OVERALL SUMMARY ({totalGamesPlayed} total games):");
            Console.WriteLine($"  Player 1 (Random AI) won: {totalPlayer1Wins} times ({(double)totalPlayer1Wins / totalGamesPlayed * 100:F2}%)");
            Console.WriteLine($"  Player 2 (Heuristic AI) won: {totalPlayer2Wins} times ({(double)totalPlayer2Wins / totalGamesPlayed * 100:F2}%)");
            Console.WriteLine($"  Draws: {totalDraws} times ({(double)totalDraws / totalGamesPlayed * 100:F2}%)");
        }

        /*
        ================================================================================
        HEURISTIC-BASED MOVE SELECTION
        ================================================================================
        */
        private static int SelectBestHeuristicMove(List<Move> moveList)
        {
            // Find the move with the highest heuristic value
            float bestHeuristic = float.MinValue;
            foreach (Move move in moveList)
            {
                if (move.Heuristic > bestHeuristic)
                {
                    bestHeuristic = move.Heuristic;
                }
            }
            
            // Find all moves that have the best heuristic value
            List<int> bestMoveIndices = new List<int>();
            for (int i = 0; i < moveList.Count; i++)
            {
                if (Math.Abs(moveList[i].Heuristic - bestHeuristic) < 0.01f)
                {
                    bestMoveIndices.Add(i);
                }
            }
            
            // Return single best move or random selection from best moves
            if (bestMoveIndices.Count == 1)
            {
                return bestMoveIndices[0];
            }
            else
            {
                return bestMoveIndices[RND.Range(0, bestMoveIndices.Count)];
            }
        }

        /*
        ================================================================================
        HEURISTIC LEARNING METHODS
        ================================================================================
        */
        private static void ApplyLosingHeuristics()
        {
            // Apply heuristic penalties for last move -0.9, second last -0.8, third last -0.7, and so on
            for (int i = 0; i < player2MoveHistory.Count; i++)
            {
                var (boardState, moveIndex) = player2MoveHistory[player2MoveHistory.Count - 1 - i];
                
                if (MovesAtBoardState.ContainsKey(boardState) && 
                    moveIndex < MovesAtBoardState[boardState].Count)
                {
                    float penalty = -0.9f + (i * 0.1f); // -0.9, -0.8, -0.7, -0.6, and so on
                    var move = MovesAtBoardState[boardState][moveIndex];
                    
                    if (penalty < move.Heuristic)
                    {
                        move.Heuristic = penalty;
                    }
                }
            }
        }

        private static void ApplyWinningHeuristics()
        {
            for (int i = 0; i < player2MoveHistory.Count; i++)
            {
                var (boardState, moveIndex) = player2MoveHistory[i];
                
                if (MovesAtBoardState.ContainsKey(boardState) && 
                    moveIndex < MovesAtBoardState[boardState].Count)
                {
                    float reward = 0.1f + (i * 0.1f); // 0.1, 0.2, 0.3, and so on
                    var move = MovesAtBoardState[boardState][moveIndex];
                    
                    if (reward > move.Heuristic)
                    {
                        move.Heuristic = reward;
                    }
                }
            }
        }
        /*
        ================================================================================
        UTILITY CLASSES & METHODS
        ================================================================================
        */

        internal class Move
        {
            // Heuristic value for the move: last losing move gets -0.9, second last gets -0.8, third last gets -0.7, and so on!

            public int MovesMade { get; set; }
            public float Heuristic { get; set; }
            public Move(int position, float value)
            {
                MovesMade = position;
                Heuristic = value;
            }
        }
        public static class RND
        {
            public static Random Rnd = new Random();
            public static int Range(int a, int b)
            {
                return Rnd.Next(a, b);
            }
        }

        private static void Board()
        {
            Console.WriteLine("     |     |      ");
            Console.WriteLine("  {0}  |  {1}  |  {2}", arr[1], arr[2], arr[3]);
            Console.WriteLine("_____|_____|_____ ");
            Console.WriteLine("     |     |      ");
            Console.WriteLine("  {0}  |  {1}  |  {2}", arr[4], arr[5], arr[6]);
            Console.WriteLine("_____|_____|_____ ");
            Console.WriteLine("     |     |      ");
            Console.WriteLine("  {0}  |  {1}  |  {2}", arr[7], arr[8], arr[9]);
            Console.WriteLine("     |     |      ");
        }

        private static int CheckWin()
        {
            #region Horizontal Winning Condition
            if (arr[1] == arr[2] && arr[2] == arr[3]) return 1;
            else if (arr[4] == arr[5] && arr[5] == arr[6]) return 1;
            else if (arr[7] == arr[8] && arr[8] == arr[9]) return 1;
            #endregion

            #region Vertical Winning Condition
            else if (arr[1] == arr[4] && arr[4] == arr[7]) return 1;
            else if (arr[2] == arr[5] && arr[5] == arr[8]) return 1;
            else if (arr[3] == arr[6] && arr[6] == arr[9]) return 1;
            #endregion

            #region Diagonal Winning Condition
            else if (arr[1] == arr[5] && arr[5] == arr[9]) return 1;
            else if (arr[3] == arr[5] && arr[5] == arr[7]) return 1;
            #endregion

            #region Checking For Draw
            else if (arr[1] != '1' && arr[2] != '2' && arr[3] != '3' && arr[4] != '4' &&
                     arr[5] != '5' && arr[6] != '6' && arr[7] != '7' && arr[8] != '8' &&
                     arr[9] != '9')
            {
                return -1;
            }
            #endregion

            else return 0;
        }

        private static string GetBoardStateString()
        {
            var state = new StringBuilder();
            for (int i = 1; i <= 9; i++)
            {
                state.Append(arr[i]);
            }
            return state.ToString();
        }
    }


}