public enum AlgorithmType
{
    AStar,
    RTT,
}

public static class AlgorithmFactory
{

    public static IAlgorithm GetAlgorithm(AlgorithmType type)
    {
        return type switch
        {
            AlgorithmType.AStar => new AStar(),
            _ => null
        };
    }
}