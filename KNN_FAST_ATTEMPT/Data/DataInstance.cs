using System;
using System.Collections.Generic;
using System.Linq;
using KNN.Data;

namespace KNN.Data {
    public class DataInstance : List<string> {
        public DataInstance() { }
        public DataInstance(IEnumerable<string> data) {
            this.AddRange(data);
        }
		public DataInstance(IEnumerable<string> data, System.Collections.Generic.List<KNN.Data.Feature> features) {
			if(data.Count() != features.Count){
				Console.WriteLine("[Error]: Invalid # of data elements in Instance Creation");
			}
			this.AddRange (data);
		}

        public override string ToString() {
            return string.Join(",", this.ToArray());
        }


//		public double[][] GetDoubleMatrixRepresentation() {
//			int l = this.Count;
//			double[][] arr = new double[l][];
//			for (int i = 0; i< l; i++) {
//				string s = this [i];
//				string[] s_array = s.Split (':');
//				arr [i] = new double[s_array.Length];
//
//				for (int j = 0; j < s_array.Length; j++) {
//					string s2 = s_array [j];
//					arr[i][j] = double.NaN;
//					double.TryParse(s2, out arr[i][j]);
//					if (double.IsNaN (arr[i][j])) {
//						Console.Write ("Could not convert string: {0}", s2);
//						return new double[0] [];
//					}
//				}
//			}
//			return arr;
//		}
    }
}
