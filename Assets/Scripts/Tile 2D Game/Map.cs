using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TileTypes
{
    Empty = -1,
    // 0, 14
    Grass = 15,
    Tree = 16,
    Hills = 17,
    Mountains = 18,
    Towns = 19,
    Castle = 20,
    Monster = 21
}

public class Map
{ 
    public int rows = 0;
    public int cols = 0;

    public Tile[] tiles;

    public Tile castleTile;
    public Tile startTile;

    private Tile[] towns;

    public Tile[] CoastTiles
    {
        
        get
        {
            return tiles.Where(t => t.autoTileId < (int)TileTypes.Grass).ToArray();
        }
    } 

    public Tile[] LandTiles
    {
        get
        {
            return tiles.Where(t => t.autoTileId >= (int)TileTypes.Grass).ToArray();
        }
    }

    public void Init(int rows, int cols)   // 0: O 1: X
    {
        this.rows = rows;
        this.cols = cols;

        tiles = new Tile[rows * cols];
        for (int i = 0; i  < tiles.Length; i++)
        {
            tiles[i] = new Tile();
            tiles[i].id = i;
        }

        for (var r = 0; r < rows; ++r)
        {
            for (var c = 0; c < cols; ++c)
            {
                var index = r * cols + c;

                var indexU = (r - 1) * cols + c;
                var indexR = r * cols + c + 1;
                var indexD = (r + 1) * cols + c;
                var indexL = r * cols + c - 1;

                if ((r - 1) >= 0)
                {
                    tiles[index].adjacents[(int)Sides.Top] = tiles[indexU];
                }
                if (c + 1 < cols)
                {
                    tiles[index].adjacents[(int)Sides.Right] = tiles[indexR];
                }
                if (r + 1 < rows)
                {
                    tiles[index].adjacents[(int)Sides.Bottom] = tiles[indexD];
                }
                if (c - 1 >= 0)
                {
                    tiles[index].adjacents[(int)Sides.Left] = tiles[indexL];
                }
            }
        }

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].UpdateAuotoTileId();
            tiles[i].UpdateAuotoFowId();
        }
    }

    public bool CreateIsland(
        float erodePercent,
        int erodeIterations,
        float lakePercent,
        float treePercent,
        float hillPercent,
        float mountainPercent,
        float townPercent,
        float monsterPercent)
    {
        DecorateTiles(LandTiles, lakePercent, TileTypes.Empty);

        for (int i = 0; i < erodeIterations; ++i)
            DecorateTiles(CoastTiles, erodePercent, TileTypes.Empty);

        DecorateTiles(LandTiles, treePercent, TileTypes.Tree);
        DecorateTiles(LandTiles, hillPercent, TileTypes.Hills);
        DecorateTiles(LandTiles, mountainPercent, TileTypes.Mountains);
        DecorateTiles(LandTiles, townPercent, TileTypes.Towns);
        DecorateTiles(LandTiles, monsterPercent, TileTypes.Monster);

        towns = tiles.Where(x => x.autoTileId == (int)TileTypes.Towns).ToArray();
        ShuffleTiles(towns);
        startTile = towns[0];

        var catsleTargets = tiles.Where(x => x.autoTileId <= (int)TileTypes.Grass &&
            x.autoTileId != (int)TileTypes.Empty).ToArray();
        castleTile = catsleTargets[Random.Range(0, catsleTargets.Length)];

        return true;
    }
    
    public bool ChangeTownToCastle(Tile player)
    {
        int Count = 0;
        while (Count < 100)
        {
            if (AStar(player, towns[Random.Range(0, towns.Length)]))
            {
                return true;
            }

            Count++;
        }
        
        return false;
    }

    public void DecorateTiles(Tile[] tiles, float percent, TileTypes tileType)
    {
        int total = Mathf.FloorToInt(tiles.Length * percent);

        ShuffleTiles(tiles);

        for (int i = 0; i < total; ++i)
        {
            if (tileType == TileTypes.Empty)
                tiles[i].ClearAdjacents();

            tiles[i].autoTileId = (int)tileType;
        }
    }

    public void ShuffleTiles(Tile[] tiles)
    {
        // Fisher-Yates 셔플 알고리즘 구현
        for (int i = tiles.Length - 1; i > 0; i--)
        {
            // 0과 i 사이의 무작위 인덱스 선택
            int randomIndex = Random.Range(0, i + 1);

            // i번째 요소와 무작위로 선택된 요소 교환
            Tile temp = tiles[i];
            tiles[i] = tiles[randomIndex];
            tiles[randomIndex] = temp;
        }
    }
    
    public List<Tile> path = new List<Tile>();
    public bool AStar(Tile start, Tile goal)
    {
        path.Clear();
        foreach (var tile in tiles)
        {
            tile.Clear();
        }

        var visited = new HashSet<Tile>(); //방문한 노드 확인용
        var pQueue = new PriorityQueue<Tile, int>(); //int : 거리(가중치) 를 이용해서 거리를 측정한다. (가중치는 이동될때마다 누적되서 갱신한다.)

        var distances = new int[tiles.Length]; //모든 노드들의 거리 배열 / 노드의 id가 인덱스가 될 것이다.
        var scores = new int[tiles.Length];
        for (int i = 0; i < distances.Length; i++)
        {
            scores[i] = distances[i] = int.MaxValue; //초기값은 int 최대값으로 한다.
        }

        distances[start.id] = start.Weight;
        scores[start.id] = distances[start.id] + Heuristic(start, goal); // 기존 거리와 휴리스틱 값을 더한 계산값
        pQueue.Enqueue(start, scores[start.id]);

        bool success = false;
        while (pQueue.Count > 0)
        {
            var currentNode = pQueue.Dequeue();
            if (visited.Contains(currentNode))
            {
                continue;
            }

            if (currentNode == goal)
            {
                success = true;
                break;
            }

            visited.Add(currentNode);
            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanMove || visited.Contains(adjacent))
                {
                    continue;
                }

                var newDistance = distances[currentNode.id] + adjacent.Weight; //인접 노드까지의 실제 비용(거리)
                if (distances[adjacent.id] > newDistance)
                {
                    distances[adjacent.id] = newDistance;
                    scores[adjacent.id] = distances[adjacent.id] + Heuristic(adjacent, goal);
                    adjacent.previous = currentNode;

                    pQueue.Enqueue(adjacent, scores[adjacent.id]);
                }
            }
        }

        //갈 수 있는 경로가 없는 경우
        if (!success)
        {
            return false;
        }

        //실제 길을 잇기 위한 왔던 노드 역추적
        Tile step = goal;
        while (step != null)
        {
            path.Add(step);
            step = step.previous;
        }
        path.Reverse();

        return true;
    }
    
    protected int Heuristic(Tile a, Tile b)
    {
        //a의 x 인덱스와 y 인덱스
        int ax = a.id % cols;
        int ay = a.id / cols;

        int bx = b.id % cols;
        int by = b.id / cols;

        return Mathf.Abs(ax - bx) + Mathf.Abs(ay - by);
    }
}
