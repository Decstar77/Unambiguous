
using Game;

namespace Game
{

    public class SyncQueues {
        public void Start() {
            player1Turns.Clear();
            player2Turns.Clear();

            for ( int i = 1; i <= slidingWindowWidth; i++ ) {
                MapTurn turn = new MapTurn();
                turn.turnNumber = i;
                player1Turns.Enqueue( turn );
                player2Turns.Enqueue( turn );
            }
        }

        public bool CanTurn() {
            int player1Count = player1Turns.Count;
            int player2Count = player2Turns.Count;

            if ( player1Count > 0 && player2Count > 0 ) {
                return true;
            }

            return false;
        }

        public void AddTurn( int playerNumber, MapTurn turn ) {
            if ( playerNumber == 1 ) {
                player1Turns.Enqueue( turn );
            }
            else {
                player2Turns.Enqueue( turn );
            }
        }

        public MapTurn GetNextTurn( int playerNumber ) {
            if ( playerNumber == 1 ) {
                return player1Turns.Peek();
            }
            else {
                return player2Turns.Peek();
            }
        }

        public void FinishTurn() {
            player1Turns.Dequeue();
            player2Turns.Dequeue();
        }

        public int GetSlidingWindowWidth() {
            return slidingWindowWidth;
        }

        public static int TurnRate = 24; // Per second
        public static float TurnRateMS = 1 / (float)TurnRate;

        int slidingWindowWidth = 4;
        Queue<MapTurn> player1Turns = new Queue<MapTurn>( 10 );
        Queue<MapTurn> player2Turns = new Queue<MapTurn>( 10 );
    }
}
