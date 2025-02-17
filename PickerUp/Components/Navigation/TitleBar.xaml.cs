using Microsoft.UI.Xaml.Controls;

namespace PickerUp.Components.Navigation
{
    public sealed partial class TitleBar : UserControl
    {
      public static Grid Root = new();
        public TitleBar()
        {
            InitializeComponent();
            Root = _root;
        }
    }
}
