using Microsoft.Xaml.Interactivity;
using System;
using System.Globalization;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation.Metadata;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NumberBoxDemo.Behaviors {
    internal class NumberBoxBehavior : Behavior<TextBox> {

        private static string _clipboarText;
        private bool _hasFocus;

        // TODO: Dependency Property
        public string Format { get; set; }
        public NumberStyles NumberStyles { get; set; } = NumberStyles.Any;
        public bool UpdateValueOnChange { get; set; }

        static NumberBoxBehavior() {
            Clipboard.ContentChanged += async (s, e) => {
                var dataPackageView = Clipboard.GetContent();
                if (dataPackageView.Contains(StandardDataFormats.Text)) {
                    _clipboarText = await dataPackageView.GetTextAsync();
                } else {
                    _clipboarText = null;
                }
            };
        }

        #region Value Dependency Property

        /// <summary> 
        /// Get or Sets the Value dependency property.  
        /// </summary> 
        public double? Value {
            get { return (double?)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        /// <summary> 
        /// Identifies the Value dependency property. This enables animation, styling, binding, etc...
        /// </summary> 
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value),
                                        typeof(double?),
                                        typeof(NumberBoxBehavior),
                                        new PropertyMetadata(default(double?), OnValuePropertyChanged));

        /// <summary>
        /// Value changed handler. 
        /// </summary>
        /// <param name="d">NumberBoxBehavior that changed its Value.</param>
        /// <param name="e">DependencyPropertyChangedEventArgs.</param> 
        private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var source = d as NumberBoxBehavior;
            if (source != null && !source._hasFocus) {
                var value = (double?)e.NewValue;
                var ci = source.GetCultureInfo();
                if (value.HasValue) {
                    source.AssociatedObject.Text = value.Value.ToString(source.Format, ci);
                } else {
                    source.AssociatedObject.Text = string.Empty;
                }
            }
        }

        #endregion Value Dependency Property

        protected override void OnAttached() {
            base.OnAttached();

            // TODO: Turn SpellChecking off?
            this.AssociatedObject.IsSpellCheckEnabled = false;

            if (ApiInformation.IsEventPresent(typeof(UIElement).FullName, nameof(UIElement.PreviewKeyDown))) {
                this.AssociatedObject.PreviewKeyDown += this.AssociatedObject_PreviewKeyDown;
            }
            if (ApiInformation.IsEventPresent(typeof(TextBox).FullName, nameof(TextBox.Paste))) {
                this.AssociatedObject.Paste += this.AssociatedObject_Paste;
            }
            this.AssociatedObject.GotFocus += this.AssociatedObject_GotFocus;
            this.AssociatedObject.LostFocus += this.AssociatedObject_LostFocus;
            if (UpdateValueOnChange) {
                this.AssociatedObject.TextChanged += this.AssociatedObject_TextChanged;
            }
        }

        protected override void OnDetaching() {
            base.OnDetaching();
            if (ApiInformation.IsEventPresent(typeof(UIElement).FullName, nameof(UIElement.PreviewKeyDown))) {
                this.AssociatedObject.PreviewKeyDown -= this.AssociatedObject_PreviewKeyDown;
            }
            if (ApiInformation.IsEventPresent(typeof(TextBox).FullName, nameof(TextBox.Paste))) {
                this.AssociatedObject.Paste -= this.AssociatedObject_Paste;
            }
            this.AssociatedObject.GotFocus -= this.AssociatedObject_GotFocus;
            this.AssociatedObject.LostFocus -= this.AssociatedObject_LostFocus;
            this.AssociatedObject.TextChanged -= this.AssociatedObject_TextChanged;
        }

        private void AssociatedObject_TextChanged(object sender, TextChangedEventArgs e) {
            var ci = GetCultureInfo();
            if (double.TryParse(this.AssociatedObject.Text, this.NumberStyles, ci, out var v)) {
                this.Value = v;
            } else {
                SetNullValue();
            }
        }

        private void SetNullValue() {
            try {
                this.Value = null;
            } catch (InvalidOperationException) {
                // Value is DataBound to a non Nullable Value
                this.Value = 0;
            }
        }

        private void AssociatedObject_GotFocus(object sender, RoutedEventArgs e) => _hasFocus = true;

        private void AssociatedObject_LostFocus(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            _hasFocus = false;
            var ci = GetCultureInfo();
            if (double.TryParse(this.AssociatedObject.Text, this.NumberStyles, ci, out var v)) {
                if (UpdateValueOnChange) {
                    this.AssociatedObject.Text = v.ToString(this.Format, ci);
                } else {
                    this.Value = v;
                }
            } else {
                SetNullValue();
            }
        }

        private void AssociatedObject_Paste(object sender, TextControlPasteEventArgs e) {
            if (_clipboarText != null) {
                var newText = GetNewText(_clipboarText);
                var ci = GetCultureInfo();
                if (!double.TryParse(newText, this.NumberStyles, ci, out var v)) {
                    e.Handled = true;
                }
            }
        }

        private void AssociatedObject_PreviewKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e) {
            string txt = null;
            var isShift = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down);
            var isControl = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down);
            if (isControl) {
                // always allowed
                return;
            }

            switch (e.Key) {
                case (Windows.System.VirtualKey)188: // Comma
                    txt = ",";
                    break;
                case (Windows.System.VirtualKey)189: // Minus
                    txt = "-";
                    break;
                case VirtualKey.Decimal:
                case (Windows.System.VirtualKey)190: // Dot
                    txt = ".";
                    break;

                case VirtualKey.LeftButton:
                case VirtualKey.RightButton:
                case VirtualKey.Cancel:
                case VirtualKey.Back:
                case VirtualKey.Tab:
                case VirtualKey.Clear:
                case VirtualKey.Enter:
                case VirtualKey.Shift:
                case VirtualKey.Control:
                case VirtualKey.Menu:
                case VirtualKey.Pause:
                case VirtualKey.CapitalLock:
                case VirtualKey.Kana:
                case VirtualKey.Junja:
                case VirtualKey.Final:
                case VirtualKey.Hanja:
                case VirtualKey.Escape:
                case VirtualKey.Convert:
                case VirtualKey.NonConvert:
                case VirtualKey.Accept:
                case VirtualKey.ModeChange:
                case VirtualKey.Space:
                case VirtualKey.PageUp:
                case VirtualKey.PageDown:
                case VirtualKey.End:
                case VirtualKey.Home:
                case VirtualKey.Left:
                case VirtualKey.Up:
                case VirtualKey.Right:
                case VirtualKey.Down:
                case VirtualKey.Select:
                case VirtualKey.Print:
                case VirtualKey.Execute:
                case VirtualKey.Snapshot:
                case VirtualKey.Insert:
                case VirtualKey.Delete:
                case VirtualKey.Help:
                    break;
                case VirtualKey.Number0:
                case VirtualKey.Number1:
                case VirtualKey.Number2:
                case VirtualKey.Number3:
                case VirtualKey.Number4:
                case VirtualKey.Number5:
                case VirtualKey.Number6:
                case VirtualKey.Number7:
                case VirtualKey.Number8:
                case VirtualKey.Number9:
                    if (isShift) {
                        e.Handled = true;
                    }
                    txt = (e.Key - VirtualKey.Number0).ToString();
                    break;
                case VirtualKey.NumberPad0:
                case VirtualKey.NumberPad1:
                case VirtualKey.NumberPad2:
                case VirtualKey.NumberPad3:
                case VirtualKey.NumberPad4:
                case VirtualKey.NumberPad5:
                case VirtualKey.NumberPad6:
                case VirtualKey.NumberPad7:
                case VirtualKey.NumberPad8:
                case VirtualKey.NumberPad9:
                    txt = (e.Key - VirtualKey.NumberPad0).ToString();
                    break;
                case VirtualKey.Multiply:
                case VirtualKey.Add:
                case VirtualKey.Separator:
                case VirtualKey.Subtract:
                case VirtualKey.Divide:
                case VirtualKey.F1:
                case VirtualKey.F2:
                case VirtualKey.F3:
                case VirtualKey.F4:
                case VirtualKey.F5:
                case VirtualKey.F6:
                case VirtualKey.F7:
                case VirtualKey.F8:
                case VirtualKey.F9:
                case VirtualKey.F10:
                case VirtualKey.F11:
                case VirtualKey.F12:
                case VirtualKey.F13:
                case VirtualKey.F14:
                case VirtualKey.F15:
                case VirtualKey.F16:
                case VirtualKey.F17:
                case VirtualKey.F18:
                case VirtualKey.F19:
                case VirtualKey.F20:
                case VirtualKey.F21:
                case VirtualKey.F22:
                case VirtualKey.F23:
                case VirtualKey.F24:
                case VirtualKey.NavigationView:
                case VirtualKey.NavigationMenu:
                case VirtualKey.NavigationUp:
                case VirtualKey.NavigationDown:
                case VirtualKey.NavigationLeft:
                case VirtualKey.NavigationRight:
                case VirtualKey.NavigationAccept:
                case VirtualKey.NavigationCancel:
                case VirtualKey.NumberKeyLock:
                case VirtualKey.Scroll:
                case VirtualKey.LeftShift:
                case VirtualKey.RightShift:
                case VirtualKey.LeftControl:
                case VirtualKey.RightControl:
                case VirtualKey.LeftMenu:
                case VirtualKey.RightMenu:
                case VirtualKey.GoBack:
                case VirtualKey.GoForward:
                case VirtualKey.Refresh:
                case VirtualKey.Stop:
                case VirtualKey.Search:
                case VirtualKey.Favorites:
                case VirtualKey.GoHome:
                case VirtualKey.GamepadA:
                case VirtualKey.GamepadB:
                case VirtualKey.GamepadX:
                case VirtualKey.GamepadY:
                case VirtualKey.GamepadRightShoulder:
                case VirtualKey.GamepadLeftShoulder:
                case VirtualKey.GamepadLeftTrigger:
                case VirtualKey.GamepadRightTrigger:
                case VirtualKey.GamepadDPadUp:
                case VirtualKey.GamepadDPadDown:
                case VirtualKey.GamepadDPadLeft:
                case VirtualKey.GamepadDPadRight:
                case VirtualKey.GamepadMenu:
                case VirtualKey.GamepadView:
                case VirtualKey.GamepadLeftThumbstickButton:
                case VirtualKey.GamepadRightThumbstickButton:
                case VirtualKey.GamepadLeftThumbstickUp:
                case VirtualKey.GamepadLeftThumbstickDown:
                case VirtualKey.GamepadLeftThumbstickRight:
                case VirtualKey.GamepadLeftThumbstickLeft:
                case VirtualKey.GamepadRightThumbstickUp:
                case VirtualKey.GamepadRightThumbstickDown:
                case VirtualKey.GamepadRightThumbstickRight:
                case VirtualKey.GamepadRightThumbstickLeft:
                    break;
                default:
                    e.Handled = true;
                    return;
            }

            if (txt != null) {
                var newText = GetNewText(txt);
                var ci = GetCultureInfo();
                if (!double.TryParse(newText, this.NumberStyles, ci, out _)) {
                    e.Handled = true;
                }
            }
        }

        private string GetNewText(string txt) {
            var text = this.AssociatedObject.Text;
            var left = text.Substring(0, this.AssociatedObject.SelectionStart);
            var sel = this.AssociatedObject.SelectedText;
            var right = text.Substring(this.AssociatedObject.SelectionStart + this.AssociatedObject.SelectionLength);
            var newText = left + txt + right;
            return newText;
        }

        private CultureInfo GetCultureInfo() => this.AssociatedObject.Language != null ? CultureInfo.GetCultureInfo(this.AssociatedObject.Language) : CultureInfo.CurrentCulture;

    }
}
