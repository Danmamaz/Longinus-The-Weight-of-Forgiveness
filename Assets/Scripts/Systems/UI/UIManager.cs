using System;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [SerializeField] private PlayerStatsManager playerStats;
    [SerializeField] private InteractionSystem interactionSystem;
    
    [Header("UI References")]
    [Header("Pause References")]
    [SerializeField] private GameObject pauseMenu;
    [Header("Player Stats")]
    [SerializeField] private TMP_Text healthText; 
    [SerializeField] private TMP_Text staminaText;
    [Header("Interactables")]
    [SerializeField] private TMP_Text interactableText;


    private bool isPaused = false;

    private void Awake()
    {
        playerStats.OnDamage.AddListener(ChangeHealthUI);
        playerStats.OnStaminaChange.AddListener(ChangeStaminaUI);
        interactionSystem.onInteractibleEnter.AddListener(AddInteractableToList);
        interactionSystem.onInteractibleLeave.AddListener(RemoveInteractableFromList);
        
        pauseMenu.SetActive(isPaused);

    }

    private void ChangeHealthUI(float currentHealth)
    {
        currentHealth =  playerStats.CurrentHealth;
        healthText.text = $"Health: {currentHealth}";
    }

    private void ChangeStaminaUI()
    {
        staminaText.text = $"Stamina: {playerStats.CurrentStamina}";
    }

    private void AddInteractableToList(List<IInteractable> interactables)
    {
        string textToShow = "";
        
        foreach (IInteractable i in interactables)
        {
            textToShow += i.GetInteractionText();
        }
        
        interactableText.text = $"Interactable: {textToShow}";
    }
    
    private void RemoveInteractableFromList(List<IInteractable> interactables)
    {
        string textToShow = "";
        
        foreach (IInteractable i in interactables)
        {
            textToShow += i.GetInteractionText();
        }
        
        interactableText.text = $"Interactable: {textToShow}";
    }

    public bool TogglePauseMenu()
    {
        if (pauseMenu == null) return false;
        isPaused = !isPaused;

        pauseMenu.SetActive(isPaused);

        Time.timeScale = isPaused ? 0 : 1;

        return isPaused;
    }

    public void ResumeGameButton()
    {
        TogglePauseMenu();


    }
}
