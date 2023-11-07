using OpenTK.Mathematics;
using Shared;
using SoLoud;

namespace Game {
    public class GameModeOptionsMenu : GameMode {
        public UIMaster uiMaster = new UIMaster();
        private SoloudObject sndButtonHover = null;
        private SoloudObject sndButtonClick = null;
        private SoloudObject testingSound = null;

        public override void Init( GameModeInitArgs args ) {
            sndButtonHover = Content.LoadWav( "sfxd03.wav" );
            sndButtonClick = Content.LoadWav( "sfxd05.wav" );
            testingSound = Content.LoadWav( "geret.wav" );
            AddSaveButton();
            AddDoneButton();
            Engine.AudioPlay( testingSound );
        }

        private void AddSaveButton() {
            UITextButton button = new UITextButton( "Save" );
            button.xConstraint = new UIConstraintRelative( 0.9f );
            button.yConstraint = new UIConstraintRelative( 0.9f );
            button.widthConstraint = new UIConstraintRelative( 0.15f );
            button.heightConstraint = new UIConstraintAspect( 0.53f, button.widthConstraint );
            button.onHover.Add( ( p ) => Engine.AudioPlay( sndButtonHover ) );
            button.onClick.Add( () => Engine.AudioPlay( sndButtonClick ) );
            uiMaster.elements.Add( button );
        }

        private void AddDoneButton() {
            UITextButton button = new UITextButton( "Done" );
            button.xConstraint = new UIConstraintRelative( 0.1f );
            button.yConstraint = new UIConstraintRelative( 0.9f );
            button.widthConstraint = new UIConstraintRelative( 0.15f );
            button.heightConstraint = new UIConstraintAspect( 0.53f, button.widthConstraint );
            button.onHover.Add( ( p ) => Engine.AudioPlay( sndButtonHover ) );
            button.onClick.Add( () => Engine.AudioPlay( sndButtonClick ) );
            button.onClick.Add( () => Engine.MoveToGameMode( new GameModeMainMenu(), new GameModeInitArgs() ) );
            uiMaster.elements.Add( button );
        }

        public override void Shutdown() {
        }

        public override void UpdateTick( float dt ) {

        }

        public override void UpdateRender( float dt ) {
            DrawCommands cmds = new DrawCommands();
            uiMaster.UpdateAndRender( cmds );
            Engine.SubmitDrawCommands( cmds );
        }
    }
}
