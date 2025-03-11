using UnityEngine;

public class BountyCoin : Coin
{
    public override int Collect()
    {
        if (IsServer)
        {
            if (alreadyCollected) return 0;

            alreadyCollected = true;

            Destroy(gameObject);
            return coinValue;
        }
        else
        {
            Show(false);
            return 0;
        }
    }
}
