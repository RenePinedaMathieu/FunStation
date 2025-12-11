using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using SinmiStation.Core;

namespace SinmiStation.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject namePanel;
        public GameObject mainMenuPanel;

        [Header("Name UI")]
        public TMP_InputField nameInput;
        public TextMeshProUGUI nameLabelError; // opcional, para mensajes tipo "escribe tu nombre"

        [Header("Main Menu UI")]
        public TextMeshProUGUI welcomeText;

        [Header("Scene names")]
        public string firstGameSceneName = "GamePlaceholder";

        private void Start()
        {
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (UserProfile.HasUserName)
            {
                // Mostrar menú principal
                namePanel.SetActive(false);
                mainMenuPanel.SetActive(true);

                if (welcomeText != null)
                {
                    welcomeText.text = $"Welcome, {UserProfile.UserName}!";
                }
            }
            else
            {
                // Pedir nombre
                namePanel.SetActive(true);
                mainMenuPanel.SetActive(false);

                if (nameInput != null)
                    nameInput.text = "";
                if (nameLabelError != null)
                    nameLabelError.text = "";
            }
        }

        // Llamar desde el botón de "Confirmar nombre"
        public void OnConfirmNameButton()
        {
            string entered = nameInput != null ? nameInput.text : "";

            if (string.IsNullOrWhiteSpace(entered))
            {
                if (nameLabelError != null)
                    nameLabelError.text = "Please enter a name 🙂";
                return;
            }

            UserProfile.SetUserName(entered);
            RefreshUI();
        }

        // Llamar desde el botón Play
        public void OnPlayButton()
        {
            if (!UserProfile.HasUserName)
            {
                // Seguridad extra: si no hay nombre, volvemos al panel
                RefreshUI();
                return;
            }

            if (!string.IsNullOrEmpty(firstGameSceneName))
            {
                SceneManager.LoadScene(firstGameSceneName);
            }
            else
            {
                Debug.LogWarning("MainMenuController: firstGameSceneName está vacío.");
            }
        }

        // Llamar desde el botón Quit (solo PC, en móvil no hace mucho)
        public void OnQuitButton()
        {
            Debug.Log("Quit game requested");
            Application.Quit();
        }
    }
}
