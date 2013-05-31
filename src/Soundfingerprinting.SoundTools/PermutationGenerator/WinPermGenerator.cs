namespace Soundfingerprinting.SoundTools.PermutationGenerator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Windows.Forms;

    using Soundfingerprinting.Hashing.MinHash;
    using Soundfingerprinting.Hashing.Utils;
    using Soundfingerprinting.SoundTools.Properties;

    /// <summary>
    ///   Permutation generator algorithm
    /// </summary>
    internal enum PermutationAlgorithm
    {
        /// <summary>
        ///   The weakest algorithm
        /// </summary>
        UniqueIndexesAcrossPermutation = 0,

        /// <summary>
        ///   Aggressive selector
        /// </summary>
        AgressiveSelector = 1,

        /// <summary>
        ///   Summed across
        /// </summary>
        SummedAccrossSelector = 2,

        /// <summary>
        ///   Conservative selector (best choice)
        /// </summary>
        ConservativeSelector = 3
    }

    public partial class WinPermGenerator : Form
    {
        private readonly IPermutationGeneratorService permutationGeneratorService;

        /// <summary>
        ///   From
        /// </summary>
        private int @from;

        /// <summary>
        ///   Keys per table (r-rows, or b-keys)
        /// </summary>
        private int keysPerTale = 5;

        /// <summary>
        ///   L hash tables
        /// </summary>
        private int hashTables = 20;

        /// <summary>
        ///   Permutations
        /// </summary>
        private string permutations;

        /// <summary>
        ///   Indexes
        /// </summary>
        private int to = 4096 * 2;

        /// <summary>
        ///   Parameter less constructor
        /// </summary>
        public WinPermGenerator(IPermutationGeneratorService permutationGeneratorService)
        {
            this.permutationGeneratorService = permutationGeneratorService;
            InitializeComponent();
            Icon = Resources.Sound;
            _nudLTables.Value = hashTables; /*20 hash tables*/
            _nudKeys.Value = keysPerTale; /*4-5 keys per table*/
            _nudFrom.Value = @from; /*0*/
            _nudTo.Value = to; /*8192*/
            _nudPermsCount.Value = hashTables*keysPerTale; /*100*/
            _pbProgress.Visible = false;
            _btnSv.Enabled = false;
            _cbmAlgorithm.SelectedIndex = 3; /*Conservative*/
        }

        /// <summary>
        ///   Save the permutations into a file
        /// </summary>
        private void BtnSaveClick(object sender, EventArgs e)
        {
            Func<PermutationAlgorithm, string> action = Generate;

            _pbProgress.Visible = true;
            _pbProgress.MarqueeAnimationSpeed = 30;
            _pbProgress.Style = ProgressBarStyle.Marquee;
            _btnSave.Enabled = false;
            action.BeginInvoke((PermutationAlgorithm) _cbmAlgorithm.SelectedIndex,
                (result) =>
                {
                    /*End of processing here!*/
                    permutations = action.EndInvoke(result);
                    Invoke(new Action(() =>
                                      {
                                          _pbProgress.MarqueeAnimationSpeed = 0;
                                          _btnSv.Enabled = true;
                                          _pbProgress.Visible = false;
                                          _btnSave.Enabled = true;
                                      }));
                    MessageBox.Show(Resources.PermutationsGenerated, Resources.Permutations, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }, action);
        }

        /// <summary>
        ///   Generate the permutations according to some algorithm
        /// </summary>
        /// <param name = "algorithm">Algorithm of generation of the permutations</param>
        /// <returns>String to be save into the file</returns>
        private string Generate(PermutationAlgorithm algorithm)
        {
            const string begin = "INSERT INTO Permutations VALUES (";
            @from = (int) _nudFrom.Value;
            to = (int) _nudTo.Value;
            hashTables = (int) _nudLTables.Value;
            keysPerTale = (int) _nudKeys.Value;
            StringBuilder final = new StringBuilder();

            Dictionary<int, int[]> perms = null;
            IMinMutualSelector selector = null;
            switch (algorithm)
            {
                case PermutationAlgorithm.UniqueIndexesAcrossPermutation: /*Unique random permutations*/
                    perms = permutationGeneratorService.GenerateRandomPermutationsUsingUniqueIndexes(hashTables, keysPerTale, @from, to);
                    break;
                case PermutationAlgorithm.AgressiveSelector: /*Aggressive selector*/
                    selector = new AgressiveSelector();
                    perms = permutationGeneratorService.GeneratePermutationsUsingMinMutualInformation(hashTables, keysPerTale, @from, to, selector);
                    break;
                case PermutationAlgorithm.SummedAccrossSelector: /*SummedAccross selector*/
                    selector = new SummedAcrossSelector();
                    perms = permutationGeneratorService.GeneratePermutationsUsingMinMutualInformation(hashTables, keysPerTale, @from, to, selector);
                    break;
                case PermutationAlgorithm.ConservativeSelector: /*Conservative selector*/
                    selector = new ConservativeSelector();
                    perms = permutationGeneratorService.GeneratePermutationsUsingMinMutualInformation(hashTables, keysPerTale, @from, to, selector);
                    break;
            }

            /*Create *.txt string*/
            if (perms != null)
                foreach (KeyValuePair<int, int[]> perm in perms)
                {
                    final.Append(begin);
                    StringBuilder permutation = new StringBuilder();
                    permutation.Append(perm.Key + ",'");
                    foreach (int t in perm.Value)
                        permutation.Append(t + ";");
                    permutation.Append("');");
                    final.AppendLine(permutation.ToString());
                }
            return final.ToString();
        }

        /// <summary>
        ///   Save the permutations
        /// </summary>
        private void BtnSvClick(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFile = new SaveFileDialog
                                             {
                                                 FileName = _cbmAlgorithm.SelectedItem + "_" + hashTables + "_" + keysPerTale + ".txt",
                                                 Filter = Resources.TextFiles
                                             })
            {
                if (saveFile.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter writer = new StreamWriter(saveFile.FileName))
                    {
                        writer.Write(permutations);
                    }
                }
            }
        }

        /// <summary>
        ///   Generate random matrix of projections
        /// </summary>
        private void BtnGenerateHashClick(object sender, EventArgs e)
        {
            string path = "randomvars.csv";
            Hash hash = new Hash();
            int rows = (int) _nudRows.Value;
            int bands = (int) _nudBands.Value;
            int[][] hashes = hash.GetRandomMatrix(rows, bands,
                (int) _nudStartPerm.Value, (int) _nudEndPerm.Value);

            object[][] toWrite = new object[bands][];
            for (int i = 0; i < bands; i++)
            {
                toWrite[i] = new object[rows];
                for (int j = 0; j < rows; j++)
                    toWrite[i][j] = hashes[i][j];
            }
            using (SaveFileDialog sfd = new SaveFileDialog {FileName = path, Filter = Resources.FileFilterCSV})
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    path = Path.GetFullPath(sfd.FileName);
                    CSVWriter writer = new CSVWriter(path);
                    writer.Write(toWrite);
                }
            }
        }
    }
}