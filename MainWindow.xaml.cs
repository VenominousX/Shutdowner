using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Threading;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;

namespace Shutdowner {
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    

    public partial class MainWindow : Window {

        static string Instant, FullShut, AdvancedReboot;
        static long time;
        Duration dur;
        DoubleAnimation Animation;

        public MainWindow() {
            InitializeComponent();

            this.Left = System.Windows.SystemParameters.WorkArea.Width - this.Width;
            this.Top = System.Windows.SystemParameters.WorkArea.Height - this.Height;

            Instant = $" /t ";
            FullShut = " /hybrid ";
            AdvancedReboot = " /s ";

        }

        private void InstantShutdown_Click(object sender, RoutedEventArgs e) {
            if(InstantShutdown.IsChecked == true) {
                ElementsOff(InputBox, InputDescription);
            }
            else {
                ElementsOn(InputBox, InputDescription);
                if(AdvancedBoot.IsChecked == true)
                    AdvancedBoot.IsChecked = false;
            }
        }
        private void FullShutdown_Click(object sender, RoutedEventArgs e) {
            FullShut = FullShutdown.IsChecked == true ? "" : " /hybrid ";
        }
        private void AdvancedBoot_Click(object sender, RoutedEventArgs e) {
            if(AdvancedBoot.IsChecked == true) {
                FullShutdown.IsChecked = true;
                InstantShutdown.IsChecked = true;
                AdvancedReboot = " /r /o";
                FullShut = "";
                ElementsOff(InputBox, InputDescription);
            }
            else {
                FullShutdown.IsChecked = false;
                InstantShutdown.IsChecked = false;
                AdvancedReboot = " /s ";
                Instant = " /t ";
                FullShut = " /hybrid ";
                ElementsOn(InputBox, InputDescription);
            }
        }

        private void Close_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            Close();
        }

        private void AcceptOperation_Click(object sender, RoutedEventArgs e) {
            string Command = $"{AdvancedReboot} {FullShut} {Instant}";
            var eingabe = InputBox.Text;

            if(!long.TryParse(eingabe, out time) && InstantShutdown.IsChecked == false) {
                if(eingabe.Contains(':')) {
                    var currentTime = DateTime.Now;
                    if(DateTime.TryParse(eingabe, out DateTime targetTime)) {
                        if(targetTime.Hour <= currentTime.Hour && targetTime.Minute < currentTime.Minute) {
                            var lastDay = DateTime.DaysInMonth(currentTime.Year, currentTime.Month);
                            targetTime = new DateTime(
                                currentTime.Month == 12 && currentTime.Day == lastDay ? currentTime.Year + 1 : currentTime.Year,
                                currentTime.Day == lastDay ? currentTime.Month + 1 : currentTime.Month,
                                currentTime.Day == lastDay ?  1 : currentTime.Day + 1,
                                targetTime.Hour,
                                targetTime.Minute,
                                0
                                );
                        }
                        time = Convert.ToInt64(Math.Ceiling(targetTime.Subtract(currentTime).TotalSeconds));
                        ForceOperation(Command + time, time);
                    }
                    else {
                        InputError("inkorrekte Uhrzeit");
                    }
                }
                else {
                    InputError("Zeitwert eingeben");
                }
            }
            else {
                if(AdvancedBoot.IsChecked == false) {
                    time *= 60;
                    ForceOperation(Command + time + " /f", time);
                }
                else {
                    ForceOperation(Command + "0 /f", time);
                }
            }
        }

        private void CancelOperation_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start("shutdown", " /a");
            ProgressPanel.Visibility = Visibility.Collapsed;
            SettingsPanel.Visibility = Visibility.Visible;
            DeleteProgressBar();
        }

        private void InputBox_MouseEnter(object sender, MouseEventArgs e) {
            InputBox.Text = "";
            InputBox.FontSize = 20;
        }

        private void ForceOperation(string Command, long time) {
            if(System.Diagnostics.Process.Start("shutdown", Command) != null) {
                SettingsPanel.Visibility = Visibility.Collapsed;
                ProgressPanel.Visibility = Visibility.Visible;
                ProgressPanelText.Text = $"Shutdown um {DateTime.Now.AddSeconds(time):HH:mm}";
                LoadProgressBar();
            }
        }

        private void LoadProgressBar() {
            dur = new Duration(TimeSpan.FromSeconds(time));
            Animation = new DoubleAnimation( 0, 100, dur);
            pb.BeginAnimation(ProgressBar.ValueProperty, Animation);
            pb.IsIndeterminate = false;
        }

        private void DeleteProgressBar() {
            Animation = null;
        }

        private void InputError(string s) {
            InputBox.FontSize = 14;
            InputBox.Text = s;
        }

        private void ElementsOff(TextBox tbox, TextBlock tblock) {
            tbox.Text = "";
            tbox.Visibility = Visibility.Collapsed;
            tblock.Visibility = Visibility.Collapsed;
        }

        private void ElementsOn(TextBox tbox, TextBlock tblock) {
            tbox.Visibility = Visibility.Visible;
            tblock.Visibility = Visibility.Visible;
        }
    }
}
