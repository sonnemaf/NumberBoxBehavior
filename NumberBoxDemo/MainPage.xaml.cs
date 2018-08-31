using NumberBoxDemo.ViewModels;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace NumberBoxDemo {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page {

        internal MainViewModel VM { get; set; } = new MainViewModel();

        public MainPage() {
            this.InitializeComponent();
        }

        public double? ToNullableDouble(int value) {
            return new double?(value);
        }

        public int ToNullableInt(double? value) {
            if (value.HasValue) {
                var v = (int)value.Value;
                return v;
            }
            return 0;
        }
    }
}
