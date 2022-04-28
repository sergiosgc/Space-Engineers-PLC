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
// GENERATED CODE GOES HERE
            readStorage();
        }

        private void readConfig()
        {
            if (Me.CustomData.Length > 0)
            {
                string[] lines = Me.CustomData.Split(new char[] { '\r', '\n' });
                foreach(string line in lines)
                {
                    string[] assignment = line.Split(new char[] { '=' }, 2);
                    if (assignment.Length == 2) petriNet.variables[assignment[0].Trim()] = assignment[1].Trim();
                }
            }
        }

        private void readStorage()
        {
            if (Storage != ""
                && Storage.Split(';').Count() == 3
                && Storage.Split(';')[0] == this.petriNet.hash()
                && Storage.Split(';')[1].Split(',').Count() == this.petriNet.P.Count()
                && Storage.Split(';')[2].Split(',').Count() == this.petriNet.T.Count())
                try
                {
                    int i = 0;
                    foreach (int timerRemaining in Storage.Split(';')[2].Split(',').ToList().ConvertAll((c) => int.Parse(c))) this.petriNet.T[i++].timerRemaining = timerRemaining;
                    i = 0;
                    foreach (int tokenCount in Storage.Split(';')[1].Split(',').ToList().ConvertAll((c) => int.Parse(c))) this.petriNet.P[i++].tokenCount = tokenCount;
                    Runtime.UpdateFrequency = this.petriNet.requiredUpdateFrequency();
                }
                catch (Exception e)
                {
                    Echo("Exception reading storage string: " + e.Message);
                    Storage = "";
                }
        }

        public void Save()
        {
            Storage = this.petriNet.hash()
                      + ";"
                      + String.Join(",", this.petriNet.P.ConvertAll((p) => p.tokenCount.ToString()))
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
                    readConfig();
                    this.petriNet.setMarking(argument);
                    Runtime.UpdateFrequency = this.petriNet.requiredUpdateFrequency();
                    break;
            }
        }
    }
}
