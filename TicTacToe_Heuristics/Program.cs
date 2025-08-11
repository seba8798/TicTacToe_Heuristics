using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace TicTacToe_Heuristics
{
    /*
    ====================================================================================================
    OPGAVE BESKRIVELSE - TIC TAC TOE AI UDVIKLING
    ====================================================================================================
    
    Step 1: Byg en AI der kan lave et tilfældigt træk. Lad denne AI være spiller 1.
            Vind over din tilfældige AI i et spil
            Bemærk dog at du skal finde ud af, hvad du gør, hvis din AI taber så meget, 
            at den ikke kan gøre et træk
    
    Step 2: Byg en AI, der kopierer hexapawns måde at lære på.
            Sæt en høj discovery i starten og lad den spille mod en tilfældig modstander ca. 200 spil.
    
    Step 3: Lav en AI til spiller 1 som bruger hexapawn måden at lære på og lad de to spil lære mod hinanden.
            sæt discovery til 0 og se, hvor hurtigt de konsekvent spiller lige op mod hinanden.
    
    ====================================================================================================
    CURRENT IMPLEMENTATION STATUS: STEP 2 - LEARNING AI VS RANDOM AI
    ====================================================================================================
    - Player 1: Random AI (X)
    - Player 2: Learning AI (O) - Uses hexapawn-style learning by removing losing moves
    - Running 20,000 games to train the learning AI
    ====================================================================================================
    */

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
        LEARNING AI VARIABLES (HEXAPAWN-STYLE)
        ================================================================================
        */

        //istedet for int skal det være object med en float value
        static Dictionary<string, List<Move>> MovesAtBoardState = new Dictionary<string, List<Move>>();
        static int lastPlayer2MoveIndex = -1;
        static string lastPlayer2BoardState = null;

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
            ============================================================================
            MAIN GAME LOOP - 20,000 TRAINING GAMES
            ============================================================================
            */
            for (int i = 0; i < 20000; i++)
            {
                // Reset game state for new game
                arr = (char[])Permanent.Clone();
                player = 1;
                flag = 0;
                timesPlayed++;

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
                        PLAYER 2 TURN - LEARNING AI (O)
                        ================================================================
                        */
                        string boardState = GetBoardStateString();

                        if (!MovesAtBoardState.ContainsKey(boardState))
                        {
                            List<char> availableMoves = arr.Where(x => x != 'X' && x != 'O' && x != '0').ToList();
                            MovesAtBoardState[boardState] = availableMoves.Select(x => int.Parse(x.ToString())).ToList();
                        }
                        var moveList = MovesAtBoardState[boardState];
                        if (moveList.Count > 0)
                        {
                            int moveIndex = RND.Range(0, moveList.Count);
                            int boardPos = moveList[moveIndex];
                            arr[boardPos] = 'O';
                            lastPlayer2BoardState = boardState;
                            lastPlayer2MoveIndex = moveIndex;
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
                    PRE-POPULATE BOARD STATES FOR LEARNING AI
                    ====================================================================
                    */
                    if (flag == 0 && player % 2 == 0)
                    {
                        string boardState = GetBoardStateString();
                        if (!MovesAtBoardState.ContainsKey(boardState))
                        {
                            List<char> availableMoves = arr.Where(x => x != 'X' && x != 'O' && x != '0').ToList();
                            MovesAtBoardState[boardState] = availableMoves.Select(x => int.Parse(x.ToString())).ToList();
                        }
                    }
                }
                while (flag != 1 && flag != -1);

                /*
                ========================================================================
                DEBUG OUTPUT (CURRENTLY ACTIVE - SHOULD BE REMOVED FOR TRAINING)
                ========================================================================
                */
                Board();
                int ShowWinner = (player % 2) + 1;
                Console.WriteLine("Player {0} has won", ShowWinner);
                Console.WriteLine("Draw");

                /*
                ========================================================================
                GAME RESULT PROCESSING & LEARNING
                ========================================================================
                */
                if (flag == 1)
                {
                    int winner = (player % 2) + 1;

                    string boardState = GetBoardStateString();
                    if (!MovesAtBoardState.ContainsKey(boardState))
                    {
                        List<char> availableMoves = arr.Where(x => x != 'X' && x != 'O' && x != '0').ToList();
                        MovesAtBoardState[boardState] = availableMoves.Select(x => int.Parse(x.ToString())).ToList();
                    }

                    if (winner == 1 && lastPlayer2BoardState != null)
                    {
                        /*
                        ================================================================
                        LEARNING: REMOVE LOSING MOVE FROM PLAYER 2 (HEXAPAWN STYLE)
                        ================================================================
                        */
                        timesPlayer1Won++;
                        var moveList = MovesAtBoardState[lastPlayer2BoardState];
                        if (moveList.Count > lastPlayer2MoveIndex)
                        {
                            moveList.RemoveAt(lastPlayer2MoveIndex);
                        }
                    }
                    else if (winner == 2)
                    {
                        timesPlayer2Won++;
                    }
                }
                else
                {
                    timesDrawn++;
                }
            }

            /*
            =========================================================================
            FINAL STATISTICS OUTPUT
            =========================================================================
            */
            Console.WriteLine("\nTotal games played: " + timesPlayed);
            Console.WriteLine("Player 1 (AI) won: " + timesPlayer1Won + " times");
            Console.WriteLine("Player 2 (Smart AI) won: " + timesPlayer2Won + " times");
            Console.WriteLine("Draws: " + timesDrawn);
        }

        /*
        ================================================================================
        UTILITY CLASSES & METHODS
        ================================================================================
        */
        public static class RND
        {
            private static Random Rnd = new Random();
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

    internal class Move
    {
        public int MovesMade { get; set; }

        //heuristic value for the move takes last loosing value and set it to - 0,9 second last loosing value to -0,8 third last loosing value to -0,7 and so on
        public float Heuristic { get; set; }
        public Move(int position, float value)
        {
            MovesMade = position;
            Heuristic = value;
        }
    }
}