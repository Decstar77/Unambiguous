//
// FixPointCS
//
// Copyright(c) Jere Sanisalo, Petri Kero
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
using FixPointCS;
using OpenTK.Mathematics;
using System.Runtime.CompilerServices;

namespace FixMath
{
    /// <summary>
    /// Vector2 struct with signed 32.32 fixed point components.
    /// </summary>
    [Serializable]
    public struct Vector2Fp : IEquatable<Vector2Fp>
    {
        // Constants
        public static Vector2Fp Zero     { [MethodImpl(FixedUtil.AggressiveInlining)] get { return new Vector2Fp(Fixed64.Zero, Fixed64.Zero); } }
        public static Vector2Fp One      { [MethodImpl(FixedUtil.AggressiveInlining)] get { return new Vector2Fp(Fixed64.One, Fixed64.One); } }
        public static Vector2Fp Down     { [MethodImpl(FixedUtil.AggressiveInlining)] get { return new Vector2Fp(Fixed64.Zero, Fixed64.Neg1); } }
        public static Vector2Fp Up       { [MethodImpl(FixedUtil.AggressiveInlining)] get { return new Vector2Fp(Fixed64.Zero, Fixed64.One); } }
        public static Vector2Fp Left     { [MethodImpl(FixedUtil.AggressiveInlining)] get { return new Vector2Fp(Fixed64.Neg1, Fixed64.Zero); } }
        public static Vector2Fp Right    { [MethodImpl(FixedUtil.AggressiveInlining)] get { return new Vector2Fp(Fixed64.One, Fixed64.Zero); } }
        public static Vector2Fp AxisX    { [MethodImpl(FixedUtil.AggressiveInlining)] get { return new Vector2Fp(Fixed64.One, Fixed64.Zero); } }
        public static Vector2Fp AxisY    { [MethodImpl(FixedUtil.AggressiveInlining)] get { return new Vector2Fp(Fixed64.Zero, Fixed64.One); } }

        // Raw components
        public long RawX;
        public long RawY;

        // F64 accessors
        public F64 X { get { return F64.FromRaw(RawX); } set { RawX = value.Raw; } }
        public F64 Y { get { return F64.FromRaw(RawY); } set { RawY = value.Raw; } }

        public Vector2Fp(F64 x, F64 y)
        {
            RawX = x.Raw;
            RawY = y.Raw;
        }

        // raw ctor only for internal usage
        private Vector2Fp(long x, long y)
        {
            RawX = x;
            RawY = y;
        }

        public static Vector2Fp FromRaw(long rawX, long rawY) { return new Vector2Fp(rawX, rawY); }
        public static Vector2Fp FromInt(int x, int y) { return new Vector2Fp(Fixed64.FromInt(x), Fixed64.FromInt(y)); }
        public static Vector2Fp FromFloat(float x, float y) { return new Vector2Fp(Fixed64.FromFloat(x), Fixed64.FromFloat(y)); }
        public static Vector2Fp FromDouble(double x, double y) { return new Vector2Fp(Fixed64.FromDouble(x), Fixed64.FromDouble(y)); }

        public static Vector2Fp operator -(Vector2Fp a) { return new Vector2Fp(-a.RawX, -a.RawY); }
        public static Vector2Fp operator +(Vector2Fp a, Vector2Fp b) { return new Vector2Fp(a.RawX + b.RawX, a.RawY + b.RawY); }
        public static Vector2Fp operator -(Vector2Fp a, Vector2Fp b) { return new Vector2Fp(a.RawX - b.RawX, a.RawY - b.RawY); }
        public static Vector2Fp operator *(Vector2Fp a, Vector2Fp b) { return new Vector2Fp(Fixed64.Mul(a.RawX, b.RawX), Fixed64.Mul(a.RawY, b.RawY)); }
        public static Vector2Fp operator /(Vector2Fp a, Vector2Fp b) { return new Vector2Fp(Fixed64.DivPrecise(a.RawX, b.RawX), Fixed64.DivPrecise(a.RawY, b.RawY)); }
        public static Vector2Fp operator %(Vector2Fp a, Vector2Fp b) { return new Vector2Fp(a.RawX % b.RawX, a.RawY % b.RawY); }

