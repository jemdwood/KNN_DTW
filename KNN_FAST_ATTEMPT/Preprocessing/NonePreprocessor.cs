﻿using System.Linq;

namespace KNN.Preprocessing
{
    public class NonePreprocessor : IPreprocessor
    {
        public double[] Preprocess(double[] data)
        {
            return data;
        }

        public override string ToString()
        {
            return "None";
        }
    }
}