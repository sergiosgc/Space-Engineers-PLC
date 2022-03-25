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
    partial class Program : MyGridProgram
    {
        public PetriNet petriNet;

        public Program()
        {
            this.petriNet = new PetriNet(this);
            this.petriNet.debug = true;
            this.petriNet.variables["piston"] = "TestPiston";
            this.petriNet.addPlace( new Place[] {
                new Place("init", null, null, null),
                new Place("extending", (PetriNet p) => p.blocks<IMyExtendedPistonBase>("$piston", (piston) => piston.Velocity = (float) 1), null, null),
                new Place("retracting", (PetriNet p) => p.blocks<IMyExtendedPistonBase>("$piston", (piston) => piston.Velocity = (float) -1), null, null),
            });
            this.petriNet.addTransition(new Transition[]
            {
                new Transition(this.petriNet.getPlace( new string[] { "init" }), this.petriNet.getPlace( new string[] { "extending" }), null),
                new Transition(this.petriNet.getPlace( new string[] { "extending" }), this.petriNet.getPlace( new string[] { "retracting" }), (PetriNet p) => p.blocks<IMyExtendedPistonBase>("$piston")[0].CurrentPosition > (float) 9.9),
                new Transition(this.petriNet.getPlace( new string[] { "retracting" }), this.petriNet.getPlace( new string[] { "extending" }), (PetriNet p) => p.blocks<IMyExtendedPistonBase>("$piston")[0].CurrentPosition < (float) 0.1)
            });
            this.petriNet.addMarking("main", new string[] { "init" });
            if (Storage != "" 
                && Storage.Split(';').Count() == 2
                && Storage.Split(';')[0].Split(',').Count() == this.petriNet.P.Count()
                && Storage.Split(';')[1].Split(',').Count() == this.petriNet.T.Count())
            {
                int i = 0;
                foreach (int tokenCount in Storage.Split(';')[0].Split(',').ToList().ConvertAll((c) => int.Parse(c))) this.petriNet.P[i++].tokenCount = tokenCount;
                i = 0;
                foreach (int timerRemaining in Storage.Split(';')[1].Split(',').ToList().ConvertAll((c) => int.Parse(c))) this.petriNet.T[i++].timerRemaining = timerRemaining;
                Runtime.UpdateFrequency = this.petriNet.requiredUpdateFrequency();
            }
        }

        public void Save()
        {
            Storage = String.Join(",", this.petriNet.P.ConvertAll((p) => p.tokenCount.ToString()))
                      + ";"
                      + String.Join(",", this.petriNet.T.ConvertAll((t) => t.timerRemaining.ToString()));
        }

        public void Main(string argument, UpdateType updateSource)
        {
            switch (updateSource)
            {
                case UpdateType.Update1:
                case UpdateType.Update10:
                case UpdateType.Update100:
                case UpdateType.Once:
                    this.petriNet.tick(updateSource == UpdateType.Update10 ? 10 : updateSource == UpdateType.Update100 ? 100 : 1);
                    break;
                default:
                    this.petriNet.setMarking(argument);
                    Runtime.UpdateFrequency = this.petriNet.requiredUpdateFrequency();
                    break;
            }
        }
    }
}
