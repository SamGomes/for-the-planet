using Thalamus;

namespace ForThePlanetParallelScreens
{
    public interface IGMTablets : IPerception
    {
        void AllConnected();
    }


    public interface ITabletsGM : IAction
    {
        void ConnectToGM(string id, string name);
    }
}