        public static Vector2Fp operator +(F64 a, Vector2Fp b) { return new Vector2Fp(a.Raw + b.RawX, a.Raw + b.RawY); }
        public static Vector2Fp operator +(Vector2Fp a, F64 b) { return new Vector2Fp(a.RawX + b.Raw, a.RawY + b.Raw); }
        public static Vector2Fp operator -(F64 a, Vector2Fp b) { return new Vector2Fp(a.Raw - b.RawX, a.Raw - b.RawY); }
        public static Vector2Fp operator -(Vector2Fp a, F64 b) { return new Vector2Fp(a.RawX - b.Raw, a.RawY - b.Raw); }
        public static Vector2Fp operator *(F64 a, Vector2Fp b) { return new Vector2Fp(Fixed64.Mul(a.Raw, b.RawX), Fixed64.Mul(a.Raw, b.RawY)); }
        public static Vector2Fp operator *(Vector2Fp a, F64 b) { return new Vector2Fp(Fixed64.Mul(a.RawX, b.Raw), Fixed64.Mul(a.RawY, b.Raw)); }
        public static Vector2Fp operator /(F64 a, Vector2Fp b) { return new Vector2Fp(Fixed64.DivPrecise(a.Raw, b.RawX), Fixed64.DivPrecise(a.Raw, b.RawY)); }
        public static Vector2Fp operator /(Vector2Fp a, F64 b) { return new Vector2Fp(Fixed64.DivPrecise(a.RawX, b.Raw), Fixed64.DivPrecise(a.RawY, b.Raw)); }
        public static Vector2Fp operator %(F64 a, Vector2Fp b) { return new Vector2Fp(a.Raw % b.RawX, a.Raw % b.RawY); }
        public static Vector2Fp operator %(Vector2Fp a, F64 b) { return new Vector2Fp(a.RawX % b.Raw, a.RawY % b.Raw); }

        public static bool operator ==(Vector2Fp a, Vector2Fp b) { return a.RawX == b.RawX && a.RawY == b.RawY; }
        public static bool operator !=(Vector2Fp a, Vector2Fp b) { return a.RawX != b.RawX || a.RawY != b.RawY; }

