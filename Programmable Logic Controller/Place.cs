using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class Place
        {
            public string name;
            public Action<PetriNet> onEntry;
            public Action<PetriNet> onTick;
            public Action<PetriNet> onExit;
            public int tokenCount = 0;
            public PetriNet petriNet;

            public Place(String name, Action<PetriNet> entryFunc, Action<PetriNet> tickFunc, Action<PetriNet> exitFunc)
            {
                this.name = name;
                this.onEntry = entryFunc;
                this.onTick = tickFunc;
                this.onExit = exitFunc;
            }
            public void tick(int tickCount) { if (this.onTick != null && this.tokenCount > 0) this.onTick(this.petriNet);  }
            public void enter()
            {
                this.tokenCount++;
                if (this.onEntry != null) this.onEntry(this.petriNet);
            }
            public void exit()
            {
                if (this.tokenCount == 0) return;
                this.tokenCount--;
                if (this.onExit != null) this.onExit(this.petriNet);
            }

        }
    }
}
