using OpenTK.Mathematics;

namespace Game {
    public class UISizeConstraint {
    }

    public class UISizeConstraintFixed : UISizeConstraint {
        public Vector2 size;
    }

    public class UIPosConstraint {
    }

    public abstract class UIElement {
        public UIElement? parent = null;
        public List<UIElement> children = new List<UIElement>();
        public List<UISizeConstraint> sizeConstraints = new List<UISizeConstraint>();
        public List<UIPosConstraint> posConstraints = new List<UIPosConstraint>();

        public abstract void Draw( DrawCommands drawCommands, RectBounds rect );
        public abstract Vector2 ComputeSize();
        public abstract void OnClick( Vector2 mousePos );
        public abstract void OnHover( Vector2 mousePos );
        public abstract void OnUnhover( Vector2 mousePos );
        public abstract void OnPress( Vector2 mousePos );
        public abstract void OnRelease( Vector2 mousePos );
    }



    public class UIButton : UIElement {
        public override void Draw( DrawCommands drawCommands, RectBounds rect ) {
            drawCommands.DrawRect( rect );
        }

        public override Vector2 ComputeSize() {
            Vector2 size = new Vector2( 100, 50 );
            return size;
        }

        public override void OnClick( Vector2 mousePos ) {
        }

        public override void OnHover( Vector2 mousePos ) {
        }

        public override void OnUnhover( Vector2 mousePos ) {
        }

        public override void OnPress( Vector2 mousePos ) {
        }

        public override void OnRelease( Vector2 mousePos ) {
        }
    }

    public class UIMaster {
        private List<UIElement> elements = new List<UIElement>();
        
    }

}
