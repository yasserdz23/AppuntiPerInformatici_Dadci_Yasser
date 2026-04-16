using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AppuntiPerInformatici_Dadci_Yasser
{
    public class Appunto
    {
        public string Titolo { get; set; }
        public string Contenuto { get; set; }
    }

    public partial class MainWindow : Window
    {
        string categoriaAttuale = "GestioneFile";

        public MainWindow()
        {
            InitializeComponent();

            // Collegamento categorie
            btnGestioneFile.Click += (s, e) => Seleziona("GestioneFile");
            btnWPF.Click += (s, e) => Seleziona("WPF");
            btnDelegate.Click += (s, e) => Seleziona("Delegate");
            btnDizionari.Click += (s, e) => Seleziona("Dizionari");
            btnCode.Click += (s, e) => Seleziona("Code");
            btnPile.Click += (s, e) => Seleziona("Pile");

            btnSalva.Click += btnSalva_Click;
            btnRecupera.Click += btnRecupera_Click;
            btnIndietro.Click += btnIndietro_Click;
        }

        private void Seleziona(string nome)
        {
            categoriaAttuale = nome;
            lblStato.Text = "Categoria: " + categoriaAttuale;
            if (pnlLettura.Visibility == Visibility.Visible) btnRecupera_Click(null, null);
        }

        private void btnSalva_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitolo.Text))
            {
                MessageBox.Show("Inserisci un titolo!");
                return;
            }

            try
            {
                string nomeFile = categoriaAttuale + ".txt";
                string titoloNuovo = txtTitolo.Text.Trim();
                string contenutoPulito = txtContenuto.Text.Replace(Environment.NewLine, "§").Replace("\n", "§").Replace("\r", "§");
                string nuovaRiga = titoloNuovo + "|" + contenutoPulito;

                List<string> linee = new List<string>();

                // 1. Se il file esiste, leggiamo le note esistenti
                if (File.Exists(nomeFile))
                {
                    linee = File.ReadAllLines(nomeFile).ToList();
                }

                // 2. Cerchiamo se esiste già una nota con lo stesso titolo
                // Usiamo lo split per confrontare solo la parte del titolo (prima del '|')
                int indiceEsistente = linee.FindIndex(l => l.Split('|')[0].Equals(titoloNuovo, StringComparison.OrdinalIgnoreCase));

                if (indiceEsistente != -1)
                {
                    // Se esiste, la sostituiamo (Modifica)
                    linee[indiceEsistente] = nuovaRiga;
                    lblStato.Text = "Nota aggiornata!";
                }
                else
                {
                    // Se non esiste, la aggiungiamo (Nuovo inserimento)
                    linee.Add(nuovaRiga);
                    lblStato.Text = "Nuova nota salvata!";
                }

                // 3. Sovrascriviamo il file con la lista aggiornata
                File.WriteAllLines(nomeFile, linee);

                // Pulizia campi
                txtTitolo.Clear();
                txtContenuto.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errore durante il salvataggio: " + ex.Message);
            }
        }

        private void btnRecupera_Click(object sender, RoutedEventArgs e)
        {
            lstArchivio.Items.Clear();
            string nomeFile = categoriaAttuale + ".txt";

            if (File.Exists(nomeFile))
            {
                var righe = File.ReadAllLines(nomeFile);
                foreach (var riga in righe)
                {
                    var parti = riga.Split('|');
                    if (parti.Length >= 2)
                        lstArchivio.Items.Add(new Appunto { Titolo = parti[0], Contenuto = parti[1].Replace("§", Environment.NewLine) });
                }
                pnlScrittura.Visibility = Visibility.Collapsed;
                pnlLettura.Visibility = Visibility.Visible;
            }
            else { MessageBox.Show("Nessun appunto trovato."); }
        }

        // --- ELIMINA ---
        private void btnElimina_Click(object sender, RoutedEventArgs e)
        {
            if (lstArchivio.SelectedItem == null)
            {
                MessageBox.Show("Seleziona l'appunto da eliminare cliccandoci sopra.");
                return;
            }

            Appunto daRimuovere = (Appunto)lstArchivio.SelectedItem;
            if (MessageBox.Show($"Eliminare '{daRimuovere.Titolo}'?", "Conferma", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                string nomeFile = categoriaAttuale + ".txt";
                var linee = File.ReadAllLines(nomeFile).ToList();
                var rigaDaTogliere = linee.FirstOrDefault(l => l.Split('|')[0] == daRimuovere.Titolo);

                if (rigaDaTogliere != null)
                {
                    linee.Remove(rigaDaTogliere);
                    File.WriteAllLines(nomeFile, linee);
                    lstArchivio.Items.Remove(daRimuovere);
                    lblStato.Text = "Eliminato.";
                }
            }
        }

        // --- APRI (DOPPIO CLICK) ---
        private void lstArchivio_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstArchivio.SelectedItem != null)
            {
                Appunto selezionato = (Appunto)lstArchivio.SelectedItem;
                txtTitolo.Text = selezionato.Titolo;
                txtContenuto.Text = selezionato.Contenuto;

                pnlLettura.Visibility = Visibility.Collapsed;
                pnlScrittura.Visibility = Visibility.Visible;
                lblStato.Text = "Modifica in corso...";
            }
        }

        private void btnIndietro_Click(object sender, RoutedEventArgs e)
        {
            pnlLettura.Visibility = Visibility.Collapsed;
            pnlScrittura.Visibility = Visibility.Visible;
        }

        private void lstArchivio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstArchivio.SelectedItem != null)
                lblStato.Text = "Selezionato: " + ((Appunto)lstArchivio.SelectedItem).Titolo;
        }
    }
}