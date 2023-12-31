﻿namespace Game {

    public class BoolCVar {
        public bool Value { get; set; }
        public BoolCVar( bool v ) {
            Value = v;
        }
    }

    public class CVars {
        public static BoolCVar DrawVanishingPoint = new BoolCVar(false);
        public static BoolCVar DrawSelectionBounds = new BoolCVar(false);
        public static BoolCVar DrawColliders = new BoolCVar(false);
        public static BoolCVar DrawPlayerStats = new BoolCVar(false);
        public static BoolCVar DrawGroundGridWorldPoints = new BoolCVar(false);
    }
}
