using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class TrashManager : MonoBehaviour
{
    public static TrashManager Instance;
    public List<Trash> AllTrash;
    public List<Trash> TrashInShip;
    private const float TRASH_BURN_INTERVAL = 3.0f;
    private const int SPECIAL_TRASH_THRESHHOLD = 4;
    [SerializeField] private Rocket _rocket;
    
    private void Start()
    {
        LaunchManager.Instance.LaunchStarted += BeginEjection;
    }

    public void AddTrashToShip(Trash trash)
    {
        TrashInShip.Append(trash);
    }

    private void BeginEjection()
    {
        StartCoroutine(EjectTrash());
    }

    private IEnumerator EjectTrash()
    {
        while (TrashInShip.Count > 0)
        {
            yield return new WaitForSeconds(TRASH_BURN_INTERVAL);
            Trash trash = TrashInShip[0];
            trash.Eject();
            TrashInShip.RemoveAt(0);
            if (trash.FuelValue >= SPECIAL_TRASH_THRESHHOLD)
            {
                StartCoroutine(_rocket.Boost());
            }
        }
    }
}
