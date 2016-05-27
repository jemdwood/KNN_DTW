using System;
using System.Collections.Generic;
using KNN.Data;

namespace KNN.Classifiers.Selectors {
    public class FeatureSelector : IClassifier {
        private readonly IClassifier m_Classifier;
        
        public FeatureSelector(IClassifier classifier) {
            m_Classifier = classifier;
        }
        public void AppendData(List<DataInstance> trainData) {
            m_Classifier.AppendData(trainData);
        }
        public void AppendFeatures(List<Feature> features) {
            m_Classifier.AppendFeatures(features);
        }
        public void ClearData() {
            m_Classifier.ClearData();
        }
        public double Test(List<DataInstance> testData) {
            return m_Classifier.Test(testData); //WHY IS THIS RETURNING A NAN?????
        }
        public KeyValuePair<int,double> Tune(List<int> indices) {
            return m_Classifier.Tune(indices);
        }

        /// <summary>
        /// Iterates through all the given feature indices to find the optimal attributes to use for the best KNN estimate.
        /// </summary>
        /// <param name="features">List of Attribute indices</param>
        /// <returns>KVP[int,List[int]]</returns>
        public KeyValuePair<int, List<int>> ForwardFeatureSelect(List<int> features) {
            Console.WriteLine("Starting Forward Feature Select using {0} features.", features.Count + 1);
			List<int> autoInclude = new List<int> {2, 3};
			List<int> toExclude = new List<int> {			//TODO hardcoded
				(int) DataFieldLabels.Time, 
		//		(int) DataFieldLabels.QuatRotW, 
				(int) DataFieldLabels.QuatRotX,
				(int) DataFieldLabels.QuatRotY,
		//		(int) DataFieldLabels.QuatRotZ
            };
            var optimal = new KeyValuePair<int, double>(0,0);
            var optimalFeatures = autoInclude;
            bool foundAdditionalFeature;
            do {
                int bestIndex = 0;
                foundAdditionalFeature = false;
                foreach(int i in features) {
					if(optimalFeatures.Contains(i) || toExclude.Contains(i)) continue;
                    var estimate = m_Classifier.Tune(new List<int>(optimalFeatures) { i } );
                    if(estimate.Value > optimal.Value) {
                        optimal = estimate;
                        bestIndex = i;
                        foundAdditionalFeature = true;
                    }
                }
                if(foundAdditionalFeature)
                    optimalFeatures.Add(bestIndex);
				Console.WriteLine("Adding optimal index: {0} \t Total Feature Set: {1}", bestIndex, string.Join(", ", optimalFeatures.ConvertAll(x=>x.ToString()).ToArray()));

                Console.WriteLine("Optimal -- K:{0} Estimate:{1}%", optimal.Key, optimal.Value * 100.0);
            }
            while(foundAdditionalFeature);
            Console.WriteLine("Forward Feature Selection complete.");
            return new KeyValuePair<int, List<int>>(optimal.Key, optimalFeatures);
        }
    }
}
