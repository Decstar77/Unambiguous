using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Tests {
    [TestClass()]
    public class MapActionSelectionGatherResourceTests {
        [TestMethod()]
        public void CheckType() {
            MapActionSelectionGatherResource action = new MapActionSelectionGatherResource();
            Assert.AreEqual( MapActionType.SELECTION_GATHER_RESOURCE, action.GetMapActionType() );
        }

        [TestMethod()]
        public void CheckSerializaiton() {
            MapActionSelectionGatherResource action = new MapActionSelectionGatherResource();
            action.entityIds = new EntityId[13];
            for( int i = 0; i < action.entityIds.Length; i++ ) {
                action.entityIds[i] = new EntityId();
                action.entityIds[i].index = i;
                action.entityIds[i].generation = i * 9384 - 309;
            }
            action.resourceNodeId = EntityId.INVALID;


            List<byte> data = new List<byte>();
            action.Plonk( data );

            MapActionSelectionGatherResource action2 = new MapActionSelectionGatherResource();
            int offset = 0;
            action2.Scoop( data.ToArray(), ref offset );

            Assert.AreEqual( action.entityIds.Length, action2.entityIds.Length );

            for( int i = 0; i < action.entityIds.Length; i++ ) {
                Assert.AreEqual( action.entityIds[i].index, action2.entityIds[i].index );
                Assert.AreEqual( action.entityIds[i].generation, action2.entityIds[i].generation );
            }
        }
    }
}