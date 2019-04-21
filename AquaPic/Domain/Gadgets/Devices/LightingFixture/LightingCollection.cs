#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2019 Goodtime Development

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/
*/

#endregion // License

using System;
using System.Collections.Generic;
using AquaPic.Globals;
using AquaPic.Service;

namespace AquaPic.Gadgets.Device.Lighting
{
    public class LightingCollection : GenericDeviceCollection
    {
        public static LightingCollection SharedLightingCollectionInstance = new LightingCollection ();

        protected LightingCollection () : base ("lightingFixtures") { }

        public override void ReadAllGadgetsFromFile () {
            var equipmentSettings = SettingsHelper.ReadAllSettingsInArray<LightingFixtureSettings> (gadgetSettingsFileName, gadgetSettingsArrayName);
            foreach (var setting in equipmentSettings) {
                CreateGadget (setting, false);
            }
        }

        protected override GenericGadget GadgetCreater (GenericGadgetSettings settings) {
            var fixtureSettings = settings as LightingFixtureSettings;
            if (fixtureSettings == null) {
                throw new ArgumentException ("Settings must be LightingFixtureSettings");
            }

            LightingFixture fixture;
            if (fixtureSettings.dimmingChannel.IsNotEmpty ()) {
                fixture = new LightingFixtureDimming (fixtureSettings);
            } else {
                fixture = new LightingFixture (fixtureSettings);
            }

            return fixture;
        }

        // Names
        public string[] GetAllDimmingFixtureNames () {
            List<string> names = new List<string> ();
            foreach (var fixture in gadgets.Values) {
                if (fixture is LightingFixtureDimming) {
                    names.Add (fixture.name);
                }
            }
            return names.ToArray ();
        }

        // Dimming Channel
        public IndividualControl GetDimmingChannelIndividualControl (string fixtureName) {
            CheckGadgetKey (fixtureName);
            var fixture = gadgets[fixtureName] as LightingFixtureDimming;
            if (fixture != null) {
                return fixture.dimmingChannel;
            }

            throw new ArgumentException ("fixtureName");
        }

        // High Temperature Lockout
        public bool GetFixtureTemperatureLockout (string fixtureName) {
            CheckGadgetKey (fixtureName);
            var fixture = gadgets[fixtureName] as LightingFixture;
            return fixture.highTempLockout;
        }

        // Check Dimming Fixture
        public bool IsDimmingFixture (string fixtureName) {
            CheckGadgetKey (fixtureName);
            return gadgets[fixtureName] is LightingFixtureDimming;
        }

        // Dimming Levels
        public float GetCurrentDimmingLevel (string fixtureName) {
            CheckGadgetKey (fixtureName);
            var fixture = gadgets[fixtureName] as LightingFixtureDimming;
            if (fixture != null)
                return fixture.currentDimmingLevel;

            throw new ArgumentException ("fixtureName");
        }

        public float GetAutoDimmingLevel (string fixtureName) {
            CheckGadgetKey (fixtureName);
            var fixture = gadgets[fixtureName] as LightingFixtureDimming;
            if (fixture != null)
                return fixture.autoDimmingLevel;

            throw new ArgumentException ("fixtureName");
        }

        public float GetRequestedDimmingLevel (string fixtureName) {
            CheckGadgetKey (fixtureName);
            var fixture = gadgets[fixtureName] as LightingFixtureDimming;
            if (fixture != null)
                return fixture.requestedDimmingLevel;

            throw new ArgumentException ("fixtureName");
        }

        public void SetDimmingLevel (string fixtureName, float level) {
            CheckGadgetKey (fixtureName);
            var fixture = gadgets[fixtureName] as LightingFixtureDimming;
            if (fixture != null) {
                if (fixture.dimmingMode == Mode.Manual)
                    fixture.requestedDimmingLevel = level;
                return;
            }

            throw new ArgumentException ("fixtureName");
        }

        // Dimming Modes
        public Mode GetDimmingMode (string fixtureName) {
            CheckGadgetKey (fixtureName);
            var fixture = gadgets[fixtureName] as LightingFixtureDimming;
            if (fixture != null)
                return fixture.dimmingMode;

            throw new ArgumentException ("fixtureName");
        }

        public void SetDimmingMode (string fixtureName, Mode mode) {
            CheckGadgetKey (fixtureName);
            var fixture = gadgets[fixtureName] as LightingFixtureDimming;
            if (fixture != null) {
                fixture.dimmingMode = mode;
                return;
            }

            throw new ArgumentException ("fixtureName");
        }

        // Lighting States
        public LightingState[] GetLightingFixtureLightingStates (string fixtureName) {
            CheckGadgetKey (fixtureName);
            var fixture = gadgets[fixtureName] as LightingFixture;
            var lightingStates = new List<LightingState> ();
            foreach (var state in fixture.lightingStates) {
                lightingStates.Add (new LightingState (state));
            }
            return lightingStates.ToArray ();
        }

        public void SetLightingFixtureLightingStates (string fixtureName, LightingState[] lightingStates, bool temporaryChange = true) {
            CheckGadgetKey (fixtureName);
            var fixture = gadgets[fixtureName] as LightingFixtureDimming;
            fixture.UpdateLightingStates (lightingStates, temporaryChange);
            if (!temporaryChange) {
                UpdateGadgetSettingsInFile (fixtureName);
            }
        }
    }
}
