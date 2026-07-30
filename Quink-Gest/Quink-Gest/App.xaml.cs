using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using QuinkGest.Models.Database;

namespace QuinkGest
{
    /// <summary>
    /// Point d'entrée de l'application : crée la base SQL Server "QuinkGest"
    /// (si absente) et ses tables avant l'affichage de la première fenêtre.
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DatabaseHelper.Initialiser();
        }

        /// <summary>Affiche le texte de Tag comme indicatif tant que le champ est vide.</summary>
        private void TextBox_ActualiserIndicatif(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox textBox) return;

            if (textBox.Text.Length == 0 && textBox.Tag is string indicatif && indicatif.Length > 0)
            {
                textBox.Background = new VisualBrush
                {
                    AlignmentX = AlignmentX.Left,
                    AlignmentY = AlignmentY.Center,
                    Stretch = Stretch.None,
                    Visual = new TextBlock
                    {
                        Text = indicatif,
                        Foreground = Brushes.Gray,
                        FontStyle = FontStyles.Italic,
                        Margin = new Thickness(6, 0, 0, 0)
                    }
                };
            }
            else
            {
                textBox.ClearValue(Control.BackgroundProperty);
            }
        }

        /// <summary>Affiche le texte de Tag comme indicatif tant que le mot de passe est vide
        /// (Password n'étant pas une propriété de dépendance, ceci ne peut pas passer par un Trigger XAML).</summary>
        private void PasswordBox_ActualiserIndicatif(object sender, RoutedEventArgs e)
        {
            if (sender is not PasswordBox passwordBox) return;

            if (string.IsNullOrEmpty(passwordBox.Password) && passwordBox.Tag is string indicatif && indicatif.Length > 0)
            {
                passwordBox.Background = new VisualBrush
                {
                    AlignmentX = AlignmentX.Left,
                    AlignmentY = AlignmentY.Center,
                    Stretch = Stretch.None,
                    Visual = new TextBlock
                    {
                        Text = indicatif,
                        Foreground = Brushes.Gray,
                        FontStyle = FontStyles.Italic,
                        Margin = new Thickness(6, 0, 0, 0)
                    }
                };
            }
            else
            {
                passwordBox.ClearValue(Control.BackgroundProperty);
            }
        }
    }
}
