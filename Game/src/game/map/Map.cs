
namespace Game {

    public class Map {
        private IsoGrid             playGrid = new IsoGrid( 10, 10, 2 );
        private Entity[]            entities = new Entity[ 100 ];
        private int                 entityCount = 0;
        private SyncQueues          syncQueues = new SyncQueues();
        private int                 turnNumber = 0;

        public Map() {
            
        }
        
        public void StartGame() {
            syncQueues.Start();
        }

        public void Tick() {

        }
    }
}
