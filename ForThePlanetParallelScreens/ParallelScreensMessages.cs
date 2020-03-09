using Thalamus;

namespace ForThePlanetParallelScreens
{
    public interface IGMTablets : IPerception
    {
        void AllConnected(string p0Id, string p0Name, string p1Id, string p1Name, string p2Id, string p2Name);
    }


    public interface ITabletsGM : IAction
    {
        void ConnectToGM(string id, string name);
        void SendBudgetAllocation(int economyAllocation, int environmentAllocation);
    }
}
