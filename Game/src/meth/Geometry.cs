using OpenTK.Mathematics;
using System.Drawing;

namespace Game {
    public struct CircleCollider {
        public Vector2 center;
        public float radius;

        public CircleCollider() {
            center = Vector2.Zero;
            radius = 0;
        }

        public CircleCollider( Vector2 center, float radius ) {
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
            return ( p - center ).LengthSquared < radius * radius;
        }
    }

    public struct RectBounds {
        public Vector2 min;
        public Vector2 max;

        public RectBounds() {
            min = Vector2.Zero;
            max = Vector2.Zero;
        }

        public RectBounds SetFromMinMax( Vector2 min, Vector2 max ) {
            this.min = min;
            this.max = max;
            return this;
        }
        public RectBounds SetFromCenterDims(Vector2 center, Vector2 dims) {
            min = center - dims / 2;
            max = center + dims / 2;
            return this;
        }

        public RectBounds SetFromCenterDims( float x, float y, float w, float h ) {
            return SetFromCenterDims( new Vector2( x, y ), new Vector2( w, h ) );
        }

        public float GetRaduis() {
            return ( max - min ).Length / 2;
        }

        public Vector2 GetCenter() {
            return ( min + max ) / 2;
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

        public void Translate( Vector2 d ) {
            min += d;
            max += d;
        }
    }

    public struct BoxCollider {
        public Vector2 center;
        public Vector2 dims;
        public float rotation;

        public BoxCollider() {
            dims = Vector2.Zero;
            center = Vector2.Zero;
            rotation = 0;
        }

        public BoxCollider( Vector2 center, Vector2 dims, float rotation ) {
            this.center = center;
            this.dims = dims;
            this.rotation = rotation;
        }

        public void GetVerts( out Vector2 v1, out Vector2 v2, out Vector2 v3, out Vector2 v4 ) {
            (float sin, float cos) = MathF.SinCos( rotation );
            float w = dims.X / 2;
            float h = dims.Y / 2;
            v1 = center + new Vector2( -w * cos - h * sin, -w * sin + h * cos );
            v2 = center + new Vector2( w * cos - h * sin, w * sin + h * cos );
            v3 = center + new Vector2( w * cos + h * sin, w * sin - h * cos );
            v4 = center + new Vector2( -w * cos + h * sin, -w * sin - h * cos );
        }
    }

    public struct ConvexCollider {
        public Vector2[] verts;

        public ConvexCollider( params Vector2[] verts ) {
            this.verts = verts;
            SortPointsIntoClockWiseOrder();
        }

        public ConvexCollider Clone() {
            ConvexCollider clone = new ConvexCollider();
            clone.verts = new Vector2[verts.Length];
            for ( int i = 0; i < verts.Length; i++ ) {
                clone.verts[i] = verts[i];
            }
            return clone;
        }

        public void Translate( Vector2 d ) {
            for ( int i = 0; i < verts.Length; i++ ) {
                verts[i] += d;
            }
        }

        public Vector2 GetClosestPoint( Vector2 p ) {
            float minDistance = float.MaxValue;
            Vector2 closePoint = Vector2.Zero;

            for ( int i = 0; i < verts.Length; i++ ) {
                Vector2 v = verts[i];
                float distance = Vector2.DistanceSquared( v, p );

                if ( distance < minDistance ) {
                    minDistance = distance;
                    closePoint = v;
                }
            }

            return closePoint;
        }

        private void SortPointsIntoClockWiseOrder() {
            Vector2 centroid = Vector2.Zero;
            for ( int i = 0; i < verts.Length; i++ ) {
                centroid += verts[i];
            }
            centroid /= verts.Length;

            Array.Sort(verts, ( a, b ) => {
                float a1 = MathF.Atan2( a.Y - centroid.Y, a.X - centroid.X );
                float a2 = MathF.Atan2( b.Y - centroid.Y, b.X - centroid.X );
                return a1.CompareTo( a2 );
            } );
        }

        private void SortPointsIntoClockWiseOrder( List<Vector2> verts ) {
            // Calculate centroid  
            Vector2 centroid = Vector2.Zero;
            for ( int i = 0; i < verts.Count; i++ ) {
                centroid += verts[i];
            }
            centroid /= verts.Count;

            verts.Sort( ( a, b ) => {
                float a1 = MathF.Atan2( a.Y - centroid.Y, a.X - centroid.X );
                float a2 = MathF.Atan2( b.Y - centroid.Y, b.X - centroid.X );
                return a1.CompareTo( a2 );
            } );
        }

        private float Determinant( Vector2 u, Vector2 v ) {
            float result = u.X * v.Y - u.Y * v.X;
            return result;
        }

        public List<Vector2> Triangulate() {
            List<Vector2> useVerts = new List<Vector2>( verts );
            List<Vector2> outVerts = new List<Vector2>();
            if ( useVerts.Count < 3 ) {
                return outVerts;
            }

            SortPointsIntoClockWiseOrder( useVerts );

            if ( useVerts.Count == 3 ) {
                outVerts.Add( useVerts[0] );
                outVerts.Add( useVerts[1] );
                outVerts.Add( useVerts[2] );
                return outVerts;
            }

            bool triangleFound = true;
            while ( useVerts.Count != 0 ) {
                if ( !triangleFound ) {
                    return outVerts;
                }

                triangleFound = false;

                for ( int i = 0; i < useVerts.Count - 2; i++ ) {
                    if ( !triangleFound ) {
                        float d = Determinant( useVerts[i + 2] - useVerts[i + 1], useVerts[i + 1] - useVerts[i] );
                        if ( d < 0 ) {
                            triangleFound = true;

                            outVerts.Add( useVerts[i] );
                            outVerts.Add( useVerts[i + 1] );
                            outVerts.Add( useVerts[i + 2] );

                            useVerts.RemoveAt( i + 1 );
                        }
                    }
                }
            }

            return outVerts;
        }
    }

