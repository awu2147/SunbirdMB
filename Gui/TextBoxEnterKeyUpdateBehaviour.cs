using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace SunbirdMB.Gui
{
    // https://stackoverflow.com/questions/23613171/wpf-how-to-make-textbox-lose-focus-after-hitting-enter
    public class TextBoxEnterKeyUpdateBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            if (this.AssociatedObject != null)
            {
                base.OnAttached();
                this.AssociatedObject.KeyDown += AssociatedObject_KeyDown;
            }
        }

        protected override void OnDetaching()
        {
            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.KeyDown -= AssociatedObject_KeyDown;
                base.OnDetaching();
            }
        }

        private void AssociatedObject_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                if (e.Key == Key.Return)
                {
                    if (e.Key == Key.Enter)
                    {
                        textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                        System.Windows.Input.Keyboard.ClearFocus();
                    }
                }
            }
        }
    }

}
