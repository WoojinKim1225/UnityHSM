
/// <summary>
/// 멀티턴 클래스가 고유한 키를 반환하도록 요구하는 인터페이스.
/// </summary>
namespace HSM
{
    public interface IMultiton
    {
        string GetMultitonKey();
    }
}