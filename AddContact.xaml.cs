using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace BaitNotes
{
    public partial class AddContact : Window
    {
        public AddContact()
        {
            InitializeComponent();
        }
        private Scammer? editingScammer = null;

        public AddContact(Scammer scammerToEdit)
        {
            InitializeComponent();

            editingScammer = scammerToEdit;

            NameBox.Text = scammerToEdit.Name;
            NumberBox.Text = scammerToEdit.Number;

            foreach (ComboBoxItem item in StatusBox.Items)
            {
                if (item.Content.ToString()?.Replace(" ", "") == scammerToEdit.Status.ToString())
                {
                    StatusBox.SelectedItem = item;
                    break;
                }
            }

            foreach (ComboBoxItem item in StatusBox_Copy.Items)
            {
                if (item.Content.ToString()?.ToLower() == scammerToEdit.ImageBase64.ToLower())
                {
                    StatusBox_Copy.SelectedItem = item;
                    break;
                }
            }
        }
        private void UpdateScammerInXml(Scammer oldScammer, string newName, string newNumber ,string newStatus, string newImageBase64)
        {
            string path = "scammers.xml";

            if (!File.Exists(path))
                return;

            XDocument doc = XDocument.Load(path);

            XElement? match = doc.Descendants("Scammer")
                .FirstOrDefault(x =>
                    ((string?)x.Element("Name") ?? "") == oldScammer.Name &&
                    ((string?)x.Element("Number") ?? "") == oldScammer.Number
                );

            if (match != null)
            {
                match.SetElementValue("Name", newName);
                match.SetElementValue("Number", newNumber);
                match.SetElementValue("Status", newStatus);
                match.SetElementValue("ImageBase64", newImageBase64);

                doc.Save(path);
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string name = NameBox.Text;
            string number = NumberBox.Text;

            string status = "";
            string imageBase64 = "";

            if (StatusBox.SelectedItem is ComboBoxItem item)
            {
                status = item.Content.ToString();
            }

            if (StatusBox_Copy.SelectedItem is ComboBoxItem colorItem)
            {
                imageBase64 = colorItem.Content.ToString().ToLower();
            }

            if (editingScammer != null)
            {
                UpdateScammerInXml(editingScammer, name, number, status, imageBase64);
                MessageBox.Show("Contact saved. Close the main window to refresh.");
                this.Close();
            }
            else
            {
                string path = "scammers.xml";

                XDocument doc;

                if (File.Exists(path))
                {
                    doc = XDocument.Load(path);
                }
                else
                {
                    doc = new XDocument(
                        new XElement("Scammers")
                    );
                }

                XElement newExample = new XElement("Scammer",
                    new XElement("Name", name),
                    new XElement("Number", number),
                    new XElement("Status", status),
                    new XElement("ImageBase64", imageBase64)
                );

                doc.Root.Add(newExample);
                doc.Save(path);

                MessageBox.Show("Contact saved. Close the main window to refresh.");
                this.Close();
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}