    public enum BoundsType {
        INVALID = 0,
        CIRCLE,
        RECT,
    }

    public struct Bounds {
        public BoundsType type = BoundsType.INVALID;
        public CircleCollider circle = new CircleCollider();
        public RectBounds rect = new RectBounds();

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
        public static bool CircleVsCircle( CircleCollider c1, CircleCollider c2 ) {
            // Do this with squared lengths to avoid a sqrt call
            float distance = (c1.center - c2.center).LengthSquared;
            float radiusSum = c1.radius + c2.radius;
            return distance <= radiusSum * radiusSum;
        }

        public static bool CircleVsRect( CircleCollider c, RectBounds r ) {
            Vector2 closest = r.GetClosestPoint(c.center);
            return ( closest - c.center ).LengthSquared < c.radius * c.radius;
        }

        public static bool RectVsRect( RectBounds r1, RectBounds r2 ) {
            return r1.min.X <= r2.max.X && r1.max.X >= r2.min.X &&
                r1.min.Y <= r2.max.Y && r1.max.Y >= r2.min.Y;
        }

        public static bool CircleVsBounds( CircleCollider c, Bounds b ) {
            switch ( b.type ) {
                case BoundsType.CIRCLE:
                    return CircleVsCircle( c, b.circle );
                case BoundsType.RECT:
                    return CircleVsRect( c, b.rect );
                default:
                    return false;
            }
        }

        public static bool RectVsBounds( RectBounds r, Bounds b ) {
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

    public struct Manifold {
        public Vector2 normal;
        public float penetration;
    }

    public static class CollisionTests {
        public static bool CircleVsConvex( CircleCollider c, ConvexCollider v, out Manifold m ) {
            Vector2 axis = Vector2.Zero;
            float axisDepth = 0;
            float minA = 0;
            float maxA = 0;
            float minB = 0;
            float maxB = 0;

            float depth = float.MaxValue;
            Vector2 normal = Vector2.Zero;

            int vertexCount = v.verts.Length;
            for ( int i = 0; i < vertexCount; i++ ) {
                Vector2 va = v.verts[i];
                Vector2 vb = v.verts[(i + 1) % vertexCount];

                Vector2 edge = vb - va;
                axis = Vector2.Normalize( new Vector2( -edge.Y, edge.X ) );

                ProjectVertices( v.verts, axis, out minA, out maxA );
                ProjectCircle( c, axis, out minB, out maxB );

                if ( minA >= maxB || minB >= maxA ) {
                    m = new Manifold();
                    return false;
                }

                axisDepth = MathF.Min( maxB - minA, maxA - minB );

                if ( axisDepth < depth ) {
                    depth = axisDepth;
                    normal = axis;
                }
            }

            Vector2 cp = v.GetClosestPoint( c.center );

            axis = cp - c.center;
            axis = Vector2.Normalize( axis );

            ProjectVertices( v.verts, axis, out minA, out maxA );
            ProjectCircle( c, axis, out minB, out maxB );

            if ( minA >= maxB || minB >= maxA ) {
                m = new Manifold();
                return false;
            }

            axisDepth = MathF.Min( maxB - minA, maxA - minB );

            if ( axisDepth < depth ) {
                depth = axisDepth;
                normal = axis;
            }

            Vector2 polygonCenter = new Vector2(0.0f, 0.0f);
            for ( int vertexIndex = 0; vertexIndex < vertexCount; ++vertexIndex ) {
                polygonCenter += v.verts[vertexIndex];
            }
            polygonCenter /= (float)vertexCount;

            Vector2 direction = polygonCenter - c.center;

            if ( Vector2.Dot( direction, normal ) < 0.0f ) {
                normal = -normal;
            }

            m = new Manifold();
            m.normal = normal;
            m.penetration = depth;

            return true;
        }

        // Assumes axis is normalized!!
        private static void ProjectVertices( Vector2[] verts, Vector2 axis, out float min, out float max ) {
            min = float.MaxValue;
            max = float.MinValue;

            for ( int i = 0; i < verts.Length; i++ ) {
                Vector2 v = verts[i];
                float proj = Vector2.Dot(v, axis);

                if ( proj < min ) { min = proj; }
                if ( proj > max ) { max = proj; }
            }
        }

        // Assumes axis is normalized!!
        private static void ProjectCircle( CircleCollider circle, Vector2 axis, out float min, out float max ) {
            Vector2 direction = axis;
            Vector2 directionAndRadius = direction * circle.radius;

            Vector2 p1 = circle.center + directionAndRadius;
            Vector2 p2 = circle.center - directionAndRadius;

            min = Vector2.Dot( p1, axis );
            max = Vector2.Dot( p2, axis );

            if ( min > max ) {
                // swap the min and max values.
                float t = min;
                min = max;
                max = t;
            }
        }

    }

}
