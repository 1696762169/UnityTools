/// <summary>
/// 全局单例对象基类
/// </summary>
public abstract class SingletonBase<T> where T : SingletonBase<T>, new()
{
    public static T Instance { get; protected set; } = new();
}
