using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Kate_s_DLL_Injector
{
    partial class Style
    {
        public Style()
        {
            InitializeComponent();
        }

        private void StatusBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (System.Windows.Input.Mouse.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                ((Window)((FrameworkElement)sender).TemplatedParent).DragMove();
            }
            e.Handled = true;
        }
    }
}
