using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using System.IO;
using System.Text.RegularExpressions;

namespace Laba2
{
    public partial class MainWindow : Window
    {
        private Regex regex = new Regex("0|1");
        private byte[] sourceBytes = null;
        private byte[] resultBytes;

        public MainWindow()
        {
            InitializeComponent();
        }

        private bool isBinaryText(string text)
        {
            return regex.IsMatch(text);
        }
        private void tbPreviewText(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !isBinaryText(e.Text);
        }

        private string bytesToString(byte[] bytes)
        {
            string res = "";
            byte a;
            int len = bytes.Length > 32 ? 32 : bytes.Length;
            for (int i = 0; i < len; i++)
            {
                a = 128;
                for (int j = 0; j < 8; j++)
                {
                    if ((bytes[i] & a) == 0)
                    {
                        res += "0";
                    }
                    else
                    {
                        res += "1";
                    }
                    a = (byte)(a >> 1);
                }
                res += ".";
            }
            return res;
        }

        private void btnClickOpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                sourceBytes = File.ReadAllBytes(openFileDialog.FileName);
                tbSourceText.Text = bytesToString(sourceBytes);
                tbResultText.Text = "";
            }
        }

        private ulong binaryStringToUlong(string s)
        {
            ulong res = 0;
            ulong a = 1;
            for (int i = s.Length - 1; i >= 0; i--)
            {
                if (s[i] == '1')
                {
                    res += a;
                }
                a *= 2;
            }
            return res;
        }


        private byte[] encrypt(byte[] sourceBytes, ulong key)
        {
            byte[] resultBytes = new byte[sourceBytes.Length];
            byte[] keys = new byte[sourceBytes.Length];
            for (int i = 0; i < sourceBytes.Length; i++)
            {
                keys[i] = 0;//0000000011111111111111111111111111111111
                for (int j = 7; j >= 0; j--)
                {
                    keys[i] = (byte)(((key >> 39) << j) | keys[i]); 
                    key = ((key << 1) & (ulong.MaxValue >>> 24)) | (((key >> 39) & 1) ^ ((key >> 20) & 1) ^ ((key >> 18) & 1) ^ ((key >> 1) & 1));
                }
                resultBytes[i] = (byte)(sourceBytes[i] ^ keys[i]);
            }
            tbKeyText.Text = bytesToString(keys);
            return resultBytes;            
        }

        private void btnClickSaveFile(object sender, RoutedEventArgs e)
        {
            if (sourceBytes != null)
            {
                if (tbKey.Text.Length == 40)
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        resultBytes = encrypt(sourceBytes, binaryStringToUlong(tbKey.Text));
                        tbResultText.Text = bytesToString(resultBytes);
                        File.WriteAllBytes(saveFileDialog.FileName, resultBytes);
                        sourceBytes = null;
                    }
                }
                else
                {
                    MessageBox.Show("Ключ недопустимой длины!");
                }
            }
            else
            {
                MessageBox.Show("Файл не открыт!");
            }          
        }
    }
}
