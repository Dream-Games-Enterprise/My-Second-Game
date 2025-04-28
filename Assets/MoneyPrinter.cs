using UnityEngine;

public class MoneyPrinter : MonoBehaviour
{
    int currency;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            AddCheatCurrency(1000); // adds 1000 currency
        }
    }

    public void AddCheatCurrency(int amount)
    {
        int current = PlayerPrefs.GetInt("currency", 0);
        int newTotal = current + amount;
        PlayerPrefs.SetInt("currency", newTotal);
        Debug.Log("CHEAT: Currency set to " + newTotal);
    }

    [ContextMenu("Add 1000 Currency (Cheat)")]
    void CheatAddCurrency()
    {
        AddCheatCurrency(1000);
    }

}
