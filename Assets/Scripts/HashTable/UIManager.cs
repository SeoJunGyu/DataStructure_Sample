using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TMP_Dropdown TableMethod;
    public TMP_Dropdown ProbingStrategy;

    public TMP_InputField keyInput;
    public TMP_InputField valueInput;

    public Button addButton;
    public Button removeButton;
    public Button clearButton;

    public ScrollRect logScrollRect;
    private RectTransform logContent;

    public ScrollRect tableScrollRect;
    private RectTransform tableContent;

    public GameObject textPrefab;
    public GameObject panelPrefab;
    public GameObject indexPrefab;
    public GameObject valueInputPrefab;

    private IDictionary<string, string> hashTable;
    private List<GameObject> panelInstances = new List<GameObject>();

    private void Awake()
    {
        logContent = logScrollRect.content;
        tableContent = tableScrollRect.content;

        //TableMethod.onValueChanged.AddListener(OnTableMethodChanged);
        //ProbingStrategy.onValueChanged.AddListener(OnProbingStrategyChanged);


    }

    // private void OnTableMethodChanged(int index)
    // {
    //     CreateHashTable();
    //     RefreshTableUI();
    //     LogMessage($"해시 테이블 방식이 변경되었습니다: {TableMethod.options[index].text}");
    // }

    // private void OnProbingStrategyChanged(int index)
    // {
    //     CreateHashTable();
    //     RefreshTableUI();
    //     LogMessage($"탐사 전략이 변경되었습니다: {ProbingStrategy.options[index].text}");
    // }
    
    private void CreateHashTable()
    {
        
    }


}
