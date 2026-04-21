using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Longinus.PlotSystem
{
    /// <summary>
    /// Прагматичний хаб. Тільки читає/пише стан і кричить на всю гру, якщо щось змінилося.
    /// </summary>
    public class PlotManager : MonoBehaviour
    {
        public static PlotManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private PlotState _plotState;

        [Header("Global Events")]
        [Tooltip("Спрацьовує, коли будь-який прапорець встановлюється. Ідеально для оновлення UI або локацій.")]
        public UnityEvent<string> OnFlagUpdated;

        public PlotState PlotState => _plotState;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Перевіряє список умов (наприклад, для активації діалогу чи об'єкта)
        /// </summary>
        public bool AreConditionsMet(List<PlotCondition> conditions)
        {
            if (conditions == null || conditions.Count == 0) return true;

            foreach (var condition in conditions)
            {
                if (!condition.IsMet(_plotState)) return false;
            }
            return true;
        }

        /// <summary>
        /// Застосовує список наслідків (наприклад, після вбивства боса)
        /// </summary>
        public void ApplyConsequences(List<PlotConsequence> consequences)
        {
            if (consequences == null || _plotState == null) return;

            foreach (var consequence in consequences)
            {
                consequence.Apply(_plotState);
                
                // Якщо ми щойно поставили новий прапорець - повідомляємо всіх слухачів
                if (consequence.SetFlag && !string.IsNullOrEmpty(consequence.FlagToSet))
                {
                    OnFlagUpdated?.Invoke(consequence.FlagToSet);
                }
            }
        }

        // --- Зручні шорткати для швидкого виклику зі скриптів зброї чи боса ---

        public void TriggerFlag(string flagId)
        {
            if (_plotState == null || string.IsNullOrEmpty(flagId)) return;
            
            _plotState.SetFlag(flagId);
            OnFlagUpdated?.Invoke(flagId);
        }

        public bool CheckFlag(string flagId)
        {
            return _plotState != null && _plotState.HasFlag(flagId);
        }
    }
}