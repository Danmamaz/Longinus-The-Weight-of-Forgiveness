using UnityEngine;
using PlotBranching;

public class Door : MonoBehaviour
{
    [Tooltip("Унікальний ID, який має збігатися з pathID у Consequence")]
    public string pathID; 

    private void Start()
    {
        if (PlotManager.Instance != null)
        {
            // Підписуємося на подію відкриття в реальному часі
            PlotManager.Instance.onPathOpened.AddListener(OnPathOpened);
            
            // Перевіряємо, чи були двері відкриті в попередніх сесіях (завантаження збереження)
            if (PlotManager.Instance.plotState.openedPathIDs.Contains(pathID))
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void OnPathOpened(string openedID)
    {
        if (pathID == openedID)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        // Завжди відписуйтесь від подій, щоб уникнути витоків пам'яті
        if (PlotManager.Instance != null)
        {
            PlotManager.Instance.onPathOpened.RemoveListener(OnPathOpened);
        }
    }
}