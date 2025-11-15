using System.Text.Json;

namespace LIB_Define.RPC;

public class JsonHelper
{
    private static readonly JsonSerializerOptions IndentedOptions = new()
    {
        WriteIndented = true
    };

    private static readonly JsonSerializerOptions CamelCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };


    /// <summary>
    /// JSON 驗證
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static bool IsValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return false;

        try
        {
            JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 格式化 JSON
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static string FormatJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(doc, IndentedOptions);
    }

    public static bool TryFormatJson(string json, out string formatted)
    {
        formatted = string.Empty;
        try
        {
            formatted = FormatJson(json);
            return true;
        }
        catch
        {
            return false;
        }
    }


    /// <summary>
    /// 壓縮成一行（Minify）
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static string MinifyJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(doc);
    }


    /// <summary>
    /// 泛型：序列化物件 → JSON
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="indented">縮排</param>
    /// <returns></returns>
    public static string ToJson<T>(T obj, bool indented = false)
    {
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            WriteIndented = indented
        });
    }

    public static bool TryToJson<T>(T obj, out string json, bool indented = false)
    {
        json = string.Empty;
        try
        {
            json = ToJson(obj, indented);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 泛型：JSON → 物件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json"></param>
    /// <returns></returns>
    public static T? FromJson<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json);
    }


    public static bool TryFromJson<T>(string json, out T? result)
    {
        result = default;
        try
        {
            result = JsonSerializer.Deserialize<T>(json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 深度複製物件（利用 JSON)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T DeepCopy<T>(T obj)
    {
        var json = JsonSerializer.Serialize(obj);
        return JsonSerializer.Deserialize<T>(json)!;
    }

    /// <summary>
    /// 比對兩個 JSON 是否相同（忽略順序）
    /// </summary>
    /// <param name="json1"></param>
    /// <param name="json2"></param>
    /// <returns></returns>
    public static bool JsonEquals(string json1, string json2)
    {
        try
        {
            using var d1 = JsonDocument.Parse(json1);
            using var d2 = JsonDocument.Parse(json2);
            return JsonElementEquality(d1.RootElement, d2.RootElement);
        }
        catch
        {
            return false;
        }
    }

    private static bool JsonElementEquality(JsonElement x, JsonElement y)
    {
        if (x.ValueKind != y.ValueKind)
            return false;

        switch (x.ValueKind)
        {
            case JsonValueKind.Object:
                var xProps = x.EnumerateObject();
                var yProps = y.EnumerateObject();

                foreach (var p in xProps)
                {
                    if (!y.TryGetProperty(p.Name, out var yp))
                        return false;

                    if (!JsonElementEquality(p.Value, yp))
                        return false;
                }
                return true;

            case JsonValueKind.Array:
                var xArr = x.EnumerateArray();
                var yArr = y.EnumerateArray();

                var xEnum = xArr.GetEnumerator();
                var yEnum = yArr.GetEnumerator();

                while (xEnum.MoveNext())
                {
                    if (!yEnum.MoveNext())
                        return false;

                    if (!JsonElementEquality(xEnum.Current, yEnum.Current))
                        return false;
                }
                return !yEnum.MoveNext();

            default:
                return x.ToString() == y.ToString();
        }
    }
}