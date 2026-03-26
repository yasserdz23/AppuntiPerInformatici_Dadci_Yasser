using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace AppuntiPerInformatici_Dadci_Yasser
{
    // Classe per gestire i dati nella ListBox
    public class Appunto
    {
        public string Titolo { get; set; }
        public string Contenuto { get; set; }
    }

    public partial class MainWindow : Window
    {
        // Categoria di default
        string categoriaAttuale = "GestioneFile";

        public MainWindow()
        {
            InitializeComponent();

            // Collegamento rapido dei bottoni categorie tramite Lambda
            btnGestioneFile.Click += (s, e) => Seleziona("GestioneFile");
            btnWPF.Click += (s, e) => Seleziona("WPF");
            btnDelegate.Click += (s, e) => Seleziona("Delegate");
            btnDizionari.Click += (s, e) => Seleziona("Dizionari");
            btnCode.Click += (s, e) => Seleziona("Code");
            btnPile.Click += (s, e) => Seleziona("Pile");

            // Eventi per i tasti principali
            btnSalva.Click += btnSalva_Click;
            btnRecupera.Click += btnRecupera_Click;
            btnIndietro.Click += btnIndietro_Click;
        }

        // Metodo per cambiare categoria e aggiornare la label
        private void Seleziona(string nome)
        {
            categoriaAttuale = nome;
            lblStato.Text = "Categoria attiva: " + categoriaAttuale;
        }

        // SCRITTURA (StreamWriter) - Gestisce anche gli "Invio"
        private void btnSalva_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitolo.Text))
            {
                MessageBox.Show("Inserisci almeno il titolo!");
                return;
            }

            string nomeFile = categoriaAttuale + ".txt";

            try
            {
                using (StreamWriter sw = new StreamWriter(nomeFile, true))
                {
                    // Trasformiamo i ritorni a capo in un simbolo speciale '§' 
                    // così l'intero appunto sta su una sola riga nel file .txt
                    string contenutoPulito = txtContenuto.Text.Replace(Environment.NewLine, "§").Replace("\n", "§").Replace("\r", "§");

                    sw.WriteLine(txtTitolo.Text + "|" + contenutoPulito);
                }

                txtTitolo.Clear();
                txtContenuto.Clear();
                lblStato.Text = "Appunto salvato in " + nomeFile;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errore durante il salvataggio: " + ex.Message);
            }
        }

        // LETTURA (StreamReader) - Ricostruisce le righe originali
        private void btnRecupera_Click(object sender, RoutedEventArgs e)
        {
            lstArchivio.Items.Clear();
            string nomeFile = categoriaAttuale + ".txt";

            if (File.Exists(nomeFile))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(nomeFile))
                    {
                        string riga;
                        while ((riga = sr.ReadLine()) != null)
                        {
                            string[] parti = riga.Split('|');
                            if (parti.Length >= 2)
                            {
                                // Ripristiniamo gli "Invio" originali sostituendo '§' con il ritorno a capo
                                string testoOriginale = parti[1].Replace("§", Environment.NewLine);

                                lstArchivio.Items.Add(new Appunto
                                {
                                    Titolo = parti[0].ToUpper(),
                                    Contenuto = testoOriginale
                                });
                            }
                        }
                    }
                    // Cambio visualizzazione: nascondo editor, mostro lista
                    pnlScrittura.Visibility = Visibility.Collapsed;
                    pnlLettura.Visibility = Visibility.Visible;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Errore durante la lettura: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Nessun appunto trovato per " + categoriaAttuale);
            }
        }

        // Torna alla schermata di inserimento
        private void btnIndietro_Click(object sender, RoutedEventArgs e)
        {
            pnlLettura.Visibility = Visibility.Collapsed;
            pnlScrittura.Visibility = Visibility.Visible;
        }

        // Quando clicchi un appunto nella lista, lo riapre nell'editor per leggerlo tutto
        private void lstArchivio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstArchivio.SelectedItem != null)
            {
                Appunto selezionato = (Appunto)lstArchivio.SelectedItem;

                // Riporto titolo e contenuto (con i suoi a capo) nei TextBox
                txtTitolo.Text = selezionato.Titolo;
                txtContenuto.Text = selezionato.Contenuto;

                // Torno alla modalità scrittura per permettere la lettura completa
                pnlLettura.Visibility = Visibility.Collapsed;
                pnlScrittura.Visibility = Visibility.Visible;

                // Resetto la selezione della lista
                lstArchivio.SelectedItem = null;
            }
        }
    }
}