namespace ChiaWorks.FutureBot.Services
{
    public interface IFutureService
    {
        void Buy(string coin, float price, long size);
        void Sell(string coin, float price, long size);
        void Test();
    }
}