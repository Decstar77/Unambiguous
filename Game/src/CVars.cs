using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game {

    public class BoolCVar {
        public bool Value { get; set; }
        public BoolCVar( bool v ) {
            Value = v;
        }
    }

    public class CVars {
        public static BoolCVar DrawVanishingPoint = new BoolCVar(false);

    }
}