        public static Vector2Fp Div(Vector2Fp a, F64 b) { long oob = Fixed64.Rcp(b.Raw); return new Vector2Fp(Fixed64.Mul(a.RawX, oob), Fixed64.Mul(a.RawY, oob)); }
        public static Vector2Fp DivFast(Vector2Fp a, F64 b) { long oob = Fixed64.RcpFast(b.Raw); return new Vector2Fp(Fixed64.Mul(a.RawX, oob), Fixed64.Mul(a.RawY, oob)); }
        public static Vector2Fp DivFastest(Vector2Fp a, F64 b) { long oob = Fixed64.RcpFastest(b.Raw); return new Vector2Fp(Fixed64.Mul(a.RawX, oob), Fixed64.Mul(a.RawY, oob)); }
        public static Vector2Fp Div(Vector2Fp a, Vector2Fp b) { return new Vector2Fp(Fixed64.Div(a.RawX, b.RawX), Fixed64.Div(a.RawY, b.RawY)); }
        public static Vector2Fp DivFast(Vector2Fp a, Vector2Fp b) { return new Vector2Fp(Fixed64.DivFast(a.RawX, b.RawX), Fixed64.DivFast(a.RawY, b.RawY)); }
        public static Vector2Fp DivFastest(Vector2Fp a, Vector2Fp b) { return new Vector2Fp(Fixed64.DivFastest(a.RawX, b.RawX), Fixed64.DivFastest(a.RawY, b.RawY)); }
        public static Vector2Fp SqrtPrecise(Vector2Fp a) { return new Vector2Fp(Fixed64.SqrtPrecise(a.RawX), Fixed64.SqrtPrecise(a.RawY)); }
        public static Vector2Fp Sqrt(Vector2Fp a) { return new Vector2Fp(Fixed64.Sqrt(a.RawX), Fixed64.Sqrt(a.RawY)); }
        public static Vector2Fp SqrtFast(Vector2Fp a) { return new Vector2Fp(Fixed64.SqrtFast(a.RawX), Fixed64.SqrtFast(a.RawY)); }
        public static Vector2Fp SqrtFastest(Vector2Fp a) { return new Vector2Fp(Fixed64.SqrtFastest(a.RawX), Fixed64.SqrtFastest(a.RawY)); }
        public static Vector2Fp RSqrt(Vector2Fp a) { return new Vector2Fp(Fixed64.RSqrt(a.RawX), Fixed64.RSqrt(a.RawY)); }
        public static Vector2Fp RSqrtFast(Vector2Fp a) { return new Vector2Fp(Fixed64.RSqrtFast(a.RawX), Fixed64.RSqrtFast(a.RawY)); }
        public static Vector2Fp RSqrtFastest(Vector2Fp a) { return new Vector2Fp(Fixed64.RSqrtFastest(a.RawX), Fixed64.RSqrtFastest(a.RawY)); }
        public static Vector2Fp Rcp(Vector2Fp a) { return new Vector2Fp(Fixed64.Rcp(a.RawX), Fixed64.Rcp(a.RawY)); }
        public static Vector2Fp RcpFast(Vector2Fp a) { return new Vector2Fp(Fixed64.RcpFast(a.RawX), Fixed64.RcpFast(a.RawY)); }
        public static Vector2Fp RcpFastest(Vector2Fp a) { return new Vector2Fp(Fixed64.RcpFastest(a.RawX), Fixed64.RcpFastest(a.RawY)); }
        public static Vector2Fp Exp(Vector2Fp a) { return new Vector2Fp(Fixed64.Exp(a.RawX), Fixed64.Exp(a.RawY)); }
        public static Vector2Fp ExpFast(Vector2Fp a) { return new Vector2Fp(Fixed64.ExpFast(a.RawX), Fixed64.ExpFast(a.RawY)); }
        public static Vector2Fp ExpFastest(Vector2Fp a) { return new Vector2Fp(Fixed64.ExpFastest(a.RawX), Fixed64.ExpFastest(a.RawY)); }
        public static Vector2Fp Exp2(Vector2Fp a) { return new Vector2Fp(Fixed64.Exp2(a.RawX), Fixed64.Exp2(a.RawY)); }
        public static Vector2Fp Exp2Fast(Vector2Fp a) { return new Vector2Fp(Fixed64.Exp2Fast(a.RawX), Fixed64.Exp2Fast(a.RawY)); }
        public static Vector2Fp Exp2Fastest(Vector2Fp a) { return new Vector2Fp(Fixed64.Exp2Fastest(a.RawX), Fixed64.Exp2Fastest(a.RawY)); }
        public static Vector2Fp Log(Vector2Fp a) { return new Vector2Fp(Fixed64.Log(a.RawX), Fixed64.Log(a.RawY)); }
        public static Vector2Fp LogFast(Vector2Fp a) { return new Vector2Fp(Fixed64.LogFast(a.RawX), Fixed64.LogFast(a.RawY)); }
        public static Vector2Fp LogFastest(Vector2Fp a) { return new Vector2Fp(Fixed64.LogFastest(a.RawX), Fixed64.LogFastest(a.RawY)); }
        public static Vector2Fp Log2(Vector2Fp a) { return new Vector2Fp(Fixed64.Log2(a.RawX), Fixed64.Log2(a.RawY)); }
        public static Vector2Fp Log2Fast(Vector2Fp a) { return new Vector2Fp(Fixed64.Log2Fast(a.RawX), Fixed64.Log2Fast(a.RawY)); }
        public static Vector2Fp Log2Fastest(Vector2Fp a) { return new Vector2Fp(Fixed64.Log2Fastest(a.RawX), Fixed64.Log2Fastest(a.RawY)); }
        public static Vector2Fp Sin(Vector2Fp a) { return new Vector2Fp(Fixed64.Sin(a.RawX), Fixed64.Sin(a.RawY)); }
        public static Vector2Fp SinFast(Vector2Fp a) { return new Vector2Fp(Fixed64.SinFast(a.RawX), Fixed64.SinFast(a.RawY)); }
        public static Vector2Fp SinFastest(Vector2Fp a) { return new Vector2Fp(Fixed64.SinFastest(a.RawX), Fixed64.SinFastest(a.RawY)); }
        public static Vector2Fp Cos(Vector2Fp a) { return new Vector2Fp(Fixed64.Cos(a.RawX), Fixed64.Cos(a.RawY)); }
        public static Vector2Fp CosFast(Vector2Fp a) { return new Vector2Fp(Fixed64.CosFast(a.RawX), Fixed64.CosFast(a.RawY)); }
        public static Vector2Fp CosFastest(Vector2Fp a) { return new Vector2Fp(Fixed64.CosFastest(a.RawX), Fixed64.CosFastest(a.RawY)); }

