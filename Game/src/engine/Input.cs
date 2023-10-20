﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game {

    public class EngineInput {
        public bool[] keys = new bool[512];
        public bool[] prevKeys = new bool[512];
    }

    public enum InputKey {
        UNKNOWN = -1,
        SPACE = 0X20,
        APOSTROPHE = 39,
        COMMA = 44,
        MINUS = 45,
        PERIOD = 46,
        SLASH = 47,
        ALPHA0 = 48,
        ALPHA1 = 49,
        ALPHA2 = 50,
        ALPHA3 = 51,
        ALPHA4 = 52,
        ALPHA5 = 53,
        ALPHA6 = 54,
        ALPHA7 = 55,
        ALPHA8 = 56,
        ALPHA9 = 57,
        SEMICOLON = 59,
        EQUAL = 61,
        A = 65,
        B = 66,
        C = 67,
        D = 68,
        E = 69,
        F = 70,
        G = 71,
        H = 72,
        I = 73,
        J = 74,
        K = 75,
        L = 76,
        M = 77,
        N = 78,
        O = 79,
        P = 80,
        Q = 81,
        R = 82,
        S = 83,
        T = 84,
        U = 85,
        V = 86,
        W = 87,
        X = 88,
        Y = 89,
        Z = 90,
        LEFTBRACKET = 91,
        BACKSLASH = 92,
        RIGHTBRACKET = 93,
        GRAVEACCENT = 96,
        WORLD1 = 161,
        WORLD2 = 162,
        ESCAPE = 0X100,
        ENTER = 257,
        TAB = 258,
        BACKSPACE = 259,
        INSERT = 260,
        DELETE = 261,
        RIGHT = 262,
        LEFT = 263,
        DOWN = 264,
        UP = 265,
        PAGEUP = 266,
        PAGEDOWN = 267,
        HOME = 268,
        END = 269,
        CAPSLOCK = 280,
        SCROLLLOCK = 281,
        NUMLOCK = 282,
        PRINTSCREEN = 283,
        PAUSE = 284,
        F1 = 290,
        F2 = 291,
        F3 = 292,
        F4 = 293,
        F5 = 294,
        F6 = 295,
        F7 = 296,
        F8 = 297,
        F9 = 298,
        F10 = 299,
        F11 = 300,
        F12 = 301,
        F13 = 302,
        F14 = 303,
        F15 = 304,
        F16 = 305,
        F17 = 306,
        F18 = 307,
        F19 = 308,
        F20 = 309,
        F21 = 310,
        F22 = 311,
        F23 = 312,
        F24 = 313,
        F25 = 314,
        NUMPAD0 = 320,
        NUMPAD1 = 321,
        NUMPAD2 = 322,
        NUMPAD3 = 323,
        NUMPAD4 = 324,
        NUMPAD5 = 325,
        NUMPAD6 = 326,
        NUMPAD7 = 327,
        NUMPAD8 = 328,
        NUMPAD9 = 329,
        NUMPADDECIMAL = 330,
        NUMPADDIVIDE = 331,
        NUMPADMULTIPLY = 332,
        NUMPADSUBTRACT = 333,
        NUMPADADD = 334,
        NUMPADENTER = 335,
        NUMPADEQUAL = 336,
        LEFTSHIFT = 340,
        LEFTCONTROL = 341,
        LEFTALT = 342,
        LEFTSUPER = 343,
        RIGHTSHIFT = 344,
        RIGHTCONTROL = 345,
        RIGHTALT = 346,
        RIGHTSUPER = 347,
        MENU = 348
    }

}