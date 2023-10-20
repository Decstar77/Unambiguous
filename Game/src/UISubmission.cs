//using System.Diagnostics;
//using System.Numerics;

//namespace Game {

//    public enum UISubmissionType {
//        INVALID = 0,
//        BUTTON,
//        BLOCK_BUTTON,
//    };
    
//    public class UISubmission {
//        public UISubmissionType type;
//        public string text = "";
//        public Vector2 textSize;
//        public Rectangle rect;
//        public bool isHovered;
//        public bool isPressed;
//        public Color baseColor;
//        public Color hoveredColor;
//        public Color pressedColor;
//    }

//    public class UIState {
//        private Vector2 surfaceMouse = new Vector2();
//        public bool elementHovered = false;
//        private List<UISubmission> submissions = new List<UISubmission>();

//        public void Reset() {
//            surfaceMouse = Raylib.GetMousePosition();
//            elementHovered = false;
//            submissions.Clear();
//        }

//        public void Draw() {
//            foreach( UISubmission sub in submissions ) {
//                switch( sub.type ) {
//                    case UISubmissionType.BUTTON: {
//                        if( sub.isPressed ) {
//                            Raylib.DrawRectangleRec( sub.rect, sub.pressedColor );
//                        }
//                        else if( sub.isHovered ) {
//                            Raylib.DrawRectangleRec( sub.rect, sub.hoveredColor );
//                        }
//                        else {
//                            Raylib.DrawRectangleRec( sub.rect, sub.baseColor );
//                        }

//                        Vector2 textPos = new Vector2(sub.rect.x + sub.rect.width / 2 - sub.textSize.X / 2,
//                                                        sub.rect.y + sub.rect.height / 2 - sub.textSize.Y / 2);
//                        Raylib.DrawTextEx( Raylib.GetFontDefault(), sub.text, textPos, 20, 1, Color.BLACK );
//                    }
//                    break;
//                    case UISubmissionType.BLOCK_BUTTON: {
//                        if( sub.isPressed ) {
//                            Raylib.DrawRectangleRec( sub.rect, sub.pressedColor );
//                        }
//                        else if( sub.isHovered ) {
//                            Raylib.DrawRectangleRec( sub.rect, sub.hoveredColor );
//                        }
//                        else {
//                            Raylib.DrawRectangleRec( sub.rect, sub.baseColor );
//                        }

//                        Vector2 textPos = new Vector2(sub.rect.x + sub.rect.width / 2 - sub.textSize.X / 2,
//                                                        sub.rect.y + sub.rect.height / 2 - sub.textSize.Y / 2);
//                        Raylib.DrawTextEx( Raylib.GetFontDefault(), sub.text, textPos, 20, 1, Color.BLACK );
//                    }
//                    break;
//                    default:
//                        Debug.Assert( false );
//                        break;
//                }
//            }
//        }

//        public bool DrawButtonCenter( int cx, int cy, string text ) {
//            Vector2 textSize = Raylib.MeasureTextEx(Raylib.GetFontDefault(), text, 20, 1);
//            Vector2 rectSize = new Vector2(textSize.X + 20, textSize.Y + 20);
//            Rectangle rect = new Rectangle((float)cx - rectSize.X / 2, (float)cy - rectSize.Y / 2, rectSize.X, rectSize.Y);

//            UISubmission submission = new UISubmission();
//            submission.type = UISubmissionType.BUTTON;
//            submission.text = text;
//            submission.textSize = textSize;
//            submission.rect = rect;
//            submission.baseColor = Color.SKYBLUE;//UIColorsGet(UI_COLOR_SLOT_BACKGROUND);
//            submission.hoveredColor = submission.baseColor;
//            submission.hoveredColor.r = (byte)( (float)submission.hoveredColor.r * 1.2f );
//            submission.hoveredColor.g = (byte)( (float)submission.hoveredColor.g * 1.2f );
//            submission.hoveredColor.b = (byte)( (float)submission.hoveredColor.b * 1.2f );
//            submission.pressedColor = submission.baseColor;
//            submission.pressedColor.r = (byte)( (float)submission.pressedColor.r * 1.5f );
//            submission.pressedColor.g = (byte)( (float)submission.pressedColor.g * 1.5f );
//            submission.pressedColor.b = (byte)( (float)submission.pressedColor.b * 1.5f );
//            submission.isHovered = Raylib.CheckCollisionPointRec( surfaceMouse, rect );
//            submission.isPressed = submission.isHovered && Raylib.IsMouseButtonReleased( MouseButton.MOUSE_BUTTON_LEFT );

//            submissions.Add( submission );
//            elementHovered = elementHovered || submission.isHovered;

//            return submission.isPressed;
//        }

//        public bool DrawButtonTopLeft( int x, int y, string text ) {
//            Vector2 textSize = Raylib.MeasureTextEx(Raylib.GetFontDefault(), text, 20, 1);
//            Vector2 rectSize = new Vector2(textSize.X + 20, textSize.Y + 20);
//            int cx = (int)(x + rectSize.X / 2);
//            int cy = (int)(y + rectSize.Y / 2);
//            return DrawButtonCenter( cx, cy, text );
//        }

//        public bool DrawBlockButton( Vector2 center, Vector2 dims, Color c, string text ) {
//            UISubmission submission = new UISubmission();
//            submission.type = UISubmissionType.BLOCK_BUTTON;
//            submission.text = text;
//            submission.textSize = Raylib.MeasureTextEx( Raylib.GetFontDefault(), text, 20, 1 );
//            submission.rect = new Rectangle( center.X - dims.X / 2, center.Y - dims.Y / 2, dims.X, dims.Y );
//            submission.baseColor = c;
//            submission.hoveredColor = c;
//            submission.hoveredColor.r = (byte)( (float)submission.hoveredColor.r * 1.2f );
//            submission.hoveredColor.g = (byte)( (float)submission.hoveredColor.g * 1.2f );
//            submission.hoveredColor.b = (byte)( (float)submission.hoveredColor.b * 1.2f );
//            submission.pressedColor = c;
//            submission.pressedColor.r = (byte)( (float)submission.pressedColor.r * 1.5f );
//            submission.pressedColor.g = (byte)( (float)submission.pressedColor.g * 1.5f );
//            submission.pressedColor.b = (byte)( (float)submission.pressedColor.b * 1.5f );
//            submission.isHovered = Raylib.CheckCollisionPointRec( surfaceMouse, submission.rect );
//            submission.isPressed = submission.isHovered && Raylib.IsMouseButtonReleased( MouseButton.MOUSE_BUTTON_LEFT );

//            submissions.Add( submission );
//            elementHovered = elementHovered || submission.isHovered;

//            return submission.isPressed;
//        }
//    }
//}
