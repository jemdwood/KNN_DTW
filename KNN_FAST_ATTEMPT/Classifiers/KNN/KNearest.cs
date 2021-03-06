﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using KNN.Data;
using KNN;
using System.Text.RegularExpressions;
using System.Diagnostics;


namespace KNN.Classifiers.KNN {
    public class KNearest : IClassifier {
        private readonly DataSet m_DataSet;
		private bool storeDynamically;
		public List<double> dtw_time_taken = new List<double>();

		public int? SakoeChiba { get; set; } = null;

		private Dictionary<string, double> dynamicDistanceRecords;
        public int K { get; set; }
        public List<int> Features { get; set; }

		public KNearest(DataSet data, bool storeDynamically = false) {
			this.storeDynamically = storeDynamically;
            m_DataSet = data;
            K = 0;
			dynamicDistanceRecords = new Dictionary<string, double> ();
        }
        public void AppendData(List<DataInstance> data) {
            m_DataSet.DataEntries.AddRange(data);
        }
        public void AppendFeatures(List<Feature> features) {
            m_DataSet.Features.AddRange(features);
        }
        public void ClearData() {
            m_DataSet.DataEntries.Clear();
        }
        public double Test(List<DataInstance> testData) {

			var est = Estimate(testData, Features, K, m_DataSet.OutputValues).Value;
			Console.WriteLine ("time ticks: " + (1.0*dtw_time_taken.Average(p=>p)/testData.Count).ToString());
			Console.WriteLine ("accuracy:" + est.ToString ());
			return est;
        }

        /// <summary>
        /// Iterates through all possible values of K, finding K nearest neighbors
        /// and returning the optimal K for the given attributes.
        /// </summary>
        /// <param name="indices">Attributes to use in distance computation</param>
        /// <returns>KVP[int,double]</returns>
        public KeyValuePair<int,double> Tune(List<int> indices) {
            Console.WriteLine("Tuning Feature Set: <{0}>", string.Join(", ", indices.Select(i=>m_DataSet.Features[i].Name).ToArray()));
            DateTime start = DateTime.Now;
            var optimal = new KeyValuePair<int, double>(0,0); 
            var outputValues = m_DataSet.OutputValues;
			for(int i=1, k=4; k< (int) m_DataSet.DataEntries.Count/4.0; k=(int)Math.Pow(2, i)-1, i++) { // I narrowed the scope a bit
				var estimate = Estimate(m_DataSet.DataEntries, indices, k, outputValues);
				if (estimate.Value > optimal.Value) {
					optimal = estimate;
				} else if ((estimate.Value + 0.1 < optimal.Value) || (Math.Abs (Math.Log (optimal.Key + 1) / Math.Log (2) - i) > 3) ) {
					break; //End prematurely if the optimal seems to have enough of an advantage or the optimal hasn't changed in over 3 rounds 
				} 
                Console.WriteLine("K:{0} -- Estimate:{1:0.##}%", estimate.Key, estimate.Value * 100.0);
            }
            Console.WriteLine("Best K={0} with Estimate:{1:0.##}% found in {2}", optimal.Key, optimal.Value * 100.0, DateTime.Now.Subtract(start));
            return optimal;
        }

		private string getMaxLabel(SortedDictionary<string, int> outputs) {
			int max = 0;
			string label = "NULL";
			foreach (string k in outputs.Keys) {
				if (outputs [k] == max) {
					label = "NULL";
				}
				else if (outputs [k] > max) {
					label = k;
					max = outputs [k];
				}
			}
			return label;
		}

		public string Classify(DataInstance instance, string[] outputOptions = default(string[]), List<int> indices = default(List<int>))
        {
			if (outputOptions == null || outputOptions.Count() == 0){
				outputOptions = m_DataSet.OutputValues;
			}
			if (indices == null || indices.Count() == 0) {
				indices = this.Features;
			}
            if (K == 0)
            {
                K = Tune(indices).Key;
        
            }
            List<KeyValuePair<double, DataInstance>> neighbors = FindNearestNeighbors(instance, indices, K);
            KeyValuePair<string, double> result = extractClassificationFromNeighbors(outputOptions, neighbors);
            Console.WriteLine("Label:{0} -- Accuracy:{1:0.##}%", result.Key, result.Value * 100.0);
            return result.Key;
        }

        private KeyValuePair<string, double> extractClassificationFromNeighbors(string[] outputValues, List<KeyValuePair<double, DataInstance>> neighbors)
        {
            SortedDictionary<string, int> outputs = new SortedDictionary<string, int>();
            foreach (string output in outputValues)
            {
				outputs.Add(output, neighbors.Count(n => n.Value.getOutput() == output));
            }
            string label = getMaxLabel(outputs);

			return new KeyValuePair<string, double>(label, (outputs[label]+1.0) / (this.K+1.0));
        }

