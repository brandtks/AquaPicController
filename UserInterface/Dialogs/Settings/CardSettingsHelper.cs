#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Goodtime Development

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
using System.Globalization;
using Newtonsoft.Json.Linq;
using GoodtimeDevelopment.TouchWidget;
using GoodtimeDevelopment.Utilites;
using AquaPic.Service;
using AquaPic.Drivers;
using AquaPic.SerialBus;

namespace AquaPic.UserInterface
{
    public class CardSettingsHelper
    {
        public static bool OnAddressSetEvent (string text, ref string card, GenericBase cardDriver) {
            var keepText = false;
            if (text.IsNotEmpty ()) {
                try {
                    int address;
                    if ((text.StartsWith ("x", StringComparison.InvariantCultureIgnoreCase)) ||
                        (text.StartsWith ("0x", StringComparison.InvariantCultureIgnoreCase))) {
                        var parseString = text.Substring (text.IndexOf ("x", StringComparison.InvariantCultureIgnoreCase) + 1);
                        address = int.Parse (parseString, NumberStyles.HexNumber);
                    } else {
                        address = Convert.ToInt32 (text);
                    }

                    if (!AquaPicBus.SlaveAddressOk (address)) {
                        MessageBox.Show ("Address already exists");
                    } else {
                        var ja = SettingsHelper.OpenSettingsFile ("equipment") as JArray;

                        card = string.Format (
                            "{0}{1}",
                            cardDriver.GetCardAcyronym (),
                            cardDriver.GetLowestCardNameIndex ());

                        cardDriver.AddCard (card, address);

                        var type = cardDriver.GetCardType ().ToString ();
                        type = Char.ToLower (type[0]) + type.Substring (1);
                        var jo = new JObject {
                                    new JProperty ("type", type),
                                    new JProperty ("address", string.Format ("0x{0:X}", address)),
                                    new JProperty ("name", card),
                                    new JProperty ("options", string.Empty)
                                };
                        ja.Add (jo);
                        SettingsHelper.WriteSettingsFile ("equipment", ja);
                        keepText = true;
                    }
                } catch {
                    MessageBox.Show ("Improper address");
                }
            }

            return keepText;
        }

        public static bool OnCardDeleteEvent (string card, GenericBase cardDriver) {
            var ja = SettingsHelper.OpenSettingsFile ("equipment") as JArray;
            var index = SettingsHelper.FindSettingsInArray (ja, card);
            if (index == -1) {
                MessageBox.Show ("Something went wrong");
                return false;
            }

            ja.RemoveAt (index);
            SettingsHelper.WriteSettingsFile ("equipment", ja);
            cardDriver.RemoveCard (card);
            return true;
        }
    }
}

