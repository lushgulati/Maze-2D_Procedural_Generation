using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    public CinemachineVirtualCamera vcam;
    public Transform Player;
    public Transform Goal;
    public Transform Walls;
    public GameObject WallTemplate;
    public GameObject FloorTemplate;
    public float MovementSmoothing;

    public int Width = 3;
    public int Height = 3;
    public bool[,] HWalls, VWalls;
    public float HoleProbability;
    public int GoalX, GoalY;
    public int PlayerX, PlayerY;
   
    void Start()
    {
        StartNext();
    }

    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) && !HWalls[PlayerX, PlayerY])
            PlayerX--;
        if (Input.GetKeyDown(KeyCode.D) && !HWalls[PlayerX + 1, PlayerY])
            PlayerX++;
        if (Input.GetKeyDown(KeyCode.W) && !VWalls[PlayerX, PlayerY + 1])
            PlayerY++;
        if (Input.GetKeyDown(KeyCode.S) && !VWalls[PlayerX, PlayerY])
            PlayerY--;

        Vector3 target = new Vector3(PlayerX + 0.5f, PlayerY + 0.5f);

        Player.transform.position = Vector3.Lerp(Player.transform.position, target, 0.1f);

        if(Vector3.Distance(Player.transform.position, Goal.position)<0.12f)
        {
            if (Rand(5) < 3)
                Width++;
            else
                Height++;

            StartNext();
        }
        if (Input.GetKeyDown(KeyCode.G))
            StartNext();
    }

    public int Rand(int max)
    {
        return UnityEngine.Random.Range(0, max);
    }
    public float frand()
    {
        return UnityEngine.Random.value;
    }

    public void StartNext()
    {
        foreach (Transform child in Walls)
            Destroy(child.gameObject);

        (HWalls, VWalls) = GenerateLevel(Width, Height);
        PlayerX = Rand(Width);
        PlayerY = Rand(Height);

        int minDiff = Mathf.Max(Width, Height) / 2;
        while(true)
        {
            GoalX = Rand(Width);
            GoalY = Rand(Height);
            if (Mathf.Abs(GoalX - PlayerX) >= minDiff) break;
            if (Mathf.Abs(GoalY - PlayerY) >= minDiff) break;
        }

        for (int x = 0; x < Width + 1; x++)
            for (int y = 0; y < Height; y++)
                if (HWalls[x, y])
                    Instantiate(WallTemplate, new Vector3(x, y + 0.5f, 0), Quaternion.Euler(0, 0, 90), Walls);
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height + 1; y++)
                if (VWalls[x, y])
                    Instantiate(WallTemplate, new Vector3(x + 0.5f, y, 0), Quaternion.Euler(0, 0, 0), Walls);
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                Instantiate(FloorTemplate, new Vector3(x + 0.5f, y + 0.5f), Quaternion.identity, Walls);

        Player.transform.position = new Vector3(PlayerX + 0.5f, PlayerY + 0.5f);
        Goal.transform.position = new Vector3(GoalX + 0.5f, GoalY + 0.5f);

        vcam.m_Lens.OrthographicSize = Mathf.Pow(Width / 3 + Height / 2, 0.7f) + 1;

    }

    public(bool[,],bool[,]) GenerateLevel(int w, int h)
    {
        bool[,] hwalls = new bool[w + 1, h];
        bool[,] vwalls = new bool[w, h + 1];

        bool[,] visited = new bool[w, h];
        bool dfs(int x, int y)
        {
            if (visited[x, y])
                return false;
            visited[x, y] = true;

            var dirs = new[]
            {
                (x-1,y,hwalls,x,y),
                (x+1,y,hwalls,x+1,y),
                (x,y -1,vwalls,x,y),
                (x,y+1,vwalls,x,y+1),
            };

            foreach (var (nx, ny, wall, wx, wy) in dirs.OrderBy(t => frand()))
                wall[wx, wy] = !(0 <= nx && nx < w && 0 <= ny && ny < h && (dfs(nx, ny) == visited[nx,ny]) && frand() > HoleProbability);
                

            return true;
        }
        dfs(0, 0);

        return (hwalls, vwalls);
    }
}

