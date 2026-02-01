namespace SkyClerik.GlobalGameStates
{
    public interface IChain
    {
        IChain NextComponent { get; set; }
        void ExecuteStep();
    }
}
