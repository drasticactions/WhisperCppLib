using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhisperCppLib;

public interface IWhisperSamplingStrategy
{
    public WhisperSamplingStrategy GetNativeStrategy();
}

public class GreedySamplingStrategy : IWhisperSamplingStrategy
{
    public WhisperSamplingStrategy GetNativeStrategy()
    {
        return WhisperSamplingStrategy.StrategyGreedy;
    }

    public int? BestOf { get; set; }
}

public class BeamSearchSamplingStrategy : IWhisperSamplingStrategy
{
    public WhisperSamplingStrategy GetNativeStrategy()
    {
        return WhisperSamplingStrategy.StrategyBeamSearch;
    }

    public int? BeamSize { get; set; }

    public float? Patience { get; set; }
}