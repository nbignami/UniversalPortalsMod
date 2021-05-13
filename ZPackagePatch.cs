using UnityEngine;

public static class ZPackageExtensions
{
    public static ZPackage FromObject<T>(this ZPackage package, T body)
    {
        package.Clear();
        package.SetPos(0);
        package.Write(JsonUtility.ToJson(body, true));
        return package;
    }
    public static T ToObject<T>(this ZPackage package)
    {
        var json = package.ReadString();

        return JsonUtility.FromJson<T>(json);
    }
}
