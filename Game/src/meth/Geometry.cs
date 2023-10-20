using System.Numerics;

namespace Game {
    public struct Circle {
        public Vector2 center;
        public float radius;

        public Circle() {
            center = Vector2.Zero;
            radius = 0;
        }

        public Circle( Vector2 center, float radius ) {
            this.center = center;
            this.radius = radius;
        }

        public Vector2 GetClosestPoint( Vector2 p ) {
            Vector2 diff = p - center;
            float dist = MathF.Sqrt(Vector2.Dot(diff, diff));
            if ( dist < radius ) {
                return p;
            }
            else {
                return center + diff / dist * radius;
            }
        }

        public bool ContainsPoint( Vector2 p ) {
            return ( p - center ).LengthSquared() < radius * radius;
        }
    }

    public struct Rect {
        public Vector2 min;
        public Vector2 max;

        public Rect() {
            min = Vector2.Zero;
            max = Vector2.Zero;
        }

        public float GetRaduis() {
            return ( max - min ).Length() / 2;
        }

        public Vector2 GetClosestPoint( Vector2 p ) {
            Vector2 closest = p;
            closest.X = MathF.Max( closest.X, min.X );
            closest.Y = MathF.Max( closest.Y, min.Y );
            closest.X = MathF.Min( closest.X, max.X );
            closest.Y = MathF.Min( closest.Y, max.Y );
            return closest;
        }

        public bool ContainsPoint( Vector2 p ) {
            return p.X >= min.X && p.X <= max.X && p.Y >= min.Y && p.Y <= max.Y;
        }
    }

    public enum BoundsType {
        INVALID = 0,
        CIRCLE,
        RECT,
    }

    public struct Bounds {
        public BoundsType type = BoundsType.INVALID;
        public Circle circle = new Circle();
        public Rect rect = new Rect();

        public Bounds() {
        }

        public Vector2 GetClosestPoint( Vector2 p ) {
            switch ( type ) {
                case BoundsType.CIRCLE:
                    return circle.GetClosestPoint( p );
                case BoundsType.RECT:
                    return rect.GetClosestPoint( p );
                default:
                    return Vector2.Zero;
            }
        }

        public bool ContainsPoint( Vector2 p ) {
            switch ( type ) {
                case BoundsType.CIRCLE:
                    return circle.ContainsPoint( p );
                case BoundsType.RECT:
                    return rect.ContainsPoint( p );
                default:
                    return false;
            }
        }
    }

    public static class Intersections {
        public static bool CircleVsCircle( Circle c1, Circle c2 ) {
            // Do this with squared lengths to avoid a sqrt call
            float distance = (c1.center - c2.center).LengthSquared();
            float radiusSum = c1.radius + c2.radius;
            return distance <= radiusSum * radiusSum;
        }

        public static bool CircleVsRect( Circle c, Rect r ) {
            Vector2 closest = r.GetClosestPoint(c.center);
            return ( closest - c.center ).LengthSquared() < c.radius * c.radius;
        }

        public static bool RectVsRect( Rect r1, Rect r2 ) {
            return r1.min.X <= r2.max.X && r1.max.X >= r2.min.X &&
                r1.min.Y <= r2.max.Y && r1.max.Y >= r2.min.Y;
        }

        public static bool CircleVsBounds( Circle c, Bounds b ) {
            switch ( b.type ) {
                case BoundsType.CIRCLE:
                    return CircleVsCircle( c, b.circle );
                case BoundsType.RECT:
                    return CircleVsRect( c, b.rect );
                default:
                    return false;
            }
        }

        public static bool RectVsBounds( Rect r, Bounds b ) {
            switch ( b.type ) {
                case BoundsType.CIRCLE:
                    return CircleVsRect( b.circle, r );
                case BoundsType.RECT:
                    return RectVsRect( r, b.rect );
                default:
                    return false;
            }
        }

        public static bool BoundsVsBounds( Bounds b1, Bounds b2 ) {
            switch ( b1.type ) {
                case BoundsType.CIRCLE:
                    return CircleVsBounds( b1.circle, b2 );
                case BoundsType.RECT:
                    return RectVsBounds( b1.rect, b2 );
                default:
                    return false;
            }
        }
    }
}
