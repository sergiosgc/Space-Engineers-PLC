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
        public class PetriNet
        {
            public List<Place> P = new List<Place>();
            public List<Transition> T = new List<Transition>();
            public Dictionary<String, int[]> markings = new Dictionary<string, int[]>();
            public Dictionary<String, String> variables = new Dictionary<String, String>();
            public UpdateFrequency updateFrequency = UpdateFrequency.None;
            public bool debug = false;
            public Dictionary<String, Object> memoizedBlock = new Dictionary<string, object>();
            public Dictionary<String, Object> memoizedBlocks = new Dictionary<string, Object>();
            int ticksToResetMemoizedBlocks = 1000;

            public Program program;
            public PetriNet(Program program)
            {
                this.program = program;
            }
            public void addPlace(Place[] ps) { foreach (Place p in ps) this.addPlace(p);  }
            public void addPlace(Place p)
            {
                p.petriNet = this;
                this.P.Add(p);
            }
            public void addTransition(Transition[] ts) { foreach (Transition t in ts) this.addTransition(t); }
            public void addTransition(Transition t) { this.T.Add(t);  }
            public void setMarking(String name) { this.setMarking(this.markings[name]); }
            public void setMarking(int[] marking) { 
                for (int i = 0; i < marking.Count(); i++) this.P[i].tokenCount = marking[i];
                foreach (Transition t in T) t.resetTimer();
            }
            public void blockMoveTo(IMyMotorStator block, float toDeg, float speedRPM)
            {
                block.Enabled = true;
                block.RotorLock = false;
                float toRad = toDeg / 180 * ((float)Math.PI);
                speedRPM = Math.Abs(speedRPM);
                float currentAngle = block.Angle;
                if (currentAngle > Math.PI) currentAngle -= 2 * ((float)Math.PI);
                if (block.UpperLimitRad < currentAngle) block.UpperLimitRad = currentAngle;
                if (block.LowerLimitRad > currentAngle) block.LowerLimitRad = currentAngle;
                if (currentAngle < toRad)
                {
                    block.UpperLimitRad = toRad;
                    block.TargetVelocityRPM = speedRPM;
                } else if (currentAngle > toRad)
                {
                    block.LowerLimitRad = toRad;
                    block.TargetVelocityRPM = -speedRPM;
                } else
                {
                    block.UpperLimitRad = toRad;
                    block.LowerLimitRad = toRad;
                    block.TargetVelocityRPM = 0;
                }
            }
            public void blockMoveTo(IMyPistonBase block, float to, float velocity)
            {
                velocity = Math.Abs(velocity);
                block.Enabled = true;
                if (block.CurrentPosition < to)
                {
                    block.Velocity = velocity;
                }
                else if (block.CurrentPosition > to)
                {
                    block.Velocity = -velocity;
                }
                else block.Velocity = 0;
            }
            public bool blockPositionIs(IMyMotorStator block, float posDeg, float precision = (float)0.1)
            {
                float currentAngle = block.Angle;
                if (currentAngle > Math.PI) currentAngle -= 2 * ((float)Math.PI);
                return Math.Abs(currentAngle * 180 / ((float) Math.PI) - posDeg) < Math.Abs(precision);
            }
            public bool blockPositionIs(IMyPistonBase block, float pos, float precision = (float) 0.1)
            {
                return Math.Abs(block.CurrentPosition - pos) < Math.Abs(precision);
            }
            internal Place[] getPlace(string[] places)
            {
                return places.ToList<string>().ConvertAll((p) => this.getPlace(p)).ToArray();
            }
            private Place getPlace(string name)
            {
                foreach (Place p in this.P) if (p.name == name) return p;
                throw new KeyNotFoundException();
            }
            internal void addMarking(string name, string[] tokenPlaces)
            {
                this.markings[name] = new int[this.P.Count];
                for (int i = 0; i < this.P.Count; i++) this.markings[name][i] = tokenPlaces.Contains(this.P[i].name) ? 1 : 0;
            }
            public string value(String val)
            {
                return (val.Length > 0 && val[0] == '$') ? this.value(this.variables[val.Substring(1)]) : val;
            }
            public List<T> blocks<T>(String name, Action<T> apply = null) where T : IMyTerminalBlock
            {
                name = this.value(name);
                if (!this.memoizedBlocks.ContainsKey(name))
                {
                    List<IMyTerminalBlock> temp = new List<IMyTerminalBlock>();
                    IMyBlockGroup group = program.GridTerminalSystem.GetBlockGroupWithName(name);
                    if (group == null) throw new Exception("Block group " + name + " not found in grid");
                    group.GetBlocks(temp);
                    List<T> result = new List<T>();
                    foreach(IMyTerminalBlock b in temp) if (b is T) result.Add((T) b);
                    this.memoizedBlocks.Add(name, result);
                }
                if (apply != null) foreach (T block in (List<T>) this.memoizedBlocks[name]) apply(block);
                return (List<T>)this.memoizedBlocks[name];
            }
            public T block<T>(String name) where T : IMyTerminalBlock
            {
                name = this.value(name);
                if (!this.memoizedBlock.ContainsKey(name))
                {
                    IMyTerminalBlock block = this.program.GridTerminalSystem.GetBlockWithName(name);
                    if (block == null) throw new Exception("Block " + name + " not found in grid");
                    if (!(block is T)) throw new Exception("Block " + name + " is not of type " + typeof(T).Name + ", it is of type " + block.GetType().Name);
                    this.memoizedBlock.Add(name, block);
                }
                return (T)this.memoizedBlock[name];
            }
            public void tick(int tickCount)
            {
                this.ticksToResetMemoizedBlocks -= tickCount;
                if (this.ticksToResetMemoizedBlocks <= 0)
                {
                    this.memoizedBlock.Clear();
                    this.memoizedBlocks.Clear();
                    this.ticksToResetMemoizedBlocks = 1000;
                }
                foreach (Place p in this.P) p.tick(tickCount);
                foreach (Transition t in this.T) if (t.tick(tickCount)) this.updateFrequency = UpdateFrequency.None;
                this.program.Runtime.UpdateFrequency = this.requiredUpdateFrequency();
                if (this.debug)
                {
                    this.program.Echo("--- tick ----");
                    foreach (Place p in this.P) if (p.tokenCount > 0) this.program.Echo(" - " + p.name + ":" + p.tokenCount.ToString());
                }
            }
            public UpdateFrequency requiredUpdateFrequency()
            {
                if (this.updateFrequency != UpdateFrequency.None) return this.updateFrequency;
                this.updateFrequency = UpdateFrequency.Update100;
                foreach (Transition t in this.T) switch(t.requiredUpdateFrequency())
                    {
                        case UpdateFrequency.Update1:
                            return this.updateFrequency = UpdateFrequency.Update1;
                        case UpdateFrequency.Update10:
                            this.updateFrequency = UpdateFrequency.Update10;
                            break;
                    }
                return this.updateFrequency;
            }
        }
    }
}
