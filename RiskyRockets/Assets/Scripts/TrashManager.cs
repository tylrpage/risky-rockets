using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashManager : MonoBehaviour
{
    public static TrashManager Instance;
    public List<Trash> TrashInShip;
    [SerializeField] private Rocket _rocket;
    public event Action ShipEmptied;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        TrashInShip = new List<Trash>();
    }

    private void Start()
    {
        LaunchManager.Instance.LaunchStarted += BeginBurn;
    }

    private void OnDestroy()
    {
        if (LaunchManager.Instance != null) LaunchManager.Instance.LaunchStarted -= BeginBurn;
    }

    public void AddTrashToShip(Trash trash)
    {
        TrashInShip.Add(trash);
    }

    public void PrepareCargo(float gravityScale)
    {
        TrashInShip.RemoveAll(trash => trash == null);

        foreach (Trash trash in TrashInShip)
        {
            trash.PrepareForLaunch(gravityScale);
        }
    }

    public void DestroyTrash(Trash trash)
    {
        TrashInShip.Remove(trash);
        Destroy(trash.gameObject);
    }

    public List<Transform> FreezeWorldTrash()
    {
        HashSet<Trash> cargo = new(TrashInShip);
        List<Transform> frozen = new();
        foreach (Trash trash in FindObjectsByType<Trash>(FindObjectsSortMode.None))
        {
            if (cargo.Contains(trash)) continue;
            trash.Freeze();
            frozen.Add(trash.transform);
        }
        return frozen;
    }

    private Trash TakeLowestTrash()
    {
        TrashInShip.RemoveAll(trash => trash == null);
        if (TrashInShip.Count == 0) return null;

        int lowest = 0;
        for (int i = 1; i < TrashInShip.Count; i++)
        {
            if (BottomY(TrashInShip[i]) < BottomY(TrashInShip[lowest])) lowest = i;
        }
        Trash result = TrashInShip[lowest];
        TrashInShip.RemoveAt(lowest);
        return result;
    }

    private static float BottomY(Trash trash)
    {
        Collider2D col = trash.GetComponent<Collider2D>();
        return col != null ? col.bounds.min.y : trash.transform.position.y;
    }

    private void BeginBurn()
    {
        StartCoroutine(BurnTrash());
    }

    private IEnumerator BurnTrash()
    {
        LaunchManager launch = LaunchManager.Instance;
        yield return new WaitForSeconds(launch.IgnitionDelay);

        // Logged at ignition so it reflects any trash the nose cone destroyed.
        TrashInShip.RemoveAll(t => t == null);
        int totalFuel = 0;
        foreach (Trash t in TrashInShip) totalFuel += t.FuelValue;
        Debug.Log($"Launching with {totalFuel} fuel (target {launch.TargetFuel})");

        float burnTime = launch.BurnTimePerItem;
        // Pick the lowest piece each time, since the stack shifts as it settles.
        Trash trash;
        while ((trash = TakeLowestTrash()) != null)
        {
            _rocket.AddBurn(trash.FuelValue * launch.DeltaVPerFuel, burnTime);
            trash.Burn(burnTime);
            yield return new WaitForSeconds(burnTime);
        }
        _rocket.StopShake();
        ShipEmptied?.Invoke();
    }
}
