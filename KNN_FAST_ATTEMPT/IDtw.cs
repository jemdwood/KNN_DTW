using System;
using KNN;

namespace KNN
{
    public interface IDtw
    {
        double GetCost();
        Tuple<int, int>[] GetPath();
        double[][] GetDistanceMatrix();
        double[][] GetCostMatrix();
        int XLength { get; }
        int YLength { get; }
        SeriesVariable[] SeriesVariables { get; }
    }
}
