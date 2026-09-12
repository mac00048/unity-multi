using PurrNet;
using TMPro;
using UnityEngine;

public class MainGameView : View
{
    [SerializeField] TMP_Text healthText;


    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);
    }

    private void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<MainGameView>();
    }
    public override void OnHide()
    {

    }

    public override void OnShow()
    {

    }

    public void UpdateHealth(int health)
    {
        if(health < 0)
            health = 0;
        
        healthText.text = health.ToString();
    }
}