        /// <summary>
        /// Given a list of attributes, k neighbors and the target concept values,
        /// return a KVP(int,double) containing the estimate based on the k value.
        /// </summary>
        /// <param name="indices">Indices of attributes to use in our estimate</param>
        /// <param name="k">Number of nearest neighbors to find</param>
        /// <param name="outputValues">String representation of our two target concept values</param>
        /// <returns>KVP(int,double)</returns>
        private KeyValuePair<int,double> Estimate(List<DataInstance> data, List<int> indices, int k, string[] outputValues) {
            double correct = 0;
			Stopwatch stopWatch = new Stopwatch (); 
            foreach(DataInstance instance in data) {
				Console.Write (".");
				stopWatch.Reset ();
				stopWatch.Start (); 
				//Console.Write (c++ / data.Count ());
				//Console.Write ("\tdone with estimate\n");
                var neighbors = FindNearestNeighbors(instance, indices, k);
                string label = extractClassificationFromNeighbors(outputValues, neighbors).Key;
				if (instance.getOutput() == label) {
					correct++;
				}
				stopWatch.Stop (); //TODO TODO
				dtw_time_taken.Add(stopWatch.ElapsedTicks); //TODO TODO
            }
			Console.WriteLine ();
            return new KeyValuePair<int,double>(k, correct/(double)data.Count);
        }

        /// <summary>
        /// Given a single tuning data instance and attribute indices, find the
        /// k nearest neighbors based on Euclidean distance.
        /// </summary>
        /// <param name="tune">Single tuning instance</param>
        /// <param name="indices">Indices of features used</param>
        /// <param name="k">Number of neighbors to find</param>
        /// <returns>KVP(double,DataInstance)</returns>
        private List<KeyValuePair<double, DataInstance>> FindNearestNeighbors(DataInstance tune, List<int> indices, int k) {
			var neighbors = new SortedList<double, DataInstance> ();
			for (int i = 0; i < m_DataSet.DataEntries.Count; i ++) {
				//if(m_DataSet.DataEntries[i] == tune) continue; Take out for optimization when always running on new data
				double distance = ComputeDistance(tune, m_DataSet.DataEntries[i], indices);
				if (double.IsInfinity (distance))
					continue;
				neighbors.Add (distance, m_DataSet.DataEntries [i]);
			}
//			foreach(DataInstance trainingInstance in m_DataSet.DataEntries) {
//				
//                if(trainingInstance == tune) continue;
//                double distance = ComputeDistance(tune, trainingInstance, indices);
//                neighbors.Add(new KeyValuePair<double, DataInstance>(distance, trainingInstance));
//            }

            return neighbors.Take(k).ToList();
        }

        /// <summary>
        /// Computes the Euclidean distance between the DataInstances tune/train using
        /// the features located at the given indices.
        /// </summary>
        /// <param name="indices">Indices of the features used</param>
        /// <param name="tune">Single tuning instance</param>
        /// <param name="train">Single training instance</param>
        /// <returns>Double</returns>
        private double ComputeDistance(DataInstance tune, DataInstance train, IEnumerable<int> indices) {

			double d = 0;
            foreach(int i in indices) {
				double add =0;
				string signature = default(string);
				if (storeDynamically) {
					List<string> signature_l = new List<string> { (i*10).ToString(), tune.GetHashCode ().ToString(), train.GetHashCode ().ToString() };
					signature_l.Sort ();
					signature = string.Join (":", signature_l.ToArray());
				} 
				if (!storeDynamically || !dynamicDistanceRecords.ContainsKey (signature)) {
					switch (m_DataSet.Features [i].Type) {
					case Types.continuous:
						add = Distance (tune [i], train [i]);
						break;
					case Types.discrete:
						add = (tune [i] == train [i]) ? 0 : 1;
						break;
					}
					if (storeDynamically) dynamicDistanceRecords.Add (signature, add);
				} else {
					add = dynamicDistanceRecords [signature];
				}
				d += add;
            }


            return Math.Sqrt(d);
        }

        /// <summary>
        /// Given two values, compute (x - y)^2.
        /// Subroutine for Euclidean Distance computation.
        /// </summary>
        /// <param name="tune">Value of colon-separated times from our local tuning set.</param>
		/// <param name="train">Value of colon-separated times from our local training set.</param>
        /// <returns>Double</returns>
		private static double Distance(double[] d_tunes, double[] d_trains) {

			//string arr_regex_line = @"([0-9\-+\.]+(?:\x3A)?)"; // Matches any decimal, optionally followed by ":"
			//MatchCollection tune_time_series_col = Regex.Matches(tune, arr_regex_line, RegexOptions.IgnorePatternWhitespace);

//			string[] s_tunes = tune.Split (':'); //new string[tune_time_series_col.Count];
			//tune_time_series_col.CopyTo (s_tunes, 0);

			//MatchCollection train_time_series_col = Regex.Matches(tune, arr_regex_line, RegexOptions.IgnorePatternWhitespace);
//			string[] s_trains = train.Split(':'); //new string[train_time_series_col.Count];
			//train_time_series_col.CopyTo (s_trains, 0);
//			double[] d_tunes = Array.ConvertAll(s_tunes, new Converter<string, double>(stringToDouble));
//			double[] d_trains = Array.ConvertAll(s_trains, new Converter<string, double>(stringToDouble));

		
//			double dtw = UCRCSharp.UCR.DTW (d_trains, d_tunes, d_tunes.Length, true, 0.3);

			return new Dtw (d_tunes, d_trains, DistanceMeasure.SquaredEuclidean,true, true, null, null, 8).GetCost ();
        }

		private static double stringToDouble(string s) {
		//	double d = double.NaN;
		//	double.TryParse (s, out d);
		//	if (double.IsNaN(d)) {
		//		Console.Write("Could not convert to double: " + s);
		//	} 
		//	return d; 
			return double.Parse(s);
		} 
		
    }
}
