using System;
using System.Collections.Generic;
using System.IO;
using Gtk;
using Cairo;
using TouchWidgetLibrary;
using AquaPic.Runtime;
using AquaPic.Utilites;

namespace AquaPic.UserInterface
{
    public class ChemistryWindow : WindowBase
    {
        List<TestProcedure> tests;
        TouchComboBox combo;
        TouchButton stepButton;
        TouchButton resetBtn;
        TouchButton skipBtn;
        TouchLabel nameLabel;
        TouchTextBox stepLabel;
        TouchLabel actionLabel;
        TouchCurvedProgressBar timerProgress;
        TouchLabel timerLabel;
        int actionOption;
        int currentTime;
        int testIdx;
        uint timerId;
        bool enableStepButton;
        bool timerRunning;

        public ChemistryWindow (params object[] options) : base () {
            screenTitle = "Chemistry";

            string path = System.IO.Path.Combine (Utils.AquaPicEnvironment, "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "TestProcedures");

            DirectoryInfo d = new DirectoryInfo (path);
            FileInfo[] files = d.GetFiles ("*.json");

            testIdx = -1;
            tests = new List<TestProcedure> ();
            foreach (var file in files) {
                try {
                    tests.Add (new TestProcedure (file.FullName));
                } catch (Exception ex) {
                    Logger.AddError (ex.ToString ());
                }
            }

            timerProgress = new TouchCurvedProgressBar (
                new TouchColor ("grey3"),
                new TouchColor ("pri"),
                100.0f);
            timerProgress.SetSizeRequest (250, 175);
            timerProgress.curveStyle = CurveStyle.ThreeQuarterCurve;
            Put (timerProgress, 275, 100);
            timerProgress.Visible = false;

            timerLabel = new TouchLabel ();
            timerLabel.WidthRequest = 200;
            timerLabel.textAlignment = TouchAlignment.Center;
            timerLabel.textSize = 20;
            Put (timerLabel, 300, 240);
            timerLabel.Visible = false;

            stepButton = new TouchButton ();
            stepButton.SetSizeRequest (200, 50);
            stepButton.buttonColor = "grey2";
            stepButton.text = "N/A";
            stepButton.ButtonReleaseEvent += OnStepButtonReleased;
            Put (stepButton, 300, 365);
            stepButton.Show ();

            resetBtn = new TouchButton ();
            resetBtn.SetSizeRequest (200, 50);
            resetBtn.text = "Restart";
            resetBtn.ButtonReleaseEvent += OnResetButtonReleased;
            Put (resetBtn, 510, 365);
            resetBtn.Visible = false;

            skipBtn = new TouchButton ();
            skipBtn.SetSizeRequest (200, 50);
            skipBtn.text = "Skip";
            skipBtn.buttonColor = "seca";
            skipBtn.ButtonReleaseEvent += OnSkipButtonReleased;
            Put (skipBtn, 90, 365);
            skipBtn.Visible = false;

            nameLabel = new TouchLabel ();
            nameLabel.WidthRequest = 700;
            nameLabel.textSize = 14;
            nameLabel.textColor = "seca";
            nameLabel.textAlignment = TouchAlignment.Center;
            Put (nameLabel, 50, 65);
            nameLabel.Show ();

            stepLabel = new TouchTextBox ();
            stepLabel.SetSizeRequest (620, 75);
            stepLabel.textAlignment = TouchAlignment.Center;
            stepLabel.text = "Please select a test procedure";
            Put (stepLabel, 90, 280);
            stepLabel.Show ();

            actionLabel = new TouchLabel ();
            actionLabel.WidthRequest = 200;
            actionLabel.textAlignment = TouchAlignment.Right;
            Put (actionLabel, 510, 420);
            actionLabel.Show ();

            combo = new TouchComboBox ();
            foreach (var test in tests) {
                combo.comboList.Add (test.name);
            }
            combo.nonActiveMessage = "Select test";
            combo.WidthRequest = 235;
            combo.ComboChangedEvent += OnComboChanged;
            Put (combo, 550, 35);
            combo.Show ();

            CanFocus = true;
            KeyPressEvent += EntryKeyPressEvent;

            ExposeEvent += (o, args) => {
                GrabFocus ();
            };
                       
            Show ();
        }

        public override void Dispose () {
            GLib.Source.Remove (timerId);
            base.Dispose ();
        }

        void EntryKeyPressEvent (object o, KeyPressEventArgs args) {
            Console.WriteLine("DEBUG: KeyValue: " + args.Event.KeyValue);
            if (args.Event.KeyValue == 32) {
                if (enableStepButton) {
                    OnStepButtonReleased (null, null);
                }
            }
        }

        protected void NextStep () {
            string step, action;
            bool done = tests [testIdx].GetNextStep (out step, out action);
            stepLabel.text = step;
            actionLabel.text = action;

            if (!string.Equals (action, "NOP", StringComparison.InvariantCultureIgnoreCase)) {
                if (action.StartsWith ("Timer", StringComparison.InvariantCultureIgnoreCase)) {
                    try {
                        int start = action.IndexOf ('(');
                        actionOption = Convert.ToInt32 (action.Substring (start + 1, action.Length - 2 - start)) * 5;
                        currentTime = actionOption;
                        stepButton.text = "Start Timer";
                        timerProgress.Visible = true;
                        timerProgress.progress = 100.0f;
                        timerProgress.progressColor = "pri";
                        timerLabel.Visible = true;
                        timerLabel.text = string.Format ("{0:D} secs", currentTime / 5);
                        skipBtn.Visible = true;
                    } catch {
                        Restart ();
                        MessageBox.Show ("Invalid procedure command");
                    }
                } else if (action.StartsWith ("Record", StringComparison.InvariantCultureIgnoreCase)) {
                    try {
                        int start = action.IndexOf ('(');
                        actionOption = Convert.ToInt32 (action.Substring (start + 1, action.Length - 2 - start));
                        if ((actionOption != 1) && (actionOption != 2)) {
                            Restart ();
                            MessageBox.Show ("Invalid procedure command");
                        } else {
                            stepButton.text = "Enter titration level";
                        }
                    } catch {
                        Restart ();
                        MessageBox.Show ("Invalid procedure command");
                    }
                }
            }

            if (done) {
                stepButton.text = "Done";
                stepButton.buttonColor = "compl";
                string result = String.Format ("{0:f2} {1}", tests [testIdx].CalculateResults (), tests [testIdx].unit);
                stepLabel.text = result;
                Logger.Add ("Tested {0}, result is {1}", tests [testIdx].name, result);
            }

            stepLabel.QueueDraw ();
            actionLabel.QueueDraw ();
            stepButton.QueueDraw ();
        }

        protected void Restart () {
            if (testIdx != -1) {
                tests [testIdx].Restart ();

                timerProgress.Visible = false;
                timerLabel.Visible = false;
                resetBtn.Visible = false;
                skipBtn.Visible = false;

                stepButton.text = "Start";
                stepButton.buttonColor = "pri";

                stepLabel.text = "Press start";
                actionLabel.text = string.Empty;

                stepButton.QueueDraw ();
                stepLabel.QueueDraw ();
                actionLabel.QueueDraw ();
            }
        }

        protected bool OnTimer () {
            --currentTime;

            timerProgress.progress = (float)currentTime / (float)actionOption;
            timerProgress.progressColor = new TouchColor ("compl").Blend(new TouchColor ("pri"), timerProgress.progress);

            if ((currentTime % 5) == 0) {
                timerLabel.text = string.Format ("{0:D} secs", currentTime / 5);
            }

            if (currentTime == 0) {
                timerProgress.Visible = false;
                timerLabel.Visible = false;
                skipBtn.Visible = false;
                enableStepButton = true;
                stepButton.text = "Next";
                stepButton.buttonColor = "seca";

                stepButton.QueueDraw ();
                timerProgress.QueueDraw ();
                timerLabel.QueueDraw ();
                skipBtn.QueueDraw ();

                NextStep ();

                timerRunning = false;
            }

            timerLabel.QueueDraw ();
            timerProgress.QueueDraw ();

            return timerRunning;
        }

        protected void OnComboChanged (object sender, ComboBoxChangedEventArgs e) {
            int newIdx = testIdx;

            if (testIdx != -1) {
                if (!tests [testIdx].InProcedure) {
                    var parent = this.Toplevel as Gtk.Window;
                    if (parent != null) {
                        if (!parent.IsTopLevel)
                            parent = null;
                    }

                    var ms = new TouchDialog (
                         "Are you sure you want to quit in the middle of a procedure", 
                         parent);

                    ms.Response += (o, a) => {
                        if (a.ResponseId == ResponseType.Yes) {
                            newIdx = e.activeIndex;
                        }
                    };

                    ms.Run ();
                    ms.Destroy ();
                } else {
                    newIdx = e.activeIndex;
                }
            } else {
                newIdx = e.activeIndex;
            }

            if (newIdx != testIdx) {
                if (testIdx != -1) {
                    tests [testIdx].Restart ();
                }

                testIdx = newIdx;

                nameLabel.text = tests [testIdx].name;
                nameLabel.QueueDraw ();

                resetBtn.Visible = false;
                skipBtn.Visible = false;

                stepButton.text = "Start";
                stepButton.buttonColor = "pri";
                enableStepButton = true;

                stepLabel.text = "Press start";
                actionLabel.text = string.Empty;

                stepButton.QueueDraw ();
                stepLabel.QueueDraw ();
                actionLabel.QueueDraw ();
            }
        }

        protected void OnStepButtonReleased (object sender, ButtonReleaseEventArgs e) {
            if (enableStepButton) {
                if (stepButton.text == "Start Timer") {
                    timerRunning = true;
                    timerId = GLib.Timeout.Add (200, OnTimer);
                    stepButton.text = "Next";
                    stepButton.buttonColor = "grey2";
                    enableStepButton = false;
                    stepButton.QueueDraw ();
                } else if (stepButton.text == "Enter titration level") {
                    var parent = this.Toplevel as Gtk.Window;
                    if (parent != null) {
                        if (!parent.IsTopLevel) {
                            parent = null;
                        }
                    }

                    double number = -1;
                    int failCount = 3;
                    while ((number == -1) && (failCount != 0)) {
                        var t = new TouchNumberInput (false, parent);
                        t.NumberSetEvent += (value) => {
                            try {
                                number = Convert.ToDouble (value);
                            } catch {
                                number = -1;
                            }
                        };
                        t.Run ();
                        t.Destroy ();

                        if (number == -1) {
                            --failCount;
                        }
                    }

                    if (failCount != 0) { 
                        if (actionOption == 1) {
                            tests [testIdx].level1 = number;
                        } else {
                            tests [testIdx].level2 = number;
                        }

                        stepButton.text = "Next";
                        stepButton.buttonColor = "seca";
                        NextStep ();
                    } else {
                        MessageBox.Show ("Too much fail");
                    }
                } else {
                    if (testIdx != -1) {
                        if (tests [testIdx].NotStarted) {
                            stepButton.text = "Next";
                            stepButton.buttonColor = "seca";
                            resetBtn.Visible = true;
                            resetBtn.QueueDraw ();
                        } 

                        if (!tests [testIdx].Done) {
                            NextStep ();
                        }
                    }
                }
            }
        }

        protected void OnResetButtonReleased (object sender, ButtonReleaseEventArgs e) {
            if (testIdx != -1) {
                bool restart = false;

                if (tests [testIdx].Done) {
                    restart = true;
                } else {
                    var parent = this.Toplevel as Gtk.Window;
                    if (parent != null) {
                        if (!parent.IsTopLevel)
                            parent = null;
                    }

                    var ms = new TouchDialog (
                        "Are you sure you want to quit in the middle of a procedure", 
                        parent);

                    ms.Response += (obj, a) => {
                        if (a.ResponseId == ResponseType.Yes) {
                            restart = true;
                        }
                    };

                    ms.Run ();
                    ms.Destroy ();
                }

                if (restart) {
                    Restart ();
                }
            }
        }

        protected void OnSkipButtonReleased (object sender, ButtonReleaseEventArgs e) {
            if (testIdx != -1) {
                if (stepButton.text == "Start Timer") {
                    stepButton.text = "Next";
                    skipBtn.Visible = false;

                    stepButton.QueueDraw ();
                    skipBtn.QueueDraw ();
                    NextStep ();
                } else if (timerRunning) {
                    timerRunning = false;
                    timerProgress.Visible = false;
                    timerLabel.Visible = false;
                    enableStepButton = true;

                    stepButton.text = "Next";
                    stepButton.buttonColor = "seca";
                    skipBtn.Visible = false;

                    stepButton.QueueDraw ();
                    skipBtn.QueueDraw ();

                    NextStep ();
                }
            }
        }
    }
}