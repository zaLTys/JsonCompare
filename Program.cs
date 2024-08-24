using JsonCompare;
using System;
using System.Collections.Generic;
using System.Text.Json;

class Program
{
    static void Main(string[] args)
    {
        // Parse the JSON strings
        using (JsonDocument doc1 = JsonDocument.Parse(JsonOld.Get()))
        using (JsonDocument doc2 = JsonDocument.Parse(JsonNew.Get()))
        {
            List<string> differences = new List<string>();
            bool areEqual = CompareJsonElements(doc1.RootElement, doc2.RootElement, differences, "");

            if (areEqual)
            {
                Console.WriteLine("JSON objects are equal.");
            }
            else
            {
                Console.WriteLine("JSON objects are not equal.");
                Console.WriteLine("Differences found:");
                foreach (var diff in differences)
                {
                    Console.WriteLine(diff);
                }
            }
        }
    }

    static bool CompareJsonElements(JsonElement elem1, JsonElement elem2, List<string> differences, string path)
    {
        if (elem1.ValueKind != elem2.ValueKind)
        {
            differences.Add($"ValueKind mismatch at {path}: {elem1.ValueKind} vs {elem2.ValueKind}");
            return false;
        }

        switch (elem1.ValueKind)
        {
            case JsonValueKind.Object:
                {
                    var obj1 = elem1.EnumerateObject();
                    var obj2 = elem2.EnumerateObject();

                    var obj1Dict = new Dictionary<string, JsonElement>();
                    foreach (var property1 in obj1)
                        obj1Dict[property1.Name] = property1.Value;

                    var obj2Dict = new Dictionary<string, JsonElement>();
                    foreach (var property2 in obj2)
                        obj2Dict[property2.Name] = property2.Value;

                    foreach (var kvp in obj1Dict)
                    {
                        if (!obj2Dict.TryGetValue(kvp.Key, out var elem2Value))
                        {
                            differences.Add($"Property {kvp.Key} is missing in the second JSON at {path}");
                            return false;
                        }

                        if (!CompareJsonElements(kvp.Value, elem2Value, differences, $"{path}.{kvp.Key}"))
                            return false;
                    }

                    foreach (var kvp in obj2Dict)
                    {
                        if (!obj1Dict.ContainsKey(kvp.Key))
                        {
                            differences.Add($"Property {kvp.Key} is missing in the first JSON at {path}");
                            return false;
                        }
                    }

                    return true;
                }

            case JsonValueKind.Array:
                {
                    var arr1 = elem1.EnumerateArray().ToArray();
                    var arr2 = elem2.EnumerateArray().ToArray();

                    if (arr1.Length != arr2.Length)
                    {
                        differences.Add($"Array length mismatch at {path}: {arr1.Length} vs {arr2.Length}");
                        return false;
                    }

                    // Sort the arrays by string representation for comparison
                    var sortedArr1 = arr1.OrderBy(x => x.ToString()).ToArray();
                    var sortedArr2 = arr2.OrderBy(x => x.ToString()).ToArray();

                    for (int i = 0; i < sortedArr1.Length; i++)
                    {
                        if (!CompareJsonElements(sortedArr1[i], sortedArr2[i], differences, $"{path}[{i}]"))
                            return false;
                    }

                    return true;
                }

            default:
                if (elem1.ToString() != elem2.ToString())
                {
                    differences.Add($"Value mismatch at {path}: {elem1} vs {elem2}");
                    return false;
                }
                return true;
        }
    }
}
