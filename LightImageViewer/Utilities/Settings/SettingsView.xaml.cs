using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LightImageViewer.Utilities
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : Window
    {
        /// <summary>
        /// If shift button is pressed
        /// </summary>
        private bool ShiftPressed { get { return (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)); } }

        /// <summary>
        /// If control button is pressed
        /// </summary>
        private bool CtrlPressed { get { return (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)); } }

        /// <summary>
        /// If alt button is pressed
        /// </summary>
        private bool AltPressed { get { return (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)); } }

        public SettingsView()
        {
            DataContext = Settings.Instance;
            InitializeComponent();
        }
        
        private void MyWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (!ShiftPressed && !CtrlPressed && !AltPressed)
                switch (e.Key)
                {
                    case Key.Escape:
                        this.Close();
                        break;
                }
        }
    }
}
