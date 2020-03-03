using Thalamus;

namespace ForThePlanetParallelScreens
{
    public interface IGMTablets : IPerception
    {
        void ReceiveZ();
    }


    public interface ITabletsGM : IAction
    {
        void SendA();
    }
}
