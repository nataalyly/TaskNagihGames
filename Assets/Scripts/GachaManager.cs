using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class GachaManager : MonoBehaviour
{
    public enum Rarity
    {
        Metal = 0,
        Bronze = 1,
        Silver = 2,
        Gold = 3,
        Diamond = 4
    }

    private int[] sellValues = { 10, 20, 30, 40, 50 };
    private int playerCoins = 200;
    private const int GachaCost = 30;
    private List<Rarity> storage = new List<Rarity>();

    // input field
    [SerializeField] private InputField metalInput;
    [SerializeField] private InputField bronzeInput;
    [SerializeField] private InputField silverInput;
    [SerializeField] private InputField goldInput;
    [SerializeField] private InputField diamondInput;

    // text
    [SerializeField] private Text totalCoinText;
    [SerializeField] private Text storageValueText;

    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Sprite[] frameSprites;

    void Start()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        totalCoinText.text = "Your total coin : " + playerCoins;
        storageValueText.text = "Storage coin value : " + GetStorageValue();
        RefreshStorage();
    }

    private void RefreshStorage()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < storage.Count; i++)
        {
            int index = i;
            GameObject item = Instantiate(itemPrefab, contentParent);
            item.GetComponent<Image>().sprite = frameSprites[(int)storage[i]];
            item.GetComponent<Button>().onClick.AddListener(() => SellItem(index));
        }
    }

    private Rarity RollRarity(float[] weights)
    {
        float total = 0f;
        foreach (float w in weights) total += w;

        float roll = Random.Range(0f, total);
        float cumulative = 0f; //total weight selama pengecekan

        for (int i = 0; i < weights.Length; i++)
        {
            cumulative += weights[i];
            if (roll < cumulative)
                return (Rarity)i;
        }

        for (int i = weights.Length - 1; i >= 0; i--)
            if (weights[i] > 0) return (Rarity)i;

        return Rarity.Metal; //pengaman klo roll = total
    }

    private void DoGacha(float[] weights)
    {
        if (playerCoins < GachaCost)
        {
            Debug.Log("Not enough coin");
            return;
        }

        float total = 0f;
        foreach (float w in weights) total += w;
        if (total == 0)
        {
            Debug.Log("Invalid percentages: total cannot be zero");
            return;
        }

        playerCoins -= GachaCost;
        Rarity result = RollRarity(weights);
        storage.Add(result);
        storage.Sort(); //urutin dari 0 ke 4
        storage.Reverse(); //balikin urutannya jadi 4 ke 0
        UpdateUI();
        CheckGameOver();
    }

    private float ReadWeight(InputField field)
    {
        float value;
        float.TryParse(field.text, out value);
        return Mathf.Max(0f, value); //angka negatif jadiin nol
    }

    private float[] GetWeights()
    {
        float w0 = ReadWeight(metalInput);
        float w1 = ReadWeight(bronzeInput);
        float w2 = ReadWeight(silverInput);
        float w3 = ReadWeight(goldInput);
        float w4 = ReadWeight(diamondInput);

        float[] hasil = { w0, w1, w2, w3, w4 };

        return hasil;
    }

    private int GetStorageValue()
    {
        int total = 0;
        foreach (Rarity r in storage)
        {
            total += sellValues[(int)r];
        }
        return total;
    }

    private void SellItem(int index)
    {
        playerCoins += sellValues[(int)storage[index]];
        storage.RemoveAt(index);
        UpdateUI();
        CheckGameOver();
    }

    public void OnGachaButtonClicked()
    {
        DoGacha(GetWeights());
    }

    private void CheckGameOver()
    {
        if (playerCoins + GetStorageValue() < GachaCost)
        {
            Debug.Log("Game Over");
        }
    }
}
