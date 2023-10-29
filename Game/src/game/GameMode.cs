namespace Game {
    public abstract class GameMode {
        public Entity[] entities = new Entity[100];

        public abstract void Init();
        public abstract void UpdateTick( float dt );
        public abstract void UpdateRender( float dt );
        public abstract void Shutdown();

        public Entity SpawnEntity( EntityType type ) {
            for ( int i = 0; i < entities.Length; i++ ) {
                if ( entities[i] == null ) {
                    entities[i] = new Entity();
                    entities[i].type = type;
                    return entities[i];
                }
            }

            return null;
        }
    }
}
