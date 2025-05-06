using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    public Toggle myToggle; // Asigna el Toggle desde el Inspector
    public TMPro.TMP_Dropdown myDropdown; // Asigna el Dropdown desde el Inspector
    public Button myButton; // Asigna el botón desde el Inspector

    public bool toggleValue = false;

    public void ToggleValueChanged()
    {
        toggleValue = !toggleValue;
    }

    void Start()
    {
        if (myButton != null)
        {
            // Suscribe la función que se ejecutará cuando se presione el botón
            myButton.onClick.AddListener(OnButtonClick);
        }

        if (myDropdown != null) {
            myDropdown.onValueChanged.AddListener(OnDropdownChanged);
        }

        if (myToggle != null) {
            myToggle.onValueChanged.AddListener(OnToggleChanged);
        }

    }

    // Función que se ejecutará cuando se presione el botón
    private void OnButtonClick()
    {
        Debug.Log("El botón ha sido presionado");
    }


    private void OnDropdownChanged(int value)
    {
        Debug.Log($"El dropdown ha cambiado {value} en variable {myDropdown.value}");
    }

    private void OnToggleChanged(bool value)
    {
        Debug.Log($"El toggle ha cambiado {value} en variable {myToggle.isOn}");
    }
    // Opcional: Si el botón puede desuscribirse (por ejemplo, cuando se destruye el objeto)
    private void OnDestroy()
    {
        if (myButton != null)
        {
            myButton.onClick.RemoveListener(OnButtonClick);
        }

        if (myDropdown != null)
        {
            myDropdown.onValueChanged.RemoveListener(OnDropdownChanged);
        }

        if (myToggle != null)
        {
            myToggle.onValueChanged.RemoveListener(OnToggleChanged);
        }
    }
}
