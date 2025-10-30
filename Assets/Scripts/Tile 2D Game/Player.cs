using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using System.Threading;
public class Player : MonoBehaviour
{
    public Stage stage;

    private Tile currentTile;

    public float moveSpeed = 5f; // 이동 속도

    private bool isMoving = false;
    private List<Tile> currentPath = new List<Tile>();
    private int currentPathIndex = 0;

    private CancellationTokenSource cts = new CancellationTokenSource();

    void Awake()
    {
        stage = GameObject.FindGameObjectWithTag("Map").GetComponent<Stage>();
        currentTile = stage.Map.startTile;

        cts = new CancellationTokenSource();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isMoving)
        {
            var tileId = stage.ScreenPosToTileId(Input.mousePosition);
            if (stage.Map.tiles[tileId].Weight == int.MaxValue)
            {
                return;
            }

            if (stage.Map.AStar(currentTile, stage.Map.tiles[tileId]))
            {
                currentPath = new List<Tile>(stage.Map.path);
                currentPathIndex = 0;
                MoveAlongPath().Forget();
            }
        }
    }

    public async UniTaskVoid MoveAlongPath()
    {
        isMoving = true;

        // 경로의 각 타일을 순회
        for (currentPathIndex = 0; currentPathIndex < currentPath.Count; currentPathIndex++)
        {
            Tile targetTile = currentPath[currentPathIndex];
            Vector3 startPos = transform.position;
            Vector3 targetPos = stage.GetTilePos(targetTile.id);

            float journey = 0f;
            float distance = Vector3.Distance(startPos, targetPos);

            // 한 타일에서 다음 타일로 부드럽게 이동
            while (journey < distance)
            {
                journey += moveSpeed * Time.deltaTime;
                float t = Mathf.Clamp01(journey / distance);
                transform.position = Vector3.Lerp(startPos, targetPos, t);
                await UniTask.Yield();
            }

            // 정확한 위치로 보정
            transform.position = targetPos;
            currentTile = targetTile;
        }

        isMoving = false;
    }

    public void CancelMovement()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();
    }
}
