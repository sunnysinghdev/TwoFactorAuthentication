using TwoFactorAuthentication.Model;
namespace TwoFactorAuthentication.Data;
public interface IPhoneCodeRespository
{
    bool Exists(string phone);
    List<CodeDetail> Get(string phone);
    void Add(string phone, int code);
    void Remove(string phone, int code);
}
public class PhoneCodeRespository : IPhoneCodeRespository
{
    private Dictionary<string, List<CodeDetail>> _data;
    public PhoneCodeRespository()
    {
        _data = new Dictionary<string, List<CodeDetail>>();
    }
    public bool Exists(string phone)
    {
        if (_data.ContainsKey(phone))
            return true;
        return false;
    }
    public List<CodeDetail> Get(string phone)
    {
        return Exists(phone) ? _data[phone] : new List<CodeDetail>();
    }

    public void Add(string phone, int code)
    {
        var codeDetail = new CodeDetail();
        codeDetail.Code = code;
        codeDetail.ExpiryDateTime = DateTime.UtcNow;
        if (!_data.ContainsKey(phone))
            _data[phone] = new List<CodeDetail>();

        _data[phone].Add(codeDetail);
    }
    public void Remove(string phone, int code)
    {
        _data[phone].RemoveAll(x => x.Code == code);
    }

}