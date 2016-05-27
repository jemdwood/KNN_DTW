using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace KNN
{
	public class DataReaderConverter
	{
		private double[][][] input_data; //array of double[dataPoint][feature][timeSlice] (after reversal)
		private string[] output;
		private string[] classes;
		private string directory;
		public string data_name = "gestures.data";
		public string names_name = "gestures.names";

		private IEnumerable<double[]> ReadFromFile(string fname) {
			using (var sr = new StreamReader (fname)) {
				string line;
				while ((line = sr.ReadLine ()) != null) {
					if (line == string.Empty || line.IndexOf ("//", 0, 2, System.StringComparison.Ordinal) == 0)
						continue;

					string[] split_line = line.Split (',');
					yield return Array.ConvertAll(split_line, new Converter<string, double>(stringToDouble));
				}
			}
		}

		private static double stringToDouble(string s) {
			double d = double.NaN;
			double.TryParse (s.Trim(), out d);
			if (double.IsNaN(d)) {
				Console.Write("Could not convert to double: " + s);
			} 
			return d;
		} 

		//Converts array of  double[dataPoint][timeSlice][feature]  to double[dataPoint][feature][timeSlice] 
		private void reverse_features_and_times () {
			double[][][] new_arr = new double[this.input_data.GetUpperBound(0)+1][][];
			for(int i = 0; i <= this.input_data.GetUpperBound(0); i++) {
				if (this.input_data [i].Length == 0) {
					return;
				}

				int n_features = this.input_data [i][0].GetUpperBound(0) +1;
				int n_times = this.input_data [i].GetUpperBound(0) +1;
				new_arr [i] = new double[n_features][];
				for (int t = 0; t < n_times; t++) {
					for (int f = 0; f < n_features; f++) {
						if (t == 0) {
							new_arr [i] [f] = new double[n_times];
						}
						new_arr [i] [f] [t] = this.input_data [i] [t] [f];
					}
				}
			}
			double test = this.input_data [0] [0] [1];
			this.input_data = new_arr;
			if (this.input_data [0] [1] [0] != test) {
				Console.WriteLine ("NEED TO DO A DEEP COPY\n\n\n");
			}
		}


		// Input: f_basenames -- should be an array of the different file basenames, e.g. {"NO", "YES", "NULL"}
		public DataReaderConverter (string[] f_basenames, string directory) 
		{
			this.directory = directory;
			classes =	 f_basenames;
			int gestures = f_basenames.Length;
			Func<string, string, string[]> dd = Directory.GetFileSystemEntries;
			string[] files = dd (directory, "*.csv");
			//string[] files = Directory.GetFiles (directory);
			input_data = new double[files.Count(fn => fn.EndsWith(".csv"))][][]; // array of all inputs double[dataPoint][timeSlice][feature]
			output = new string[input_data.GetLength(0) + 1]; //the labels applied to each data point
			int i = 0;
			foreach (string g in f_basenames) {
				Regex g_rgx = new Regex(@"[/\\]" + g);

				IEnumerable<string> g_files = from fs in files
						where g_rgx.IsMatch(fs)	
						select fs;
				if (g_files.Count ()== 0) {
					Console.WriteLine ("Could not identify any files");
				}
				foreach (string g_file in g_files) {
					IEnumerable	<double[]> reader = ReadFromFile (g_file);
					int n_lines = reader.Count ();
					input_data [i] = new double[n_lines][];
					int ln = 0;
					foreach (double[] line in reader) {
						input_data [i] [ln] = line;
						ln++;
					}
					output [i] = g;
					i++; //Move to the next file in the counter
				}
			}
			reverse_features_and_times ();
		}


		public double[][][] getInput() {
			return input_data;
		}

		public string[] getOutput() {
			return output;
		}
			

		private void write_data_file(string dir) {
			string fn = dir + "/" + data_name.Split(new char[]{'/','\\'}).Last();
			if (File.Exists(fn)) { 
				Console.WriteLine ("Deleting old condensed data file");
				File.Delete (fn); 
			}
			using (StreamWriter writer = File.CreateText(fn)) {
				Console.WriteLine ("Writing new condensed data file");
				for (int d = 0; d <= this.input_data.GetUpperBound(0); d++) {
					writer.Write (this.output [d]);
					for (int f = 0; f <= this.input_data [d].GetUpperBound(0); f++) {
						writer.Write (',');
						for (int t = 0; t <= this.input_data[d][f].GetUpperBound(0); t++) {
							if (t > 0) {
								writer.Write (':');
							}
							writer.Write (this.input_data [d] [f] [t]);
						}

					}
					writer.WriteLine ();
				}
			}
			this.data_name = fn;
		}

		private void write_names_file(string dir) {
			string fn = dir + "/" +  names_name.Split(new char[]{'/','\\'}).Last();
			if (File.Exists(fn)) { 
				Console.WriteLine ("Deleting old condensed names file");
				File.Delete (fn); 
			}
			using (StreamWriter writer = File.CreateText(fn)) {
				Console.WriteLine ("Writing new condensed names file");
				//writer.WriteLine ("Gesture\t\t output\t\t " + string.Join (",", classes));
				foreach (string label in Enum.GetNames(typeof(DataFieldLabels))) {
					writer.WriteLine ("{0}\t\t continuous\t\t -360, 360", label); //TODO just uses max of our current data, not great!!
				}
				//writer.WriteLine ("Gesture\t\t output\t\t " + string.Join (",", classes));
			}
			names_name = fn;
		}

		public void write_files(bool write_names = false) {
			write_data_file (directory);
			if (write_names) {
				write_names_file (directory);
			} else {
				names_name = directory + "/" +  names_name.Split(new char[]{'/','\\'}).Last();

			}		
		}
	}
}

