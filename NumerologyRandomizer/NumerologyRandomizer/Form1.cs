using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace NumerologyRandomizer
{
    public partial class Form1 : Form
    {
        private NameHelper _helper;
        private BackgroundWorker _bgw;
        private List<ResultObject> _results;
        private bool _showAll;
        private bool _init;
        private string _filePath;

        private const int MAX_LEVEL = 1000;
        private const int SLEEP_TIME = 100;
        private const int SLEEP_INT = 100;

        public Form1()
        {
            InitializeComponent();
            
            _init = true;

            txtLastName.Text = "Rivera";

            string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _filePath = string.Concat(basePath, @"\NumerologyResults_", Guid.NewGuid(), ".txt");
            
            _helper = new NameHelper(txtLastName.Text);           
            _bgw = new BackgroundWorker();
            //_bgw.WorkerSupportsCancellation = true;
            _bgw.WorkerReportsProgress = true;
            _bgw.DoWork += new DoWorkEventHandler(bgw_DoWork);
            _bgw.ProgressChanged += new ProgressChangedEventHandler(bgw_ProgressChanged);
            _bgw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw_RunWorkerCompleted);
            _results = new List<ResultObject>();


            string[] header = new string[] {"First Name", "Middle Name", "Last Name", "All Numbers Used",
                                            "Soul Number Single", "Soul Number 2", "Outer Personality Number Single",
                                            "Outer Personality Number 2", "Path Of Destiny Number Single", "Path Of Destiny Number 2",
                                            "First Name Numbers", "Middle Name Numbers", "Last Name Numbers", Environment.NewLine};

            File.WriteAllText(_filePath, string.Join(";", header));

            _init = false;
        }

        private void Reset()
        {
            _results.Clear();
            this.dgvResults.DataSource = new List<ResultObject>();
            this.clbNames.Items.Clear();
            txtInfo.Text = string.Empty;
            this.chkSelectAll.Checked = false;

            string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _filePath = string.Concat(basePath, @"\NumerologyResults_", Guid.NewGuid(), ".txt");
            string[] header = new string[] {"First Name", "Middle Name", "Last Name", "All Numbers Used",
                                            "Soul Number Single", "Soul Number 2", "Outer Personality Number Single",
                                            "Outer Personality Number 2", "Path Of Destiny Number Single", "Path Of Destiny Number 2",
                                            "First Name Numbers", "Middle Name Numbers", "Last Name Numbers", Environment.NewLine};

            File.WriteAllText(_filePath, string.Join(";", header));
        }

        private void ShowError(Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void txtLastName_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (_init) return;

                _helper = new NameHelper(txtLastName.Text);
            }
            catch (Exception ex)
            {
                ShowError(ex);
                Reset();
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                AddName frmAdd = new AddName();
                frmAdd.ShowDialog();

                this.clbNames.Items.Add(frmAdd.AddedName);
            }
            catch (Exception ex)
            {
                ShowError(ex);
                Reset();
            }
        }

        private void chkShowAll_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                _showAll = chkShowAll.Checked;
            }
            catch (Exception ex)
            {
                ShowError(ex);
                Reset();
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            Reset();
        }

        private void chkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < clbNames.Items.Count; i++)
                {
                    clbNames.SetItemCheckState(i, chkSelectAll.CheckState);
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);
                Reset();
            }
        }

        private void btnLoadNames_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog1.Title = "Select a file with names in it...";
                openFileDialog1.Multiselect = false;
                openFileDialog1.Filter = "";
                if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var names = _helper.OpenFile(openFileDialog1.FileName);
                    this.clbNames.Items.AddRange(names.ToArray());
                }               
            }
            catch (Exception ex)
            {
                ShowError(ex);
                Reset();
            }
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            if (_bgw.IsBusy)
            {
                return; //do nothing
            }

            var prevCursor = Cursor;
            Cursor = Cursors.WaitCursor;
            try
            {
                int itemsChecked = clbNames.CheckedItems.Count;
                _results = new List<ResultObject>();
                txtInfo.Text = string.Empty;
                progressBar1.Step = 1;
                progressBar1.Maximum = itemsChecked + (itemsChecked * itemsChecked);

                if (progressBar1.Maximum > 100000)
                {
                    if (MessageBox.Show("Over 100,000 pending operations, are you sure you want to continue?", "Alert",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.No)
                    {
                        return;
                    }
                }

                _bgw.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                ShowError(ex);
                Reset();
            }
            finally
            {
                Cursor = prevCursor;
            }
        }
        
        void bgw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.PerformStep();
            ResultObject state = e.UserState as ResultObject;
            ShowInfo(string.Format("Processing {0} of {1}: {2} {3} {4}", progressBar1.Value, progressBar1.Maximum, state.FirstName, state.MiddleName, state.LastName));
            if (string.IsNullOrWhiteSpace(state.FirstName))
            {
                return;
            }

            if (_showAll || state.AllNumbersUsed_P)
            {
                _results.Add(state);
                //write to comma delimited flat file
                string outputLine = GenerateCommaDelimitedString(state);
                File.AppendAllText(_filePath, string.Join(",", outputLine));
            }
        }

        void bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            dgvResults.DataSource = _results;
        }

        void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            
            int itemCount = clbNames.CheckedItems.Count;
            
            List<string> allSingelNames = new List<string>();
            foreach (var item in clbNames.CheckedItems)
            {
                allSingelNames.Add(item.ToString());    
            }

            Task task = FirstNamesAsync(allSingelNames, worker);
            task.Wait();
            //Thread.Sleep(100);
            Application.DoEvents();

            if (itemCount > 1)
            {
                foreach(string name in allSingelNames)
                {
                    task = FirstNamesAndMiddleAsync(name, allSingelNames, worker);
                    task.Wait();
                    //Thread.Sleep(100);
                    Application.DoEvents();
                }
            }
        }

        private async Task FirstNamesAsync(List<string> allSingleNames, BackgroundWorker worker)
        {
            int nextIndex = 0;
            List<Task<ResultObject>> resultTasks = new List<Task<ResultObject>>();
            
            while (nextIndex < MAX_LEVEL && nextIndex < allSingleNames.Count)
            {
                resultTasks.Add(ProcessFirstNameAsync(allSingleNames[nextIndex]));
                nextIndex++;
            }

            while (resultTasks.Count > 0)
            {
                try
                {
                    Task<ResultObject> resultTask = await Task.WhenAny(resultTasks);
                    resultTasks.Remove(resultTask);
                    ResultObject result = await resultTask;
                    worker.ReportProgress(1, result);

                    if (nextIndex % SLEEP_INT == 0) Thread.Sleep(SLEEP_TIME);
                
                }
                catch
                {

                }
                if (nextIndex < allSingleNames.Count)
                {
                    resultTasks.Add(ProcessFirstNameAsync(allSingleNames[nextIndex]));
                    nextIndex++;
                }
            }
        }

        private async Task FirstNamesAndMiddleAsync(string firstName, List<string> allSingelNames, BackgroundWorker worker)
        {
            int nextIndex = 0;
            List<Task<ResultObject>> resultTasks = new List<Task<ResultObject>>();
            
            while (nextIndex < MAX_LEVEL && nextIndex < allSingelNames.Count)
            {
                resultTasks.Add(ProcessBothNamesAsync(firstName, allSingelNames[nextIndex]));
                nextIndex++;
            }

            while (resultTasks.Count > 0)
            {
                Task<ResultObject> resultTask = await Task.WhenAny(resultTasks);
                resultTasks.Remove(resultTask);
                ResultObject result = await resultTask;
                worker.ReportProgress(1, result);

                if (nextIndex%SLEEP_INT == 0) Thread.Sleep(SLEEP_TIME);

                if (nextIndex < allSingelNames.Count)
                {
                    resultTasks.Add(ProcessBothNamesAsync(firstName, allSingelNames[nextIndex]));
                    nextIndex++;
                }
            }
        }

        private async Task<ResultObject> ProcessFirstNameAsync(string first)
        {
            //Func<ResultObject> func = new Func<ResultObject>(() => _helper.CalculateNumbersForFirstNameOnly(first));
            //return new Task<ResultObject>(() => _helper.CalculateNumbersForFirstNameOnly(first));
            //await Task.Delay(500);
            return await Task.Run(() => _helper.CalculateNumbersForFirstNameOnly(first));
        }

        private async Task<ResultObject> ProcessBothNamesAsync(string first, string middle)
        {
            return await Task.Run(() => _helper.CalculateNumbersForFirstAndMiddleNames(first, middle));
        }

        private void ShowInfo(string p)
        {
            if (txtInfo.Text.Length > 100)
            {
                txtInfo.Text = txtInfo.Text.Substring(0, 100);
            }

            txtInfo.Text = string.Concat(p, Environment.NewLine, txtInfo.Text);
        }

        private string GenerateCommaDelimitedString(ResultObject state)
        {
            string[] broken = new string[] { NullCheck(state.FirstName), NullCheck(state.MiddleName), NullCheck(state.LastName), NullCheck(state.AllNumbersUsed_P.ToString()),
                                            NullCheck(state.SoulNumber.ToString()), NullCheck(state.SoulNumber_2.ToString()), NullCheck(state.OuterPersonalityNumber.ToString()),
                                            NullCheck(state.OuterPersonalityNumber_2.ToString()), NullCheck(state.PathOfDestinyNumber.ToString()), NullCheck(state.PathOfDestinyNumber_2.ToString()),
                                            NullCheck(state.FirstNameNumbers_P), NullCheck(state.MiddleNameNumbers_P), NullCheck(state.LastNameNumbers_P), Environment.NewLine};

            return string.Join(";", broken);
        }

        private string NullCheck(string p)
        {
            if (string.IsNullOrEmpty(p))
                return string.Empty;
            else
                return p;
        }
    }
}
