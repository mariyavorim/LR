using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace TPRLab5
{

    public delegate double P(double d);

    public partial class Form1 : Form
    {
        public Form1()
        {
            CritPs = new List<PFunc>();
            InitializeComponent();
        }

        private void nuAlternatives_ValueChanged(object sender, EventArgs e)
        {
            dgvInput.RowCount = (int)nuAlternatives.Value;
            dgvInput.Rows[dgvInput.RowCount - 1].HeaderCell.Value = ("a" + dgvInput.RowCount);
        }


        private void nuCriteries_ValueChanged(object sender, EventArgs e)
        {
            dgvInput.ColumnCount = (int)nuCriteries.Value + 1;
            dgvCrits.ColumnCount = (int)nuCriteries.Value;
            dgvCrits.RowCount = 1;
            if ((int)nuCriteries.Value > 0)
            {
                dgvCrits.Columns[dgvCrits.ColumnCount - 1].HeaderText = "w" + dgvCrits.ColumnCount;
                dgvInput.Columns[dgvInput.ColumnCount - 1].HeaderText = "f" + (dgvInput.ColumnCount - 1);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dgvInput.ColumnCount = (int)nuCriteries.Value + 1;
            dgvInput.RowCount = (int)nuAlternatives.Value;
            dgvInput.RowCount = 1;
            dgvInput.Rows[dgvInput.RowCount - 1].HeaderCell.Value = ("a" + dgvInput.RowCount);
            if (dgvInput.ColumnCount > 1)
                dgvInput.Columns[dgvInput.ColumnCount - 1].HeaderText = "f" + (dgvInput.ColumnCount - 1);


            dgvCrits.ColumnCount = (int)nuCriteries.Value;
            dgvCrits.RowCount = 1;
            dgvCrits.Columns[dgvCrits.ColumnCount - 1].HeaderText = "w" + dgvCrits.ColumnCount;

        }

        string[] altNames;
        int crits;
        int alts;
        Matrix critMat;
        Matrix[] d;
        double[] w;
        void Input()
        {
            crits = (int)nuCriteries.Value;
            alts = (int)nuAlternatives.Value;
            altNames = new string[alts];
            for (int i = 0; i < alts && i < dgvInput.RowCount; i++)
                if (dgvInput[0, i] != null && dgvInput[0, i].Value != null)
                    altNames[i] = dgvInput[0, i].Value.ToString();
                else
                    altNames[i] = "не задано";

            critMat = new Matrix(alts, crits);
            for (int i = 0; i < alts && i < dgvInput.RowCount; i++)
                for (int j = 0; j < crits && j + 1 < dgvInput.ColumnCount; j++)
                {
                    if (dgvInput[j + 1, i] != null && dgvInput[j + 1, i].Value != null &&
                           double.TryParse(dgvInput[j + 1, i].Value.ToString().Replace('.', ','), out double d))
                        critMat[i, j] = d;
                    else
                        critMat[i, j] = 0;
                }

            w = new double[crits];
            for (int crit = 0; crit < crits && crit < dgvCrits.ColumnCount; crit++)
                if (dgvCrits[crit, 0] != null && dgvCrits[crit, 0].Value != null
                    && double.TryParse(dgvCrits[crit, 0].Value.ToString().Replace('.', ','), out double d))
                    w[crit] = d;
                else
                    w[crit] = 0;

            while (CritPs.Count > crits)
                CritPs.RemoveAt(CritPs.Count - 1);

            while (CritPs.Count < crits)
            {
                InputPForm pf = new InputPForm(CritPs.Count);
                pf.ShowDialog();
                CritPs.Add(pf.result);
            }


        }
        double[] F_min, F_plus, F;
        Matrix pi;
        Matrix mat2;
        List<PFunc> CritPs;
        void Calculate()
        {

            d = new Matrix[crits];
            for (int c = 0; c < crits; c++)
            {
                Matrix mat = new Matrix(alts, alts);
                for (int i = 0; i < alts; i++)
                    for (int j = 0; j < alts; j++)
                        mat[i, j] = critMat[i, c] - critMat[j, c];


                d[c] = mat;
            }

            mat2 = new Matrix(alts * crits, alts);

            for (int alt1 = 0; alt1 < alts; alt1++)
                for (int alts2 = 0; alts2 < alts; alts2++)
                {
                    for (int crit = 0; crit < crits; crit++)
                        mat2[crit * alts + alt1, alts2] = CritPs[crit].func(d[crit][alt1, alts2]);
                }


            pi = new Matrix(alts, alts);

            for (int i = 0; i < alts; i++)
                for (int j = 0; j < alts; j++)
                {
                    double sum = 0;
                    for (int crit = 0; crit < crits; crit++)
                        sum += w[crit] * CritPs[crit].func(d[crit][i, j]);
                    pi[i, j] = sum;
                }


            F_min = new double[alts]; F_plus = new double[alts]; F = new double[alts];

            for (int i = 0; i < alts; i++)
            {
                F_min[i] = 0;
                F_plus[i] = 0;
                for (int j = 0; j < alts; j++)
                    F_min[i] += pi[j, i];
                for (int j = 0; j < alts; j++)
                    F_plus[i] += pi[i, j];
                F[i] = F_plus[i] - F_min[i];
            }


            F_alts = new SortedDictionary<double, string>();
            for (int i = 0; i < alts; i++)
                F_alts.Add(F[i], altNames[i]);

        }
        SortedDictionary<double, string> F_alts;

        private void btnSave_Click(object sender, EventArgs e)
        {
            Input();
            SaveFileDialog sfd = new SaveFileDialog();

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                StreamWriter sw = new StreamWriter(sfd.FileName);

                sw.WriteLine(alts);
                sw.WriteLine(crits);

                for (int i = 0; i < alts; i++)
                    sw.WriteLine(altNames[i]);

                critMat.Save(sw);

                for (int i = 0; i < crits; i++)
                    sw.WriteLine(w[i]);

                for (int i = 0; i < crits; i++)
                    CritPs[i].Save(sw);

                sw.Close();
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            FileDialog sfd = new OpenFileDialog();

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                StreamReader sw = new StreamReader(sfd.FileName);

                int alts = int.Parse(sw.ReadLine());
                int crits = int.Parse(sw.ReadLine());
                var altNames = new string[alts];
                for (int i = 0; i < alts; i++)
                    altNames[i] = sw.ReadLine();

                var critMat = new Matrix(alts, crits);
                critMat.Load(sw);

                var w = new double[crits];
                for (int i = 0; i < crits; i++)
                {
                    w[i] = double.Parse(sw.ReadLine());
                }

                CritPs.Clear();
                for (int i = 0; i < crits; i++)
                {
                    CritPs.Add(PFunc.Load(sw));
                }

                sw.Close();

                nuAlternatives.Value = alts;
                nuCriteries.Value = crits;

                for (int i = 0; i < crits; i++)
                {
                    dgvCrits.Columns[i].HeaderText = "w" + (i + 1);
                    dgvCrits[i, 0].Value = w[i];

                    dgvInput.Columns[i + 1].HeaderText = "f" + (i + 1);
                }

                for (int i = 0; i < alts; i++)
                {
                    dgvInput[0, i].Value = altNames[i];
                    dgvInput.Rows[i].HeaderCell.Value = "a" + (i + 1);
                }

                for (int i = 0; i < alts; i++)
                    for (int j = 0; j < crits; j++)
                        dgvInput[j + 1, i].Value = critMat[i, j];
            }
            Input();
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                Input();
                Calculate();
                Output();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        void Output()
        {
            //////////////////////////////////////
            ///матрицы сравнений по критериям
            /////////////////////////////////////
            //
            dgvDelta.RowCount = alts;
            dgvDelta.ColumnCount = crits * (alts + 1);

            for (int i = 0; i < alts; i++)
            {
                dgvDelta.Rows[i].HeaderCell.Value = altNames[i];
            }

            for (int c = 0; c < crits; c++)
            {
                for (int i = 0; i < alts; i++)
                {
                    dgvDelta[(alts + 1) * c, i].Value = "Критерий" + (c + 1).ToString();
                    dgvDelta.Columns[(alts + 1) * c + i + 1].HeaderText = altNames[i];
                }
                for (int i = 0; i < alts; i++)
                    for (int j = 0; j < alts; j++)
                        dgvDelta[j + c * (alts + 1) + 1, i].Value = d[c][i, j];
            }
            //////////////////////////////////

            ///////////////////////////////////
            ///матрицы с p 
            /////////////////////
            dgvFun.RowCount = crits * alts;
            dgvFun.ColumnCount = alts + 1;
            for (int i = 0; i < dgvFun.RowCount; i++)
                dgvFun.Rows[i].HeaderCell.Value = CritPs[i/alts].name; 

            for (int i = 0; i < dgvFun.RowCount ; i++)
                dgvFun[0, i].Value = "a" + (i % alts + 1).ToString();

            for (int i = 0; i < alts; i++)
                dgvFun.Columns[i + 1].HeaderText = "a" + (i + 1).ToString();

            for (int i = 0; i < mat2.n; i++)
                for (int j = 0; j < mat2.m; j++)
                    dgvFun[j + 1, i].Value = mat2[i, j];


            ////////////////////////
            ///матрица пи и Ф+, Ф-
            //////////////////////////////

            dgvPi.ColumnCount = alts + 1;
            dgvPi.RowCount = alts + 1;

            for (int i = 0; i < alts; i++)
                dgvPi.Columns[i].HeaderText = "π(ai, a" + (i + 1).ToString() + ")";
            dgvPi.Columns[alts].HeaderText = "Ф+";

            for (int i = 0; i < alts; i++)
                dgvPi.Rows[i].HeaderCell.Value = "π(a" + (i + 1).ToString() + ", aj)";
            dgvPi.Rows[alts].HeaderCell.Value = "Ф-";

            for (int i = 0; i < alts; i++)
                for (int j = 0; j < alts; j++)
                    dgvPi[j, i].Value = pi[i, j];

            for (int i = 0; i < alts; i++)
                dgvPi[alts, i].Value = F_plus[i];

            for (int i = 0; i < alts; i++)
                dgvPi[i, alts].Value = F_min[i];

            ////////////////////////////////
            ///результаты
            ///////////////////////////////

            dgvEndResult.RowCount = 2;
            dgvEndResult.ColumnCount = alts;
            dgvEndResult.Rows[0].HeaderCell.Value = "a";
            dgvEndResult.Rows[1].HeaderCell.Value = "F";

            int iter = 0;
            foreach (var par in F_alts.Reverse())
            {
                dgvEndResult[iter, 0].Value = par.Value;
                dgvEndResult[iter, 1].Value = par.Key;
                iter++;
            }
        }
    }
}
