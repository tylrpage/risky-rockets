using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using System;

public class TrashManager : MonoBehaviour
{
    public static TrashManager Instance;
    public List<Trash> TrashInShip;
    private const float TRASH_BURN_DELAY = 1f;
    private const int SPECIAL_TRASH_THRESHHOLD = 10;
    [SerializeField] private Rocket _rocket;
    public event Action ShipEmptied;
    
    private void Awake()
    {
        if (Instance)
        {
            Destroy(this.gameObject);
        }
        Instance = this.gameObject.GetComponent<TrashManager>();
    }

    private void Start()
    {
        LaunchManager.Instance.LaunchStarted += BeginEjection;
        TrashInShip = new List<Trash>();
    }

    public void AddTrashToShip(Trash trash)
    {
        TrashInShip.Add(trash);
    }

    private void BeginEjection()
    {
        StartCoroutine(EjectTrash());
    }

    private IEnumerator EjectTrash()
    {
        yield return new WaitForSeconds(5f);
        while (TrashInShip.Count > 0)
        {
            Trash trash = TrashInShip[0];
            trash.Eject();
            if (trash.FuelValue >= SPECIAL_TRASH_THRESHHOLD)
            {
                StartCoroutine(_rocket.Boost());
                yield return new WaitForSeconds(Rocket.BOOST_DURATION + TRASH_BURN_DELAY);
            }
            else
            {
                yield return new WaitForSeconds(TRASH_BURN_DELAY);
            }
            TrashInShip.RemoveAt(0);
        }
        _rocket.StopShake();
        ShipEmptied?.Invoke();
    }
}
