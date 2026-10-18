using UnityEngine;
using System.Collections.Generic;

public static class ListExtensions
{
    public static void Shuffle<T>(this List<T> list)
    {
        int n = list.Count;
        for (int i = 0; i < n - 1; i++)
        {
            // Pick a random index from i to the end of the list
            int randomIndex = Random.Range(i, n);
            
            // Swap elements
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
}