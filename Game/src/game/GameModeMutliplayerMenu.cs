﻿using OpenTK.Mathematics;
using SoLoud;

namespace Game {
    public class GameModeMutliplayerMenu : GameMode {
        public UIMaster uiMaster = new UIMaster();
        private float buttonY = 0.45f;
        private float buttonYStep = 0.1f;
        private SoloudObject sndButtonHover = null;
        private SoloudObject sndButtonClick = null;

        public override void Init() {
            sndButtonHover = Content.LoadWav( "sfxd03.wav" );
            sndButtonClick = Content.LoadWav( "sfxd05.wav" );
            AddButton( "Connect", () => { Engine.AudioPlay( sndButtonClick ); Engine.NetworkConnectToServer(); } );
            AddButton( "Back", () => { Engine.AudioPlay( sndButtonClick ); Engine.MoveToGameMode( new GameModeMainMenu() ); }  );
        }

        private void AddButton( string text, OnClickDelegate onClick ) {
            UITextButton button = new UITextButton( text );
            button.xConstraint = new UIConstraintRelative( 0.5f );
            button.yConstraint = new UIConstraintRelative( buttonY );
            button.widthConstraint = new UIConstraintRelative( 0.15f );
            button.heightConstraint = new UIConstraintAspect( 0.53f, button.widthConstraint );
            button.onClick = onClick;
            button.onHover = ( Vector2 p ) => Engine.AudioPlay( sndButtonHover );
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