namespace SkyClerik.Utils
{
    public interface IChain
    {
        IChain NextComponent { get; set; }
        void ExecuteStep();
    }
}
