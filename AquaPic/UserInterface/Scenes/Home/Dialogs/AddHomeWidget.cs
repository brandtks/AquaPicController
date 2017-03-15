﻿using System;
using Cairo;
using Gtk;
using AquaPic.Utilites;
using TouchWidgetLibrary;
using System.Text.RegularExpressions;

namespace AquaPic.UserInterface
{
    partial class HomeSettings
    {
        private class AddHomeWidget : Dialog
        {
            private Fixed fix;
            private TouchButton addBtn;
            private TouchButton cancelButton;
            private TouchComboBox typeCombo;
            private SettingsTextBox nameTextBox;
            private SettingsTextBox groupTextBox;

            public HomeSettingsWidget newWidget;

            public AddHomeWidget () {
                Name = "Add Home Screen Widget";
                Title = "Add Home Screen Widget";
                WindowPosition = (WindowPosition)4;
                SetSizeRequest (600, 110);

#if RPI_BUILD
                Decorated = false;

                ExposeEvent += (o, args) => {
                    using (Context cr = Gdk.CairoHelper.Create (GdkWindow)) {
                        cr.MoveTo (Allocation.Left, Allocation.Top);
                        cr.LineTo (Allocation.Right, Allocation.Top);
                        cr.LineTo (Allocation.Right, Allocation.Bottom);
                        cr.LineTo (Allocation.Left, Allocation.Bottom);
                        cr.ClosePath ();
                        cr.LineWidth = 1.8;
                        TouchColor.SetSource (cr, "grey4");
                        cr.Stroke ();
                    }
                };
#endif

                ModifyBg (StateType.Normal, TouchColor.NewGtkColor ("grey0"));

                foreach (Widget w in Children) {
                    Remove (w);
                    w.Dispose ();
                }

                fix = new Fixed ();
                fix.SetSizeRequest (600, 110);

                addBtn = new TouchButton ();
                addBtn.SetSizeRequest (100, 30);
                addBtn.text = "Add";
                addBtn.ButtonReleaseEvent += OnAddButtonReleased;
                fix.Put (addBtn, 495, 75);
                addBtn.Show ();

                cancelButton = new TouchButton ();
                cancelButton.SetSizeRequest (100, 30);
                cancelButton.text = "Cancel";
                cancelButton.ButtonReleaseEvent += (o, args) => {
                    Destroy ();
                };
                fix.Put (cancelButton, 385, 75);
                cancelButton.Show ();

                var widgetLabel = new TouchLabel ();
                widgetLabel.text = "Home Screen Widgets";
                widgetLabel.textAlignment = TouchAlignment.Right;
                widgetLabel.WidthRequest = 185;
                fix.Put (widgetLabel, 5, 11);
                widgetLabel.Show ();

                nameTextBox = new SettingsTextBox ();
                nameTextBox.text = "Name";
                fix.Put (nameTextBox, 5, 40);
                nameTextBox.Show ();

                groupTextBox = new SettingsTextBox ();
                groupTextBox.text = "Group";
                fix.Put (groupTextBox, 305, 40);
                groupTextBox.Show ();

                typeCombo = new TouchComboBox ();
                typeCombo.comboList.Add ("Line Plot");
                typeCombo.comboList.Add ("Curved Bar Plot");
                typeCombo.comboList.Add ("Button");
                typeCombo.comboList.Add ("Timer");
                typeCombo.comboList.Add ("Bar Plot");
                typeCombo.nonActiveMessage = "Select type";
                typeCombo.WidthRequest = 400;
                typeCombo.maxListHeight = 2;
                fix.Put (typeCombo, 195, 5);
                typeCombo.Show ();

                ExposeEvent += (obj, args) => {
                    typeCombo.Visible = false;
                    typeCombo.Visible = true;
                };

                Add (fix);
                fix.Show ();
            }

            protected void OnAddButtonReleased (object sender, ButtonReleaseEventArgs args) {
                if (typeCombo.activeIndex == -1) {
                    MessageBox.Show ("Please select a widget type");
                    return;
                }

                if (nameTextBox.textBox.text.IsEmpty ()) {
                    MessageBox.Show ("Please enter name of widget");
                    return;
                }

                var type = Regex.Replace (typeCombo.activeText, @"\s+", "");
                var name = nameTextBox.textBox.text;
                var group = groupTextBox.textBox.text;

                newWidget = new HomeSettingsWidget (name, group, type, 0, 0);

                Destroy ();
            }
        }
    }
}