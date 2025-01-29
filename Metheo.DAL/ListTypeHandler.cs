namespace Metheo.DAL;

using Dapper;
using Newtonsoft.Json;
using System.Data;

public class ListDateTimeTypeHandler : SqlMapper.TypeHandler<List<DateTime?>>
{
    public override void SetValue(IDbDataParameter parameter, List<DateTime?> value)
    {
        parameter.Value = JsonConvert.SerializeObject(value);
    }

    public override List<DateTime?> Parse(object value)
    {
        return JsonConvert.DeserializeObject<List<DateTime?>>(value.ToString());
    }
}

public class ListFloatNullableTypeHandler : SqlMapper.TypeHandler<List<float?>>
{
    public override void SetValue(IDbDataParameter parameter, List<float?> value)
    {
        parameter.Value = JsonConvert.SerializeObject(value);
    }

    public override List<float?> Parse(object value)
    {
        return JsonConvert.DeserializeObject<List<float?>>(value.ToString());
    }
}

public class ListIntNullableTypeHandler : SqlMapper.TypeHandler<List<int?>>
{
    public override void SetValue(IDbDataParameter parameter, List<int?> value)
    {
        parameter.Value = JsonConvert.SerializeObject(value);
    }

    public override List<int?> Parse(object value)
    {
        return JsonConvert.DeserializeObject<List<int?>>(value.ToString());
    }
}