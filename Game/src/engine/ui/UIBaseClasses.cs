using OpenTK.Mathematics;
using static System.Net.Mime.MediaTypeNames;

namespace Game {

    public interface UIConstraint {
        public float Compute( float parentSize );
    }

    public class UIConstraintFixed : UIConstraint {
        public float pos;

        public UIConstraintFixed( float pos ) {
            this.pos = pos;
        }

        public float Compute( float parentSize ) {
            return pos;
        }
    }

    public class UIConstraintRelative : UIConstraint {
        public float pos;

        public UIConstraintRelative( float pos ) {
            this.pos = pos;
        }

        public float Compute( float parentSize ) {
            return pos * parentSize;
        }
    }

    public class UIConstraintAspect : UIConstraint {
        public float aspect;
        public UIConstraint subject;

        public UIConstraintAspect( float aspect, UIConstraint subject ) {
            this.aspect = aspect;
            this.subject = subject;
        }

        public float Compute( float parentSize ) {
            return subject.Compute( parentSize ) * aspect;
        }
    }

    public delegate void OnClickDelegate();
    public delegate void OnHoverDelegate( Vector2 mousePos );
    
    public abstract class UIElement {
        public Vector2 computedSize = Vector2.Zero;
        public Vector2 computedPos = Vector2.Zero;
        public bool isHovered = false;
        public bool isPressed = false;

        public UIConstraint xConstraint = new UIConstraintFixed( 0 );
        public UIConstraint yConstraint = new UIConstraintFixed( 0 );
        public UIConstraint widthConstraint = new UIConstraintFixed( 0 );
        public UIConstraint heightConstraint = new UIConstraintFixed( 0 );

        public OnClickDelegate? onClick = null;
        public OnHoverDelegate? onHover = null;

        public abstract void Draw( DrawCommands drawCommands, RectBounds rect );
        public abstract void ComputeSize( Vector2 parentSize );
        public abstract void ComputePos( Vector2 parentSize );
        public RectBounds GetRect() {
            return new RectBounds().SetFromCenterDims( computedPos, computedSize );
        }

        public virtual void OnHover( Vector2 mousePos ) {
            if ( onHover != null ) {
                onHover( mousePos );
            }
        }
        
        public virtual void OnUnhover( Vector2 mousePos ) { }
        public virtual void OnPress( Vector2 mousePos ) { }
        public virtual void OnRelease( Vector2 mousePos ) {
            if ( onClick != null ) {
                onClick();
            }
        }
    }

    public class UIBlock : UIElement {
        public override void Draw( DrawCommands drawCommands, RectBounds rect ) {
            drawCommands.DrawScreenRect( rect );
        }

        public override void ComputeSize( Vector2 parentSize ) {
            float w = widthConstraint.Compute( parentSize.X );
            float h = heightConstraint.Compute( parentSize.Y );
            computedSize = new Vector2( w, h );
        }

        public override void ComputePos( Vector2 parentSize ) {
            float x = xConstraint.Compute( parentSize.X );
            float y = yConstraint.Compute( parentSize.Y );
            computedPos = new Vector2( x, y );
        }
    }

    public class UITextButton : UIElement {
        private string text;
        private Vector4 color =         Colors.PETER_RIVER;
        private Vector4 hoverColor =    Colors.PETER_RIVER * 1.2f;
        private Vector4 pressColor =    Colors.PETER_RIVER * 1.45f;
        private FontStashSharp.DynamicSpriteFont font;
        private Vector2 textDims = Vector2.Zero;
        private Vector2 padding = new Vector2( 8, 8 );

        public UITextButton( string text ) {
            this.text = text;
            font = Content.GetDefaultFont();
            System.Numerics.Vector2 v = font.MeasureString( text );
            textDims = new Vector2( v.X, v.Y );
        }

        public override void Draw( DrawCommands drawCommands, RectBounds rect ) {
            drawCommands.currentColor = color;
            if ( isHovered == true ) {
                drawCommands.currentColor = hoverColor;
            }
            if ( isPressed == true ) {
                drawCommands.currentColor = pressColor;
            }

            drawCommands.DrawScreenRect( rect );
            drawCommands.DrawText( text, rect.GetCenter(), true );
        }

        public override void ComputeSize( Vector2 parentSize ) {
            float w = widthConstraint.Compute( parentSize.X );
            float h = heightConstraint.Compute( parentSize.Y );
            computedSize = new Vector2( w, h ) + padding;
        }

        public override void ComputePos( Vector2 parentSize ) {
            float x = xConstraint.Compute( parentSize.X );
            float y = yConstraint.Compute( parentSize.Y );
            computedPos = new Vector2( x, y );
        }
    }

    public class UIMaster {
        public List<UIElement> elements = new List<UIElement>();

        public UIMaster() {
            //test.xConstraint = new UIConstraintRelative( 0.5f );
            //test.yConstraint = new UIConstraintRelative( 0.5f );
            //test.widthConstraint = new UIConstraintRelative( 0.10f );
            //test.heightConstraint = new UIConstraintAspect( 1.66f, test.widthConstraint );
        }

        public void UpdateAndRender( DrawCommands cmds ) {
            for ( int i = 0; i < elements.Count; i++ ) {
                UIElement element = elements[i];

                Vector2 size = Engine.GetSurfaceSize();
                element.ComputeSize( size );
                element.ComputePos( size );
                RectBounds rect = element.GetRect();
                Vector2 mousePos = Engine.MouseScreenPos();


                element.isPressed = false;
                if ( rect.ContainsPoint( mousePos ) ) {
                    if ( element.isHovered == false ) {
                        //Console.WriteLine( "OnHovered" );
                        element.OnHover( mousePos );
                    }

                    element.isHovered = true;

                    if ( Engine.MouseDown( 1 ) ) {
                        element.isPressed = true;
                    }

                    if ( Engine.MouseJustDown( 1 ) ) {
                        //Console.WriteLine( "press" );
                        element.OnPress( mousePos );
                    }

                    if ( Engine.MouseJustUp( 1 ) ) {
                        //Console.WriteLine( "up" );
                        element.OnRelease( mousePos );
                    }
                }
                else {
                    if ( element.isHovered == true ) {
                        //Console.WriteLine( "OnUnhovered" );
                        element.OnUnhover( mousePos );
                    }
                    element.isHovered = false;
                }

                element.Draw( cmds, rect );
            }
        }
    }

}
