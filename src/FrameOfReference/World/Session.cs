/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.ComponentModel;
using AlphaFramework.World;
using NanoByte.Common;

#if NETFRAMEWORK
using System.Linq;
using System.Xml.Serialization;
using FrameOfReference.World.Positionables;
using LuaInterface;
#endif

namespace FrameOfReference.World;

/// <summary>
/// Represents a game session (i.e. a game actually being played).
/// It is equivalent to the content of a savegame.
/// </summary>
public sealed partial class Session : SessionBase<Universe>
{
#if NETFRAMEWORK
    /// <summary>
    /// The scripting engine used to execute story scripts.
    /// </summary>
    [XmlIgnore]
    public Lua Lua { get; set; }
#endif

    /// <summary>
    /// Creates a new game session based upon a given <see cref="Universe"/>.
    /// </summary>
    /// <param name="baseUniverse">The universe to base the new game session on.</param>
    public Session(Universe baseUniverse) : base(baseUniverse)
    {}

    /// <summary>The maximum number of seconds to handle in one call to <see cref="Update"/>. Additional time is simply dropped.</summary>
    private const double MaximumUpdate = 0.75;

    /// <summary>
    /// <see cref="UniverseBase{T}.GameTime"/> time left over from the last <see cref="Update"/> call due to the fixed update step size.
    /// </summary>
    [DefaultValue(0.0)]
    public double LeftoverGameTime { get; set; }

    /// <inheritdoc/>
    public override double Update(double elapsedRealTime)
    {
        if (TimeTravelInProgress) return UpdateTimeTravel(elapsedRealTime);

        double elapsedGameTime = elapsedRealTime * TimeWarpFactor;
        double gameTimeDelta = LeftoverGameTime + elapsedGameTime.Clamp(-MaximumUpdate, MaximumUpdate);
        LeftoverGameTime = UpdateDeterministic(gameTimeDelta);
        return elapsedGameTime;
    }

    /// <summary>
    /// Updates the world to a specific point in game time.
    /// </summary>
    /// <param name="gameTime">The target value for <see cref="UniverseBase{TCoordinates}.GameTime"/>.</param>
    public void UpdateTo(double gameTime)
    {
        UpdateDeterministic(gameTime - Universe.GameTime);
    }

    /// <summary>Fixed step size for updates in seconds. Makes updates deterministic.</summary>
    private const double UpdateStepSize = 0.015;

    private double UpdateDeterministic(double gameTimeDelta)
    {
        while (Math.Abs(gameTimeDelta) >= UpdateStepSize)
        {
            // Handle negative time
            double effectiveStep = Math.Sign(gameTimeDelta) * UpdateStepSize;

            Universe.Update(effectiveStep);
#if NETFRAMEWORK
            if (Lua != null && !TimeTravelInProgress) HandleTriggers();
#endif
            gameTimeDelta -= effectiveStep;
        }

        return gameTimeDelta;
    }

#if NETFRAMEWORK
    private void HandleTriggers()
    {
        var playerEntities = Universe.Positionables.OfType<Entity>().Where(x => x.IsPlayerControlled).ToList();

        foreach (var trigger in Universe.Positionables.OfType<Trigger>().Where(x => !x.WasTriggered))
        {
            // Skip triggers with unmet dependencies
            if (!string.IsNullOrEmpty(trigger.DependsOn))
                if (!Universe.GetTrigger(trigger.DependsOn).WasTriggered) continue;

            var targetEntity = playerEntities.FirstOrDefault(x => x.Name == trigger.TargetEntity);
            if (targetEntity != null)
            {
                if (trigger.IsInRange(targetEntity))
                {
                    trigger.WasTriggered = true;
                    if (!string.IsNullOrEmpty(trigger.OnActivation)) Lua.DoString(trigger.OnActivation);
                }
                else if (Universe.GameTime >= trigger.DueTime)
                {
                    trigger.WasTriggered = true;
                    if (!string.IsNullOrEmpty(trigger.OnTimeout)) Lua.DoString(trigger.OnTimeout);
                }
            }
        }
    }
#endif
}
