using UnityEngine;
using System.Collections;
using System.Reflection;

/// <summary>
/// @Author Calvin Truong
/// @Version 1.0
/// 
/// @Description: When the button ("btn_Towers) is clicked, call the onClick() function
/// which will proceed to create an array of tower buttons based on the towers in the
/// towers array.
/// 
/// </summary>
public class GUI_PanelInterface : MonoBehaviour
{

    public ButtonScript buttonTemplate;
    public GameObject panel;
    public GameObject[] antiViruses;
    public GameObject[] viruses;

    public int offsetX;

    private ArrayList buttons;
    private bool showButtons;

    private int buttonStatus; // 0 = None, 1 = AntiVirus, 2 = Virus
    private TowerAI selectedTower;



    // Use this for initialization
    void Start()
    {
        buttons = new ArrayList();
        showButtons = false;
        buttonStatus = 0;
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Instantiates tower buttons based on the number of elements in the towers array.
    /// The tower buttons's text is based on the element's name.
    /// </summary>
    public void onClick(int status)
    {
        if (status != 0 && status != 1 && status != 2)
            return;

        if (buttonStatus != status && showButtons)
            removeButtons();

        if (!showButtons)
        {
            buttonStatus = status;

            // Hard-coded value
            int offsetXtmp = this.offsetX;
            int offsetY = 0;

            // Get the Panel's Position
            Vector3 panelPosition = panel.transform.position;

            // Get the Button's Rect Transformation
            Rect buttonRect = buttonTemplate.gameObject.GetComponent<RectTransform>().rect;

            // Get the Panel's Rect Transformation
            Rect panelRect = panel.gameObject.GetComponent<RectTransform>().rect;

            GameObject[] buttonObjects;

            switch (status)
            {
                case 1: buttonObjects = antiViruses; break;
                case 2: buttonObjects = viruses; break;
                default: buttonObjects = null; break;
            }

            for (int i = 0; i < buttonObjects.Length; i++)
            {
                Vector3 newPosition = panelPosition;
                Vector3 spawnPosition = panelPosition;
                // We set the x position of the new button
                newPosition.x += offsetXtmp + buttonRect.width * i;
                spawnPosition.x += offsetXtmp + buttonRect.width * 0;
                newPosition.y -= offsetY;

                // Create the new button
                ButtonScript newButton = (ButtonScript)Instantiate(buttonTemplate, spawnPosition, transform.rotation);

                // Name of the object on Unity Editor
                newButton.name = buttonObjects[i].name;

                // Text of the object to display in the game
                newButton.gameObject.GetComponentInChildren<UnityEngine.UI.Text>().text = buttonObjects[i].name;

                // Set the parent of the object to this object (GUI_Towers)
                // If you don't then the object isn't part of the canvas / child of canvas
                newButton.transform.SetParent(panel.transform);

                // Set the position for the button to slide to
                newButton.MoveLocation = newPosition;

                // Assign the button to the game controller
                newButton.GameController = GameObject.FindGameObjectWithTag("GameController");

                // Assign the tower for the button that should be built
                newButton.SourceObject = buttonObjects[i];

                // Add button to array list
                buttons.Add(newButton);
            }

            showButtons = true;
        }
    }

    /// <summary>
    /// Remove Buttons currently on the panel
    /// </summary>
    private void removeButtons()
    {
        Vector3 moveLocation = ((ButtonScript)buttons[0]).MoveLocation;

        foreach (ButtonScript btn in buttons)
        {
            btn.MoveLocation = moveLocation;
            btn.Hide = true;
        }


        showButtons = false;
    }

    /// <summary>
    /// Create Buttons for a specific tower
    /// </summary>
    /// <param name="tower"></param>
    public void showTowerInfo(TowerAI tower)
    {
        int numButtons = 5;
        int textButtonWidth = 50;
        
        if(selectedTower != null)
            if(selectedTower == tower)
                return;

        if (showButtons)
            removeButtons();

        if (!showButtons)
        {
            buttonStatus = 0;

            selectedTower = tower;

            // Hard-coded value
            int offsetXtmp = this.offsetX;

            // Get the Panel's Position
            Vector3 panelPosition = panel.transform.position;

            // Get the Button's Rect Transformation
            Rect buttonRect = buttonTemplate.gameObject.GetComponent<RectTransform>().rect;

            // Get the Panel's Rect Transformation
            Rect panelRect = panel.gameObject.GetComponent<RectTransform>().rect;            

            for (int i = 0; i < numButtons; i++)
            {
                // Create Positions for the buttons to spawn to, and slide to (for sliding effect)
                Vector3 newPosition = panelPosition;
                Vector3 spawnPosition = panelPosition;

                // We set the x position of the new button
                newPosition.x += offsetXtmp + buttonRect.width * i;
                spawnPosition.x += offsetXtmp + buttonRect.width * 0;

                // Create the new button
                ButtonScript newButton = (ButtonScript)Instantiate(buttonTemplate, spawnPosition, transform.rotation);

                // Name of the object on Unity Editor
                switch (i)
                {
                    case 0: newButton.name = "Tower Thumbnail"; newButton.UpgradeTower = "Image"; newButton.UpgradeTowerLevel = 4;  break;
                    case 1: newButton.name = "Tower Information"; newButton.UpgradeTower = "Information"; newButton.UpgradeTowerLevel = 4; break;
                    case 2: newButton.name = "Upgrade Attack Strength"; newButton.UpgradeTower = "towerDamage"; newButton.UpgradeTowerLevel = tower.Levels[0]; break;
                    case 3: newButton.name = "Upgrade  Attack  Speed"; newButton.UpgradeTower = "attackSpd"; newButton.UpgradeTowerLevel = tower.Levels[1]; break;
                    case 4: newButton.name = "Upgrade  Attack  Range"; newButton.UpgradeTower = "attackRange"; newButton.UpgradeTowerLevel = tower.Levels[2]; break;
                }

                if (newButton.UpgradeTowerLevel == 3)
                    newButton.name += "(Maxed)";

                // Text of the object to display in the game
                newButton.gameObject.GetComponentInChildren<UnityEngine.UI.Text>().text = newButton.name;

                if (i == 0)
                {
                    GameObject.Destroy(newButton.GetComponentInChildren<UnityEngine.UI.Text>());
                    newButton.GetComponent<UnityEngine.UI.Button>().image.overrideSprite = tower.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;                   
                    //newButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
                }


                // Tower Description
                if(i == 1)
                {
                    RectTransform textRect = newButton.GetComponent<RectTransform>();

                    textRect.sizeDelta = new Vector2(textRect.rect.width + textButtonWidth, textRect.rect.height);
                    newPosition.x += textButtonWidth/2;
                    offsetXtmp += textButtonWidth;
                    newButton.GetComponent<UnityEngine.UI.Button>().image.color = Color.black;
                    newButton.GetComponentInChildren<UnityEngine.UI.Text>().color = Color.white;
                }

                // Set the parent of the object to this object (GUI_Towers)
                // If you don't then the object isn't part of the canvas / child of canvas
                newButton.transform.SetParent(panel.transform);

                // Set the position for the button to slide to
                newButton.MoveLocation = newPosition;

                // Assign the button to the game controller
                newButton.GameController = GameObject.FindGameObjectWithTag("GameController");

                // Assign the tower for the button that should be built
                newButton.SourceObject = tower.gameObject;

                // Add button to array list
                buttons.Add(newButton);
            }

            showButtons = true;
        }
    }
}
