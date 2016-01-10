/*
    A function which converges smoothly on a (possibly moving) target value over time.

    Copyright (C) 2016, Andrew Innes.

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
*/

using UnityEngine;
using System.Collections;

/*
 * Plus:
 *      The target value can change over time too, and the algorithm accomodates that without problem.
 *      The algorithm always converges in the fastest possible time.
 *      The algorithm never overshoots, unless that's physically unavoidable.
 *      The algorithm never gets into a degenerative state, where it ends up oscillating around the target.
 *      
 * Minus:
 *      It's more heavyweight than other mathematical interpolators, so probably not suited to particles, etc.
 */

public static class PBInterp
{
    public static float GetAcceleration(float initialPos, float finalPos, float initialSpeed, float maxAccel)
    {
        // Can we get there in one?
        if (Abs(initialSpeed) <= maxAccel)
        {
            float requiredSpeed = finalPos - initialPos;
            if (Abs(requiredSpeed) <= maxAccel)
            {
                float requiredAccel = requiredSpeed - initialSpeed;
                if (Abs(requiredAccel) <= maxAccel)
                {
                    return requiredAccel;
                }
            }
        }

        float result = 0.0f;
        bool flipped = false;
        float restPosNow;
        float shortfallNow;
        float restPosNext;
        bool needToBrakeNext;

        // ensure finalPos > initialPos to simplify calculations.
        if (initialPos > finalPos)
        {
            initialPos = -initialPos;
            finalPos = -finalPos;
            initialSpeed = -initialSpeed;
            flipped = true;
        }

        // are we going in the wrong direction?
        if (initialSpeed < 0.0f)
        {
            // about turn at maximum acceleration
            result = maxAccel;
            goto End;
        }

        // where would we stop if we slammed the brakes on right now?
        restPosNow = GetRestPos(initialPos, initialSpeed, maxAccel);

        // if we would overshoot (or stop within spitting distance of the target) then we need to brake now
        shortfallNow = finalPos - restPosNow;
        if (shortfallNow < maxAccel)
        {
            result = -maxAccel;
            goto End;
        }

        // where would we stop if we accelerated now and started braking next tick?
        restPosNext = GetRestPos(initialPos + initialSpeed + maxAccel, initialSpeed + maxAccel + maxAccel, maxAccel);

        // if we would overshoot then we can't accelerate now
        needToBrakeNext = (finalPos < restPosNext);
        if (needToBrakeNext)
        {
            // can we afford to leave the speed alone this tick?
            if (initialSpeed <= shortfallNow)
            {
                float trickleSpeed = maxAccel * 0.1f;

                // if we're not moving (or doing so very very slowly), then speed up a wee bit
                if (initialSpeed < trickleSpeed)
                {
                    result = trickleSpeed;
                }
                else
                {
                    // keep going at the current speed
                    result = 0.0f;
                }
            }
            else
            {
                result = -maxAccel;
            }
            goto End;
        }

        result = maxAccel;

    End:

        if (flipped)
        {
            result = -result;
        }

        return result;
    }

    static float GetRestPos(float initialPos, float initialSpeed, float maxAccel)
    {
        // Special case (no deceleration).
        if (initialSpeed == 0.0f)
        {
            return initialPos;
        }

        // If we brake at max acceleration, how long would it take to come to rest?
        float timeToStop = initialSpeed / maxAccel;

        // Round up because we can't process fractional ticks.
        float fraction = timeToStop - (float)((int)timeToStop);
        float correction = 0.0f;
        if (fraction > 0.0f)
        {
            correction = 1.0f - fraction;
            timeToStop += correction;
        }

        // Where would we stop?  (sum to N terms of the series).
        int numAccelUnits = (int)(timeToStop * ((timeToStop * 0.5f) + 0.5f));
        float brakingDistance = (timeToStop * initialSpeed) + (numAccelUnits * -maxAccel);
        float restPos = initialPos + brakingDistance;

        // Did we stop before using all of the final acceleration unit?
        if (correction != 0.0f)
        {
            restPos += correction * maxAccel;
        }

        return restPos;
    }

    private static float Abs(float val)
    {
        return val >= 0 ? val : -val;
    }
}