        public static Vector2Fp Pow(Vector2Fp a, F64 b) { return new Vector2Fp(Fixed64.Pow(a.RawX, b.Raw), Fixed64.Pow(a.RawY, b.Raw)); }
        public static Vector2Fp PowFast(Vector2Fp a, F64 b) { return new Vector2Fp(Fixed64.PowFast(a.RawX, b.Raw), Fixed64.PowFast(a.RawY, b.Raw)); }
        public static Vector2Fp PowFastest(Vector2Fp a, F64 b) { return new Vector2Fp(Fixed64.PowFastest(a.RawX, b.Raw), Fixed64.PowFastest(a.RawY, b.Raw)); }
        public static Vector2Fp Pow(F64 a, Vector2Fp b) { return new Vector2Fp(Fixed64.Pow(a.Raw, b.RawX), Fixed64.Pow(a.Raw, b.RawY)); }
        public static Vector2Fp PowFast(F64 a, Vector2Fp b) { return new Vector2Fp(Fixed64.PowFast(a.Raw, b.RawX), Fixed64.PowFast(a.Raw, b.RawY)); }
        public static Vector2Fp PowFastest(F64 a, Vector2Fp b) { return new Vector2Fp(Fixed64.PowFastest(a.Raw, b.RawX), Fixed64.PowFastest(a.Raw, b.RawY)); }
        public static Vector2Fp Pow(Vector2Fp a, Vector2Fp b) { return new Vector2Fp(Fixed64.Pow(a.RawX, b.RawX), Fixed64.Pow(a.RawY, b.RawY)); }
        public static Vector2Fp PowFast(Vector2Fp a, Vector2Fp b) { return new Vector2Fp(Fixed64.PowFast(a.RawX, b.RawX), Fixed64.PowFast(a.RawY, b.RawY)); }
        public static Vector2Fp PowFastest(Vector2Fp a, Vector2Fp b) { return new Vector2Fp(Fixed64.PowFastest(a.RawX, b.RawX), Fixed64.PowFastest(a.RawY, b.RawY)); }

