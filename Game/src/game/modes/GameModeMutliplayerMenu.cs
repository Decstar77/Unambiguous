using OpenTK.Mathematics;
using Shared;
using SoLoud;

namespace Game {
    public class GameModeMutliplayerMenu : GameMode {
        public UIMaster uiMaster = new UIMaster();
        private float buttonY = 0.45f;
        private float buttonYStep = 0.1f;
        private SoloudObject sndButtonHover = null;
        private SoloudObject sndButtonClick = null;
        private int localPlayerNumber = 0;

        public override void Init( GameModeInitArgs args ) {
            sndButtonHover = Content.LoadWav( "sfxd03.wav" );
            sndButtonClick = Content.LoadWav( "sfxd05.wav" );
            AddButton( "Connect", () => {
                Engine.NetworkConnectToServer();
            } );
            AddButton( "Back", () => {
                Engine.AudioPlay( sndButtonClick );
                Engine.MoveToGameMode( new GameModeMainMenu(), new GameModeInitArgs() );
            } );
        }

        private void AddButton( string text, OnClickDelegate onClick ) {
            UITextButton button = new UITextButton(text);
            button.xConstraint = new UIConstraintRelative( 0.5f );
            button.yConstraint = new UIConstraintRelative( buttonY );
            button.widthConstraint = new UIConstraintRelative( 0.15f );
            button.heightConstraint = new UIConstraintAspect( 0.53f, button.widthConstraint );
            button.onClick.Add( () => Engine.AudioPlay( sndButtonClick ) );
            button.onClick.Add( onClick );
            button.onHover.Add( ( p ) => Engine.AudioPlay( sndButtonHover ) );
            uiMaster.elements.Add( button );
            buttonY += buttonYStep;
        }

        public override void Shutdown() {
        }

        public override void NetworkPacketReceived( byte[] data, int length ) {
            if ( length > 0 ) {
                GamePacketType type = (GamePacketType )data[ 0 ];
                if ( type == GamePacketType.MAP_START ) {
                    localPlayerNumber = data[1];
                }
            }
        }

        public override void UpdateTick( float dt ) {
            if ( localPlayerNumber != 0 ) {
                GameModeInitArgs args = new GameModeInitArgs();
                args.multiplayer = true;
                args.localPlayerNumber = localPlayerNumber;
                Engine.MoveToGameMode( new GameModeGame(), args );
            }
        }

        public override void UpdateRender( float dt ) {
            DrawCommands cmds = new DrawCommands();
            uiMaster.UpdateAndRender( cmds );
            Engine.SubmitDrawCommands( cmds );
        }
    }
}
