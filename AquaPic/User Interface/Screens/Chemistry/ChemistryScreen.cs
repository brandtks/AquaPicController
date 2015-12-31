using System;
using System.Collections.Generic;
using System.IO;
using Gtk;
using Cairo;
using MyWidgetLibrary;

using AquaPic.Runtime;

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
            MyBox box1 = new MyBox (780, 395);
            Put (box1, 10, 30);
            box1.Show ();

            string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
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
                new MyColor ("pri"),
                new MyColor ("grey4"),
                0.0f);
            timerProgress.SetSizeRequest (250, 120);
            Put (timerProgress, 275, 100);
            timerProgress.Visible = false;

            timerLabel = new TouchLabel ();
            timerLabel.WidthRequest = 100;
            timerLabel.textAlignment = MyAlignment.Center;
            Put (timerLabel, 350, 170);
            timerLabel.Visible = false;

            stepButton = new TouchButton ();
            stepButton.SetSizeRequest (200, 80);
            stepButton.buttonColor = "grey2";
            stepButton.text = "N/A";
            stepButton.ButtonReleaseEvent += OnStepButtonReleased;
            Put (stepButton, 300, 335);
            stepButton.Show ();

            resetBtn = new TouchButton ();
            resetBtn.SetSizeRequest (200, 80);
            resetBtn.text = "Restart";
            resetBtn.ButtonReleaseEvent += OnResetButtonReleased;
            Put (resetBtn, 510, 335);
            resetBtn.Visible = false;

            skipBtn = new TouchButton ();
            skipBtn.SetSizeRequest (200, 80);
            skipBtn.text = "Skip";
            skipBtn.buttonColor = "seca";
            skipBtn.ButtonReleaseEvent += OnSkipButtonReleased;
            Put (skipBtn, 90, 335);
            skipBtn.Visible = false;

            nameLabel = new TouchLabel ();
            nameLabel.WidthRequest = 300;
            nameLabel.textSize = 13;
            nameLabel.textColor = "pri";
            nameLabel.textAlignment = MyAlignment.Center;
            Put (nameLabel, 250, 40);
            nameLabel.Show ();

            stepLabel = new TouchTextBox ();
            stepLabel.SetSizeRequest (620, 75);
            stepLabel.textAlignment = MyAlignment.Center;
            stepLabel.text = "Please select a test procedure";
            Put (stepLabel, 90, 250);
            stepLabel.Show ();

            actionLabel = new TouchLabel ();
            actionLabel.WidthRequest = 200;
            actionLabel.textAlignment = MyAlignment.Left;
            Put (actionLabel, 20, 385);
            actionLabel.Show ();

            combo = new TouchComboBox ();
            foreach (var test in tests) {
                combo.List.Add (test.name);
            }
            combo.NonActiveMessage = "Select test";
            combo.WidthRequest = 235;
            combo.ChangedEvent += OnComboChanged;
            Put (combo, 550, 35);
            combo.Show ();
                       
            Show ();
        }

        public override void Dispose () {
            GLib.Source.Remove (timerId);
            base.Dispose ();
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
                        timerProgress.progress = 0.0f;
                        timerProgress.backgroundColor = "pri";
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
                stepLabel.text = String.Format ("{0:f2} {1}", tests [testIdx].CalculateResults (), tests [testIdx].unit);
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

            timerProgress.progress = 1.0f - ((float)currentTime / (float)actionOption);
            timerProgress.backgroundColor = new MyColor ("pri").Blend(new MyColor ("compl"), timerProgress.progress);

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

                    var ms = new TouchMessageDialog (
                         "Are you sure you want to quit in the middle of a procedure", 
                         parent);

                    ms.Response += (o, a) => {
                        if (a.ResponseId == ResponseType.Yes) {
                            newIdx = e.Active;
                        }
                    };

                    ms.Run ();
                    ms.Destroy ();
                } else {
                    newIdx = e.Active;
                }
            } else {
                newIdx = e.Active;
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

                    var ms = new TouchMessageDialog (
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