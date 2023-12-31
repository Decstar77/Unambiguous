﻿using OpenTK.Mathematics;
using SoLoud;

namespace Game {

    public class GameModeMainMenu : GameMode {
        public UIMaster uiMaster = new UIMaster();
        private float buttonY = 0.35f;
        private float buttonYStep = 0.1f;
        private SoloudObject sndButtonHover;
        private SoloudObject sndButtonClick;

        public override void Init( GameModeInitArgs args ) {
            sndButtonHover = Content.LoadWav( "sfxd03.wav" );
            sndButtonClick = Content.LoadWav( "sfxd05.wav" );
            AddButton( "Single Pringle", () => Engine.MoveToGameMode( new GameModeGame(), new GameModeInitArgs() ) );
            AddButton( "Many Penny", () => Engine.MoveToGameMode( new GameModeMutliplayerMenu(), new GameModeInitArgs() ) );
            AddButton( "Options", () => Engine.MoveToGameMode( new GameModeOptionsMenu(), new GameModeInitArgs() ) );
            AddButton( "Quit", () => Engine.QuitGame() );
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

        public override void UpdateTick( float dt ) {
        }

        public override void UpdateRender( float dt ) {
            DrawCommands cmds = new DrawCommands();
            uiMaster.UpdateAndRender( cmds );
            Engine.SubmitDrawCommands( cmds );
        }
    }
}
