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
        public class Transition
        {
            public Place[] inFlows;
            public Place[] outFlows;
            public Func<PetriNet, bool> enabledTest;
            public int timer;
            public int timerRemaining;
            public UpdateFrequency updateFrequency;

            public Transition(Place[] inFlows, Place[] outFlows, Func<PetriNet, bool> enabledTest, int timer = 1, UpdateFrequency updateFrequency = UpdateFrequency.Update100)
            {
                if (inFlows.Count() == 0 && outFlows.Count() == 0) throw new Exception("A transition must have at least one inFlow or one outFlow");
                this.inFlows = inFlows;
                this.outFlows = outFlows;
                this.enabledTest = enabledTest;
                this.timer = this.timerRemaining = timer;
                this.updateFrequency = updateFrequency;
            }
            public PetriNet petriNet()
            {
                return (this.inFlows.Count() > 0 ? this.inFlows : this.outFlows)[0].petriNet;
            }
            public bool enabled()
            {
                foreach (Place p in this.inFlows) if (p.tokenCount == 0) return false;
                if (this.enabledTest == null) return true;
                return this.enabledTest(this.petriNet());
            }
            public UpdateFrequency requiredUpdateFrequency()
            {
                foreach (Place p in this.inFlows) if (p.tokenCount == 0) return UpdateFrequency.Update100;
                return this.updateFrequency;
            }
            public bool tick(int tickCount)
            {
                if (!this.enabled()) return false;
                this.timerRemaining -= tickCount;
                if (this.timerRemaining > 0) return false;
                this.timerRemaining = this.timer;
                foreach (Place p in this.inFlows) p.exit();
                foreach (Place p in this.outFlows) p.enter();
                return true;
            }
        }
    }
}