        public static F64 Length(Vector2Fp a) { return F64.FromRaw(Fixed64.Sqrt(Fixed64.Mul(a.RawX, a.RawX) + Fixed64.Mul(a.RawY, a.RawY))); }
        public static F64 LengthFast(Vector2Fp a) { return F64.FromRaw(Fixed64.SqrtFast(Fixed64.Mul(a.RawX, a.RawX) + Fixed64.Mul(a.RawY, a.RawY))); }
        public static F64 LengthFastest(Vector2Fp a) { return F64.FromRaw(Fixed64.SqrtFastest(Fixed64.Mul(a.RawX, a.RawX) + Fixed64.Mul(a.RawY, a.RawY))); }
        public static F64 LengthSqr(Vector2Fp a) { return F64.FromRaw(Fixed64.Mul(a.RawX, a.RawX) + Fixed64.Mul(a.RawY, a.RawY)); }
        public static Vector2Fp Normalize(Vector2Fp a) { F64 ooLen = F64.FromRaw(Fixed64.RSqrt(Fixed64.Mul(a.RawX, a.RawX) + Fixed64.Mul(a.RawY, a.RawY))); return ooLen * a; }
        public static Vector2Fp NormalizeFast(Vector2Fp a) { F64 ooLen = F64.FromRaw(Fixed64.RSqrtFast(Fixed64.Mul(a.RawX, a.RawX) + Fixed64.Mul(a.RawY, a.RawY))); return ooLen * a; }
        public static Vector2Fp NormalizeFastest(Vector2Fp a) { F64 ooLen = F64.FromRaw(Fixed64.RSqrtFastest(Fixed64.Mul(a.RawX, a.RawX) + Fixed64.Mul(a.RawY, a.RawY))); return ooLen * a; }

        public static F64 Dot(Vector2Fp a, Vector2Fp b) { return F64.FromRaw(Fixed64.Mul(a.RawX, b.RawX) + Fixed64.Mul(a.RawY, b.RawY)); }
        public static F64 Distance(Vector2Fp a, Vector2Fp b) { return Length(a - b); }
        public static F64 DistanceFast(Vector2Fp a, Vector2Fp b) { return LengthFast(a - b); }
        public static F64 DistanceFastest(Vector2Fp a, Vector2Fp b) { return LengthFastest(a - b); }

        public static Vector2Fp Min(Vector2Fp a, Vector2Fp b) { return new Vector2Fp(Fixed64.Min(a.RawX, b.RawX), Fixed64.Min(a.RawY, b.RawY)); }
        public static Vector2Fp Max(Vector2Fp a, Vector2Fp b) { return new Vector2Fp(Fixed64.Max(a.RawX, b.RawX), Fixed64.Max(a.RawY, b.RawY)); }

        public static Vector2Fp Clamp(Vector2Fp a, F64 min, F64 max)
        {
            return new Vector2Fp(
                Fixed64.Clamp(a.RawX, min.Raw, max.Raw),
                Fixed64.Clamp(a.RawY, min.Raw, max.Raw));
        }

        public static Vector2Fp Clamp(Vector2Fp a, Vector2Fp min, Vector2Fp max)
        {
            return new Vector2Fp(
                Fixed64.Clamp(a.RawX, min.RawX, max.RawX),
                Fixed64.Clamp(a.RawY, min.RawY, max.RawY));
        }

        public static Vector2Fp Lerp(Vector2Fp a, Vector2Fp b, F64 t)
        {
            long tb = t.Raw;
            long ta = Fixed64.One - tb;
            return new Vector2Fp(
                Fixed64.Mul(a.RawX, ta) + Fixed64.Mul(b.RawX, tb),
                Fixed64.Mul(a.RawY, ta) + Fixed64.Mul(b.RawY, tb));
        }

        public Vector2 ToV2() {
            return new Vector2(Fixed64.ToFloat(RawX), Fixed64.ToFloat(RawY));
        }

        public bool Equals(Vector2Fp other)
        {
            return (this == other);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vector2Fp))
                return false;
            return ((Vector2Fp)obj) == this;
        }

        public override string ToString()
        {
            return "(" + Fixed64.ToString(RawX) + ", " + Fixed64.ToString(RawY) + ")";
        }

        public override int GetHashCode()
        {
            return RawX.GetHashCode() ^ RawY.GetHashCode() * 7919;
        }
    }
}
