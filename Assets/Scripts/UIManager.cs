using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static int GoldCount = 0;

    [SerializeField] private Text goldCountLabel = null;

    public void RefreshGoldHitCount()
    {
        GoldCount += 1;
        goldCountLabel.text = "Gold: " + GoldCount;
    }
}
