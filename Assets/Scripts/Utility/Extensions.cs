using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class IEnumerableExtensions
{

    public static T RandomElementByWeight<T>(this IEnumerable<T> sequence, System.Func<T, float> weightSelector)
    {
        float totalWeight = sequence.Sum(weightSelector);
        // The weight we are after...
        float itemWeightIndex = Random.Range(0f, 1f) * totalWeight;
        float currentWeightIndex = 0;

        foreach (var item in sequence)
        {
            currentWeightIndex += weightSelector(item);

            // If we've hit or passed the weight we are after for this item then it's the one we want....
            if (currentWeightIndex >= itemWeightIndex)
                return item;

        }

        return sequence.LastOrDefault();
    }
}
