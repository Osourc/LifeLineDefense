using UnityEngine;
using UnityEngine.UI;

public class SwipeManager : MonoBehaviour
{
    public GameObject scrollbar; // Scrollbar reference
    private float scroll_pos = 0; // Current scroll position
    private float[] pos; // Positions for buttons
    private bool isDragging = false; // Detect if dragging

    void Start()
    {
        pos = new float[transform.childCount];
        AssignButtonClickListeners(); // Assign click listeners to each button
    }

    void Update()
    {
        float distance = 1f / (pos.Length - 1f); // Calculate distance between positions

        for (int i = 0; i < pos.Length; i++)
        {
            pos[i] = distance * i; // Calculate positions
        }

        // Check if input is dragging
        if (Input.GetMouseButton(0) || Input.touchCount > 0)
        {
            isDragging = true; // Set dragging flag
            scroll_pos = scrollbar.GetComponent<Scrollbar>().value;
        }
        else
        {
            isDragging = false; // Reset dragging flag
            SnapToClosestButton(distance);
        }

        // Detect the currently hovered item
        DetectHoveredItem(distance);
    }

    private void SnapToClosestButton(float distance)
    {
        for (int i = 0; i < pos.Length; i++)
        {
            if (scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2))
            {
                scrollbar.GetComponent<Scrollbar>().value = Mathf.Lerp(scrollbar.GetComponent<Scrollbar>().value, pos[i], 0.1f);
            }
        }
    }

    private void DetectHoveredItem(float distance)
    {
        for (int i = 0; i < pos.Length; i++)
        {
            if (scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2))
            {
                // Scale the hovered button
                transform.GetChild(i).localScale = Vector2.Lerp(transform.GetChild(i).localScale, new Vector2(1f, 1f), 0.1f);

                // Scale down other buttons
                for (int a = 0; a < pos.Length; a++)
                {
                    if (a != i)
                    {
                        transform.GetChild(a).localScale = Vector2.Lerp(transform.GetChild(a).localScale, new Vector2(0.8f, 0.8f), 0.1f);
                    }
                }
            }
        }
    }

    private void AssignButtonClickListeners()
    {
        // Add a click listener to each child button
        for (int i = 0; i < transform.childCount; i++)
        {
            int index = i; // Prevent closure issues
            Button button = transform.GetChild(i).GetComponent<Button>();

            if (button != null)
            {
                button.onClick.AddListener(() => OnButtonClick(index));
            }
        }
    }

    private void OnButtonClick(int index)
    {
        // Set the scroll position to the clicked button
        scroll_pos = pos[index];
        scrollbar.GetComponent<Scrollbar>().value = scroll_pos;

        // Immediately update the hover state
        DetectHoveredItem(1f / (pos.Length - 1f));
    }
}
