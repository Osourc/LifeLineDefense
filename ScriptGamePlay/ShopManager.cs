using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public LayerMask EnemyLayer;
    public float SlowDuration = 5f;
    public float speedMultiplier = 0.50f;

    public int AttackSpeedBuffCost = 50;
    public int SlowSerumCost = 75;
    public int ParacatukmolCost = 100;

    public float AttackSpeedBuffCooldown = 600f;
    public float SlowSerumCooldown = 600f;
    public float ParacatukmolCooldown = 900f;

    private bool isAttackSpeedBuffOnCooldown = false;
    private bool isSlowSerumOnCooldown = false;
    private bool isParacatukmolOnCooldown = false;

    public Button AttackSpeedBuffButton;
    public Button SlowSerumButton;
    public Button ParacatukmolButton;

    private DnaManager dnaManager;
    public TMP_Text dnaCountX;

    private void Start() 
    {
        dnaManager = DnaManager.Instance;
    }

    private void Update() 
    {
        dnaCountX.text = "DNA: " + dnaManager.DnaCount.ToString("00");
    }

    public void IncreaseAttackSpeedForAllTowers(float speedMultiplier)
    {
        if (dnaManager.DnaCount >= AttackSpeedBuffCost)
        {
            dnaManager.SpendDnaForBuff(AttackSpeedBuffCost);
            BaseCellTower[] allTowers = FindObjectsOfType<BaseCellTower>();
            foreach (var tower in allTowers)
            {
                tower.AdjustAttackSpeed(speedMultiplier);
            }

            Debug.Log("All towers' attack speeds increased.");
            StartCoroutine(Cooldown(AttackSpeedBuffCooldown, AttackSpeedBuffButton, () => isAttackSpeedBuffOnCooldown = false));
            isAttackSpeedBuffOnCooldown  = true;

        }
        else
        {
            Debug.Log("Not enough DNA for attack speed buff.");
            StartCoroutine(FlashButtonRed(AttackSpeedBuffButton));
        }
    }

    public void BuySlowSerum()
    {
        if (dnaManager.DnaCount >= SlowSerumCost)
        {
            dnaManager.SpendDnaForBuff(SlowSerumCost);
            // Find all BaseEnemyScript instances in the scene
            BaseEnemyScript[] enemies = FindObjectsOfType<BaseEnemyScript>();

            foreach (var enemy in enemies)
            {
                StartCoroutine(SlowEnemyTemporarily(enemy));
            }

            Debug.Log("Slow Serum purchased! Enemy speed reduced by 80%.");
            StartCoroutine(Cooldown(SlowSerumCooldown, SlowSerumButton, () => isSlowSerumOnCooldown = false));
            isSlowSerumOnCooldown = true;
        }
        else
        {
            Debug.Log("Not enough DNA for Slow Serum.");
            StartCoroutine(FlashButtonRed(SlowSerumButton));
        }
    }

    private IEnumerator SlowEnemyTemporarily(BaseEnemyScript enemy)
    {
        float originalSpeed = enemy.Speed;
        enemy.Speed *= 0.20f; // Reduce speed by 80%

        // Wait for the duration of the slow effect
        yield return new WaitForSeconds(SlowDuration);

        // Restore the original speed after the duration
        enemy.Speed = originalSpeed;
        Debug.Log($"{enemy.UniqueID} speed restored after {SlowDuration} seconds.");
    }

    public void BuyParacatukmol()
    {
        if (dnaManager.DnaCount >= ParacatukmolCost)
        {
            dnaManager.SpendDnaForBuff(ParacatukmolCost);
            BaseEnemyScript[] enemies = FindObjectsOfType<BaseEnemyScript>();
            foreach (var enemy in enemies)
            {
            enemy.TakeDamage(100);  // Deal 100 damage to each enemy
            }
            Debug.Log("Duke Nukem!");
            StartCoroutine(Cooldown(ParacatukmolCooldown, ParacatukmolButton, () => isParacatukmolOnCooldown = false));
            isParacatukmolOnCooldown = true;
        }
        else
        {
            Debug.Log("Not enough DNA for Paracatukmol.");
            StartCoroutine(FlashButtonRed(ParacatukmolButton));
        }
    }

    private IEnumerator Cooldown(float duration, Button button, System.Action onCooldownEnd)
    {
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        button.interactable = false;
        float remainingTime = duration;

        while (remainingTime > 0)
        {
            buttonText.text = $"{remainingTime:F1}s";
            yield return new WaitForSeconds(0.1f); // Update every 0.1 seconds for smooth countdown
            remainingTime -= 0.1f;
        }

        button.interactable = true;
        onCooldownEnd?.Invoke();
    }

    private IEnumerator FlashButtonRed(Button button)
    {
        Image buttonImage = button.GetComponent<Image>();
        Color originalColor = buttonImage.color;
        buttonImage.color = Color.red;

        yield return new WaitForSeconds(0.5f); // Change duration as needed

        buttonImage.color = originalColor;
    }
}
