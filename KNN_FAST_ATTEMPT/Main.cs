using System;
using System.Linq;
using KNN.Classifiers.Selectors;
using KNN.Data;
using KNN.Classifiers.KNN;

namespace KNN {
    internal class RunGestures {
        private static void Main(string[] args) {
			var converter = new DataReaderConverter (new string[]{ "YES", "NO", "NULL" }, "./csv_data");
			converter.write_files (true);
			var builder = new DSBuilder (new string[]{converter.names_name, converter.data_name});
			DataSet data = builder.BuildDataSet ();
			var sets = data.RandomInstance(710);  //TODO TODO TODO a bad split!
			var knn = new KNearest(sets[0], true);
			//var fs = new FeatureSelector(knn);
			//var optimal = fs.ForwardFeatureSelect(Enumerable.Range(0, data.Features.Count - 1).Where(x => x != data.OutputIndex).ToList());
			//knn.K = optimal.Key;
			//knn.Features = optimal.Value;
			knn.K = 5; //TODO HARDCODED!
			knn.Features = new System.Collections.Generic.List<int>{3, 4, 2, 9, 8}; //TODO HARDCODED!
			string[] finalFeatures = knn.Features.Select(i => data.Features[i].Name).ToArray();
			Console.WriteLine("Final Result: {0:0.##}% with K:{1} using Features:{2}",
								knn.Test(sets[1].DataEntries)*100.0,
								//fs.Test(sets[1].DataEntries)*100.0, //TODO fix sets used, should be 1 tODO TODO TODO
			                  knn.K,
			                  string.Join(", ", finalFeatures));
			
//            if (args.Length != 2) {
//                Console.WriteLine("KNN.exe *.names *.data");
//                return;
//            }
//            DateTime start = DateTime.Now;
//            Console.WriteLine("Start Time: {0}", start);
//            var builder = new DSBuilder(args);
//            DataSet data = builder.BuildDataSet();
//
//            var sets = data.RandomInstance(800);
//            var knn = new KNearest(sets[0]);
//            var fs = new FeatureSelector(knn);
//            var optimal = fs.ForwardFeatureSelect(Enumerable.Range(0, data.Features.Count - 1).Where(x => x != data.OutputIndex).ToList());
//            knn.K = optimal.Key;
//            knn.Features = optimal.Value;
//            Console.WriteLine("Final Result: {0:0.##}% with K:{1} using Features:{2}",
//                              fs.Test(sets[1].DataEntries)*100.0,
//                              knn.K,
//                              string.Join(", ", knn.Features.Select(i => data.Features[i].Name).ToArray()));
//            Console.WriteLine("Run-Time: {0}", DateTime.Now - start);
        }
    }
}