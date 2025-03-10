using System;
using UnityEngine;

public class RespawningCoin : Coin
{
    public event Action<RespawningCoin> OnCollected;

    private Vector3 previousPosition;

    void Update()
    {
        if (previousPosition != transform.position)
        {
            Show(true);
        }

        previousPosition = transform.position;
    }

    public override int Collect()
    {
        if (IsServer)
        {
            if (alreadyCollected) return 0;

            alreadyCollected = true;

            OnCollected?.Invoke(this);
            return coinValue;
        }
        else
        {
            Show(false);
            return 0;
        }
    }

    public void Reset()
    {
        alreadyCollected = false;
    }
}
