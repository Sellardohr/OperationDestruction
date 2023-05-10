using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveAbilityButtonScript : MonoBehaviour
    
{
    // Start is called before the first frame update
    public ActiveAbility myActiveAbility;
    public Button thisButton;
    public BattleManager battleManager;
    public List<Vector2Int> abilityRange;
    void Start()
    {
        thisButton = GetComponent<Button>();
        thisButton.onClick.AddListener(PrepareToExecuteActiveAbility);
        battleManager = GameObject.Find("Battle Manager").GetComponent<BattleManager>();
        abilityRange = new List<Vector2Int>();
    }

    public void PrepareToExecuteActiveAbility()
    {
        //In case we've already loaded up a different ability, we need to abort that ability to prevent loading too many squares.
        if (battleManager.shoppingForAnActiveAbility && battleManager.abilityToConsider != this & battleManager.abilityToConsider != null)
        {
            battleManager.abilityToConsider.AbortAbility();
        }
        //This function is called when the user clicks on an Active Ability in the list.
        myActiveAbility.myButton = this;
        var firingCharacter = new BattleManager.CharacterPosition(battleManager.focusedMech, battleManager.focusedMech.gridPositionsIAmOver, battleManager.focusedMech.myOrientation);
        abilityRange = battleManager.FindAFiringArc(firingCharacter, myActiveAbility);
        battleManager.abilityToConsider = this;
        foreach (Vector2Int grid in abilityRange)
        {
            battleManager.terrainTileDictionary[grid].PrepareMeForMechMove();
        }
        PrepareTheCursorAnchorForThisAbility();
        battleManager.HighlightAListOfSquares(abilityRange);
        //battleManager.StartCoroutine(battleManager.WaitThenUnHighlight(abilityRange, 3));   
    }

    public void PrepareTheCursorAnchorForThisAbility()
    {
        //This function adjusts the appearance of the Cursor Anchor based on the active ability being queued up. Different abilities have different effect areas, and the cursor anchor changes to reflect the new area.
        battleManager.cursorAnchorScript.InitializeMe();
        battleManager.cursorAnchor.SetActive(true);
        battleManager.cursorAnchorScript.readyToChooseAbilityLocus = true;
        battleManager.cursorAnchorScript.abilityIAmCursingFor = this;
        var snapNodes = new List<Vector2>();
        foreach (Vector2Int rangeGrid in abilityRange)
        {
            snapNodes.Add(battleManager.terrainGridToWorldDictionary[rangeGrid]);
        }
        battleManager.cursorAnchorScript.snapToNodes = snapNodes;
        if (myActiveAbility.effectArea == "Single")
        {
            return;
        }
        else if (myActiveAbility.effectArea == "Plus")
        {
            InstantiateACursorAnchorChild(battleManager.cursorAnchor, "North", 1);
            InstantiateACursorAnchorChild(battleManager.cursorAnchor, "East", 1);
            InstantiateACursorAnchorChild(battleManager.cursorAnchor, "South", 1);
            InstantiateACursorAnchorChild(battleManager.cursorAnchor, "West", 1);
            return;
        }
        else if (myActiveAbility.effectArea == "Ninesquare")
        {
            var northChild = InstantiateACursorAnchorChild(battleManager.cursorAnchor, "North", 1);
            InstantiateACursorAnchorChild(northChild, "East", 1);
            InstantiateACursorAnchorChild(northChild, "West", 1);
            InstantiateACursorAnchorChild(battleManager.cursorAnchor, "East", 1);
            var southChild = InstantiateACursorAnchorChild(battleManager.cursorAnchor, "South", 1);
            InstantiateACursorAnchorChild(southChild, "East", 1);
            InstantiateACursorAnchorChild(southChild, "West", 1);
            InstantiateACursorAnchorChild(battleManager.cursorAnchor, "West", 1);
            return;
        }
        else if (myActiveAbility.effectArea == "All")
        {
            battleManager.cursorAnchorScript.isSnapped = true;
            return;
        }
        else if (myActiveAbility.effectArea == "Diamond")
        {
            var northChild = InstantiateACursorAnchorChild(battleManager.cursorAnchor, "North", 1);
            InstantiateACursorAnchorChild(northChild, "East", 1);
            InstantiateACursorAnchorChild(northChild, "West", 1);
            InstantiateACursorAnchorChild(northChild, "North", 1);
            var eastChild = InstantiateACursorAnchorChild(battleManager.cursorAnchor, "East", 1);
            InstantiateACursorAnchorChild(eastChild, "East", 1);
            var southChild = InstantiateACursorAnchorChild(battleManager.cursorAnchor, "South", 1);
            InstantiateACursorAnchorChild(southChild, "East", 1);
            InstantiateACursorAnchorChild(southChild, "West", 1);
            InstantiateACursorAnchorChild(southChild, "South", 1);
            var westChild = InstantiateACursorAnchorChild(battleManager.cursorAnchor, "West", 1);
            InstantiateACursorAnchorChild(westChild, "West", 1);
            return;
        }
        else if (myActiveAbility.effectArea == "Random")
        {
            battleManager.cursorAnchorScript.isSnapped = true;
            return;
        }
        else
        {
            battleManager.cursorAnchorScript.isSnapped = true;
            return;
        }
    }

    public GameObject InstantiateACursorAnchorChild(GameObject cursorAnchor, string orientation, int numberOfSpaces)
    {
        var vectorToInstantiateAt = new Vector3(0, 0, 0);
        vectorToInstantiateAt = cursorAnchor.transform.position;
        var vectorToTranslateBy = new Vector3(0, 0, 0);
        if (orientation == "North")
        {
            var unitVector = new Vector3(0.5f, 0.25f, 0);
            vectorToTranslateBy = unitVector * numberOfSpaces;
            vectorToInstantiateAt += vectorToTranslateBy;
            var instantiatedClone = Instantiate(cursorAnchor, vectorToInstantiateAt, cursorAnchor.transform.rotation);
            instantiatedClone.transform.SetParent(cursorAnchor.transform);
            return instantiatedClone;
        }
        else if (orientation == "South")
        {
            var unitVector = new Vector3(-0.5f, -0.25f, 0);
            vectorToTranslateBy = unitVector * numberOfSpaces;
            vectorToInstantiateAt += vectorToTranslateBy;
            var instantiatedClone = Instantiate(cursorAnchor, vectorToInstantiateAt, cursorAnchor.transform.rotation);
            instantiatedClone.transform.SetParent(cursorAnchor.transform);
            return instantiatedClone;
        }
        else if (orientation == "East")
        {
            var unitVector = new Vector3(0.5f, -0.25f, 0);
            vectorToTranslateBy = unitVector * numberOfSpaces;
            vectorToInstantiateAt += vectorToTranslateBy;
            var instantiatedClone = Instantiate(cursorAnchor, vectorToInstantiateAt, cursorAnchor.transform.rotation);
            instantiatedClone.transform.SetParent(cursorAnchor.transform);
            return instantiatedClone;
        }
        else if (orientation == "West")
        {
            var unitVector = new Vector3(-0.5f, 0.25f, 0);
            vectorToTranslateBy = unitVector * numberOfSpaces;
            vectorToInstantiateAt += vectorToTranslateBy;
            var instantiatedClone = Instantiate(cursorAnchor, vectorToInstantiateAt, cursorAnchor.transform.rotation);
            instantiatedClone.transform.SetParent(cursorAnchor.transform);
            return instantiatedClone;
        }
        else
        {
            return null;
        }
    }

    public void AbortAbility()
    {
        //Debug.Log("Calling AbortAbility()");
        foreach (Vector2Int grid in abilityRange)
        {
            battleManager.terrainTileDictionary[grid].EndPreparingForMechMove();
        }
        battleManager.cursorAnchorScript.InitializeMe();
        battleManager.cursorAnchor.SetActive(false);
        abilityRange.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
