using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using IsoTools.Iso9660;

namespace IsoTools {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            string[] cmdLine = Environment.GetCommandLineArgs();
            if (cmdLine.Length == 2 && File.Exists(cmdLine[1]) && !File.Exists(Path.ChangeExtension(cmdLine[1], ".md5"))) {
                List<PS3File> lvFilesRaw = new List<PS3File>();
                IrdFile irdFile = IrdFile.Load(cmdLine[1]);
                PS3CDReader pS3CDReader = new PS3CDReader(irdFile.Header);
                ICollection<DirectoryMemberInformation> members = pS3CDReader.Members;
                foreach (DirectoryMemberInformation directoryMemberInformation in (from d in members
                                                                                   where d.IsFile
                                                                                   select d).Distinct<DirectoryMemberInformation>())
                    lvFilesRaw.Add(new PS3File() { Lenght = directoryMemberInformation.Length, Filename = directoryMemberInformation.Path, MD5 = irdFile.FileHashes.FirstOrDefault<KeyValuePair<long, byte[]>>((KeyValuePair<long, byte[]> f) => f.Key == directoryMemberInformation.StartSector).Value.AsString() });

                using (TextWriter tw = File.CreateText(Path.ChangeExtension(cmdLine[1], ".md5")))
                    foreach (PS3File F in lvFilesRaw)
                        tw.WriteLine("{0} *{1}", F.MD5, F.Filename.TrimStart('/'));

                Application.Exit();
            }
        }

        struct PS3File {
            public string Filename;
            public long Lenght;
            public string MD5;
        }

        private void bStart_Click(object sender, EventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog() { Title = "Select an IRD File", Filter = "IRD files (*.ird)|*.ird" };
            if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK || Path.GetExtension(ofd.FileName) != ".ird") {
                MessageBox.Show("Not and IRD File?");
                return;
            }
            SaveFileDialog sfd = new SaveFileDialog() { Title = "Select an MD5 File", Filter = "MD5 files (*.md5)|*.md5" };
            if (sfd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            this.Enabled = false;

            List<PS3File> lvFilesRaw = new List<PS3File>();
            IrdFile irdFile = IrdFile.Load(ofd.FileName);
            PS3CDReader pS3CDReader = new PS3CDReader(irdFile.Header);
            ICollection<DirectoryMemberInformation> members = pS3CDReader.Members;
            foreach (DirectoryMemberInformation directoryMemberInformation in (from d in members
                                                                               where d.IsFile
                                                                               select d).Distinct<DirectoryMemberInformation>())
                lvFilesRaw.Add(new PS3File() { Lenght = directoryMemberInformation.Length, Filename = directoryMemberInformation.Path, MD5 = irdFile.FileHashes.FirstOrDefault<KeyValuePair<long, byte[]>>((KeyValuePair<long, byte[]> f) => f.Key == directoryMemberInformation.StartSector).Value.AsString() });

            using (TextWriter tw = File.CreateText(sfd.FileName))
                foreach (PS3File F in lvFilesRaw)
                    tw.WriteLine("{0} *{1}", F.MD5, F.Filename.TrimStart('/'));

            tbResults.Text = "MD5 Created Successfully";
            this.Enabled = true;
        }
    }
}
