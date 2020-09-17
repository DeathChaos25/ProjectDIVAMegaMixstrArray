using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MegaMixstrArray
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private strArrayFile currentstrArray;
        public uint strArrayPointer { get; set; }
        public List<String> strArrayStrings { get; private set; }
        public string[] readText { get; private set; }
        public string nameOfFile { get; set; }
        Encoding utf8WithoutBom = new UTF8Encoding(false);
        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "str_array.bin/str_array.txt (*.bin, *.txt)|*.bin;*.txt|All files (*.*)|*.*";
                dialog.FilterIndex = 1;
                dialog.RestoreDirectory = true;
                dialog.Title = "open strArray.bin (str_array.bin) or a converted text file";

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FilePathTxtBox.Text = dialog.FileName;
                }
                dialog.Dispose();
            }
        }
        private void displayTXTBox_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
            else
                e.Effect = DragDropEffects.None;
        }
        private void displayTXTBox_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[]; // get all files droppeds  
            if (files != null && files.Any())
            {
                FilePathTxtBox.Text = files.First(); //select the first one 
                ConvertFile(files.First());
            }
        }

        private void btnLoadFile_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(FilePathTxtBox.Text))
            {
                ConvertFile(FilePathTxtBox.Text);
            }
        }
        public void ConvertFile(String fileToConvert)
        {
            var fileExtension = Path.GetExtension(fileToConvert);

            if (fileExtension == ".bin" || fileExtension == ".BIN")
            {
                //Read the contents of the file into a stream
                using (var fileStream = File.Open(fileToConvert, FileMode.Open))
                {
                    currentstrArray = new strArrayFile();
                    currentstrArray.ReadstrArray(fileStream);

                    using (SaveFileDialog saveFileDialog1 = new SaveFileDialog())
                    {

                        saveFileDialog1.Filter = "text file (*.txt)|*.txt|All files (*.*)|*.*";
                        saveFileDialog1.FilterIndex = 1;
                        saveFileDialog1.RestoreDirectory = true;

                        if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            displayTXTBox.Text += "Converting binary file to .txt:\r\n" + fileToConvert + "\r\n";
                            var savePath = saveFileDialog1.FileName;
                            File.WriteAllLines(savePath, currentstrArray.strArrayMessages, utf8WithoutBom);
                            MessageBox.Show("File saved to: " + savePath, "File saved");
                        }
                    }
                }
            }
            else if (fileExtension == ".txt" || fileExtension == ".TXT")
            {
                string[] readText = File.ReadAllLines(fileToConvert, Encoding.UTF8);

                strArrayStrings = new List<String>();

                int k = 0;
                foreach (string s in readText)
                {
                    strArrayStrings.Add(readText[k]);
                    k++;
                }

                using (SaveFileDialog saveFileDialog1 = new SaveFileDialog())
                {
                    saveFileDialog1.Filter = "str_array.bin (*.bin)|*.bin|All files (*.*)|*.*";
                    saveFileDialog1.FilterIndex = 1;
                    saveFileDialog1.RestoreDirectory = true;

                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        var savePath = saveFileDialog1.FileName;
                        using (EndianBinaryWriter newstrArray = new EndianBinaryWriter(File.Open(savePath, FileMode.Create, FileAccess.Write), utf8WithoutBom, false, Endianness.Little))
                        {
                            displayTXTBox.Text += "Converting file text file to Project DIVA Mega Mix message binary\r\n:" + fileToConvert + "\r\n";
                            currentstrArray = new strArrayFile();
                            currentstrArray.WritestrArray(newstrArray, strArrayStrings);
                            MessageBox.Show("File saved to: " + savePath, "File saved");
                        }
                    }
                }
            }
            else MessageBox.Show("This file is not a valid message binary or text file", "Invalid file");
        }
        private void displayTXTBox_Click(object sender, System.EventArgs e)
        {
            displayTXTBox.Clear();
        }
    }
}
