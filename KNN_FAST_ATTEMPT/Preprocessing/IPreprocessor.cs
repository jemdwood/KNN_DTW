namespace KNN.Preprocessing
{
    public interface IPreprocessor
    {
        double[] Preprocess(double[] data);
        string ToString();
    }
}