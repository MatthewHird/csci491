private bool EnumerableSetEquivalent<T>(IEnumerable<T> first, IEnumerable<T> second) where T: IIdentifiable
{
    if (first?.Count() != second?.Count()) { return false; }

    var idSet = new HashSet<string>();
    var firstDict = new Dictionary<string, T>();
    var secondDict = new Dictionary<string, T>();

    foreach (var item in first)
    {
        idSet.Add(item.Id);
        firstDict.Add(item.Id, item);
    }

    foreach (var item in second)
    {
        idSet.Add(item.Id);
        secondDict.Add(item.Id, item);
    }

    foreach (var id in idSet)
    {
        if (firstDict.TryGetValue(id, out T item1) && secondDict.TryGetValue(id, out T item2))
        {
            if (!Equivalent(item1, item2))
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    return true;
}

private bool Equivalent<T>(T first, T second) where T : IIdentifiable
{
    if (typeof(T) == typeof(TypeParameter))
    {
        return Equivalent((TypeParameter)(object)first, (TypeParameter)(object)second);
    }
    //else if (typeof(T) == typeof(FixedParameter))
    //{
    //    return Equivalent((FixedParameter)(object)first, (FixedParameter)(object)second);
    //}


    else
    {
        throw new Exception();
    }

}

public interface IIdentifiable
{
    string Id { get; }
}
