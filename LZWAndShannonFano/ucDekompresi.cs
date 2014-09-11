﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

using FileTypeDetective;
using LZWAndShannonFano.LZW;

namespace LZWAndShannonFano
{
    public partial class ucDekompresi : UserControl
    {
        public bool LZW = true;

        int byteProcessed = 0;
        bool procesedFinished = false;
        string compressFile;
        int fileSizeConvert;

        delegate void SetTextCallback(string text, int type);

        public const int LBL_INFO = 1;
        public const int TXT_KOMPRES = 2;
        public const int TXT_KOMPRES_SIZE = 3;
        public const int TXT_KOMPRES_TIME = 4;
        public const int TXT_FILE_NAME = 5;
        public const int TXT_FILE_TYPE = 6;
        public const int TXT_FILE_SIZE = 7;
        public const int TXT_RASIO = 8;

        public ucDekompresi(bool lzw)
        {
            InitializeComponent();
            backgroundWorker1.WorkerReportsProgress = true;
            openFileDialog1.FileName = "";
            this.LZW = lzw;
            if (LZW)
            {
                lblTitleUC.Text = "Dekompresi LZW Algorithim";
            }
            else
            {
                lblTitleUC.Text = "Dekompresi Shannon-Fano Algorithim";
            }
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            if (LZW)
            {
                openFileDialog1.Filter = "LZW Files (*.lzw) |*.lzw";
            }
            else
            {
                openFileDialog1.Filter = "Shannon Fano Files (*.sf) |*.sf";
            }
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            txtFile.Text = openFileDialog1.FileName;

            FileInfo fileinfo = new FileInfo(openFileDialog1.FileName);
            txtKompresiFile.Text = fileinfo.Name;
            txtFileSizeKompresi.Text = fileinfo.Length.ToString() + " Bytes";

            byteProcessed = 0;
            progressBar1.Value = 0;
            txtFilename.Text = "";
            txtFilesize.Text = "";
            txtWktKompresi.Text = "";
            lblInfo.Text = "";
            txtRasio.Text = "";
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            //Your background task goes here
            while (procesedFinished == false)
            {
                backgroundWorker1.ReportProgress(byteProcessed);
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void SetText(string text, int type)
        {
            if (type == LBL_INFO)
            {
                lblInfo.Text = text;
            }
            else if (type == TXT_KOMPRES)
            {
                txtKompresiFile.Text = text;
            }
            else if (type == TXT_KOMPRES_SIZE)
            {
                txtFileSizeKompresi.Text = text;
            }
            else if (type == TXT_KOMPRES_TIME)
            {
                txtWktKompresi.Text = text;
            }
            else if (type == TXT_FILE_NAME)
            {
                txtFilename.Text = text;
            }
            else if (type == TXT_FILE_SIZE)
            {
                txtFilesize.Text = text;
            }
            else if (type == TXT_FILE_TYPE)
            {
                txtFiletype.Text = text;
            }
            else if (type == TXT_RASIO)
            {
                txtRasio.Text = text;
            }
        }

        private void btnDelompres_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.FileName == "")
            {
                MessageBox.Show("Silahkan pilih file yang akan didekompresi!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (txtSimpan.Text == "")
            {
                MessageBox.Show("Silahkan pilih folder pemnyimpanan file dekompresi", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (new DirectoryInfo(txtSimpan.Text).Exists == false)
            {
                MessageBox.Show("Folder penyimpanan salah", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SetTextCallback de = new SetTextCallback(SetText);
            System.Diagnostics.Stopwatch sWatch = new System.Diagnostics.Stopwatch();
            progressBar1.Visible = true;
            procesedFinished = false;
            byteProcessed = 0;
            progressBar1.Value = 0;
            pctImage.Image = null;
            if (LZW)
            {
                
                string text = File.ReadAllText(txtFile.Text, System.Text.ASCIIEncoding.Default);
                backgroundWorker1.RunWorkerAsync();
                Thread LZWThread = new Thread(
                    new ThreadStart(() =>
                    {
                        //this.Invoke(de, new object[] { "Generate ASCI table", LBL_INFO });
                        sWatch.Start();
                        FileInfo fileInfo = new FileInfo(openFileDialog1.FileName);
                        //this.Invoke(de, new object[] { "Start decoding " + fileInfo.Name, LBL_INFO });

                        compressFile = txtSimpan.Text + "\\" + fileInfo.Name.Substring(0, fileInfo.Name.Length - 4);
                        LZWAndShannonFano.LZW.Decoder decoder = new LZW.Decoder();
                        byte[] bo = File.ReadAllBytes(txtFile.Text);
                        string decodedOutput = decoder.Apply(bo, ref byteProcessed);
                        File.WriteAllText(compressFile, decodedOutput, System.Text.Encoding.Default);
                        String resultFile = changeExtension(compressFile);
                        pctImage.Image = (Bitmap) Bitmap.FromFile(resultFile);
                        procesedFinished = true;
                        
                        sWatch.Stop();
                        fileInfo = new FileInfo(resultFile);
                        int rasio = (int)((double)bo.Length / fileInfo.Length * 100);
                        if (IsHandleCreated)
                            this.BeginInvoke(de, new object[] { "", TXT_FILE_SIZE });

                        this.Invoke(de, new object[] { fileInfo.Length + " Bytes", TXT_FILE_SIZE });
                        this.Invoke(de, new object[] { fileInfo.Name, TXT_FILE_NAME });
                        this.Invoke(de, new object[] { fileInfo.Extension, TXT_FILE_TYPE });
                        this.Invoke(de, new object[] { Math.Round(sWatch.Elapsed.TotalSeconds, 2).ToString() + " second", TXT_KOMPRES_TIME });
                        //this.Invoke(de, new object[] { "", LBL_INFO });
                        this.Invoke(de, new object[] { rasio.ToString() + " %", TXT_RASIO });
                        MessageBox.Show("Success", "Information", MessageBoxButtons.OK);
                    })
                    );
                LZWThread.Start();
            }
            else
            {
                string resultPath = txtSimpan.Text;
                string sfcFile = openFileDialog1.FileName.Substring(0, openFileDialog1.FileName.Length - 3);
                if (!File.Exists(sfcFile + ".sfc"))
                {
                    MessageBox.Show("File SF Code tidak ditemukan", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                backgroundWorker1.RunWorkerAsync();
                Thread SFThread = new Thread(
                    new ThreadStart(() =>
                    {
                        sWatch.Start();

                        FileInfo fileInfo = new FileInfo(openFileDialog1.FileName);
                        //this.Invoke(de, new object[] { "Start decoding " + fileInfo.Name, LBL_INFO });

                        compressFile = txtSimpan.Text + "\\" + fileInfo.Name.Substring(0, fileInfo.Name.Length - 3);
                        ShannonFano.Decoder decoder = new ShannonFano.Decoder();
                        String sfc = File.ReadAllText(sfcFile + ".sfc");
                        decoder.SetSFCode(sfc);

                        byte[] decodingByte = File.ReadAllBytes(openFileDialog1.FileName);
                        byte[] decImage = decoder.Decoding(decodingByte.GetBinaryString(), ref byteProcessed);

                        File.WriteAllBytes(compressFile, decImage);
                        String resultFile = changeExtension(compressFile);


                        pctImage.Image = (Bitmap)Bitmap.FromFile(resultFile); ;
                        sWatch.Stop();
                        procesedFinished = true;
                        fileInfo = new FileInfo(resultFile);
                        int rasio = (int)((double)decodingByte.Length / fileInfo.Length * 100);
                        if(IsHandleCreated)
                            this.Invoke(de, new object[] {"", TXT_FILE_SIZE });

                        this.Invoke(de, new object[] { fileInfo.Length + " Bytes", TXT_FILE_SIZE });
                        this.Invoke(de, new object[] { fileInfo.Name, TXT_FILE_NAME });
                        this.Invoke(de, new object[] { fileInfo.Extension, TXT_FILE_TYPE });
                        this.Invoke(de, new object[] { Math.Round(sWatch.Elapsed.TotalSeconds, 2).ToString() + " second", TXT_KOMPRES_TIME });
                        //this.Invoke(de, new object[] { "", LBL_INFO });
                        this.Invoke(de, new object[] { rasio.ToString() + " %", TXT_RASIO });
                        MessageBox.Show("Success", "Information", MessageBoxButtons.OK);
                    }));
                SFThread.Start();
            }
        }

        private String changeExtension(string sourceFile)
        {
            FileInfo infoFile = new FileInfo(sourceFile);
            FileType fileType = infoFile.GetFileType();
            String fileName = "";
            if (fileType.extension == "jpg")
            {
                fileName = Path.ChangeExtension(sourceFile, "jpg");
                checkFileExist(fileName);
                File.Move(sourceFile, fileName);
            }
            else if (fileType.extension == "bmp")
            {
                fileName = Path.ChangeExtension(sourceFile, "bmp");
                checkFileExist(fileName);
                File.Move(sourceFile, fileName);
            }
            else if (fileType.extension == "png")
            {
                fileName = Path.ChangeExtension(sourceFile, "png");
                checkFileExist(fileName);
                File.Move(sourceFile, fileName);
            }
            else if (fileType.extension == "gif")
            {
                fileName = Path.ChangeExtension(sourceFile, "gif");
                checkFileExist(fileName);
                File.Move(sourceFile, fileName);
            }
            return fileName;
        }

        private void checkFileExist(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
            catch
            { }
        }

        private void btnSimpan_Click(object sender, EventArgs e)
        {
            DialogResult path = folderBrowserDialog1.ShowDialog();
            if (path == DialogResult.OK)
            {
                txtSimpan.Text = folderBrowserDialog1.SelectedPath;
            }
        }
    }
}
