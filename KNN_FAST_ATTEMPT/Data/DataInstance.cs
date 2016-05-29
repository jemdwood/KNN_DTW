using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KNN.Data;

namespace KNN.Data {
	public class DataInstance : List<double[]> {

		private string output;

        public DataInstance() { }
        public DataInstance(IEnumerable<string> data) {
			foreach (string s in data) {
				this.Add (s);
			}
        }

		public DataInstance(IEnumerable<double[]> data) {
			this.AddRange (data);
		}

		public DataInstance(IEnumerable<string> data, System.Collections.Generic.List<KNN.Data.Feature> features) {
			if(data.Count() != features.Count){
				Console.WriteLine("[Error]: Invalid # of data elements in Instance Creation");
			}
			foreach (string s in data) {
				this.Add (s);
			}
		}

        public override string ToString() {
			//return string.Join(",", this.ToArray());
			return this.ToList().ToString();
        }

		public string getOutput() {
			return output;
		}

		public void Add(string data) {
			string[] strs = data.Split (':');
			IEnumerable<double> to_add_total =  Array.ConvertAll(strs, new Converter<string, double>(stringToDouble));
			IEnumerable<double> to_add = to_add_total.SkipWhile (p => double.IsNaN (p));
			if (to_add.Count() == 0) {
				this.output = strs [0];
			} else {
				this.Add (to_add.ToArray());
			}
		}

		private static double stringToDouble(string s) {
			double d; // Will be a NaN if cannot be parsed
			if (double.TryParse (s, out d)) {
				return d;
			} else {
				return double.NaN;
			}
		}

		private double[][] GetDoubleMatrixRepresentation(string[] data) {
			int l = this.Count;
			double[][] arr = new double[l][];
			for (int i = 0; i< l; i++) {
				string s = data [i];
				string[] s_array = s.Split (':');
				arr [i] = new double[s_array.Length];

				for (int j = 0; j < s_array.Length; j++) {
					string s2 = s_array [j];
					arr[i][j] = double.NaN;
					double.TryParse(s2, out arr[i][j]);
					if (double.IsNaN (arr[i][j])) {
						Console.Write ("Could not convert string: {0}", s2);
						return new double[0] [];
					}
				}
			}
			return arr;
		}
    }
}
