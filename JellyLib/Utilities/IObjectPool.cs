namespace JellyLib.Utilities
{
    public interface IObjectPool<T>
    {
        T RequestObject();
        void Pool(T obj);
        void Clear();
        int Count();
    }
}

