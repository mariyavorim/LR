using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace TPRLab5
{
    public class PFunc
    {
        double q, s, sigma;
        int funType;
        bool sign;
        public P func;
        public string name;

        public PFunc(int type, double q, double s, double sigma)
        {
            funType = type;
            this.q = q;
            this.s = s;
            this.sigma = sigma;
            name = "P" + (funType + 1);
            func = createFunc();
        }

        public PFunc(int type, double q, double s, double sigma, bool sign)
        {
            funType = type;
            this.q = q;
            this.s = s;
            this.sigma = sigma;
            this.sign = sign;
            name = "P" + (funType + 1);
            func = createFunc(sign);
        }

        public void Save(TextWriter tw)
        {
            tw.WriteLine(funType);
            tw.WriteLine(q);
            tw.WriteLine(s);
            tw.WriteLine(sigma);
            tw.WriteLine(sign);
        }
        public static PFunc Load(StreamReader tr)
        {
            int funType = int.Parse(tr.ReadLine());
            double q = double.Parse(tr.ReadLine());
            double s = double.Parse(tr.ReadLine());
            double sigma = double.Parse(tr.ReadLine());
            TextReader tr2 = new StreamReader( tr.BaseStream);
            string tmp = tr2.ReadLine();
            if(bool.TryParse(tmp, out bool sign))
            {
                tr.ReadLine();
                return new PFunc(funType, q, s, sigma, sign);
            }
            return new PFunc(funType, q, s, sigma);
        }


        P createFunc(bool sign=false)
        {
            switch (funType)
            {
                case 0: return createP1(sign);
                case 1: return createP2(q, sign);
                case 2: return createP3(s, sign);
                case 3: return createP4(q, s, sign);
                case 4: return createP5(q, s, sign);
                case 5: return createP6(sigma, sign);
                default: throw new Exception("Unknown func");
            }
        }

        P createP1(bool sign = false)
        {
            if (sign)
                return ((double d) => d >= 0 ? 0 : 1);
            else
                return ((double d) => d <= 0 ? 0 : 1);
        }
        P createP2(double q, bool sign = false)
        {
            if (sign)
                return ((double d) => d >= q ? 0 : 1);
            else
                return ((double d) => d <= q ? 0 : 1);
        }
        P createP3(double s, bool sign = false)
        {
            if (sign)
                return (double d) => d >= 0 ? 0 : d >s ? d / s : 1;
            else
                return (double d) => d <= 0 ? 0 : d < s ? d / s : 1;
        }
        P createP4(double q, double s, bool sign = false)
        {
            if(sign)
                return (double d) => d >= q ? 0 : d >= s ? 0.5 : 1;
            else
            return (double d) => d <= q ? 0 : d <= s ? 0.5 : 1;
        }
        P createP5(double q, double s, bool sign=false)
        {
            if(sign)
                return (double d) => d >= q ? 0 : d >= s ? (d - q) / (s - q) : 1;
            else
            return (double d) => d <= q ? 0 : d <= s ? (d - q) / (s - q) : 1;
        }
        P createP6(double s, bool sign = false)
        {
            if(sign)
                return (double d) => d >= 0 ? 0 : 1 - Math.Exp(-d * d / (2 * s * s));
            else
            return (double d) => d <= 0 ? 0 : 1 - Math.Exp(-d * d / (2 * s * s));
        }


    }
}
