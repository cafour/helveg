using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using Helveg.Render;

namespace Helveg.Landscape
{
    public class LTurtle<TKind>
        where TKind : Enum
    {
        public LTurtle(
            TKind push,
            TKind pop,
            TKind yawChange,
            TKind pitchChange,
            TKind rollChange,
            ImmutableDictionary<TKind, LDraw> drawRules)
        {
            Push = push;
            Pop = pop;
            YawChange = yawChange;
            PitchChange = pitchChange;
            RollChange = rollChange;
            DrawRules = drawRules;
        }

        public TKind Push { get; }
        public TKind Pop { get; }
        public TKind YawChange { get; }
        public TKind PitchChange { get; }
        public TKind RollChange { get; }
        public ImmutableDictionary<TKind, LDraw> DrawRules { get; }

        public void Draw(
            ImmutableArray<LSymbol<TKind>> sentence,
            WorldBuilder world,
            Point3 worldPosition,
            Quaternion orientation)
        {
            var queue = new Queue<(Quaternion orientation, Point3 position, int index)>();
            queue.Enqueue((orientation, worldPosition, 0));
            while (queue.Count != 0)
            {
                var state = queue.Dequeue();
                for (; state.index < sentence.Length; ++state.index)
                {
                    var current = sentence[state.index];

                    void AdjustOrientation(float yaw, float pitch, float roll)
                    {
                        state.orientation *= Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
                    }

                    if (current.Kind.Equals(Push))
                    {
                        state.index++;
                        queue.Enqueue(state);
                        int nest = 1;

                        // skip over the entire nested part
                        for (int i = state.index; i < sentence.Length; ++i)
                        {
                            if (sentence[i].Kind.Equals(Push))
                            {
                                nest++;
                            }
                            else if (sentence[i].Kind.Equals(Pop))
                            {
                                nest--;
                                if (nest == 0)
                                {
                                    state.index = i;
                                    break;
                                }
                            }
                        }
                        if (nest != 0)
                        {
                            throw new InvalidOperationException($"Push at {state.index} is missing a Pop.");
                        }
                    }
                    else if (current.Kind.Equals(Pop))
                    {
                        // continue to the next state in the queue
                        break;
                    }
                    else if (current.Kind.Equals(YawChange))
                    {
                        AdjustOrientation(current.Parameters[0], 0, 0);
                    }
                    else if (current.Kind.Equals(PitchChange))
                    {
                        AdjustOrientation(0, current.Parameters[0], 0);
                    }
                    else if (current.Kind.Equals(RollChange))
                    {
                        AdjustOrientation(0, 0, current.Parameters[0]);
                    }
                    else if (DrawRules.TryGetValue(current.Kind, out var rule))
                    {
                        var forwardQ = state.orientation * new Quaternion(Vector3.UnitZ, 0)
                                * Quaternion.Conjugate(state.orientation);
                        var forward = new Vector3(forwardQ.X, forwardQ.Y, forwardQ.Z);
                        forward /= forward.Length(); // this is necessary for some reason
                        state.position = rule(
                            state: new LTurtleState
                            {
                                Position = state.position,
                                Orientation = state.orientation,
                                Forward = forward,
                                Parameters = current.Parameters
                            },
                            world: world);
                    }
                }
            }
        }
    }
}
