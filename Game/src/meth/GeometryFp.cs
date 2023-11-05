
using FixMath;
using OpenTK.Mathematics;

namespace Game {
    public struct CircleCollider {
        public Vector2Fp center;
        public F64 radius;

        public CircleCollider( Vector2Fp c, F64 r ) {
            center = c;
            radius = r;
        }

        public CircleCollider Clone() {
            CircleCollider c = new CircleCollider();
            c.center = center;
            c.radius = radius;
            return c;
        }

        public CircleBounds ToBounds() {
            CircleBounds clone = new CircleBounds();
            clone.center = center.ToV2();
            clone.radius = radius.Float;
            return clone;
        }

        public void Translate( Vector2Fp t ) {
            center += t;
        }
    }

    public struct ConvexCollider {
        public Vector2Fp[] verts;

        public ConvexCollider Clone() {
            ConvexCollider c = new ConvexCollider();
            c.verts = new Vector2Fp[verts.Length];
            for ( int i = 0; i < verts.Length; i++ ) {
                c.verts[i] = verts[i];
            }
            return c;
        }

        public Vector2Fp ComputeCenter() {
            Vector2Fp center = Vector2Fp.Zero;
            for ( int i = 0; i < verts.Length; i++ ) {
                center += verts[i];
            }
            return center / F64.FromInt( verts.Length );
        }

        public void Translate( Vector2Fp t ) {
            for ( int i = 0; i < verts.Length; i++ ) {
                verts[i] += t;
            }
        }

        public ConvexBounds ToBounds() {
            ConvexBounds clone = new ConvexBounds();
            clone.verts = new Vector2[verts.Length];
            for ( int i = 0; i < verts.Length; i++ ) {
                clone.verts[i] = verts[i].ToV2();
            }
            return clone;
        }

        public Vector2Fp GetClosestPoint( Vector2Fp p ) {
            F64 minDistance = F64.MaxValue;
            Vector2Fp closePoint = Vector2Fp.Zero;

            for ( int i = 0; i < verts.Length; i++ ) {
                Vector2Fp v = verts[i];
                F64 distance = Vector2Fp.DistanceSqr( v, p );

                if ( distance < minDistance ) {
                    minDistance = distance;
                    closePoint = v;
                }
            }

            return closePoint;
        }

        public void SortPointsIntoClockWiseOrder() {
            Vector2Fp centroid = Vector2Fp.Zero;
            for ( int i = 0; i < verts.Length; i++ ) {
                centroid += verts[i];
            }
            centroid /= F64.FromInt( verts.Length );

            // @HACK(DECLAN): We should use our own sorting algorithms to ensure determinism.
            Array.Sort( verts, ( a, b ) => {
                F64 a1 = F64.Atan2( a.Y - centroid.Y, a.X - centroid.X );
                F64 a2 = F64.Atan2( b.Y - centroid.Y, b.X - centroid.X );
                return a1.CompareTo( a2 );
            } );
        }
    }

    public struct ManifoldFp {
        public Vector2Fp normal;
        public F64 penetration;
    }

    public static class CollisionTestsFp {
        public static bool CircleVsConvex( CircleCollider c, ConvexCollider v, out ManifoldFp m ) {
            Vector2Fp axis = Vector2Fp.Zero;
            F64 axisDepth = F64.Zero;
            F64 minA = F64.Zero;
            F64 maxA = F64.Zero;
            F64 minB = F64.Zero;
            F64 maxB = F64.Zero;

            F64 depth = F64.MaxValue;
            Vector2Fp normal = Vector2Fp.Zero;

            int vertexCount = v.verts.Length;
            for ( int i = 0; i < vertexCount; i++ ) {
                Vector2Fp va = v.verts[i];
                Vector2Fp vb = v.verts[(i + 1) % vertexCount];

                Vector2Fp edge = vb - va;
                axis = Vector2Fp.Normalize( new Vector2Fp( -edge.Y, edge.X ) );

                ProjectVertices( v.verts, axis, out minA, out maxA );
                ProjectCircle( c, axis, out minB, out maxB );

                if ( minA >= maxB || minB >= maxA ) {
                    m = new ManifoldFp();
                    return false;
                }

                axisDepth = F64.Min( maxB - minA, maxA - minB );

                if ( axisDepth < depth ) {
                    depth = axisDepth;
                    normal = axis;
                }
            }

            Vector2Fp cp = v.GetClosestPoint( c.center );

            axis = cp - c.center;
            axis = Vector2Fp.Normalize( axis );

            ProjectVertices( v.verts, axis, out minA, out maxA );
            ProjectCircle( c, axis, out minB, out maxB );

            if ( minA >= maxB || minB >= maxA ) {
                m = new ManifoldFp();
                return false;
            }

            axisDepth = F64.Min( maxB - minA, maxA - minB );

            if ( axisDepth < depth ) {
                depth = axisDepth;
                normal = axis;
            }

            Vector2Fp polygonCenter = Vector2Fp.Zero;
            for ( int vertexIndex = 0; vertexIndex < vertexCount; ++vertexIndex ) {
                polygonCenter += v.verts[vertexIndex];
            }
            polygonCenter /= F64.FromInt( vertexCount );

            Vector2Fp direction = polygonCenter - c.center;

            if ( Vector2Fp.Dot( direction, normal ) < F64.Zero ) {
                normal = -normal;
            }

            m = new ManifoldFp();
            m.normal = normal;
            m.penetration = depth;

            return true;
        }

        // Assumes axis is normalized!!
        private static void ProjectVertices( Vector2Fp[] verts, Vector2Fp axis, out F64 min, out F64 max ) {
            min = F64.MaxValue;
            max = F64.MinValue;

            for ( int i = 0; i < verts.Length; i++ ) {
                Vector2Fp v = verts[i];
                F64 proj = Vector2Fp.Dot(v, axis);

                if ( proj < min ) { min = proj; }
                if ( proj > max ) { max = proj; }
            }
        }

        // Assumes axis is normalized!!
        private static void ProjectCircle( CircleCollider circle, Vector2Fp axis, out F64 min, out F64 max ) {
            Vector2Fp direction = axis;
            Vector2Fp directionAndRadius = direction * circle.radius;

            Vector2Fp p1 = circle.center + directionAndRadius;
            Vector2Fp p2 = circle.center - directionAndRadius;

            min = Vector2Fp.Dot( p1, axis );
            max = Vector2Fp.Dot( p2, axis );

            if ( min > max ) {
                // swap the min and max values.
                F64 t = min;
                min = max;
                max = t;
            }
        }
    }


}
