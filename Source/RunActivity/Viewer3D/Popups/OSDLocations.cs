﻿// COPYRIGHT 2014 by the Open Rails project.
// 
// This file is part of Open Rails.
// 
// Open Rails is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Open Rails is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Open Rails.  If not, see <http://www.gnu.org/licenses/>.

// This file is the responsibility of the 3D & Environment Team. 

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ORTS.Common;
using System;
using System.Collections.Generic;

namespace Orts.Viewer3D.Popups
{
    public class OSDLocations : LayeredWindow
    {
        Matrix Identity = Matrix.Identity;

        internal const float MaximumDistancePlatform = 1000;
        internal const float MaximumDistanceSiding = 500;
        internal const float MinimumDistance = 100;

        public enum DisplayState
        {
            Platforms = 0x1,
            Sidings = 0x2,
            All = 0x3,
        }
        DisplayState State = DisplayState.All;

        Dictionary<TrItemLabel, LabelPrimitive> Labels = new Dictionary<TrItemLabel, LabelPrimitive>();

        public OSDLocations(WindowManager owner)
            : base(owner, 0, 0, "OSD Locations")
        {
        }

        public override bool Interactive
        {
            get
            {
                return false;
            }
        }

        public override void TabAction()
        {
            if (State == DisplayState.All) State = DisplayState.Platforms;
            else if (State == DisplayState.Platforms) State = DisplayState.Sidings;
            else if (State == DisplayState.Sidings) State = DisplayState.All;
        }

        public override void PrepareFrame(RenderFrame frame, ORTS.Common.ElapsedTime elapsedTime, bool updateFull)
        {
            if (updateFull)
            {
                var labels = Labels;
                var newLabels = new Dictionary<TrItemLabel, LabelPrimitive>(labels.Count);
                var worldFiles = Owner.Viewer.World.Scenery.WorldFiles;
                var cameraLocation = Owner.Viewer.Camera.CameraWorldLocation;
                foreach (var worldFile in worldFiles)
                {
                    if ((State & DisplayState.Platforms) != 0 && worldFile.platforms != null)
                    {
                        foreach (var platform in worldFile.platforms)
                        {
                            // Calculates distance between camera and platform label.
                            var distance = WorldLocation.GetDistance(platform.Location.WorldLocation, cameraLocation).Length();
                            if (distance <= MaximumDistancePlatform)
                            {
                                if (labels.ContainsKey(platform))
                                    newLabels[platform] = labels[platform];
                                else
                                    newLabels[platform] = new LabelPrimitive(Owner.Label3DMaterial, Color.Yellow, Color.Black, 0) { Position = platform.Location, Text = platform.ItemName };

                                // Change color with distance.
                                var ratio = (MathHelper.Clamp(distance, MinimumDistance, MaximumDistancePlatform) - MinimumDistance) / (MaximumDistancePlatform - MinimumDistance);
                                newLabels[platform].Color.A = newLabels[platform].Outline.A = (byte)MathHelper.Lerp(255, 0, ratio);
                            }
                        }
                    }

                    if ((State & DisplayState.Sidings) != 0 && worldFile.sidings != null)
                    {
                        foreach (var siding in worldFile.sidings)
                        {
                            // Calculates distance between camera and siding label.
                            var distance = WorldLocation.GetDistance(siding.Location.WorldLocation, cameraLocation).Length();
                            if (distance <= MaximumDistanceSiding)
                            {
                                if (labels.ContainsKey(siding))
                                    newLabels[siding] = labels[siding];
                                else
                                    newLabels[siding] = new LabelPrimitive(Owner.Label3DMaterial, Color.Orange, Color.Black, 0) { Position = siding.Location, Text = siding.ItemName };

                                // Change color with distance.
                                var ratio = (MathHelper.Clamp(distance, MinimumDistance, MaximumDistanceSiding) - MinimumDistance) / (MaximumDistanceSiding - MinimumDistance);
                                newLabels[siding].Color.A = newLabels[siding].Outline.A = (byte)MathHelper.Lerp(255, 0, ratio);
                            }
                        }
                    }
                }
                Labels = newLabels;
            }

            foreach (var primitive in Labels.Values)
                frame.AddPrimitive(Owner.Label3DMaterial, primitive, RenderPrimitiveGroup.Labels, ref Identity);
        }

        public DisplayState CurrentDisplayState
        {
            get
            {
                return State;
            }
        }
    }
}
