using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public enum HashTableMethod
    {
        OpenAddressing,
        ChainingHash,
    }

    public TMP_Dropdown TableMethod;
    public TMP_Dropdown ProbingStrategy;

    public TMP_InputField keyInput;
    public TMP_InputField valueInput;

    public Button addButton;
    public Button removeButton;
    public Button clearButton;

    public TextMeshProUGUI logText;

    public ScrollRect tableScrollRect;
    private RectTransform tableContent;

    public GameObject panelPrefab;
    public GameObject indexPrefab;
    public GameObject valueInputPrefab;

    private HashTableMethod currentMethod = HashTableMethod.OpenAddressing;

    private OpenAddressingHashTable<string, int> openAddressingHashTable;
    private ChainingHashTable<string, int> chainingHashTable;

    private string inputKey;
    private int inputValue;

    private bool isCleared;

    private List<GameObject> visualObjs;
    private bool[] usedChainingTable;

    private int currentSize = 16;

    private void Start()
    {
        tableContent = tableScrollRect.content;

        openAddressingHashTable = new OpenAddressingHashTable<string, int>();
        chainingHashTable = new ChainingHashTable<string, int>();

        visualObjs = new List<GameObject>();

        TableMethod.onValueChanged.AddListener((i) => OnMethodValueChanged(i));
        ProbingStrategy.onValueChanged.AddListener((i) => OnProbingValueChanged(i));

        keyInput.onValueChanged.AddListener(s => OnKeyFieldChanged(s));
        valueInput.onValueChanged.AddListener(s => OnValueFieldChanged(s));

        addButton.onClick.AddListener(() => OnAddClicked());
        removeButton.onClick.AddListener(() => OnRemoveKVPClicked());
        clearButton.onClick.AddListener(() => OnClearClicked());

        isCleared = true;

        ResetVisualObjs();
        logText.text = string.Empty;

    }

    private void SetSlotText(GameObject obj, int index, string key, int value)
    {
        obj.GetComponentInChildren<TextMeshProUGUI>().text = $"I: {index} K: {key}\tV: {value}";
    }

    private void SetEmptyText(GameObject obj, int index)
    {
        obj.GetComponentInChildren<TextMeshProUGUI>().text = $"I: {index}";
    }

    private void AddLogText(string text)
    {
        logText.text += text + " -> ";
    }

    private void OnMethodValueChanged(int index)
    {
        if (!isCleared)
        {
            return;
        }

        currentMethod = (HashTableMethod)index;
    }

    private void OnProbingValueChanged(int index)
    {
        if (!isCleared)
        {
            return;
        }

        openAddressingHashTable.ProbingStrategy = (ProbingStrategy)index;
    }

    private void OnKeyFieldChanged(string key)
    {
        inputKey = key;
    }

    private void OnValueFieldChanged(string value)
    {
        inputValue = int.Parse(value);
    }

    private void SizeUpChainging(int inputIndex)
    {
        this.currentSize = chainingHashTable.Size;
        ResetVisualObjs();

        chainingHashTable.isSizeChanged = false;

        foreach (var key in chainingHashTable.Keys)
        {
            int index = chainingHashTable.GetProbeIndex(key);
            if (inputIndex == index)
                continue;

            if (!usedChainingTable[index])
                Destroy(visualObjs[index].transform.GetChild(0).gameObject);
            var obj = Instantiate(valueInputPrefab, visualObjs[index].transform);
            SetSlotText(obj, index, key, chainingHashTable[key]);

            usedChainingTable[index] = true;
        }
    }
    private void SizeUpOpen(int inputIndex)
    {
        this.currentSize = openAddressingHashTable.Size;
        ResetVisualObjs();
        
        foreach (var key in openAddressingHashTable.Keys)
        {
            int index = openAddressingHashTable.FindIndex(key);
            if (inputIndex == index)
                continue;

            Destroy(visualObjs[index].transform.GetChild(0).gameObject);
            var obj = Instantiate(valueInputPrefab, visualObjs[index].transform);
            SetSlotText(obj, index, key, openAddressingHashTable[key]);
        }
        openAddressingHashTable.isSizeChanged = false;
    }

    private void OnAddClicked()
    {
        isCleared = false;

        var kvp = new KeyValuePair<string, int>(inputKey, inputValue);

        switch (currentMethod)
        {
            case HashTableMethod.OpenAddressing:
                openAddressingHashTable.Add(kvp);
                if (openAddressingHashTable.isSizeChanged)
                {
                    SizeUpOpen(openAddressingHashTable.FindIndex(inputKey));
                }

                if (openAddressingHashTable.FindIndex(inputKey) == -1)
                {
                    return;
                }

                AddLogText($"ADD {inputKey}");
                CheckUpdateSlot(kvp, valueInputPrefab, false, openAddressingHashTable.FindIndex(inputKey));
                break;
            case HashTableMethod.ChainingHash:
                CheckChainAddAndUpdateSlot(kvp);
                break;
        }
    }

    private void CheckUpdateSlot(KeyValuePair<string, int> kvp, GameObject slot, bool isRemove, int index)
    {
        if (index == -1)
            return;

        Destroy(visualObjs[index].transform.GetChild(0).gameObject);
        var obj = Instantiate(slot, visualObjs[index].transform);
        if (!isRemove)
            SetSlotText(obj, index, inputKey, inputValue);
        else
            SetEmptyText(obj, index);
    }

    private void CheckChainAddAndUpdateSlot(KeyValuePair<string, int> kvp)
    {
        var index2 = chainingHashTable.GetProbeIndex(inputKey);

        chainingHashTable.Add(kvp);

        if (chainingHashTable.isSizeChanged)
        {
            SizeUpChainging(index2);
        }

        bool destroyEmpty = false;
        if (!usedChainingTable[index2])
            destroyEmpty = true;

        if (chainingHashTable.ContainsKey(inputKey))
        {
            AddLogText($"ADD {inputKey}");
            if (destroyEmpty)
                Destroy(visualObjs[index2].transform.GetChild(0).gameObject);

            var obj = Instantiate(valueInputPrefab, visualObjs[index2].transform);
            SetSlotText(obj, index2, inputKey, inputValue);
            usedChainingTable[index2] = true;
        }
    }
    
    private void CheckChainRemoveAndUpdateSlot(KeyValuePair<string, int> kvp)
    {
        var index2 = chainingHashTable.GetProbeIndex(inputKey);

        if (!chainingHashTable.Remove(kvp))
            return;

        AddLogText($"Remove {inputKey}");
        var list = chainingHashTable.GetlistForKey(kvp.Key);

        if (list != null)
        {
            for (int i = 0; i < visualObjs[index2].transform.childCount; i++)
            {
                Destroy(visualObjs[index2].transform.GetChild(i).gameObject);
            }

            foreach (var ele in list)
            {
                var obj = Instantiate(valueInputPrefab, visualObjs[index2].transform);
                SetSlotText(obj, index2, ele.Key, ele.Value);
            }
        }
        else
        {
            Destroy(visualObjs[index2].transform.GetChild(0).gameObject);
            var obj = Instantiate(indexPrefab, visualObjs[index2].transform);
            SetEmptyText(obj, index2);
            usedChainingTable[index2] = false;
        }
    }

    private void OnRemoveKVPClicked()
    {
        isCleared = false;

        var kvp = new KeyValuePair<string, int>(inputKey, inputValue);

        switch (currentMethod)
        {
            case HashTableMethod.OpenAddressing:
                int index = openAddressingHashTable.FindIndex(kvp.Key);
                if (openAddressingHashTable.Remove(kvp))
                {
                    CheckUpdateSlot(kvp, indexPrefab, true, index);
                    AddLogText($"Remove {inputKey}");
                }
                break;
            case HashTableMethod.ChainingHash:
                CheckChainRemoveAndUpdateSlot(kvp);
                break;
        }
    }

    private void OnClearClicked()
    {
        openAddressingHashTable.Clear();
        chainingHashTable.Clear();

        openAddressingHashTable = new OpenAddressingHashTable<string, int>();
        chainingHashTable = new ChainingHashTable<string, int>();

        currentSize = 16;
        ResetVisualObjs();
        isCleared = true;

        logText.text = string.Empty;
    }

    private void ResetVisualObjs()
    {
        visualObjs.Clear();

        for (int i = 0; i < tableContent.childCount; i++)
        {
            Destroy(tableContent.GetChild(i).gameObject);
        }

        for (int i = 0; i < currentSize; i++)
        {
            GameObject panelObj = Instantiate(panelPrefab, tableContent);
            Instantiate(indexPrefab, panelObj.transform);
            SetEmptyText(panelObj, i);
            visualObjs.Add(panelObj);
        }
        
        usedChainingTable = new bool[visualObjs.Count];
    }

}
